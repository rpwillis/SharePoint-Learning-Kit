/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// PageHelper.cs
//
// Contains classes that help implement this MLC web-based application.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Security.Principal;
using System.Web.Configuration;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = BasicWebPlayer.Schema;

// <summary>
// Helps implement this MLC web-based application.  ASP.NET web pages can be
// based on this class.
// </summary>
//
public class PageHelper : System.Web.UI.Page 
{
	///////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>UserKey</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_userKey;

    /// <summary>
    /// Holds the value of the <c>UserName</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_userName;
    
	/// <summary>
	/// Holds the value of the <c>LStoreConnectionString</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_lstoreConnectionString;

	/// <summary>
	/// Holds the value of the <c>LStore</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	LearningStore m_lstore;

	/// <summary>
	/// Holds the value of the <c>PStoreDirectoryPath</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_pstoreDirectoryPath;

	/// <summary>
	/// Holds the value of the <c>PStore</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    FileSystemPackageStore m_pstore;

	///////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the string provided by the operating environment which uniquely
	/// identifies this user.  Since this application uses Windows
	/// authentication, we'll use the user's security ID (e.g.
	/// "S-1-5-21-2127521184-...") as the user key.
	/// </summary>
	///
	public virtual string UserKey
	{
		get
		{
		    if (m_userKey == null)
		    {
		        using(WindowsIdentity userIdentity = WindowsIdentity.GetCurrent())
		        {
		            m_userKey = userIdentity.User.ToString();
		        }
		    }

			return m_userKey;
		}
	}


    /// <summary>
    /// Gets the name of the current user.
    /// </summary>
    ///
    public virtual string UserName
    {
        get
        {
            if (m_userName == null)
            {
                using (WindowsIdentity userIdentity = WindowsIdentity.GetCurrent())
                {
                    m_userName = userIdentity.Name;
                }
            }

            return m_userName;
        }
    }

    /// <summary>
	/// Gets the SQL Server connection string that LearningStore will use to
	/// access this application's database.  The string is stored in
	/// "appSettings" section of Web.config.
	/// </summary>
	///
	public string LStoreConnectionString
	{
		get
		{
			if (m_lstoreConnectionString == null)
			{
				m_lstoreConnectionString = WebConfigurationManager.AppSettings
					["learningComponentsConnnectionString"];
			}
			return m_lstoreConnectionString;
		}
	}

	/// <summary>
	/// Gets a reference to this application's LearningStore database.
	/// </summary>
	///
	public LearningStore LStore
	{
		get
		{
			if (m_lstore == null)
			{
				m_lstore = new LearningStore(
                    LStoreConnectionString, UserKey, ImpersonationBehavior.UseOriginalIdentity);
			}
			return m_lstore;
		}
	}

	/// <summary>
	/// The full path to the directory which contains the unzipped package
	/// files stored in PackageStore.
	/// </summary>
	///
	public string PStoreDirectoryPath
	{
		get
		{
			if (m_pstoreDirectoryPath == null)
			{
				// set <m_pstoreDirectoryPath> to the full path to the
				// directory
				m_pstoreDirectoryPath = WebConfigurationManager.AppSettings
					["packageStoreDirectoryPath"];
			}
			return m_pstoreDirectoryPath;
		}
	}

	/// <summary>
	/// Gets a reference to this application's PackageStore, which consists of
	/// the "PackageFiles" subdirectory (within this application's directory)
	/// containing unzipped package files, plus information about these
	/// packages stored in the LearningStore database.  
	/// </summary>
	///
	public FileSystemPackageStore PStore
	{
		get
		{
			if (m_pstore == null)
			{
				m_pstore = new FileSystemPackageStore( LStore, 
					PStoreDirectoryPath, ImpersonationBehavior.UseOriginalIdentity);
			}
			return m_pstore;
		}
	}

	///////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

    /// <summary>
    /// Requests that information about the current user be retrieved from the
	/// LearningStore database.  Adds the request to a given
    /// <c>LearningStoreJob</c> for later execution.
    /// </summary>
    /// 
    /// <param name="job">A <c>LearningStoreJob</c> to add the new query to.
    ///     </param>
    /// 
    /// <remarks>
    /// After executing this method, and later calling <c>Job.Execute</c>,
    /// call <c>GetCurrentUserInfoResults</c> to convert the <c>DataTable</c>
    /// returned by <c>Job.Execute</c> into an <c>LStoreUserInfo</c> object.
    /// </remarks>
    ///
    protected void RequestCurrentUserInfo(LearningStoreJob job)
	{
        // look up the user in the UserItem table in the database using their
        // user key, and set <userId> to the LearningStore numeric identifier
        // of the user (i.e. UserItem.Id -- the "Id" column of the UserItem
        // table) and <userName> to their full name (e.g. "Karen Berg"); if
        // there's no UserItem for the user, add one and set <userId> to its
        // ID
        LearningStoreQuery query = LStore.CreateQuery(
            Schema.Me.ViewName);
        query.AddColumn(Schema.Me.UserId);
        query.AddColumn(Schema.Me.UserName);
        job.PerformQuery(query);
	}

    /// <summary>
    /// Reads a <c>DataTable</c>, returned by <c>Job.Execute</c>, containing
    /// the results requested by a previous call to
	/// <c>RequestCurrentUserInfo</c>.  Returns an <c>LStoreUserInfo</c>
	/// object containing information about the user.  If the user isn't
	/// already listed in LearningStore, a separate call to the database is
	/// made to add them.
    /// </summary>
    ///
    /// <param name="dataTable">A <c>DataTable</c> returned from
    ///     <c>Job.Execute</c>.</param>
    ///
	protected LStoreUserInfo GetCurrentUserInfoResults(DataTable dataTable)
    {
        DataRowCollection results = dataTable.Rows;
        LearningStoreJob job = LStore.CreateJob();
        UserItemIdentifier userId;
        string userName;
        if (results.Count == 0)
        {
            // the user isn't listed in the UserItem table -- add them...

            // set <userName> to the name of the user that SCORM will use
#if false
            // the following code queries Active Directory for the full name
            // of the user (for example, "Karen Berg") -- this code assumes a
            // particular Active Directory configuration which may or may not
            // work in your situation
            string adsiPath = String.Format("WinNT://{0},user",
                UserIdentity.Name.Replace(@"\", "/"));
            using (DirectoryEntry de = new DirectoryEntry(adsiPath))
                userName = (string)de.Properties["FullName"].Value;
#else
			// the following code uses the "name" portion of the user's
            // "domain\name" network account name as the name of the user
            userName = UserName;
            int backslash = userName.IndexOf('\\');
            if (backslash >= 0)
                userName = userName.Substring(backslash + 1);
#endif

            // create the UserItem for this user in LearningStore; we use
            // AddOrUpdateItem() instead of AddItem() in case this learner
            // was added by another application between the check above and
            // the code below
            job = LStore.CreateJob();
            Dictionary<string, object> uniqueValues =
                new Dictionary<string, object>();
            uniqueValues[Schema.UserItem.Key] = UserKey;
            Dictionary<string, object> addValues =
                new Dictionary<string, object>();
            addValues[Schema.UserItem.Name] = userName;
            job.AddOrUpdateItem(Schema.UserItem.ItemTypeName,
                uniqueValues, addValues, null, true);
            userId = new UserItemIdentifier(job.Execute<LearningStoreItemIdentifier>());
        }
        else
        {
            userId = new UserItemIdentifier((LearningStoreItemIdentifier)
                results[0][Schema.Me.UserId]);
            userName = (string)results[0][Schema.Me.UserName];
        }

        // return a LStoreUserInfo object
        return new LStoreUserInfo(userId, userName);
    }

	/// <summary>
	/// Retrieves information about the current user from the LearningStore
	/// database.
	/// </summary>
	///
	public LStoreUserInfo GetCurrentUserInfo()
	{
        LearningStoreJob job = LStore.CreateJob();
		RequestCurrentUserInfo(job);
		return GetCurrentUserInfoResults(job.Execute<DataTable>());
	}

    /// <summary>
    /// A delegate with no parameters and no return value.
    /// </summary>
    ///
    public delegate void VoidDelegate();

    /// <summary>
    /// Executes a supplied delegate while impersonating the application pool
    /// account.
    /// </summary>
    ///
    /// <param name="del">The delegate to execute.</param>
    ///
    public void ImpersonateAppPool(VoidDelegate del)
    {
        try
        {
            WindowsImpersonationContext context = null;
            try
            {
                context = WindowsIdentity.Impersonate(IntPtr.Zero);
                del();
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
        }
        catch
        {
            // prevent exception filter exploits
            throw;
        }
    }

    /// <summary>
    /// Formats a message using <c>String.Format</c> and writes to the event
    /// log.
    /// </summary>
    ///
    /// <param name="type">The type of the event log entry.</param>
    ///
    /// <param name="format">A string containing zero or more format items;
    ///     for example, "An exception occurred: {0}".</param>
    /// 
    /// <param name="args">Formatting arguments.</param>
    ///
    public void LogEvent(EventLogEntryType type, string format,
        params object[] args)
    {
        ImpersonateAppPool(delegate()
        {
            EventLog.WriteEntry("BasicWebPlayer", String.Format(format, args),
                type);
        });
    }
}

// <summary>
// Holds LearningStore information about the current user.  Use
// <c>GetCurrentUserInfo</c> to retrieve this information.
// </summary>
//
public class LStoreUserInfo
{
	///////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>Id</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	UserItemIdentifier m_id;

	/// <summary>
	/// Holds the value of the <c>Name</c> property.
	/// </summary>
	///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_name;

	///////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the UserItem.Id of this user, i.e. LearningStore's numeric
	/// identifier for this user.
	/// </summary>
	///
	public UserItemIdentifier Id
	{
		get
		{
			return m_id;
		}
	}

	/// <summary>
	/// Gets the full name of the user; for example, "Karen Berg".
	/// </summary>
	///
	public string Name
	{
		get
		{
			return m_name;
		}
	}

	///////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

	// <summary>
	// Initializes an instance of this class.
	// </summary>
	//
	// <param name="id">The value to use for the <c>Id</c> property.</param>
	//
	// <param name="name">The value to use for the <c>Name</c> property.</param>
	//
	public LStoreUserInfo(UserItemIdentifier id, string name)
	{
		m_id = id;
		m_name = name;
	}
}

