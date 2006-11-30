/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
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

    [DllImport("Netapi32.dll")]
    extern static int NetLocalGroupGetMembers([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string localgroupname, int level, out IntPtr bufptr, int prefmaxlen, out int entriesread, out int totalentries, out int resumehandle);

    [DllImport("Netapi32.dll")]
    extern static int NetApiBufferFree(IntPtr Buffer);

    [DllImport("Netapi32.dll")]
    extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

    /// <summary>
    /// Retrieves information about a user from a <c>LOCALGROUP_MEMBERS_INFO_3</c> and stores it in a
    /// <c>SPUserInfo</c>.
    /// </summary>
    ///
    /// <param name="lgmi3">The <c>LOCALGROUP_MEMBERS_INFO_3</c> to retrieve from.</param>
    /// 
    /// <param name="spUserInfo">Where the user's information is stored</param>
    ///
    /// <returns>
    /// True if the user information is found, false otherwise.
    /// </returns>
    ///
    static private bool GetSPUserInfoFromLocalGroupMembersInfo(LOCALGROUP_MEMBERS_INFO_3 lgmi3, out SPUserInfo spUserInfo)
    {
        IntPtr bufPtr;
        USER_INFO_2 user = new USER_INFO_2();
        int backslash = lgmi3.lgrmi3_domainandname.IndexOf('\\');
        string domainName = lgmi3.lgrmi3_domainandname.Substring(0, backslash);
        string loginName = lgmi3.lgrmi3_domainandname.Substring(backslash + 1);
        if(NetUserGetInfo(domainName, loginName, 2, out bufPtr) != 0)
        {
            try
            {
                using(DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainName))
                {
                    using(DirectorySearcher mySearcher = new DirectorySearcher(entry))
                    {
                        //mySearcher.ClientTimeout = timeout;
                        mySearcher.Filter = "(cn=" + loginName + ")";
                        using(SearchResultCollection resultCollection = mySearcher.FindAll())
                        {
                            foreach(SearchResult result in resultCollection)
                            {
                                foreach(object member in result.Properties["member"])
                                {
                                    // check for timeout here because the DirectoryEntry constructor
                                    // may take some time and has no timeout
                                    //TimeSpan remainingTime = timeoutTime - DateTime.Now;
                                    //if(remainingTime <= TimeSpan.Zero)
                                    //{
                                    //    throw new DomainGroupEnumerationException(
                                    //        AppResources.DomainGroupEnumTimeout);
                                    //}
                                    try
                                    {
                                        using(DirectoryEntry de = new DirectoryEntry("LDAP://" + (string)member))
                                        {
                                            spUserInfo = GetSPUserInfoFromDirectoryEntry(de);
                                            spUserInfo.Name = loginName;
                                            return true;
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
            }
            catch(COMException ex)
            {
                throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.DomainGroupEnumFailed, "LDAP://" + domainName), ex);
            }
            // construct an <spUserInfo> to contain information about this user
            spUserInfo = new SPUserInfo();
            return false;
        }
        user = (USER_INFO_2)Marshal.PtrToStructure(bufPtr, typeof(USER_INFO_2));

        // construct an <spUserInfo> to contain information about this user
        spUserInfo = new SPUserInfo();
        // set <spUserInfo.LoginName> to the user's domain name, of the form "domain\username"
        spUserInfo.LoginName = lgmi3.lgrmi3_domainandname;
        spUserInfo.Name = user.usri2_full_name;

        NetApiBufferFree(bufPtr);
        return true;
    }

    /// <summary>
    /// Tests if the passed group ID is a local group and if so, enumerates the members. 
    /// </summary>
    /// <param name="domainName">Domain name where the local group is stored (e.g. MYMACHINE).</param>
    /// <param name="groupName">Name of the group to check (e.g. Administrators).</param>
    /// <param name="users">If the domainName and groupName indeed refer to a viable local group, this collection will contain
    /// the resulting members of that group.</param>
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
    /// 	<c>DomainGroupEnumerationException</c> will be thrown.</param>
    ///
    /// <returns>True if this is a local group, false otherwise.</returns>
    private static bool ReadLocalGroup(string domainName, string groupName, DateTime timeoutTime, Dictionary<string, SPUserInfo> users)
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
                    throw new DomainGroupEnumerationException(
                        AppResources.DomainGroupEnumTimeout);
                }
                members[i] = (LOCALGROUP_MEMBERS_INFO_3)Marshal.PtrToStructure(iter, typeof(LOCALGROUP_MEMBERS_INFO_3));
                iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(LOCALGROUP_MEMBERS_INFO_3)));

                SPUserInfo spUserInfo;
                if(GetSPUserInfoFromLocalGroupMembersInfo(members[i], out spUserInfo))
                {
                    users[spUserInfo.LoginName] = spUserInfo;
                }
            }
            NetApiBufferFree(bufPtr);

            return true;
        }
        return false;
    }

    /// <summary>
    /// Enumerates the members of a domain group, given a <c>DirectoryEntry</c> representing that
	/// group.
    /// </summary>
	///
    /// <param name="entry">The <c>DirectoryEntry</c> to enumerate users of.</param>
	///
    /// <param name="timeoutTime">If this method runs past this time (approximately), a
	/// 	<c>DomainGroupEnumerationException</c> will be thrown.</param>
	///
    /// <param name="users">Information about each enumerated user is added to this list.</param>
	///
	/// <exception cref="DomainGroupEnumerationException">
	/// Enumeration of the domain group failed.
	/// </exception>
	///
	private static void AddGroupChildren(DirectoryEntry entry, DateTime timeoutTime,
        Dictionary<string, SPUserInfo> users)
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
                                throw new DomainGroupEnumerationException(
                                    AppResources.DomainGroupEnumTimeout);
                            }
							try
							{
								using(DirectoryEntry de = new DirectoryEntry("LDAP://" + (string)member))
								{
									if(de.SchemaClassName == "group")
									{
										AddGroupChildren(de, timeoutTime, users);
									}
									else if(de.SchemaClassName == "user")
									{
                                        SPUserInfo spUserInfo = GetSPUserInfoFromDirectoryEntry(de);
                                        users[spUserInfo.LoginName] = spUserInfo;
									}
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
    /// Retrieves information about a user from a <c>DirectoryEntry</c> and stores it in a
    /// <c>SPUserInfo</c>.
    /// </summary>
    ///
    /// <param name="de">The <c>DirectoryEntry</c> to retrieve from.</param>
    ///
    /// <returns>
    /// An <c>SPUserInfo</c> containing information about the specified user.  Note that we're
    /// just using <c>SPUserInfo</c> because it's a convenient structure for typical callers of
    /// this method -- this method does not interact with SharePoint in any way.
    /// </returns>
    ///
	static private SPUserInfo GetSPUserInfoFromDirectoryEntry(DirectoryEntry de)
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
		string[] parts = de.Path.Split(',');
		bool addDot = false;
		foreach(string part in parts)
		{
			if(part.StartsWith("DC=", StringComparison.Ordinal))
			{
				if(addDot)
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

    /// <summary>
    /// Enumerates the members of a domain group.
    /// </summary>
    /// 
    /// <param name="groupName">The name of the group to enumerate; e.g. "mydomain\mygroup".</param>
    ///
    /// <param name="timeout">An exception is thrown if enumeration takes longer than approximately
    ///     this amount of time.</param>
    ///
    /// <returns>
    /// A collection of <c>SPUserInfo</c> objects referring to the members of the group.  Note that
    /// these users are not necessarily members of any SharePoint site collection --
    /// <c>SpUserInfo</c> is used simply as a convenient class to contain information about users.
    /// </returns>
    ///
	static public ICollection<SPUserInfo> EnumerateDomainGroup(string groupName, TimeSpan timeout)
	{
        // track the time that a timeout will occur
		DateTime timeoutTime = DateTime.Now + timeout;

        // set <domainName> to the "domain" part of "domain\username"
		int backslashIndex = groupName.IndexOf('\\');
		if (backslashIndex <= 0)
		{
            throw new DomainGroupEnumerationException(
                String.Format(CultureInfo.CurrentCulture, AppResources.DomainGroupNameHasNoBackslash, groupName));
		}
		string domainName = groupName.Substring(0, backslashIndex);
        string loginName = groupName.Substring(backslashIndex + 1);

        // initialize <users> to be a dictionary which will hold an SPUserInfo of each user in
        // the enumerated group; the key is the login name of the user, e.g. "domain\username"
        Dictionary<string, SPUserInfo> users = new Dictionary<string, SPUserInfo>();

        if(ReadLocalGroup(domainName, loginName, timeoutTime, users))
        {
            return users.Values;
        }

        // set <path> to the Directory Services path that we'll enumerate
        string path = "LDAP://" + domainName;

        // enumerate users in <path>
		try
		{
			using(DirectoryEntry entry = new DirectoryEntry(path))
			{
				using(DirectorySearcher mySearcher = new DirectorySearcher(entry))
				{
					mySearcher.ClientTimeout = timeout;
					mySearcher.Filter = "(&(objectClass=group)(sAMAccountName=" +
                        loginName + "))";
					using(SearchResultCollection resultCollection = mySearcher.FindAll())
					{
						foreach(SearchResult result in resultCollection)
						{
							foreach(object member in result.Properties["member"])
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
									using(DirectoryEntry de = new DirectoryEntry("LDAP://" + (string)member))
									{
										if(de.SchemaClassName == "group")
										{
											AddGroupChildren(de, timeoutTime, users);
										}
										else if(de.SchemaClassName == "user")
										{
											SPUserInfo spUserInfo = GetSPUserInfoFromDirectoryEntry(de);
											users[spUserInfo.LoginName] = spUserInfo;
										}
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
		}
		catch(COMException ex)
		{
            throw new DomainGroupEnumerationException(String.Format(CultureInfo.CurrentCulture,
                AppResources.DomainGroupEnumFailed, path), ex);
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
