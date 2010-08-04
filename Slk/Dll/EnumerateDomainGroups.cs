/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
#if !TEST_DOMAIN_GROUPS
using Microsoft.SharePoint;
using Resources.Properties;
#endif

namespace Microsoft.SharePointLearningKit
{

/// <summary>
/// Contains utilities for manipulating domain groups.
/// </summary>
///
static class DomainGroupUtilities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LOCALGROUP_MEMBERS_INFO_3
    {
        public string lgrmi3_domainandname;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct USER_INFO_2
    {
        public string usri2_name;
        public string usri2_password;
        public uint usri2_password_age;
        public uint usri2_priv;
        public string usri2_home_dir;
        public string usri2_comment;
        public uint usri2_flags;
        public string usri2_script_path;
        public uint usri2_auth_flags;
        public string usri2_full_name;
        public string usri2_usr_comment;
        public string usri2_parms;
        public string usri2_workstations;
        public uint usri2_last_logon;
        public uint usri2_last_logoff;
        public uint usri2_acct_expires;
        public uint usri2_max_storage;
        public uint usri2_units_per_week;
        public IntPtr usri2_logon_hours;
        public uint usri2_bad_pw_count;
        public uint usri2_num_logons;
        public string usri2_logon_server;
        public uint usri2_country_code;
        public uint usri2_code_page;
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("Netapi32.dll")]
    extern static int NetLocalGroupGetMembers([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string localgroupname, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out int resumehandle);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("Netapi32.dll")]
    extern static int NetApiBufferFree(IntPtr Buffer);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("Netapi32.dll")]
    extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

    /// <summary>
    /// Retrieves information about a user from a <c>LOCALGROUP_MEMBERS_INFO_3</c> and populates users list.
    /// </summary>
    ///
    /// <param name="lgmi3">The <c>LOCALGROUP_MEMBERS_INFO_3</c> to retrieve from.</param>
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
    ///     <c>DomainGroupEnumerationException</c> will be thrown.</param>
    /// <param name="users">Collection of <c>spUserInfo</c></param>
    /// <param name="configRoot">Last successful AD Configuration root searched.</param>
    /// <param name="configResult">Last successful AD Configuration search result.</param>
    /// <param name="configPath">DN Path from last successfully processed directory entry.</param>
    /// <param name="hideDisabledUsers">Indicates is disabled users should be returned or not.</param>
    ///
    static private void GetSPUserInfoFromLocalGroupMembersInfo(LOCALGROUP_MEMBERS_INFO_3 lgmi3,
        DateTime timeoutTime, Dictionary<string, SPUserInfo> users,
        ref DirectoryEntry configRoot, ref SearchResult configResult, ref String configPath, bool hideDisabledUsers)
    {
        IntPtr bufPtr;
        USER_INFO_2 user = new USER_INFO_2();
        int backslash = lgmi3.lgrmi3_domainandname.IndexOf('\\');
        string domainName = lgmi3.lgrmi3_domainandname.Substring(0, backslash);
        string loginName = lgmi3.lgrmi3_domainandname.Substring(backslash + 1);
        if (NetUserGetInfo(domainName, loginName, 2, out bufPtr) != 0)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainName))
                {
                    using (DirectorySearcher mySearcher = new DirectorySearcher(entry))
                    {
                        //mySearcher.ClientTimeout = timeout;
                        mySearcher.Filter = "(sAMAccountname=" + loginName + ")";
                        SearchResult result = mySearcher.FindOne();
                        if (result != null)
                        {
                            DirectoryEntry de = result.GetDirectoryEntry();
                            ProcessDirectoryEntry(de, timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers);
                        }
                    }
                }
            }
            catch (COMException ex)
            {
                throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.DomainGroupEnumFailed, "LDAP://" + domainName), ex);
            }
        }
        else
        {
            user = (USER_INFO_2)Marshal.PtrToStructure(bufPtr, typeof(USER_INFO_2));

            // construct an <spUserInfo> to contain information about this user
            SPUserInfo spUserInfo = new SPUserInfo();
            // set <spUserInfo.LoginName> to the user's domain name, of the form "domain\username"
            spUserInfo.LoginName = lgmi3.lgrmi3_domainandname;
            spUserInfo.Name = user.usri2_full_name;
            
            users[spUserInfo.LoginName] = spUserInfo;

            NetApiBufferFree(bufPtr);
        }
    }

    /// <summary>
    /// Tests if the passed group ID is a local group and if so, enumerates the members. 
    /// </summary>
    /// 
    /// <param name="domainName">Domain name where the local group is stored (e.g. MYMACHINE).</param>
    /// <param name="groupName">Name of the group to check (e.g. Administrators).</param>
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
    ///     <c>DomainGroupEnumerationException</c> will be thrown.</param>
    /// <param name="users">If the domainName and groupName indeed refer to a viable local group, 
    ///     this collection will contain the resulting members of that group.</param>
    /// <param name="configRoot">Last successful AD Configuration root searched.</param>
    /// <param name="configResult">Last successful AD Configuration search result.</param>
    /// <param name="configPath">DN Path from last successfully processed directory entry.</param>
    /// <param name="hideDisabledUsers">Indicates is disabled users should be returned or not.</param>
    ///
    /// <returns>True if this is a local group, false otherwise.</returns>
    private static bool ReadLocalGroup(string domainName, string groupName, DateTime timeoutTime,
        Dictionary<string, SPUserInfo> users, ref DirectoryEntry configRoot, 
        ref SearchResult configResult, ref String configPath, bool hideDisabledUsers)
    {
        int entriesRead;
        int totalEntries;
        int resume;
        IntPtr bufPtr;

        NetLocalGroupGetMembers(domainName, groupName, 3, out bufPtr, -1, out entriesRead, out totalEntries, out resume);

        if(entriesRead > 0)
        {
            LOCALGROUP_MEMBERS_INFO_3[] members = new LOCALGROUP_MEMBERS_INFO_3[entriesRead];
            IntPtr iter = bufPtr;
            for(int i = 0 ; i < entriesRead ; i++)
            {
                TimeSpan remainingTime = timeoutTime - DateTime.Now;
                if(remainingTime <= TimeSpan.Zero)
                {
                    throw new DomainGroupEnumerationException(AppResources.DomainGroupEnumTimeout);
                }
                members[i] = (LOCALGROUP_MEMBERS_INFO_3)Marshal.PtrToStructure(iter, typeof(LOCALGROUP_MEMBERS_INFO_3));
                iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(LOCALGROUP_MEMBERS_INFO_3)));

                GetSPUserInfoFromLocalGroupMembersInfo(members[i], timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers);
            }
            NetApiBufferFree(bufPtr);

            return true;
        }
        return false;
    }

    /// <summary>
    /// Processes a <c>DirectoryEntry</c>, enumerating members if a group.
    /// </summary>
    /// 
    /// <param name="entry">The <c>DirectoryEntry</c> to process.</param>
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
    ///     <c>DomainGroupEnumerationException</c> will be thrown.</param>
    /// <param name="users">Information about each enumerated user is added to this list.</param>
    /// <param name="configRoot">Last successful AD Configuration root searched.</param>
    /// <param name="configResult">Last successful AD Configuration search result.</param>
    /// <param name="configPath">DN Path from last successfully processed directory entry.</param>
    /// <param name="hideDisabledUsers">Whether to hide disabled users or not.</param>
    /// 
    /// <exception cref="DomainGroupEnumerationException">
    /// Enumeration of the domain group failed.
    /// </exception>
    ///
    private static void ProcessDirectoryEntry(DirectoryEntry entry, DateTime timeoutTime,
        Dictionary<string, SPUserInfo> users, ref DirectoryEntry configRoot, 
        ref SearchResult configResult, ref String configPath, bool hideDisabledUsers)
    {
        if (entry.SchemaClassName == "group")
        {
            AddGroupChildren(entry, timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers);
        }
        else if (entry.SchemaClassName == "user")
        {
            bool addUser = true;
            if (hideDisabledUsers)
            {
                PropertyValueCollection accountControl = entry.Properties["userAccountControl"]; 
                if (accountControl != null && accountControl.Value != null)
                {
                    // 2 is the flag for account disabled
                    if (((int)accountControl.Value & 2) == 2)
                    {
                        addUser = false;
                    }
                }
            }
            
            if (addUser)
            {
                SPUserInfo spUserInfo = GetSPUserInfoFromDirectoryEntry(entry, ref configRoot, ref configResult, ref configPath);
                users[spUserInfo.LoginName] = spUserInfo;
            }
        }
    }

    /// <summary>
    /// Enumerates the members of a domain group, given a <c>DirectoryEntry</c> representing that
    /// group.
    /// </summary>
    ///
    /// <param name="entry">The <c>DirectoryEntry</c> to enumerate users of.</param>
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
    ///     <c>DomainGroupEnumerationException</c> will be thrown.</param>
    /// <param name="users">Information about each enumerated user is added to this list.</param>
    /// <param name="configRoot">Last successful AD Configuration root searched.</param>
    /// <param name="configResult">Last successful AD Configuration search result.</param>
    /// <param name="configPath">DN Path from last successfully processed directory entry.</param>
    /// <param name="hideDisabledUsers">Indicates is disabled users should be returned or not.</param>
    ///
    /// <exception cref="DomainGroupEnumerationException">
    /// Enumeration of the domain group failed.
    /// </exception>
    ///
    private static void AddGroupChildren(DirectoryEntry entry, DateTime timeoutTime,
        Dictionary<string, SPUserInfo> users, ref DirectoryEntry configRoot, 
        ref SearchResult configResult, ref String configPath, bool hideDisabledUsers)
    {
            try
            {
            // enumerate the domain group using a DirectorySearcher
                using(DirectorySearcher mySearcher = new DirectorySearcher(entry))
                {
                    // calculate the remaining time before timeout and set the timeout of the
                    // DirectorySearcher to that value; if we've already exceeded the timeout,
                    // throw an exception
                    TimeSpan remainingTime = timeoutTime - DateTime.Now;
                    if (remainingTime <= TimeSpan.Zero)
                    {
                        throw new DomainGroupEnumerationException(
                            AppResources.DomainGroupEnumTimeout);
                    }
                    mySearcher.ClientTimeout = remainingTime;

                    // specify other parameters for the DirectorySearcher
                    mySearcher.Filter = "(objectClass=group)";

                    // do the search
                    using(SearchResultCollection resultCollection = mySearcher.FindAll())
                    {
                        foreach(SearchResult result in resultCollection)
                        {
                                foreach(object member in result.Properties["member"])
                                {
                                        // check for timeout here because the DirectoryEntry constructor may
                                        // take some time and has no timeout
                                        // alternately, we could just parse the string for member, but that is
                                        // somewhat iffy as to getting the various pieces (e.g. SchemaClassName)
                                        // and definitely doesn't tell us if the member returned is valid or not
                            remainingTime = timeoutTime - DateTime.Now;
                            if (remainingTime <= TimeSpan.Zero)
                            {
                                throw new DomainGroupEnumerationException( AppResources.DomainGroupEnumTimeout);
                            }
                            try
                            {
                                using(DirectoryEntry de = new DirectoryEntry("LDAP://" + (string)member))
                                {
                                    ProcessDirectoryEntry(de, timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers);
                                }
                            }
                            catch(COMException)
                            {
                                // just ignore this exception - for some reason sometimes an AD group has some members that aren't valid AD entries
                            }
                        }
                    }
                }
            }
        }
        catch(COMException ex)
        {
            // convert COM exceptions into DomainGroupEnumerationException exceptions
            throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentCulture,
                AppResources.DomainGroupEnumFailed, entry.Path), ex);
        }
    }

    /// <summary>
    /// Retrieves AD domain NETBios name for specified <c>DirectoryEntry</c>.
    /// </summary>
    ///
    /// <param name="de">The <c>DirectoryEntry</c> to process.</param>
    /// <param name="rootEntry">Last successful AD Configuration root searched.</param>
    /// <param name="searchResult">Last successful AD Configuration search result.</param>
    /// <param name="rootPath">DN Path from last successfully processed directory entry.</param>
    ///
    /// <returns>
    /// Success or Failure (True/False) of NETBios name search.
    /// </returns>
    ///
    private static bool GetDomainNetBIOSName(DirectoryEntry de, ref DirectoryEntry rootEntry, 
        ref SearchResult searchResult, ref String rootPath)
    {
        try
        {
            // Assuming Path of LDAP://[Server]/DN
            int slashPos = (de == null ? -1 : de.Path.LastIndexOf("/"));
            if (slashPos < 0) return false;

            // We need DCs to find root Configuration
            int dcPos = de.Path.IndexOf("DC=", StringComparison.OrdinalIgnoreCase);
            if (dcPos < 0) return false;

            string domainPath = de.Path.Substring(dcPos).Trim();
            bool matched = false;

            /* Check last successfully processed details and use them if matched
             * This is necessary to speed up this process especially as most group
             * members are likely to be from same domain and we don't want to 
             * do unnecessary AD lookups
             */
            if (rootEntry != null && rootPath != null && rootPath.Length > 0)
            {
                if (domainPath.Length > rootPath.Length)
                    matched = domainPath.EndsWith(rootPath, StringComparison.OrdinalIgnoreCase);
                else
                    matched = rootPath.EndsWith(domainPath, StringComparison.OrdinalIgnoreCase);
            }
            
            if (!matched)
            {
                // Clear cached Configuration root as it doesn't match this de
                if (rootEntry != null)
                {
                    try { rootEntry.Dispose(); }
                    catch { }
                }
                rootEntry = null;
                rootPath = "";
                searchResult = null;
            }
            else
            {
                // Clear cached Configuration search result if it doesn't match this dn
                if (searchResult != null && !searchResult.Properties["nCName"][0].ToString().Equals(domainPath, StringComparison.OrdinalIgnoreCase))
                    searchResult = null;
            }

            if (searchResult == null)
            {
                /* Some cached Configuration info not applicable so start new search
                 * 
                 * On first pass use cached Configuration root if it matched this de
                 * otherwise go straight to loop of try increasing domain hierarchy
                 * for this dn. 
                 * 
                 * e.g. for LDAP://CN=User,OU=Unit,DN=admin,DN=headoffice,DN=acme,DN=co,DN=uk
                 * try 
                 * 1. DN=admin,DN=headoffice,DN=acme,DN=co,DN=uk
                 * 2. DN=headoffice,DN=acme,DN=co,DN=uk
                 * 3. DN=acme,DN=co,DN=uk
                 * 
                 * This is necessary because Configuration is always at tree root 
                 * 
                 * Looking for LDAP://CN=?,CN=Partitions,CN=Configuration,DN=..
                 * where container property nCName="DN=admin,DN=headoffice,DN=acme,DN=co,DN=uk" 
                 * which holds the NETBIOS (Pre Windows 2000) domain name in property 
                 * nETBIOSName
                 */

                StringBuilder sb = null;
                string[] dcs = null;

                using (DirectorySearcher searcher = new DirectorySearcher())
                {
                    searcher.PropertiesToLoad.Add("nCName");
                    searcher.PropertiesToLoad.Add("nETBIOSName");
                    searcher.Filter = "(&(objectClass=crossRef)(nCName=" + domainPath + "))";
                    int d = (matched ? -2 : -1);
                    do
                    {
                        try
                        {
                            if (++d >= 0)
                            {
                                if (d == 0)
                                {
                                    sb = new StringBuilder();
                                    dcs = domainPath.Split(',');
                                }
                                if (d >= dcs.Length) break;

                                sb.Length = 0;
                                for (int i = d; i < dcs.Length; i++)
                                    sb.Append((sb.Length > 0 ? "," : "") + dcs[i]);

                                rootEntry = new DirectoryEntry(de.Path.Substring(0, slashPos + 1) + "CN=Configuration," + sb.ToString());
                                rootPath = domainPath;
                            }
                            searcher.SearchRoot = rootEntry;
                            searchResult = searcher.FindOne();
                        }
                        catch { }
                        finally
                        {
                            if (searchResult == null)
                            {
                                try { rootEntry.Dispose(); }
                                catch { }
                                rootEntry = null;
                            }
                        }
                    }
                    while (searchResult == null);
                }
            }
        }
        catch { }
        return (searchResult != null);
    }

    /// <summary>
    /// Retrieves information about a user from a <c>DirectoryEntry</c> and stores it in a
    /// <c>SPUserInfo</c>.
    /// </summary>
    ///
    /// <param name="de">The <c>DirectoryEntry</c> to retrieve from.</param>
    /// <param name="configRoot">Last successful AD Configuration root searched.</param>
    /// <param name="configResult">Last successful AD Configuration search result.</param>
    /// <param name="configPath">DN Path from last successfully processed directory entry.</param>
    ///
    /// <returns>
    /// An <c>SPUserInfo</c> containing information about the specified user.  Note that we're
    /// just using <c>SPUserInfo</c> because it's a convenient structure for typical callers of
    /// this method -- this method does not interact with SharePoint in any way.
    /// </returns>
    ///
    static private SPUserInfo GetSPUserInfoFromDirectoryEntry(DirectoryEntry de,
        ref DirectoryEntry configRoot, ref SearchResult configResult, ref String configPath)
    {
#if false
        // for debugging purposes, the following code can display all property values
        #warning slow debugging code enabled
        foreach (string str in de.Properties.PropertyNames)
            System.Diagnostics.Debug.WriteLine(str + "=" + de.Properties[str].Value);
#endif

        // construct an <spUserInfo> to contain information about this user
        SPUserInfo spUserInfo = new SPUserInfo();

        // set <spUserInfo.LoginName> to the user's domain name, of the form "domain\username"
        StringBuilder sb = new StringBuilder();

        // try getting NETBios domain name from AD Configuration
        if (GetDomainNetBIOSName(de, ref configRoot, ref configResult, ref configPath) &&
            configResult.Properties.Contains("nETBIOSName") && configResult.Properties["nETBIOSName"].Count > 0)
        {
            sb.Append(configResult.Properties["nETBIOSName"][0].ToString());
        }
        else
        {
            string[] parts = de.Path.Split(',');
            bool addDot = false;
            foreach (string part in parts)
            {
                if (part.StartsWith("DC=", StringComparison.Ordinal))
                {
                    if (addDot)
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        addDot = true;
                    }
                    sb.Append(part.Remove(0, 3));

                    // NOTE:  If the following "break" statement is removed, the returned domain and
                    // user name may be of the form e.g. "part1.part2.part2\username", where
                    // "part1.part2.part2" is the fully-qualified domain name.  With the "break"
                    // statement comment included, the returned domain and user name is of the form
                    // "part1\username" -- which works better in some situations.
                    break;
                }
            }
        }

        sb.Append('\\');
        sb.Append((string)de.Properties["sAMAccountName"][0]);
        spUserInfo.LoginName = sb.ToString();

        // set <spUserInfo.Name> to the user's display name, if available -- otherwise use their
        // domain name
        object value;
        if ((value = de.Properties["displayName"].Value) != null)
            spUserInfo.Name = value.ToString();
        else
        if ((value = de.Properties["name"].Value) != null)
            spUserInfo.Name = value.ToString();
        else
            spUserInfo.Name = spUserInfo.LoginName;

        // set <spUserInfo.Email> to the user's email address, if available
        if ((value = de.Properties["mail"].Value) != null)
            spUserInfo.Email = value.ToString();
        else
        if ((value = de.Properties["userPrincipalName"].Value) != null)
            spUserInfo.Email = value.ToString();

        return spUserInfo;
    }

    ///<summary>
    /// Enumerates the members of a domain group.
    /// </summary>
    ///<param name="groupName">The name of the group to enumerate; e.g. "mydomain\mygroup".</param>
    ///<param name="timeout">An exception is thrown if enumeration takes longer than approximately
    ///     this amount of time.</param>
    ///<param name="hideDisabledUsers">Whether to retrieve disabled users or not.</param>
    ///<returns>
    /// A collection of <c>SPUserInfo</c> objects referring to the members of the group.  Note that
    /// these users are not necessarily members of any SharePoint site collection --
    /// <c>SpUserInfo</c> is used simply as a convenient class to contain information about users.</returns>
    static public ICollection<SPUserInfo> EnumerateDomainGroup(string groupName, TimeSpan timeout, bool hideDisabledUsers)
    {
        /* Placeholders for cached last AD Configuration processed
         * to speed up NETBIOS domain name retrieval - most group entries 
         * will be from same domain and this therefore allows us to minimise 
         * AD lookups
         */
        DirectoryEntry configRoot = null;
        SearchResult configResult = null;
        String configPath = "";

        // track the time that a timeout will occur
        DateTime timeoutTime = DateTime.Now + timeout;

        // set <domainName> to the "domain" part of "domain\username"
        int backslashIndex = groupName.IndexOf('\\');
        if (backslashIndex <= 0)
        {
            throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentUICulture, AppResources.DomainGroupNameHasNoBackslash, groupName));
        }
        string domainName = groupName.Substring(0, backslashIndex);
        string loginName = groupName.Substring(backslashIndex + 1);

        // initialize <users> to be a dictionary which will hold an SPUserInfo of each user in
        // the enumerated group; the key is the login name of the user, e.g. "domain\username"
        Dictionary<string, SPUserInfo> users = new Dictionary<string, SPUserInfo>();

        if (ReadLocalGroup(domainName, loginName, timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers))
        {
            return users.Values;
        }

        DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, domainName);
        Domain objDomain = Domain.GetDomain(objContext);

        // enumerate users
        try
        {
            using (DirectoryEntry entry = objDomain.GetDirectoryEntry())
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry))
                {
                    mySearcher.ClientTimeout = timeout;
                    mySearcher.Filter = string.Format(CultureInfo.InvariantCulture, "(&(objectClass=group)(sAMAccountName={0}))", loginName);
                    using (SearchResultCollection resultCollection = mySearcher.FindAll())
                    {
                        foreach (SearchResult result in resultCollection)
                        {
                            foreach (object member in result.Properties["member"])
                            {
                                // check for timeout here because the DirectoryEntry constructor
                                // may take some time and has no timeout
                                TimeSpan remainingTime = timeoutTime - DateTime.Now;
                                if (remainingTime <= TimeSpan.Zero)
                                {
                                    throw new DomainGroupEnumerationException(
                                        AppResources.DomainGroupEnumTimeout);
                                }
                                try
                                {
                                    using (DirectoryEntry de = new DirectoryEntry("LDAP://" + (string)member))
                                    {
                                        ProcessDirectoryEntry(de, timeoutTime, users, ref configRoot, ref configResult, ref configPath, hideDisabledUsers);
                                    }
                                }
                                catch (COMException)
                                {
                                    // just ignore this exception - for some reason sometimes an AD group has some members that aren't valid AD entries
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (COMException ex)
        {
            throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentCulture,
                AppResources.DomainGroupEnumFailed, objDomain.Name), ex);
        }

        return users.Values;
    }
}

/// <summary>
/// Indicates an error that occurred while attempting to enumerate the members of a
/// domain group using <c>DomainGroupUtilities.EnumerateDomainGroup</c>.
/// </summary>
[Serializable]
public class DomainGroupEnumerationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <Typ>DomainGroupEnumerationException</Typ> class.
    /// </summary>
    public DomainGroupEnumerationException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>DomainGroupEnumerationException</Typ> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainGroupEnumerationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>DomainGroupEnumerationException</Typ> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainGroupEnumerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>DomainGroupEnumerationException</Typ> class.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DomainGroupEnumerationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

}
