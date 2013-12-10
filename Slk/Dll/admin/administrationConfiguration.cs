using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Encapsulates the administration configuration.</summary>
    public class AdministrationConfiguration
    {
        Guid siteId;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AdministrationConfiguration"/>.</summary>
        /// <param name="siteId">The site collection the configuration is for.</param>
        public AdministrationConfiguration(Guid siteId)
        {
            this.siteId = siteId;
        }
#endregion constructors

#region properties
        /// <summary>Set to the name of the database server associated with
        ///     the specified SPSite.  If no database is currently associated with
        ///     the site collection, this parameter is set to the name of the database
        ///     server containing the SharePoint configuration database.
        ///     </summary>
        public string DatabaseServer { get; set; }
        /// <summary>Set to the name of the database associated with the
        ///     specified SPSite.  If no database is currently associated with
        ///     the site collection, this parameter is set to a default database name.
        ///     </summary>
        public string DatabaseName { get; set; }
        /// <summary>Set to <c>true</c> if the database specified by the values
        ///     returned in DatabaseServer and DatabaseName
        ///     currently exists, <c>false</c> if not.  This can be used as the default value for the
        ///     "Create a new database" checkbox in Configure.aspx.</summary>
        public bool CreateDatabase { get; set; }
        /// <summary>Set to the name of the SharePoint permission that
        ///     identifies instructors.  If no database is currently associated with the specified
        ///     SPSite, this is set to a default value such as "SLK Instructor".</summary>
        public string InstructorPermission { get; set; }
        /// <summary>Set to the name of the SharePoint permission that
        ///     identifies learners.  If no database is currently associated with the specified
        ///     SPSite, this is set to a default value such as "SLK Learner".</summary>
        public string LearnerPermission { get; set; }
        /// <summary>Set to the name of the SharePoint permission that
        ///     identifies observers.  If no database is currently associated with the specified
        ///     SPSite, this is set to a default value such as "SLK Observer".</summary>
        public string ObserverPermission { get; set; }
        /// <summary>Set to <c>false</c> if both the permission values returned
        ///     in parameters InstructorPermission and
        ///     LearnerPermission already exist in the root SPWeb of the
        ///     specified SPSite, <c>true</c> otherwise.  This can be used as the default value
        ///     for the "Create permissions" checkbox in Configure.aspx.</summary>
        public bool CreatePermissions 
        {
            get { return PermissionsExist() == false ;}
        }

        /// <summary>Indicates if the configuration is existing or new.</summary>
        public bool IsNewConfiguration { get; set; }

#endregion properties

#region public methods
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        /// <summary>
        /// Returns <c>true</c> if all SharePoint permissions specified by a given set of permission
        /// names exist in the root web site of a given SPSite, <c>false</c> if not.
        /// </summary>
        bool PermissionsExist()
        {
            bool returnValue = true;
            SlkUtilities.ImpersonateAppPool(delegate()
            {
                bool catchAccessDenied = SPSecurity.CatchAccessDeniedException;
                try
                {
                    SPSecurity.CatchAccessDeniedException = false;

                    // populate <existingPermissions> with the existing permissions on the root SPWeb of the
                    // site with GUID <spSiteGuid>
                    Dictionary<string, bool> existingPermissions = new Dictionary<string, bool>(20);
                    using (SPSite spSite = new SPSite(siteId))
                    {
                        using (SPWeb rootWeb = spSite.RootWeb)
                        {
                            foreach (SPRoleDefinition roleDef in rootWeb.RoleDefinitions)
                                existingPermissions[roleDef.Name] = true;
                        }
                    }

                    if (existingPermissions.ContainsKey(LearnerPermission) == false)
                    {
                        returnValue = false;
                    }
                    else if (existingPermissions.ContainsKey(InstructorPermission) == false)
                    {
                        returnValue = false;
                    }
                    else if (existingPermissions.ContainsKey(ObserverPermission) == false)
                    {
                        returnValue = false;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    SlkCulture culture = new SlkCulture();
                    throw new SafeToDisplayException(culture.Resources.NoAccessToSite);
                }
                finally
                {
                    SPSecurity.CatchAccessDeniedException = catchAccessDenied;
                }
            });

            return returnValue;
        }
#endregion private methods

#region static members
#endregion static members
    }
}

