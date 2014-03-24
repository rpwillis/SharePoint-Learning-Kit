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
        internal const string dropBoxName = "DropBox";
        internal const string ColumnAssignmentKey = "Assignment";
        internal const string ColumnAssignmentDate = "AssignmentDate";
        internal const string ColumnAssignmentName = "AssignmentName";
        internal const string ColumnAssignmentId = "AssignmentId";
        internal const string ColumnIsLatest = "IsLatest";
        internal const string ColumnLearnerId = "LearnerId";
        internal const string ColumnLearner = "Learner";
        const string propertyKey = "SLKDropBox";
        const string noPermissionsFolderName = "NoPermissions";
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
                    object property = web.AllProperties[propertyKey];
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
                        CreateDropBoxLibrary(0);
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
        /// <returns>The last submitted files.</returns>
        public AssignmentFile[] LastSubmittedFiles(SlkUser user, long assignmentKey)
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

        /// <summary>Creates the drop box.</summary>
        /// <returns>The drop box.</returns>
        public SPList Create()
        {
            CreateDropBoxLibrary(0);
            return DropBoxList;
        }

        /// <summary>Creates the assignment folder.</summary>
        /// <param name="properties">The assignment properties.</param>
        /// <returns>The assignment folder.</returns>
        public AssignmentFolder CreateAssignmentFolder(AssignmentProperties properties)
        {
            string url = DropBoxList.RootFolder.ServerRelativeUrl;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;

            SPFolder noPermissionsFolder = GetNoPermissionsFolder().Folder;

            try
            {
                web.AllowUnsafeUpdates = true;
                string name = GenerateFolderName(properties);
                SPFolder folder = noPermissionsFolder.SubFolders.Add(name);
                folder.MoveTo(url + "\\" + name);
                folder.Update();
                SPListItem assignmentFolder = folder.Item;
                DropBox.ClearPermissions(assignmentFolder);
                DropBoxList.Update();
                CreateAssignmentView(properties);
                return new AssignmentFolder(assignmentFolder, false, properties);
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
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
            DropBoxManager.Debug("DropBox.GetAssignmentFolder: start");
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + folderName + "</Value></Eq></Where>";
            DropBoxManager.Debug("DropBox.GetAssignmentFolder: query {0}", query.Query);
            SPListItemCollection items = DropBoxList.GetItems(query);
            DropBoxManager.Debug("DropBox.GetAssignmentFolder: run query. count {0}", items.Count);
            if (items.Count != 0)
            {
                DropBoxManager.Debug("DropBox.GetAssignmentFolder: returning folder");
                DropBoxManager.Debug("DropBox.GetAssignmentFolder: url {0}", items[0].Url);
                return new AssignmentFolder(items[0], false ,properties);
            }
            else
            {
                DropBoxManager.Debug("DropBox.GetAssignmentFolder: folder not found");
                return null;
            }
        }

        SPListItem GetNoPermissionsFolder()
        {
            try
            {
                SPFolder folder = DropBoxList.RootFolder.SubFolders[noPermissionsFolderName];
                return folder.Item;
            }
            catch (ArgumentException)
            {
                return CreateNoPermissionsFolder();
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
            DropBox.RemoveFieldNameFromGroupHeader(view2);
            view2.Update();
            DropBoxList.Update();
        }


        void CreateDropBoxLibrary(int recursiveNumber)
        {
            DropBoxManager.Debug("Starting CreateDropBoxLibrary");
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                Guid id;
                string name = dropBoxName;
                if (recursiveNumber > 0)
                {
                    name = name + recursiveNumber.ToString(CultureInfo.InvariantCulture);
                }

                try
                {
                    DropBoxManager.Debug("Creating DropBox {0}", name);
                    id = web.Lists.Add(name, dropBoxName, SPListTemplateType.DocumentLibrary);
                    DropBoxManager.Debug("Created DropBox {0}", name);
                }
                catch (SPException e)
                {
                    DropBoxManager.Debug("{0}", e);
                    // Library already exists, add a number to make it unique
                    if (recursiveNumber < 2)
                    {
                        CreateDropBoxLibrary(recursiveNumber + 1);
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }

                SetUpDropBox(web, id);
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
            DropBoxManager.Debug("Ending CreateDropBoxLibrary");
        }

        void AddFields()
        {
            DropBoxManager.Debug("Adding fields");

            // can only set up the formula if in an English locale, so change then change back after
            int webLocaleId = web.Locale.LCID;
            bool localeChanged = false;

            if (webLocaleId != 1033)
            {
                web.Locale = new CultureInfo(1033);
                web.Update();
                localeChanged = true;
            }


            try
            {
                SPField assignmentId = AddField(ColumnAssignmentId, SPFieldType.Text, false);
                assignmentId.Indexed = true;
                assignmentId.Update();

                AddField(ColumnAssignmentDate, SPFieldType.DateTime, true);
                AddField(ColumnAssignmentName, SPFieldType.Text, true);
                SPFieldCalculated assignmentKey = (SPFieldCalculated)AddField(ColumnAssignmentKey, SPFieldType.Calculated, true, false);
                string formula = "=TEXT([{0}], \"yyyy-mm-dd\")&\" \"&[{1}]";
                assignmentKey.Formula = string.Format(formula, ColumnAssignmentDate, ColumnAssignmentName);
                assignmentKey.Update();
                AddField(ColumnIsLatest, SPFieldType.Boolean, true);
                AddField(ColumnLearner, SPFieldType.User, true);
                AddField(ColumnLearnerId, SPFieldType.Text, false);
                DropBoxList.Update();
                DropBoxManager.Debug("End Adding fields");
            }
            finally
            {
                if (localeChanged)
                {
                    web.Locale = new CultureInfo(webLocaleId);
                    web.Update();
                }
            }
        }

        void ChangeToInternationalNames()
        {
            //This is done separately so the internal names are consistent.
            // Change name to internationalized name
#if SP2007
            CultureInfo uiCulture = CultureInfo.CurrentUICulture;
#else
            foreach (CultureInfo uiCulture in web.SupportedUICultures)
#endif
            {
                AppResourcesLocal resources = new AppResourcesLocal();
                resources.Culture = uiCulture;

#if SP2007
                DropBoxList.Title = resources.DropBoxTitle;
#else
                DropBoxList.TitleResource.SetValueForUICulture(uiCulture, resources.DropBoxTitle);
#endif
                ChangeColumnTitle(ColumnAssignmentKey, resources.DropBoxColumnAssignmentKey, uiCulture);
                ChangeColumnTitle(ColumnAssignmentName, resources.DropBoxColumnAssignmentName, uiCulture);
                ChangeColumnTitle(ColumnAssignmentDate, resources.DropBoxColumnAssignmentDate, uiCulture);
                ChangeColumnTitle(ColumnAssignmentId, resources.DropBoxColumnAssignmentId, uiCulture);
                ChangeColumnTitle(ColumnIsLatest, resources.DropBoxColumnIsLatest, uiCulture);
                ChangeColumnTitle(ColumnLearner, resources.DropBoxColumnLearner, uiCulture);
                ChangeColumnTitle(ColumnLearnerId, resources.DropBoxColumnLearnerId, uiCulture);
            }

            DropBoxList.Update();
        }

        void SetUpDropBox(SPWeb web, Guid id)
        {
            dropBoxList = web.Lists[id];
            bool okToContinue = false;

            try
            {
                AddFields();
                ChangeToInternationalNames();

                // Set up versioning
                DropBoxList.EnableVersioning = true;
                DropBoxList.Update();

                // Reached point at which list is usable, so any errors from now, just log but continue.
                okToContinue = true;

                ClearPermissions();
                ModifyDefaultView();
                CreateNoPermissionsFolder();

            }
            catch (SPException e)
            {
                store.LogError(culture.Resources.DropBoxListCreateFailure, e);
                if (okToContinue == false)
                {
                    // Error creating list - delete it
                    dropBoxList.Delete();
                    web.Update();
                    dropBoxList = null;
                    throw new SafeToDisplayException(string.Format(SlkCulture.GetCulture(), culture.Resources.DropBoxListCreateFailure, e.Message));
                }
            }
            catch (Exception e)
            {
                store.LogError(culture.Resources.DropBoxListCreateFailure, e);
                if (okToContinue == false)
                {
                    // Error creating list - delete it
                    dropBoxList.Delete();
                    web.Update();
                    dropBoxList = null;
                    throw;
                }
            }
            finally
            {
                if (dropBoxList != null)
                {
                    web.AllProperties[propertyKey] = dropBoxList.ID.ToString("D", CultureInfo.InvariantCulture);
                    web.Update();
                }
            }
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView)
        {
            return AddField(name, type, addToDefaultView, true);
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView, bool required)
        {
            DropBoxManager.Debug("AddField {0} {1} {2} {3}", name, type, addToDefaultView, required);
            DropBoxList.Fields.Add(name, type, required);
            SPField field = DropBoxList.Fields[name];
            if (addToDefaultView)
            {
                DropBoxList.DefaultView.ViewFields.Add(field);
            }
            DropBoxManager.Debug("End AddField {0} {1} {2} {3}", name, type, addToDefaultView, required);
            return field;
        }


        void ChangeColumnTitle(string columnKey, string newName, CultureInfo uiCulture)
        {
            SPField field = DropBoxList.Fields[columnKey];
#if SP2007
            field.Title = newName;
#else
            field.TitleResource.SetValueForUICulture(uiCulture, newName);
#endif
            field.Update();
        }


        SPListItem CreateNoPermissionsFolder()
        {
            DropBoxManager.Debug("Create no permissions folder");

            string url = DropBoxList.RootFolder.ServerRelativeUrl;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;

            try
            {
                web.AllowUnsafeUpdates = true;
                SPListItem folder = DropBoxList.Items.Add(url, SPFileSystemObjectType.Folder, noPermissionsFolderName);
                folder.Update();
                ClearPermissions(folder);
                DropBoxList.Update();
                DropBoxManager.Debug("End Create no permissions folder");
                return folder;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        void ClearPermissions()
        {
            DropBoxManager.Debug("Clear Permissions");
            bool allowUnsafeUpdatesValue = web.AllowUnsafeUpdates;

            try
            {
                RemoveRoleAssignments(DropBoxList, web);
            }
            finally
            {
                web.AllowUnsafeUpdates = allowUnsafeUpdatesValue;
            }
            DropBoxManager.Debug("End Clear Permissions");
        }

        void ModifyDefaultView()
        {
            DropBoxManager.Debug("ModifyDefaultView");
            SPView defaultView = DropBoxList.DefaultView;
            defaultView.Title = culture.Resources.DropBoxDefaultViewTitle;
            string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /></GroupBy>";
            defaultView.Query = string.Format(CultureInfo.InvariantCulture, query, ColumnAssignmentKey, ColumnLearner);

            defaultView.Update();
            DropBoxList.Update();

            SPView view = DropBoxList.Views[defaultView.ID];
            view.Scope = SPViewScope.Recursive;
            RemoveFieldNameFromGroupHeader(view);
            view.Update();
            DropBoxList.Update();
            DropBoxManager.Debug("End ModifyDefaultView");
        }

#endregion private methods

#region static members
        static readonly System.Text.RegularExpressions.Regex nameRegex = new System.Text.RegularExpressions.Regex(@"[\*#%\&:<>\?/{|}\\@]");
        static readonly System.Text.RegularExpressions.Regex repeatedDotRegex = new System.Text.RegularExpressions.Regex(@"\.\.");
        static void RemoveFieldNameFromGroupHeader(SPView view)
        {
            string fieldNameHtml = "<GetVar Name=\"GroupByField\" HTMLEncode=\"TRUE\" /><HTML><![CDATA[</a> :&nbsp;]]></HTML>";
            if (view.GroupByHeader != null)
            {
                view.GroupByHeader = view.GroupByHeader.Replace(fieldNameHtml, string.Empty);
            }
        }

        /// <summary>Clears the permissions on a folder.</summary>
        /// <param name="folder">The folder to clear the permissions for.</param>
        public static void ClearPermissions(SPListItem folder)
        {
            RemoveRoleAssignments(folder, folder.ParentList.ParentWeb);
            folder.Update();
        }

#if SP2007
        static void RemoveRoleAssignments(ISecurableObject securable, SPWeb web)
#else
        static void RemoveRoleAssignments(SPSecurableObject securable, SPWeb web)
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

