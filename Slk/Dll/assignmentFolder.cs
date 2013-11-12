using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>An assignment folder in the drop box.</summary>
    public class AssignmentFolder
    {
        SPListItem assignmentFolder;
        bool isLearnerFolder;
        AssignmentProperties properties;
        SPWeb web;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentFolder"/>.</summary>
        /// <param name="folder">The actual SharePoint foldder.</param>
        /// <param name="isLearnerFolder">Indicates if it is a learner folder or the main assignment folder.</param>
        /// <param name="properties">The assignment properties.</param>
        public AssignmentFolder(SPListItem folder, bool isLearnerFolder, AssignmentProperties properties)
        {
            this.assignmentFolder = folder;
            this.isLearnerFolder = isLearnerFolder;
            this.properties = properties;
            web = assignmentFolder.Web;
        }
#endregion constructors

#region properties
        /// <summary>The url of the folder.</summary>
        public string Url
        {
            get { return assignmentFolder.Url ;}
        }
#endregion properties

#region public methods
        /// <summary>Finds a learner's folder.</summary>
        /// <param name="learner">The learner.</param>
        /// <returns>The folder for the learner.</returns>
        public AssignmentFolder FindLearnerFolder(SPUser learner)
        {
            SPQuery subFolderQuery = new SPQuery();
            subFolderQuery.Folder = assignmentFolder.Folder;
            subFolderQuery.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + LearnerFolderName(learner) + "</Value></Eq></Where>";
            SPListItemCollection allSubFolders = assignmentFolder.ParentList.GetItems(subFolderQuery);
            if (allSubFolders.Count != 0)
            {
                return new AssignmentFolder(allSubFolders[0], true, properties);
            }
            else
            {
                return null;
            }
        }

        /// <summary>Changes the folder's name.</summary>
        /// <param name="oldAssignmentFolderName">The old folder name.</param>
        /// <param name="newAssignmentFolderName">The new folder name.</param>
        public void ChangeName(string oldAssignmentFolderName, string newAssignmentFolderName)
        {
            string newUrl = assignmentFolder.Url.Replace(oldAssignmentFolderName, newAssignmentFolderName);
            if (!web.GetFolder(newUrl).Exists)
            {
                assignmentFolder.Folder.MoveTo(newUrl);
            }
            else
            {
                throw new SafeToDisplayException(AppResources.AssignmentNameAlreadyUsed);
            }
        }

        /// <summary>Creates a learner's folder.</summary>
        /// <param name="user">The learner.</param>
        /// <returns>The learner's folder.</returns>
        public AssignmentFolder CreateLearnerAssignmentFolder(SPUser user)
        {
            bool originalAllow = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;

            try
            {
                SPListItem subFolder = CreateSubFolder(LearnerFolderName(user));
                ApplyPermissionInternal(subFolder, user, SPRoleType.Reader, new UpdateListItem(assignmentFolder));
                return new AssignmentFolder(subFolder, true, properties);
            }
            finally
            {
                web.AllowUnsafeUpdates = originalAllow;
            }

        }

        /// <summary>Applies the permission for a user.</summary>
        /// <param name="user">The user to give the permission to.</param>
        /// <param name="roleType">The type of permission.</param>
        public void ApplyPermission(SPUser user, SPRoleType roleType)
        {
            if (assignmentFolder != null)
            {
                ApplyPermission(user, roleType, new UpdateListItem(assignmentFolder));
            }
        }

        /// <summary>Removes all permissions for a user.</summary>
        /// <param name="user">The user.</param>
        public void RemovePermissions(SPUser user)
        {
            if (user != null)
            {
                bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
                try
                {
                    web.AllowUnsafeUpdates = true;

                    if (assignmentFolder.HasUniqueRoleAssignments == false)
                    {
                        assignmentFolder.BreakRoleInheritance(false);
                    }

                    assignmentFolder.RoleAssignments.Remove(user);
                    assignmentFolder.Update();
                }
                finally
                {
                    web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
                }
            }
        }


        /// <summary>Resets the IsLatest value for all files to false.</summary>
        public void ResetIsLatestFiles()
        {
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                foreach (SPFile file in assignmentFolder.Folder.Files)
                {
                    file.Item[DropBox.ColumnIsLatest] = false;
                    file.Item.Update();
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }


        /// <summary>Saves a file in the folder.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileStream">The contents of the file.</param>
        /// <param name="learner">The learner the file is for.</param>
        /// <returns>The saved file.</returns>
        public AssignmentFile SaveFile(string fileName, Stream fileStream, SlkUser learner)
        {
            SPFolder folder = assignmentFolder.Folder;
            string fileUrl = folder.Url + '/' + fileName;

            SPFile file = folder.Files.Add(fileUrl, fileStream, true);

            file.Item[DropBox.ColumnAssignmentDate] = properties.StartDate;
            file.Item[DropBox.ColumnAssignmentName] = properties.Title.Trim();
            file.Item[DropBox.ColumnAssignmentId] = properties.Id.GetKey();
            file.Item[DropBox.ColumnLearnerId] = learner.Key;
            file.Item[DropBox.ColumnLearner] = learner.SPUser;
            file.Item[DropBox.ColumnIsLatest] = true; 
            file.Item.Update();
            return new AssignmentFile(file.Name, file.ServerRelativeUrl, (string)file.Item["PermMask"]);
        }

        /// <summary>Removes all permissions on the folder.</summary>
        public void RemoveAllPermissions()
        {
            List<SPPrincipal> toRemove = new List<SPPrincipal>();
            foreach (SPRoleAssignment role in assignmentFolder.RoleAssignments)
            {
                //role.RoleDefinitionBindings.RemoveAll();
                toRemove.Add(role.Member);
            }

            foreach (SPPrincipal principal in toRemove)
            {
                assignmentFolder.RoleAssignments.Remove(principal);
            }

            assignmentFolder.Update();
        }

        /// <summary>Deletes the folder.</summary>
        public void Delete()
        {
            assignmentFolder.Delete();
            if (isLearnerFolder == false)
            {
                DeleteAssociatedView();
            }
        }

        /// <summary>Removes observer permissions.</summary>
        public void RemoveObserverPermission()
        {
            bool isReader = false, isObserver = false;

            foreach (SPRoleAssignment learnerSubfolderRoleAssignment in assignmentFolder.RoleAssignments)
            {
                for (int i = 0; i < learnerSubfolderRoleAssignment.RoleDefinitionBindings.Count; i++)
                {
                    if (learnerSubfolderRoleAssignment.RoleDefinitionBindings[i].Type == SPRoleType.Reader)
                    {
                        isReader = true;

                        foreach (SPRoleAssignment webRoleAssignment in web.RoleAssignments)
                        {
                            for (int j = 0; j < webRoleAssignment.RoleDefinitionBindings.Count; j++)
                            {
                                if (webRoleAssignment.Member.Name == learnerSubfolderRoleAssignment.Member.Name &&
                                    webRoleAssignment.RoleDefinitionBindings[j].Name == SlkStore.GetStore(web).Mapping.ObserverPermission)
                                {
                                    isObserver = true;
                                    break;
                                }
                            }
                            if (isObserver)
                                break;
                        }
                    }
                    if (isReader && isObserver)
                    {
                        learnerSubfolderRoleAssignment.RoleDefinitionBindings.RemoveAll();
                        web.AllowUnsafeUpdates = true;
                        learnerSubfolderRoleAssignment.Update();
                        web.AllowUnsafeUpdates = false;
                        isReader = false;
                        isObserver = false;
                        break;
                    }
                }
            }
        }
        
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        void DeleteAssociatedView()
        {
            SPList dropBox = assignmentFolder.ParentList;
            try
            {
                SPView view = dropBox.Views[DropBox.AssignmentViewName(properties)];
                dropBox.Views.Delete(view.ID);
            }
            catch (ArgumentException)
            {
                // No view
            }
        }

        void ApplyPermission(SPUser user, SPRoleType roleType, UpdateItem updateItem)
        {
            ApplyPermissionInternal(assignmentFolder, user, roleType, updateItem);
        }

        void ApplyPermissionInternal(SPListItem folder, SPUser user, SPRoleType roleType, UpdateItem updateItem)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }


            SPRoleDefinition roleDefinition;
            try
            {
                roleDefinition = web.RoleDefinitions.GetByType(roleType);
            }
            catch (ArgumentException)
            {
                throw new SafeToDisplayException(string.Format(SlkCulture.GetCulture(), AppResources.DropBoxManagerNoRole, roleType));
            }

            SPRoleAssignment roleAssignment = null;

            bool currentUnsafeUpdates = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;
            try
            {
                bool isNewRoleDefintion = true;

                if (folder.HasUniqueRoleAssignments == false)
                {
                    folder.BreakRoleInheritance(false);
                    roleAssignment = new SPRoleAssignment(user);
                }
                else
                {
                    try
                    {
                        roleAssignment = folder.RoleAssignments.GetAssignmentByPrincipal(user);
                    }
                    catch (ArgumentException)
                    {
                        // No role assignments for user
                    }

                    if (roleAssignment == null)
                    {
                        roleAssignment = new SPRoleAssignment(user);
                    }
                    else
                    {
                        isNewRoleDefintion = false;
                        bool needsRoleAdding = true;
                        foreach (SPRoleDefinition iterator in roleAssignment.RoleDefinitionBindings)
                        {
                            if (iterator.Id == roleDefinition.Id)
                            {
                                needsRoleAdding = false;
                                break;
                            }
                        }

                        if (needsRoleAdding)
                        {
                            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                            roleAssignment.Update();
                        }
                    }
                }

                if (isNewRoleDefintion)
                {
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
#if SP2007
                    folder.RoleAssignments.Add(roleAssignment);
#else
                    folder.RoleAssignments.AddToCurrentScopeOnly(roleAssignment);
#endif
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = currentUnsafeUpdates;
            }
        }

        /// <summary>
        /// Creates sub folder under a specific assignment folder, it is being named after the specified learner's name.
        /// </summary>
        SPListItem CreateSubFolder(string newFolderName)
        {
            SPFolder folder = assignmentFolder.Folder;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                SPFolder learnerSubFolder = folder.SubFolders.Add(newFolderName);
                learnerSubFolder.Update();
                DropBox.ClearPermissions(learnerSubFolder.Item);
                return learnerSubFolder.Item;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }
#endregion private methods

#region static members
        static string LearnerFolderName(SPUser learner)
        {
            string folderName = learner.LoginName;
            return DropBox.MakeTitleSafe(folderName);
        }

#endregion static members

#region UpdateItem classes
        abstract class UpdateItem
        {
            public abstract void Update();
        }

        class UpdateList : UpdateItem
        {
            SPList list;

            public UpdateList(SPList list)
            {
                this.list = list;
            }

            public override void Update()
            {
                list.Update();
            }
        }

        class UpdateListItem : UpdateItem
        {
            SPListItem listItem;

            public UpdateListItem(SPListItem listItem)
            {
                this.listItem = listItem;
            }

            public override void Update()
            {
                listItem.Update();
            }
        }
#endregion UpdateItem classes

    }
}

