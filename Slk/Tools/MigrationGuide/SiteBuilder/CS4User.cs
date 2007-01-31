/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.ComponentModel;
using MigrationHelper;




namespace SiteBuilder
{

    /// <summary>
    /// Represents list of Class Server users
    /// </summary>
    public class CS4UserCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Methods for supporting CollectionBase interface
        /// </summary>
        public void Add(CS4User user)
        {
            List.Add(user);
        }

        public void Remove(int index)
        {
            List.RemoveAt(index); 
        }

        public CS4User Item(int index)
        {
            return (CS4User) List[index];
        }

        public CS4User GetByUserId(int userId)
        {
            CS4User returnUser = null;
            foreach (CS4User user in List)
            {
                if (user.UserId == userId)
                {
                    returnUser = user;
                    break;
                }
            }
            return returnUser;
        }
    
        private int m_totalUsers;
        private int m_usersNotInAD;
        private int m_usersProcessed;

        /// <summary>
        /// number of users that cannot be found in Active Directory
        /// </summary>
        public int UsersNotInActiveDirectory
        {
            get
            {
                return m_usersNotInAD;
            }
        }

        /// <summary>
        /// Gets the list of users from Class Server database and checks 
        /// if users belong to Active Directory. This is necessary as
        /// SLK sites base their permissions and membership on AD authentication
        /// </summary>
        /// <param name="ActiveDirectoryPath">LDAP path of the Active Directory</param>
        /// <param name="LogFileName">file name of the log file (will be created in the working directory)</param>
        /// <param name="worker">BackgroundWorker object, used to report progress</param>
        /// <param name="e">not used</param>
        /// <returns>"Completed" if successful, or exception message if there was an error.</returns>
        public string VerifyAgainstActiveDirectory(string activeDirectoryPath, string logFileName, 
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            try
            {
                CS4Database database = new CS4Database(SiteBuilder.Default.ClassServerDBConnectionString);
                DataTable usersTable = null;
                m_totalUsers = database.GetUsersList(ref usersTable);
                LogFile log = new LogFile(logFileName);
                log.WriteToLogFile("Verifying Class Server users against Active Directory at " + activeDirectoryPath);
                ADHelper helper = new ADHelper();
                helper.DoADBinding(activeDirectoryPath);
                for (int userIndex = 0; userIndex < m_totalUsers; userIndex++)
                {
                    CS4User user = new CS4User(usersTable.Rows[userIndex][0].ToString(), usersTable.Rows[userIndex][1].ToString(), System.String.Empty, System.String.Empty);
                    if (!helper.UserExistsInActiveDirectory(user.UserDomain, user.UserLogin))
                    {
                        log.WriteToLogFile(user.UserLoginWithDomain  + " NOT FOUND");
                        m_usersNotInAD++;
                    }
                    else
                    {
                        log.WriteToLogFile(user.UserLoginWithDomain + " found");
                    }
                    m_usersProcessed++;
                    worker.ReportProgress(0, "Verified " + m_usersProcessed.ToString() + " of " + m_totalUsers.ToString() + " users.");

                }
                log.FinishLogging();
                string sReport = "Completed.";
                return sReport;
            }
            catch (System.Exception ex)
            {
                //catching any exception here as this will be executed in a separate thread
                return ex.Message;
            }
        }
    }

    /// <summary>
    /// Represents a User in the system, 
    /// used to manage the transfer of users from Class Server database to SLK sites
    /// </summary>
    public class CS4User
    {
        /// <summary>
        /// Struct used to hold user's permissions. Depending on user's role
        /// user will have different level of permissions to different sites,
        /// e.g. class library site, school site, class site
        /// </summary>
        public struct UserPermission
        {
            string m_WebUrl;
            string m_Role;

            public UserPermission(string WebUrl, string Role)
            {
                m_WebUrl = WebUrl;
                m_Role = Role;
            }

            public string WebUrl
            {
                get
                {
                    return m_WebUrl;
                }
            }

            public string Role
            {
                get
                {
                    return m_Role;
                }
            }
        }

        private bool m_IsTeacher;
        private string m_LoginName;
        private string m_DomainName;
        private string m_UserName;
        private string m_Email;
        private bool m_Transfer;
        private int m_UserId;
        private List<UserPermission> m_UserRoles = new List<UserPermission>();


        public CS4User(string loginDomainName, string loginName, string userName, string email)
        {
            //check if login name contains domain name
            if (loginName.Contains("\\"))
            {
                m_DomainName = loginName.Substring(0, loginName.IndexOf("\\"));
                m_LoginName = loginName.Substring(loginName.IndexOf("\\")+1);
            }
            else
            {
                m_LoginName = loginName;
                m_DomainName = loginDomainName;
            }
            m_UserName = userName;
            m_Email = email;
        }

        public CS4User(int userId, string loginDomainName, string loginName, string userName, string email, bool transferUser, bool isTeacher) : this(loginDomainName, loginName, userName, email)
        {
            m_UserId = userId;
            m_Transfer = transferUser;
            m_IsTeacher = isTeacher;
        }

        /// <summary>
        /// list of SLK Roles that will be assigned to the user
        /// </summary>
        public List<UserPermission> UserRoles
        {
            get
            {
                return m_UserRoles;
            }
            set
            {
                m_UserRoles = value;
            }
        }

        /// <summary>
        /// 
        /// This is autogenerated from domain and user login. If domain is empty then only user name is returned
        /// </summary>
        public string UserLoginWithDomain
        {
            get
            {
                if (m_DomainName.Length > 0)
                {
                    return m_DomainName + "\\" + m_LoginName;
                }
                else
                {
                    return m_LoginName;
                }
            }            
        }
        public bool Transfer
        {
            get
            {
                return m_Transfer;
            }
        }

        public string UserName
        {
            get
            {
                return m_UserName;
            }
        }

        public bool IsTeacher
        {
            get
            {
                return m_IsTeacher;
            }
        }


        public string UserDomain
        {
            get
            {
                return m_DomainName;
            }
        }


        public int UserId
        {
            get
            {
                return m_UserId;
            }
        }

        public string UserLogin
        {
            get
            {
                return m_LoginName;
            }
        }

        public string Email
        {
            get
            {
                return m_Email;
            }
        }

    }

}
