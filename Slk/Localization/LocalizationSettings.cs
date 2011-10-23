using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.SharePoint;

namespace Microsoft.LearningComponents.Localization
{
    public class LocalizationSettings:ConfigurationSection
    {
        private string localizationProviderTypeName;

        [ConfigurationProperty("ProviderTypeName", IsRequired = true)]
        public string LocalizationProviderTypeName
        {
            get { return localizationProviderTypeName; }
            set { localizationProviderTypeName = value; }
        }

        private string localizationProviderAssemblyName;
        [ConfigurationProperty("ProviderAssemblyName", IsRequired = true)]
        public string LocalizationProviderAssemblyName
        {
            get { return localizationProviderAssemblyName; }
            set { localizationProviderAssemblyName = value; }
        }

    }

    internal static class LocalizationSettingsReader
    {
        private static LocalizationSettings currentSettings;
        private static object sync = new object();
        private const string localizationSettingsSectionName = "SLKLocalizationSettings";
        internal static LocalizationSettings ReadLocalizationSettings()
        {

            try
            {
                if (currentSettings == null)
                    lock (sync)
                    {
                        //Configuration config = WebConfigurationManager.OpenWebConfiguration("/", SPContext.Current.Site.WebApplication.Name);
                        currentSettings = ConfigurationManager.GetSection(localizationSettingsSectionName) as LocalizationSettings;
                        //currentSettings = config.GetSection(localizationSettingsSectionName) as LocalizationSettings;
                    }
                return currentSettings;
            }
            catch (Exception)
            {
                //nothing configured, or worng configuration

                //log the exception
                
                return null;
                
            }
        }
    }
}
