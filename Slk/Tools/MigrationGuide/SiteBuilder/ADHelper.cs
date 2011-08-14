/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

namespace SiteBuilder
{
    /// <summary>
    /// Implements interaction with Active Directory
    /// </summary>
    public class ADHelper : IDisposable 
    {
        private System.DirectoryServices.DirectoryEntry m_currentDirectory;

        /// <summary>
        /// Connects to AD and retrieves a parameter of created DirectoryEntry object as
        /// a verification of object's validity
        /// </summary>
        /// <param name="ActiveDirectoryPath">The LDAP path for the ActiveDirectory</param>
        public void DoADBinding(string activeDirectoryPath)
        {
            m_currentDirectory = new DirectoryEntry(activeDirectoryPath);
            string dirName = m_currentDirectory.Name;            
        }

        /// <summary>
        /// Releases DirectoryEntry object
        /// </summary>
        public void Dispose()
        {
            m_currentDirectory.Close();
        }

        /// <summary>
        /// Using DirectorySearcher object locates a user in Active Directory
        /// </summary>
        /// <param name="Domain">Domain name</param>
        /// <param name="Login">User login name</param>
        /// <returns>True if the user is found, false otherwise.</returns>
        public bool UserExistsInActiveDirectory(string domain, string login)
        {
            DirectorySearcher userSearcher = new DirectorySearcher(m_currentDirectory);
            // search for user by login name
            userSearcher.Filter = ("(&(samaccountname=" + login + ")(objectCategory=person))");
            SearchResult userSearchResult = userSearcher.FindOne();
            userSearcher.Dispose();
            if (userSearchResult == null)
            {
                return false;
            }
            else
            {
                if (!userSearchResult.Path.ToLower().Contains("dc=" + domain.ToLower()))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }            

        }
    }
}
