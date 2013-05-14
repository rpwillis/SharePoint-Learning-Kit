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
using System.Security;
using System.Security.Principal;
using System.Text;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{

    /// <summary>Contains utilities for manipulating domain groups.</summary>
    class DomainGroupUtilities
    {
        DateTime timeoutTime;
        TimeSpan timeout;
        bool hideDisabledUsers;
        Dictionary<string, SPUserInfo> processed = new Dictionary<string, SPUserInfo>();
        Dictionary<string, object> processedThisGroup;
        Dictionary<string, string> domainNames = new Dictionary<string, string>();
        List<string> errors = new List<string>();
        string[] searchAttributes = new string[]{"member", "distinguishedName", "objectClass", "sAMAccountName", "userPrincipalName" , "mail", "name", "displayName", "userAccountControl", "objectSid"};

    #region Native Methods
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
    #endregion Native Methods

    #region properties
        /// <summary>Any non-fatal errors.</summary>
        public IEnumerable<string> Errors
        {
            get { return errors ;}
        }

        TimeSpan RemainingTime
        {
            get 
            {
                TimeSpan remainingTime = timeoutTime - DateTime.Now;
                if (remainingTime <= TimeSpan.Zero)
                {
                    throw new DomainGroupEnumerationException(AppResources.DomainGroupEnumTimeout);
                }
                else
                {
                    return remainingTime;
                }
            }
        }
    #endregion properties

    #region constructors
        /// <summary>Initializes a new instance of <see cref="DomainGroupUtilities"/>.</summary>
        ///<param name="timeout">An exception is thrown if enumeration takes longer than approximately this amount of time.</param>
        ///<param name="hideDisabledUsers">Whether to retrieve disabled users or not.</param>
        public DomainGroupUtilities(TimeSpan timeout, bool hideDisabledUsers)
        {
            timeoutTime = DateTime.Now + timeout;
            this.timeout = timeout;
            this.hideDisabledUsers = hideDisabledUsers;
        }
    #endregion constructors

    #region public methods
        ///<summary>Enumerates the members of a domain group.</summary>
        ///<returns>
        /// A collection of <c>SPUserInfo</c> objects referring to the members of the group.  Note that
        /// these users are not necessarily members of any SharePoint site collection --
        /// <c>SpUserInfo</c> is used simply as a convenient class to contain information about users.</returns>
        public ICollection<SPUserInfo> EnumerateDomainGroup(SPUser group)
        {

            processedThisGroup  = new Dictionary<string, object>();
            string ldapPath = string.Format("LDAP://<SID={0}>", SidToOctetString(group));
            try
            {
                using (DirectoryEntry groupEntry = new DirectoryEntry(ldapPath))
                {
                    processedThisGroup.Add(groupEntry.Properties["distinguishedName"][0].ToString(), null);
                    return Expand(groupEntry);
                }
            }
            catch (DirectoryServicesCOMException e)
            {
                if (e.ErrorCode == -2147016656)
                {
                    return EnumerateLocalGroup(group);
                }
                else
                {
                    throw new DomainGroupEnumerationException(string.Format(SlkCulture.GetCulture(), AppResources.DomainGroupNotFound, group.Name));
                }
            }
        }

    #endregion public methods

    #region private methods
        ICollection<SPUserInfo> EnumerateLocalGroup(SPUser group)
        {
            int entriesRead;
            int totalEntries;
            int resume;
            IntPtr bufPtr;

            DomainAndName names = new DomainAndName(group.LoginName);
            List<SPUserInfo> users = new List<SPUserInfo>();

            NetLocalGroupGetMembers(names.Domain, names.LoginName, 3, out bufPtr, -1, out entriesRead, out totalEntries, out resume);

            try
            {
                if (entriesRead > 0)
                {
                    LOCALGROUP_MEMBERS_INFO_3[] members = new LOCALGROUP_MEMBERS_INFO_3[entriesRead];
                    IntPtr pointer = bufPtr;
                    for(int i = 0 ; i < entriesRead ; i++)
                    {
                        TimeSpan remaining = RemainingTime;
                        members[i] = (LOCALGROUP_MEMBERS_INFO_3)Marshal.PtrToStructure(pointer, typeof(LOCALGROUP_MEMBERS_INFO_3));
                        pointer = (IntPtr)((int)pointer + Marshal.SizeOf(typeof(LOCALGROUP_MEMBERS_INFO_3)));

                        ProcessLocalGroupMember(users, names.Domain, members[i].lgrmi3_domainandname);
                    }
                }
            }
            finally
            {
                NetApiBufferFree(bufPtr);
            }

            return users;
        }

        void ProcessLocalGroupMember(List<SPUserInfo> users, string machineName, string domainAndName)
        {
            DomainAndName names = new DomainAndName(domainAndName);

            if (string.Compare(names.Domain, machineName, true, CultureInfo.InvariantCulture) == 0)
            {
                // Same machine so is a local group
                IntPtr bufPtr = IntPtr.Zero;
                try
                {
                    if (NetUserGetInfo(names.Domain, names.LoginName, 2, out bufPtr) == 0)
                    {
                        USER_INFO_2 user = new USER_INFO_2();
                        user = (USER_INFO_2)Marshal.PtrToStructure(bufPtr, typeof(USER_INFO_2));

                        SPUserInfo spUser = new SPUserInfo();
                        spUser.LoginName = domainAndName;
                        spUser.Name = user.usri2_full_name;
                        users.Add(spUser);
                    }
                }
                finally
                {
                    if (bufPtr != IntPtr.Zero)
                    {
                        NetApiBufferFree(bufPtr);
                    }
                }
            }
            else
            {
                // Domain user
                try
                {
                    using (DirectoryEntry root = new DirectoryEntry("LDAP://" + names.Domain))
                    {
                        string filter = string.Format(CultureInfo.InvariantCulture, "(sAMAccountname={0})", names.LoginName);
                        using (DirectorySearcher searcher = new DirectorySearcher(root, filter, searchAttributes, SearchScope.Subtree))
                        {
                            ProcessSearchResults(searcher, users);
                        }
                    }
                }
                catch (COMException ex)
                {
                    throw new DomainGroupEnumerationException(String.Format(SlkCulture.GetCulture(), AppResources.DomainGroupEnumFailed, "LDAP://" + names.Domain), ex);
                }
            }
        }

        static void CheckSid(byte[] sid)
        {
            SecurityIdentifier identifier = new SecurityIdentifier(sid, 0);
            CheckSid(identifier.Value);
        }

        static void CheckSid(string sddl)
        {
            if (sddl == "S-1-5-11")
            {
                throw new DomainGroupEnumerationException(AppResources.DomainGroupAuthenticatedUsers);
            }
            else if (sddl.StartsWith("S-1-5-21-"))
            {
                if (sddl.EndsWith("-513"))
                {
                    // Domain users group
                    throw new DomainGroupEnumerationException(AppResources.DomainGroupDomainUsers);
                }
            }
        }

        /// <summary>Returns the groups SID in octet format.</summary>
        /// <remarks>Required for Windows 2000.</remarks>
        /// <returns>The SID in octet format.</returns>
        static string SidToOctetString(SPUser group)
        {
            string sddl = group.Sid;
            if (string.IsNullOrEmpty(sddl))
            {
                // using claims based authentication
                string loginName = group.LoginName;
                int index = loginName.IndexOf("|");
                if (index == -1)
                {
                    throw new DomainGroupEnumerationException(AppResources.DomainGroupInvalid);
                }
                else
                {
                    sddl = loginName.Substring(index + 1);
                }
            }

            CheckSid(sddl);

            try
            {
                SecurityIdentifier sid = new SecurityIdentifier(sddl);
                byte[] bytes = new byte[sid.BinaryLength];
                sid.GetBinaryForm(bytes, 0);
                StringBuilder builder = new StringBuilder();
                for (int i=0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
            catch (ArgumentException)
            {
                throw new DomainGroupEnumerationException(string.Format(SlkCulture.GetCulture(), AppResources.DomainGroupInvalidSid, sddl));
            }
        }

        void ProcessSearchResults(DirectorySearcher searcher, List<SPUserInfo> users)
        {
            searcher.ServerTimeLimit = RemainingTime;
            using (SearchResultCollection results = searcher.FindAll())
            {
                foreach (SearchResult result in results)
                {
                    string dn = ResultValue(result, "distinguishedName");

                    if (processedThisGroup.ContainsKey(dn) == false)
                    {
                        processedThisGroup.Add(dn, null);
                        if (result.Properties["objectClass"].Contains("group"))
                        {
                            try
                            {
                                CheckSid((byte[])result.Properties["objectSid"][0]);
                                using (DirectoryEntry childGroup = result.GetDirectoryEntry())
                                {
                                    users.AddRange(Expand(childGroup));
                                }
                            }
                            catch (DomainGroupEnumerationException e)
                            {
                                errors.Add(e.Message);
                            }
                        }
                        else if (result.Properties["objectClass"].Contains("user"))
                        {
                            ProcessUser(result, dn, users);
                        }
                    }
                }
            }
        }

        List<SPUserInfo> Expand(DirectoryEntry group)
        {
            List<SPUserInfo> users = new List<SPUserInfo>();

            try
            {
                using (DirectorySearcher searcher = new DirectorySearcher(group, "(objectClass=*)", searchAttributes, SearchScope.Base))
                {
                    searcher.AttributeScopeQuery = "member";
                    searcher.PageSize = 1000;
                    ProcessSearchResults(searcher, users);
                }
            }
            catch (COMException e)
            {
                errors.Add(string.Format(SlkCulture.GetCulture(), AppResources.GroupEnumerationFail, group.Name));
                Microsoft.SharePointLearningKit.WebControls.SlkError.WriteToEventLog(e);
            }

            return users;
        }

        string ResultValue(SearchResult result, string item)
        {
            ResultPropertyValueCollection property = result.Properties[item];

            if (property != null && property.Count > 0)
            {
                return (string)property[0];
            }
            else
            {
                return null;
            }
        }

        void ProcessUser(SearchResult result, string distinguishedName, List<SPUserInfo> users)
        {
            bool addUser = true;

            if (hideDisabledUsers)
            {
                ResultPropertyValueCollection accountControl = result.Properties["userAccountControl"]; 
                if (accountControl != null && accountControl[0] != null)
                {
                    // 2 is the flag for account disabled
                    if (((int)accountControl[0] & 2) == 2)
                    {
                        addUser = false;
                    }
                }
            }
            
            if (addUser)
            {
                users.Add(ProcessUser(result, distinguishedName));
            }
        }

        SPUserInfo ProcessUser(SearchResult result, string distinguishedName)
        {
            SPUserInfo info;

            if (processed.TryGetValue(distinguishedName, out info) == false)
            {
                info = new SPUserInfo();
                info.Name = ResultValue(result, "displayName");
                if (string.IsNullOrEmpty(info.Name))
                {
                    info.Name = ResultValue(result, "name");
                }

                info.Email = ResultValue(result, "mail");

                byte[] sid = (byte[])result.Properties["objectSid"][0];
                SecurityIdentifier identifier = new SecurityIdentifier(sid, 0);
                NTAccount account = (NTAccount)identifier.Translate(typeof(NTAccount));
                info.LoginName = account.ToString();
                processed.Add(distinguishedName, info);
            }

            return info;
        }

        string FindNamingContext(string distinguishedName)
        {
            int index = distinguishedName.IndexOf("DC=", StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                return distinguishedName.Substring(index).Trim();
            }
            else
            {
                return null;
            }
        }

        string FindDnsName(string namingContext)
        {

            string[] parts = namingContext.Split(',');
            bool firstPart = true;
            StringBuilder builder = new StringBuilder();
            foreach (string part in parts)
            {
                if (part.StartsWith("DC=", StringComparison.Ordinal))
                {
                    if (firstPart == false)
                    {
                        builder.Append(".");
                    }
                    else
                    {
                        firstPart = false;
                    }

                    builder.Append(part.Remove(0, 3));
                }
            }
            return builder.ToString();
        }

        string FindDomain(string dnsName, string namingContext)
        {
            string domain = null;

            if (domainNames.TryGetValue(dnsName, out domain) == false)
            {
                string ldapPath = string.Format(CultureInfo.InvariantCulture, "LDAP://{0}/CN=Partitions,CN=Configuration,{1}", dnsName, namingContext);
                using (DirectoryEntry parts = new DirectoryEntry(ldapPath))
                {
                    foreach (DirectoryEntry part in parts.Children)
                    {
                        string ncName = (string)part.Properties["nCName"][0];
                        if (string.Compare(ncName, namingContext, true, CultureInfo.InvariantCulture) == 0)
                        {
                            domain = (string)part.Properties["NetBIOSName"][0];
                            domainNames.Add(dnsName, domain);
                            break;
                        }
                    }
                }
            }

            return domain;
        }

#endregion private methods

#region DomainAndName
        class DomainAndName
        {
            public string Domain { get; private set; }
            public string LoginName { get; private set; }

            public DomainAndName(string domainAndName)
            {
                int backslash = domainAndName.IndexOf('\\');
                Domain = domainAndName.Substring(0, backslash);
                LoginName = domainAndName.Substring(backslash + 1);
            }
        }
    }
#endregion DomainAndName

#region DomainGroupEnumerationException
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
#endregion DomainGroupEnumerationException

}
