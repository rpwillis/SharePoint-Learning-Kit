using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLogic.SharedBusinessLogic.Security;
using Axelerate.BusinessLogic.SharedBusinessLogic.Contacts;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Support
{
    public class clsActiveDirectory
    {
        public static string CurrentUserName
        {
            get
            {
                string UserName;
                string debugName = clsConfigurationProfile.Current.getPropertyValue("Debug_Identity");
                if (debugName != null && debugName != "")
                    UserName = debugName;
                else
                {
                    //UserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
                    if (System.Web.HttpContext.Current != null)
                    {
                        //If we are in web need get the user that is accessing the page instead of the user running the thread.
                        UserName = System.Web.HttpContext.Current.User.Identity.Name;
                    }
                    else
                    {
                        UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    }
                }

                return UserName;
            }
        }

        public static clsADUser CurrentUser
        {
            get
            {
                string UserName = CurrentUserName;

                clsADUsers Users = clsADUsers.GetCollectionByUserName(UserName);
                if (Users.Count == 0 )
                {
                    clsADUser User = CreateUserFromAD(UserName);
                    return User;
                }
                else
                {
                    try
                    {
                        string name = Users[0].Contact.Name;
                    }
                    catch
                    {
                        clsADUser User = CreateUserFromAD(UserName);
                        return User;
                    }
                    return Users[0];
                }
            }
        }
        public static clsADUser CreateUserFromAD(string userName)
        {
            try
            {
                clsContact contactInfo = new clsContact();
                string domain;
                SearchResultCollection results = GetDirectoryObject(userName);
                if (results == null || results.Count == 0)
                {
                    clsLog.Trace(Resources.ErrorMessages.errGetDirectoryObject);
                    int dash = userName.IndexOf('\\');
                    contactInfo.Name = userName.Substring(dash+1);
                    contactInfo.Email = contactInfo.Name;
                    domain = userName.Substring(0,dash);
                }
                else
                {
                    contactInfo.Name = (string)results[0].Properties["name"][0];
                    contactInfo.Email = (string)results[0].Properties["sAMAccountName"][0];
                    domain = FindDomain((string)results[0].Properties["ADspath"][0]);
                }
                contactInfo.Email += "@" + clsConfigurationProfile.Current.getPropertyValue("Mail_Domain");
                // Create the contact info
                contactInfo.Save();

                // Create the ADUser piece
                clsADUser adUser = new clsADUser();
                adUser.ContactGUID = contactInfo.GUID;
                adUser.UserName = userName;
                adUser.Domain = domain;
                adUser.Save();
                return adUser;
            }
            catch (Exception ex)
            {
                //throw new clsBadIOException(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        public static void GetDirectoryObject(string userName, ref ArrayList listName, ref ArrayList listAlias, int maxNames)
        {
            listName = null;
            listAlias = null;

            userName = clsSecurityAssuranceHelper.AvoidLDAPInjectionInFilter(userName); 

            SearchResultCollection results = GetDirectoryObject(userName, maxNames);

            if (results != null)
            {
                listAlias = new ArrayList();
                listName = new ArrayList();
                for (int counter = 0; counter < results.Count; counter++)
                {
                    listAlias.Add(FindDomain((string)results[0].Properties["ADspath"][0]) + "\\" + (string)results[counter].Properties["sAMAccountName"][0]);
                    listName.Add((string)results[counter].Properties["name"][0]);
                }
            }
        }

        private static string FindDomain(string domain)
        {
            // Look for the first DC=
            int index = domain.IndexOf("DC=") + 3;
            int comma = domain.IndexOf(",", index);
            if (comma <= 0)
                comma = domain.Length;
            domain = domain.Substring(index, comma - index);
            return domain;
        }
        public static SearchResultCollection GetDirectoryObject(string userName)
        {
            return GetDirectoryObject(userName, 1);
        }
        public static SearchResultCollection GetDirectoryObject(string userName, int maxCount)
        {

            int index;
            string domain, filter;
            userName = clsSecurityAssuranceHelper.AvoidLDAPInjectionInFilter(userName);

            if (userName.Contains("\\"))
            {
                index = userName.IndexOf("\\");
                domain = userName.Substring(0, index);
                userName = userName.Substring(index + 1);
                filter = "(&(objectClass=person)(sAMAccountName=" + userName + "*))";
                StringCollection domains = EnumerateDomains();
                if (domains == null)
                {
                    return GetDirectoryEntries(filter, "LDAP://" + domain, maxCount);
                }
                else
                {
                    for (int i = 0; i < domains.Count; i++)
                    {
                        string src = domains[i].ToUpper();
                        string dom = domain.ToUpper();
                        if (src.Contains(dom))
                        {
                            return GetDirectoryEntries(filter, "LDAP://" + domains[i], maxCount);
                        }
                    }
                }
            }
            else if (userName.Contains(" ") || userName.Contains("*"))
            {
                //if (!userName.Contains("*"))
                //    userName += "*";
                filter = "(&(&(objectClass=user)(objectCategory=person))(Name=" + userName + "))";
                StringCollection domains = EnumerateDomains();
                if (domains == null)
                {
                    // do a default domain...
                    return GetDirectoryEntries(filter, "LDAP://REDMOND", maxCount);
                }
                else
                {
                    for (int i = 0; i < domains.Count; i++)
                    {
                        SearchResultCollection result = GetDirectoryEntries(filter, "LDAP://" + domains[i], maxCount);
                        if (result != null && result.Count != 0)
                            return result;
                    }
                }
            }
            else
            {
                filter = "(&(objectClass=user)(sAMAccountName=" + userName + "))";
                StringCollection domains = EnumerateDomains();
                if (domains == null)
                {
                    // do a default domain...
                    return GetDirectoryEntries(filter, "LDAP://REDMOND", maxCount);
                }
                else
                {
                    for (int i = 0; i < domains.Count; i++)
                    {
                        domain = "LDAP://" + domains[i];
                        SearchResultCollection result = GetDirectoryEntries(filter, domain, maxCount);
                        if (result != null && result.Count != 0)
                            return result;
                    }
                }
            }
            return null;
        }
        protected static SearchResultCollection GetDirectoryEntries(string filter, string domain, int maxCount)
        {
            SearchResultCollection results = null;
            DirectorySearcher deSearch = new DirectorySearcher();

            try
            {
                // Setup the search and time limit
                deSearch.Asynchronous = false;
                deSearch.ClientTimeout = new TimeSpan(0, 0, 10);     // Client will  wait no more than 10 seconds
                deSearch.ServerPageTimeLimit = new TimeSpan(0, 0, 5);
                deSearch.ServerTimeLimit = new TimeSpan(0, 0, 10);
                deSearch.SizeLimit = maxCount;
                deSearch.SearchScope = SearchScope.Subtree;
                deSearch.PropertiesToLoad.Add("name");
                deSearch.PropertiesToLoad.Add("sAMAccountName");
                deSearch.PropertiesToLoad.Add("ADspath");

                string ServiceName = (string)clsConfigurationProfile.Current.getPropertyValue("Default_UserName");
                string ServicePassword = (string)clsConfigurationProfile.Current.getPropertyValue("Default_Password");

                DirectoryEntry searchRoot = new DirectoryEntry(domain);
                deSearch.SearchRoot = searchRoot;
                deSearch.Filter = filter;
                results = deSearch.FindAll();

            }
            // All errors fall here
            catch (Exception e)
            {
                results = null;
                clsLog.Trace(Resources.ErrorMessages.excGetDirectoryObject + e.Message);
            }
            return results;
        }
        public static StringCollection EnumerateDomains()
        {
            try
            {
                StringCollection alDomains = new StringCollection();
                Forest currentForest = Forest.GetCurrentForest();
                DomainCollection myDomains = currentForest.Domains;

                foreach (Domain objDomain in myDomains)
                {
                    alDomains.Add(objDomain.Name);
                }
                return alDomains;
            }
            catch (Exception ex)
            {
                clsLog.Trace(Resources.ErrorMessages.clsDirectoryEnumerateDomains + ex.Message);
            }
            return null;
        }
        public string[] SplitFullName(string FullName)
        {
            string[] VectorNames;
            VectorNames = FullName.Split(new string[] { "\\" }, StringSplitOptions.None);
            return VectorNames;
        }
    }
}
