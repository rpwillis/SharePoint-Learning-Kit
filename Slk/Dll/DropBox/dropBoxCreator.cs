using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Creates the drop box list.</summary>
    public class DropBoxCreator : IDisposable
    {
        internal const string dropBoxName = "DropBox";
        SPWeb web;
        SPSite site;
        SlkCulture culture;
        SPList dropBoxList;
        ISlkStore store;
        bool alreadyCreated;

#region constructors
        /// <summary>Initializes a new instance of <see cref="DropBox"/>.</summary>
        /// <param name="store">The ISlkStore to use.</param>
        /// <param name="web">The web to create the drop box in.</param>
        public DropBoxCreator(ISlkStore store, SPWeb web)
        {
            this.site = new SPSite(web.Url);
            this.web = site.OpenWeb();
            web.AllowUnsafeUpdates = true;
            this.store = store;
            culture = new SlkCulture(web);
        }
#endregion constructors

#region properties
#endregion properties

#region public methods
        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            if (web != null)
            {
                web.Dispose();
            }

            if (site != null)
            {
                site.Dispose();
            }
        }

        /// <summary>Creates the drop box.</summary>
        public Guid Create()
        {
            using (new AllowUnsafeUpdates(web))
            {
                Guid id = CreateLibrary(0);

                if (id != Guid.Empty)
                {
                    if (alreadyCreated == false)
                    {
                        // list has been created. Save the id  to try to prevent another simultaneous request creating it
                        web.AllProperties[DropBox.PropertyKey] = id.ToString("D", CultureInfo.InvariantCulture);
                        web.Update();

                        // Reload all objects to avoid errors adding fields in web with non-US locale and multilingual
                        ReloadWebObjects();
                        SetUpDropBox(id);
                    }
                }

                return id;
            }
        }

        /// <summary>Create a folder without any permissions.</summary>
        public static SPListItem CreateNoPermissionsFolder(SPList list)
        {
            string url = list.RootFolder.ServerRelativeUrl;

            using (new AllowUnsafeUpdates(list.ParentWeb))
            {
                SPListItem folder = list.Items.Add(url, SPFileSystemObjectType.Folder, DropBox.NoPermissionsFolderName);
                folder.Update();
                ClearPermissions(folder);
                list.Update();
                return folder;
            }
        }

        /// <summary>Clears the permissions on a folder.</summary>
        /// <param name="folder">The folder to clear the permissions for.</param>
        public static void ClearPermissions(SPListItem folder)
        {
            DropBox.RemoveRoleAssignments(folder, folder.ParentList.ParentWeb);
            folder.Update();
        }

#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        private Guid CreateLibrary(int recursiveNumber)
        {
            string name = dropBoxName;
            if (recursiveNumber > 0)
            {
                name = name + recursiveNumber.ToString(CultureInfo.InvariantCulture);
            }

            try
            {
                return web.Lists.Add(name, dropBoxName, SPListTemplateType.DocumentLibrary);
            }
            catch (SPException e)
            {
                // Library already exists, add a number to make it unique
                Guid id = CheckIfCreatedInMeantime();
                if (id == Guid.Empty)
                {
                    if (recursiveNumber < 2)
                    {
                        return CreateLibrary(recursiveNumber + 1);
                    }
                    else
                    {
                        throw new SafeToDisplayException(string.Format(SlkCulture.GetCulture(), culture.Resources.DropBoxListCreateFailure, e.Message));
                    }
                }
                else
                {
                    // The reload in objects should have given the other process time to complete setting up the list - at least adding the fields
                    alreadyCreated = true;;
                    return id;
                }
            }
        }

        private Guid CheckIfCreatedInMeantime()
        {
            ReloadWebObjects();
            object property = web.AllProperties[DropBox.PropertyKey];
            if (property != null)
            {
                // List has been created - return
                return new Guid(property.ToString());
            }

            return Guid.Empty;
        }

        void SetUpDropBox(Guid id)
        {
            // Library is created, now need to set it up
            dropBoxList = web.Lists[id];
            bool okToContinue = false;

            try
            {
                AddFields();

                // Set up versioning
                dropBoxList.EnableVersioning = true;
                dropBoxList.Update();

                // Reached point at which list is usable, so any errors from now, just log but continue.
                okToContinue = true;

                ChangeToInternationalNames();

                ClearPermissions();
                CreateNoPermissionsFolder(dropBoxList);
                ModifyDefaultView();
            }
            catch (SPException e)
            {
                store.LogError(culture.Resources.DropBoxListCreateFailure, e);
                if (okToContinue == false)
                {
                    // Error creating list - delete it
                    dropBoxList.Delete();
                    web.AllProperties[DropBox.PropertyKey] = null;
                    dropBoxList = null;
                    throw new SafeToDisplayException(string.Format(SlkCulture.GetCulture(), culture.Resources.DropBoxListCreateFailure, e.Message));
                }
            }
            catch (Exception e)
            {
                store.LogError(culture.Resources.DropBoxListCreateFailure, e);
                throw;
            }
        }

        void ModifyDefaultView()
        {
            SPView defaultView = dropBoxList.DefaultView;
            defaultView.Title = culture.Resources.DropBoxDefaultViewTitle;
            string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /></GroupBy>";
            defaultView.Query = string.Format(CultureInfo.InvariantCulture, query, DropBox.ColumnAssignmentKey, DropBox.ColumnLearner);

            defaultView.Update();
            dropBoxList.Update();

            SPView view = dropBoxList.Views[defaultView.ID];
            view.Scope = SPViewScope.Recursive;
            RemoveFieldNameFromGroupHeader(view);
            view.Update();
            dropBoxList.Update();
        }

        void ClearPermissions()
        {
            DropBox.RemoveRoleAssignments(dropBoxList, web);
        }

        internal static void RemoveFieldNameFromGroupHeader(SPView view)
        {
            string fieldNameHtml = "<GetVar Name=\"GroupByField\" HTMLEncode=\"TRUE\" /><HTML><![CDATA[</a> :&nbsp;]]></HTML>";
            if (view.GroupByHeader != null)
            {
                view.GroupByHeader = view.GroupByHeader.Replace(fieldNameHtml, string.Empty);
            }
        }

        void AddFields()
        {
            SPField assignmentId = AddField(DropBox.ColumnAssignmentId, SPFieldType.Text, false);
            assignmentId.Indexed = true;
            assignmentId.Update();
            dropBoxList.Update();
            web.Update();

            AddField(DropBox.ColumnAssignmentDate, SPFieldType.DateTime, true);
            AddField(DropBox.ColumnAssignmentName, SPFieldType.Text, true);

            AddField(DropBox.ColumnIsLatest, SPFieldType.Boolean, true);
            AddField(DropBox.ColumnLearner, SPFieldType.User, true);
            AddField(DropBox.ColumnLearnerId, SPFieldType.Text, false);
            dropBoxList.Update();
            web.Update();

            SPFieldCalculated assignmentKey = (SPFieldCalculated)AddField(DropBox.ColumnAssignmentKey, SPFieldType.Calculated, true, false);
            // The formula depends on the regional settings of the web, notably the list separator required. 
            // So for a standard english site the formula will be 
            // =TEXT(AssignmentDate,"yyyy-mm-dd")&" "&AssignmentName
            // but for many regions, notably Europe, the list separator is ; so the formula needs to be
            // =TEXT(AssignmentDate;"yyyy-mm-dd")&" "&AssignmentName
            // SharePoint automatically converts formulae when you change the regional settings. Originally the code changed the web Locale before adding the fields
            // and then changed back at the end, but this is simpler, and in some environments that change caused errors which prevented any fields being added.
            string formula = "=TEXT([{0}]{2} \"yyyy-mm-dd\")&\" \"&[{1}]";
            assignmentKey.Formula = string.Format(formula, DropBox.ColumnAssignmentDate, DropBox.ColumnAssignmentName, web.Locale.TextInfo.ListSeparator);
            assignmentKey.Update();
            dropBoxList.Update();
            web.Update();
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView)
        {
            return AddField(name, type, addToDefaultView, true);
        }

        SPField AddField(string name, SPFieldType type, bool addToDefaultView, bool required)
        {
            try
            {
                dropBoxList.Fields.Add(name, type, required);
            }
            catch (SPException e)
            {
                AppResourcesLocal resources = new AppResourcesLocal();
                string message = string.Format(CultureInfo.CurrentUICulture, resources.DropBoxFailCreateField, name, e.Message);
                throw new SPException(message, e);
            }

            SPField field = dropBoxList.Fields[name];
            if (addToDefaultView)
            {
                dropBoxList.DefaultView.ViewFields.Add(field);
            }

            return field;
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
                dropBoxList.Title = resources.DropBoxTitle;
#else
                dropBoxList.TitleResource.SetValueForUICulture(uiCulture, resources.DropBoxTitle);
#endif
                ChangeColumnTitle(DropBox.ColumnAssignmentKey, resources.DropBoxColumnAssignmentKey, uiCulture);
                ChangeColumnTitle(DropBox.ColumnAssignmentName, resources.DropBoxColumnAssignmentName, uiCulture);
                ChangeColumnTitle(DropBox.ColumnAssignmentDate, resources.DropBoxColumnAssignmentDate, uiCulture);
                ChangeColumnTitle(DropBox.ColumnAssignmentId, resources.DropBoxColumnAssignmentId, uiCulture);
                ChangeColumnTitle(DropBox.ColumnIsLatest, resources.DropBoxColumnIsLatest, uiCulture);
                ChangeColumnTitle(DropBox.ColumnLearner, resources.DropBoxColumnLearner, uiCulture);
                ChangeColumnTitle(DropBox.ColumnLearnerId, resources.DropBoxColumnLearnerId, uiCulture);
            }

            dropBoxList.Update();
        }


        void ChangeColumnTitle(string columnKey, string newName, CultureInfo uiCulture)
        {
            SPField field = dropBoxList.Fields[columnKey];
#if SP2007
            field.Title = newName;
#else
            field.TitleResource.SetValueForUICulture(uiCulture, newName);
#endif
            field.Update();
        }

        private void ReloadWebObjects()
        {
            string url = web.Url;
            Dispose();
            this.site = new SPSite(url);
            this.web = site.OpenWeb();
            web.AllowUnsafeUpdates = true;
        }


#endregion private methods

#region static members
#endregion static members
    }
}

