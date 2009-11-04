//SLK Release 1.4 – ITWorx
//Created 04-2009
//Drop Box feature

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Resources.Properties;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Class to create the instructor and learner permission levels, while instructor has the right only to view and learner has the right to add to his subfolder.
    /// </summary>
    public class ConfigureDropBox : SPFeatureReceiver
    {
        public ConfigureDropBox()
        {
            //AppResources.Culture = LocalizationManager.GetCurrentCulture();
            AppResources.Culture = Thread.CurrentThread.CurrentCulture;
        }

        string dropBoxName = AppResources.DropBoxDocLibName;

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {

            BreakDropBoxPermisssionsInheritance(GetWebFromProperties(properties));
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            DeleteDropBox(GetWebFromProperties(properties));
        }

        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
        }

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
        }

        private SPWeb GetWebFromProperties(SPFeatureReceiverProperties properties)
        {
            SPWeb currentWeb = properties.Feature.Parent as SPWeb;
            return currentWeb;
        }

        /// <summary>
        /// Breaks the Drop Box document library permissions inheritance
        /// </summary>
        private void BreakDropBoxPermisssionsInheritance(SPWeb currentWeb)
        {
            SPList dropBoxDocLib = currentWeb.Lists[dropBoxName];
            dropBoxDocLib.BreakRoleInheritance(false);
        }

        /// <summary>
        /// Deletes Drop Box document library
        /// </summary>
        private void DeleteDropBox(SPWeb currentWeb)
        {
            currentWeb.Lists[dropBoxName].Delete(); 
        }
    }
}
