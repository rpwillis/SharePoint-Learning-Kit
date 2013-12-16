using System;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    class AssignmentObjectsFromQueryString
    {
#region constructors
#endregion constructors

#region properties
        public SPList List { get; private set; }
        public SPListItem ListItem { get; private set; }
        public SPFile File { get; private set; }

        private AppResourcesLocal Resources
        {
            get { return SlkCulture.GetResources() ;}
        }
#endregion properties

#region public methods
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        public void LoadObjects(SPWeb web)
        {
            try
            {
                Guid listId = QueryString.ParseGuid(QueryStringKeys.ListId);
                List = web.Lists[listId];
                int itemId = QueryString.Parse(QueryStringKeys.ItemId);
                ListItem = List.GetItemById(itemId);

                // reject folders
                if (ListItem.FileSystemObjectType == SPFileSystemObjectType.Folder)
                {
                    throw new SafeToDisplayException(Resources.ActionsItemIsFolder);
                }
                // reject anything but a file
                else if (ListItem.FileSystemObjectType != SPFileSystemObjectType.File)
                {
                    throw new SafeToDisplayException(Resources.ActionsItemNotFound);
                }

                File = ListItem.File;
            }
            catch (SPException)
            {
                // The list isn't found
                throw new SafeToDisplayException(Resources.ActionsItemNotFound);
            }
            catch (ArgumentException)
            {
                // The file isn't found
                throw new SafeToDisplayException(Resources.ActionsItemNotFound);
            }
        }
#endregion private methods

#region static members
#endregion static members
    }
}

