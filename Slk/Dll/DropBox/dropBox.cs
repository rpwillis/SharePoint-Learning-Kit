using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>An object responsible for returning and creating the drop box.</summary>
    public class DropBox
    {
        internal const string ColumnAssignmentKey = "Assignment";
        internal const string ColumnAssignmentDate = "AssignmentDate";
        internal const string ColumnAssignmentName = "AssignmentName";
        internal const string ColumnAssignmentId = "AssignmentId";
        internal const string ColumnIsLatest = "IsLatest";
        internal const string ColumnLearnerId = "LearnerId";
        internal const string ColumnLearner = "Learner";
        internal const string PropertyKey = "SLKDropBox";
        internal const string NoPermissionsFolderName = "NoPermissions";
        SPWeb web;
        SlkCulture culture;
        SPList dropBoxList;
        ISlkStore store;

#region constructors
        /// <summary>Initializes a new instance of <see cref="DropBox"/>.</summary>
        /// <param name="store">The ISlkStore to use.</param>
        /// <param name="web">The web to create the drop box in.</param>
        public DropBox(ISlkStore store, SPWeb web)
        {
            this.web = web;
            this.store = store;
            culture = new SlkCulture(web);
        }
#endregion constructors

#region properties
        /// <summary>The list which is the drop box.</summary>
        public SPList DropBoxList
        {
            get
            {
                if (dropBoxList == null)
                {
                    object property = web.AllProperties[PropertyKey];
                    if (property != null)
                    {
                        try
                        {
                            Guid id = new Guid(property.ToString());
                            dropBoxList = web.Lists.GetList(id, true);
                        }
                        catch (FormatException) {}
                        catch (OverflowException) {}
                        catch (SPException) {}
                    }

                    if (dropBoxList == null)
                    {
                        Guid listId;
                        using (DropBoxCreator creator = new DropBoxCreator(store, web))
                        {
                            listId = creator.Create();
                        }

                        dropBoxList = web.Lists.GetList(listId, true);
                    }
                }

                return dropBoxList;
            }
        }

#endregion properties

#region public methods
        /// <summary>The last submitted files of a user.</summary>
        /// <param name="user">The user to get the files for.</param>
        /// <param name="assignmentKey">The key of the assignment.</param>
        /// <param name="forceUnlock">Force unlocking of all the files.</param>
        /// <param name="currentUser">The current user id.</param>
        /// <returns>The last submitted files.</returns>
        public AssignmentFile[] LastSubmittedFiles(SlkUser user, long assignmentKey, bool forceUnlock, int currentUser)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            string queryXml = @"<Where>
                                <And>
                                    <Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq>
                                    <Eq><FieldRef Name='{2}'/><Value Type='Text'>{3}</Value></Eq>
                                </And>
                             </Where>";
            queryXml = string.Format(CultureInfo.InvariantCulture, queryXml, ColumnAssignmentId, assignmentKey, ColumnLearnerId, user.Key);
            SPQuery query = new SPQuery();
            query.ViewAttributes = "Scope=\"Recursive\"";
            query.Query = queryXml;
            SPListItemCollection items = DropBoxList.GetItems(query);

            List<AssignmentFile> files = new List<AssignmentFile>();


            foreach (SPListItem item in items)
            {
                if (item[ColumnIsLatest] != null)
                {
                    if ((bool)item[ColumnIsLatest])
                    {
                        SPFile file = item.File;
                        if (forceUnlock)
                        {
                            DropBoxManager.UnlockFile(file, currentUser);
                        }

                        files.Add(new AssignmentFile(item.ID, file.Name, file.ServerRelativeUrl, (string)file.Item["PermMask"]));
                    }
                }
            }
            return files.ToArray();
        }

        /// <summary>Returns all files for an assignment grouped by learner.</summary>
        /// <param name="assignmentKey">The key of the assignment.</param>
        public Dictionary<string, List<SPFile>> AllFiles(long assignmentKey)
        {
            string queryXml = @"<Where>
                                <And>
                                    <Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq>
                                    <Eq><FieldRef Name='{2}'/><Value Type='Boolean'>1</Value></Eq>
                                </And>
                             </Where>";
            queryXml = string.Format(CultureInfo.InvariantCulture, queryXml, ColumnAssignmentId, assignmentKey, ColumnIsLatest);
            SPQuery query = new SPQuery();
            query.ViewAttributes = "Scope=\"Recursive\"";
            query.Query = queryXml;
            SPListItemCollection items = DropBoxList.GetItems(query);

            SPFieldUser learnerField = (SPFieldUser)DropBoxList.Fields[ColumnLearner];

            Dictionary<string, List<SPFile>> files = new Dictionary<string, List<SPFile>>();

            foreach (SPListItem item in items)
            {
                SPFile file = item.File;
                SPFieldUserValue learnerValue = (SPFieldUserValue) learnerField.GetFieldValue(item[ColumnLearner].ToString());
                SPUser learner = learnerValue.User;

                List<SPFile> learnerFiles;
                string learnerAccount = learner.LoginName.Replace("\\", "-");
                if (files.TryGetValue(learnerAccount, out learnerFiles) == false)
                {
                    learnerFiles = new List<SPFile>();
                    files.Add(learnerAccount, learnerFiles);
                }

                learnerFiles.Add(item.File);
            }

            return files;
        }

        /// <summary>Creates the assignment folder.</summary>
        /// <param name="properties">The assignment properties.</param>
        /// <returns>The assignment folder.</returns>
        public AssignmentFolder CreateAssignmentFolder(AssignmentProperties properties)
        {
            string url = DropBoxList.RootFolder.ServerRelativeUrl;

            SPFolder noPermissionsFolder = GetNoPermissionsFolder().Folder;

            using (new AllowUnsafeUpdates(web))
            {
                string name = GenerateFolderName(properties);
                SPFolder folder = noPermissionsFolder.SubFolders.Add(name);
                folder.MoveTo(url + "\\" + name);
                folder.Update();
                SPListItem assignmentFolder = folder.Item;
                DropBoxCreator.ClearPermissions(assignmentFolder);
                DropBoxList.Update();
                CreateAssignmentView(properties);
                return new AssignmentFolder(assignmentFolder, false, properties);
            }
        }

        /// <summary>Gets the assignment folder and creates it if it doesn't exist.</summary>
        /// <param name="properties">The assignment properties.</param>
        /// <returns>The assignment folder.</returns>
        public AssignmentFolder GetOrCreateAssignmentFolder(AssignmentProperties properties)
        {
            AssignmentFolder folder = GetAssignmentFolder(properties);

            if (folder == null)
            {
                return CreateAssignmentFolder(properties);
            }
            else
            {
                return folder;
            }
        }

        /// <summary>Function searches for the assignment folder.</summary>
        /// <returns>The folder object</returns>
        public AssignmentFolder GetAssignmentFolder(AssignmentProperties properties)
        {
            return GetAssignmentFolder(GenerateFolderName(properties), properties);
        }

        /// <summary>Changes the assignment folder name.</summary>
        /// <param name="oldAssignmentFolderName">The old assignment name.</param>
        /// <param name="newAssignmentFolderName">The new assignment name.</param>
        public void ChangeFolderName(string oldAssignmentFolderName, string newAssignmentFolderName)
        {
            //Get old assignment folder
            AssignmentFolder oldAssignmentFolder = GetAssignmentFolder(oldAssignmentFolderName, null);
            if (oldAssignmentFolder != null)
            {
                oldAssignmentFolder.ChangeName(oldAssignmentFolderName, newAssignmentFolderName);
            }
            else
            {
                throw new SafeToDisplayException(culture.Resources.AssFolderNotFound);
            }
        }

#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        AssignmentFolder GetAssignmentFolder(string folderName, AssignmentProperties properties)
        {
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + folderName + "</Value></Eq></Where>";
            SPListItemCollection items = DropBoxList.GetItems(query);
            if (items.Count != 0)
            {
                return new AssignmentFolder(items[0], false ,properties);
            }
            else
            {
                return null;
            }
        }

        SPListItem GetNoPermissionsFolder()
        {
            try
            {
                SPFolder folder = DropBoxList.RootFolder.SubFolders[NoPermissionsFolderName];
                return folder.Item;
            }
            catch (ArgumentException)
            {
                return DropBoxCreator.CreateNoPermissionsFolder(DropBoxList);
            }
        }

        void CreateAssignmentView(AssignmentProperties properties)
        {
            StringCollection fields = new StringCollection();
            fields.Add(DropBoxList.Fields[SPBuiltInFieldId.DocIcon].InternalName);
            fields.Add(DropBoxList.Fields[SPBuiltInFieldId.LinkFilename].InternalName);
            fields.Add(DropBox.ColumnLearner);
            fields.Add(DropBoxList.Fields[SPBuiltInFieldId.Modified].InternalName);
            fields.Add(DropBox.ColumnIsLatest);

            string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /></GroupBy><Where><Eq><FieldRef Name=\"{1}\" /><Value Type=\"Text\">{2}</Value></Eq></Where>";
            query = string.Format(CultureInfo.InvariantCulture, query, DropBox.ColumnLearner, DropBox.ColumnAssignmentId, properties.Id.GetKey());
            SPView view = DropBoxList.Views.Add(AssignmentViewName(properties), fields, query, 100, true, false, SPViewCollection.SPViewType.Html, false);
            view.Update();
            DropBoxList.Update();

            // Seem to need to re-get the view to set scope and group header.
            SPView view2 = DropBoxList.Views[view.ID];
            view2.Scope = SPViewScope.Recursive;
            DropBoxCreator.RemoveFieldNameFromGroupHeader(view2);
            view2.Update();
            DropBoxList.Update();
        }


#endregion private methods

#region static members
        static readonly System.Text.RegularExpressions.Regex nameRegex = new System.Text.RegularExpressions.Regex(@"[\*#%\&:<>\?/{|}\\@]");
        static readonly System.Text.RegularExpressions.Regex repeatedDotRegex = new System.Text.RegularExpressions.Regex(@"\.\.");

        /// <summary>Clears the permissions on an object.</summary>
        /// <param name="securable">The folder to clear the permissions for.</param>
        /// <param name="web">The web containing the object.</param>
#if SP2007
        public static void RemoveRoleAssignments(ISecurableObject securable, SPWeb web)
#else
        public static void RemoveRoleAssignments(SPSecurableObject securable, SPWeb web)
#endif
        {
            // There's a bug with BreakRoleInheritance which means that passing false resets AllowUnsafeUpdates to
            // false so the call fails. http://www.ofonesandzeros.com/2008/11/18/the-security-validation-for-this-page-is-invalid/
            securable.BreakRoleInheritance(true);

            // No need to cache the original value as already set to true before calling here and
            // BreakRoleInheritance has reset to false
            web.AllowUnsafeUpdates = true;

            SPRoleAssignmentCollection roleAssigns = securable.RoleAssignments;

            while (roleAssigns.Count > 0)
            {
                roleAssigns.Remove(0);
            }

            for (int i = roleAssigns.Count-1; i >= 0; i--)
            {
                roleAssigns.Remove(i);
            }
        }

        /// <summary>
        /// Truncate the time part and the '/' from the Created Date and convert it to string to be a valid folder name.
        /// </summary>
        /// <param name="fullDate">The date as it is returned from the CreatedDate property of the assignment</param>
        /// <returns> The date converted to string and without the time and the '/' character </returns>
        static string GetDateOnly(DateTime fullDate)
        {
            return fullDate.ToString("yyyy-MM-dd");
        }

        /// <summary>Generates the name for a assignment folder.</summary>
        internal static string GenerateFolderName(AssignmentProperties properties)
        {
            string title = MakeTitleSafe(properties.Title.Trim());
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", GetDateOnly(properties.StartDate), title, properties.Id.GetKey());
        }

        internal static string AssignmentViewName(AssignmentProperties properties)
        {
            string title = MakeTitleSafe(properties.Title.Trim());
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", GetDateOnly(properties.StartDate), title);
        }

        /// <summary>Makes a string a safe title for a folder.</summary>
        /// <param name="title">The starting point.</param>
        public static string MakeTitleSafe(string title)
        {
            string modified = nameRegex.Replace(title, "-").Replace("\"", "-");
            return repeatedDotRegex.Replace(modified, "__");
        }

#endregion static members
    }
}

