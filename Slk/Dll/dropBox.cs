using System;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>An object responsible for returning and creating the drop box.</summary>
    public class DropBox
    {
        const string dropBoxName = "DropBox";
        public const string ColumnAssignmentKey = "Assignment";
        public const string ColumnAssignmentDate = "AssignmentDate";
        public const string ColumnAssignmentName = "AssignmentName";
        public const string ColumnAssignmentId = "AssignmentId";
        public const string ColumnIsLatest = "IsLatest";
        public const string ColumnLearnerId = "LearnerId";
        public const string ColumnLearner = "Learner";
        const string propertyKey = "SLKDropBox";
        const string noPermissionsFolderName = "NoPermissions";
        SPWeb web;
        SPList dropBoxList;

#region constructors
        /// <summary>Initializes a new instance of <see cref="DropBoxCreator"/>.</summary>
        /// <param name="web">The web to create the drop box in.</param>
        public DropBox(SPWeb web)
        {
            this.web = web;
        }
#endregion constructors

#region properties
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
        /// <summary>Creates the drop box.</summary>
        /// <returns>The drop box.</returns>
        public SPList Create()
        {
            CreateDropBoxLibrary(0);
            return DropBoxList;
        }

        public AssignmentFolder CreateAssignmentFolder(AssignmentProperties properties)
        {
            DropBoxManager.Debug("Starting DropBox.CreateAssignmentFolder");
            string url = DropBoxList.RootFolder.ServerRelativeUrl;
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: url {0}", url);
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;

            SPFolder noPermissionsFolder = GetNoPermissionsFolder().Folder;
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: got no permissions folder");

            try
            {
                web.AllowUnsafeUpdates = true;
                string name = GenerateFolderName(properties);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: folder name {0}", name);
                SPFolder folder = noPermissionsFolder.SubFolders.Add(name);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: created folder in no permissions");
                folder.MoveTo(url + "\\" + name);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: moved");
                folder.Update();
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: updated");
                SPListItem assignmentFolder = folder.Item;
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: item url {0}", assignmentFolder.Url);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: before clear permissions");
                DropBox.ClearPermissions(assignmentFolder);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: after clear permissions");
                DropBoxList.Update();
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: updated");
                CreateAssignmentView(properties);
            DropBoxManager.Debug("DropBox.CreateAssignmentFolder: Created view");
                return new AssignmentFolder(assignmentFolder, false, properties);
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

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
            DropBoxManager.Debug("DropBox.GetAssignmentFolder: start");
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + GenerateFolderName(properties) + "</Value></Eq></Where>";
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

        public void ChangeFolderName(AssignmentProperties oldAssignmentProperties, AssignmentProperties newAssignmentProperties)
        {
            string oldAssignmentFolderName = GenerateFolderName(oldAssignmentProperties);
            string newAssignmentFolderName = GenerateFolderName(newAssignmentProperties);
            //Get old assignment folder
            AssignmentFolder oldAssignmentFolder = GetAssignmentFolder(oldAssignmentProperties);
            if (oldAssignmentFolder != null)
            {
                oldAssignmentFolder.ChangeName(oldAssignmentFolderName, newAssignmentFolderName);
            }
            else
            {
                throw new SafeToDisplayException(AppResources.AssFolderNotFound);
            }
        }

#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
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
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                Guid id;
                try
                {
                    string name = dropBoxName;
                    if (recursiveNumber > 0)
                    {
                        name = name + recursiveNumber.ToString(CultureInfo.InvariantCulture);
                    }
                    id = web.Lists.Add(name, dropBoxName, SPListTemplateType.DocumentLibrary);
                    SetUpDropBox(web, id);
                }
                catch (SPException)
                {
                    // Library already exists, add a number to make it unique
                    if (recursiveNumber < 10)
                    {
                        CreateDropBoxLibrary(recursiveNumber++);
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        void SetUpDropBox(SPWeb web, Guid id)
        {
            dropBoxList = web.Lists[id];

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

            // Set up versioning
            DropBoxList.EnableVersioning = true;

            ModifyDefaultView();
            DropBoxList.Update();

            web.AllProperties[propertyKey] = DropBoxList.ID.ToString("D", CultureInfo.InvariantCulture);
            web.Update();

            // Change name to internationalized name
            DropBoxList.Title = AppResources.DropBoxTitle;
            ChangeColumnTitle(ColumnAssignmentKey, AppResources.DropBoxColumnAssignmentKey);
            ChangeColumnTitle(ColumnAssignmentName, AppResources.DropBoxColumnAssignmentName);
            ChangeColumnTitle(ColumnAssignmentDate, AppResources.DropBoxColumnAssignmentDate);
            ChangeColumnTitle(ColumnAssignmentId, AppResources.DropBoxColumnAssignmentId);
            ChangeColumnTitle(ColumnIsLatest, AppResources.DropBoxColumnIsLatest);
            ChangeColumnTitle(ColumnLearner, AppResources.DropBoxColumnLearner);
            ChangeColumnTitle(ColumnLearnerId, AppResources.DropBoxColumnLearnerId);
            DropBoxList.Update();

            ClearPermissions();
            CreateNoPermissionsFolder();
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView)
        {
            return AddField(name, type, addToDefaultView, true);
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView, bool required)
        {
            DropBoxList.Fields.Add(name, type, required);
            SPField field = DropBoxList.Fields[name];
            if (addToDefaultView)
            {
                DropBoxList.DefaultView.ViewFields.Add(field);
            }
            return field;
        }


        void ChangeColumnTitle(string columnKey, string newName)
        {
            SPField field = DropBoxList.Fields[columnKey];
            field.Title = newName;
            field.Update();
        }


        SPListItem CreateNoPermissionsFolder()
        {
            string url = DropBoxList.RootFolder.ServerRelativeUrl;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;

            try
            {
                web.AllowUnsafeUpdates = true;
                SPListItem folder = DropBoxList.Items.Add(url, SPFileSystemObjectType.Folder, noPermissionsFolderName);
                folder.Update();
                ClearPermissions(folder);
                DropBoxList.Update();
                return folder;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        void ClearPermissions()
        {
            bool allowUnsafeUpdatesValue = web.AllowUnsafeUpdates;

            try
            {
                RemoveRoleAssignments(DropBoxList, web);
            }
            finally
            {
                web.AllowUnsafeUpdates = allowUnsafeUpdatesValue;
            }
        }

        void ModifyDefaultView()
        {
            SPView defaultView = DropBoxList.DefaultView;
            defaultView.Title = AppResources.DropBoxDefaultViewTitle;
            string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /></GroupBy>";
            defaultView.Query = string.Format(CultureInfo.InvariantCulture, query, ColumnAssignmentKey, ColumnLearner);

            defaultView.Update();
            DropBoxList.Update();

            SPView view = DropBoxList.Views[defaultView.ID];
            view.Scope = SPViewScope.Recursive;
            RemoveFieldNameFromGroupHeader(view);
            view.Update();
        }

#endregion private methods

#region static members
        public static void RemoveFieldNameFromGroupHeader(SPView view)
        {
            string fieldNameHtml = "<GetVar Name=\"GroupByField\" HTMLEncode=\"TRUE\" /><HTML><![CDATA[</a> :&nbsp;]]></HTML>";
            if (view.GroupByHeader != null)
            {
                view.GroupByHeader = view.GroupByHeader.Replace(fieldNameHtml, string.Empty);
            }
        }

        public static void ClearPermissions(SPListItem folder)
        {
            RemoveRoleAssignments(folder, folder.ParentList.ParentWeb);
            folder.Update();
        }

        static void RemoveRoleAssignments(ISecurableObject securable, SPWeb web)
        {
            // There's a bug with BreakRoleInheritance which means that passing false resets AllowUnsafeUpdates to
            // false so the call fails. http://www.ofonesandzeros.com/2008/11/18/the-security-validation-for-this-page-is-invalid/
            securable.BreakRoleInheritance(true);

            // No need to cache the original value as already set to true before calling here and
            // BreakRoleInheritance has reset to false
            web.AllowUnsafeUpdates = true;

            SPRoleAssignmentCollection roleAssigns = securable.RoleAssignments;

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

        static string GenerateFolderName(AssignmentProperties properties)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", GetDateOnly(properties.StartDate), properties.Title.Trim(), properties.Id.GetKey());
        }

        internal static string AssignmentViewName(AssignmentProperties properties)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", GetDateOnly(properties.StartDate), properties.Title.Trim());
        }

#endregion static members
    }
}

