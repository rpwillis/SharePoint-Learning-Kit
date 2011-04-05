using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.SharePoint;
using System.Reflection;

namespace Microsoft.SharePointLearningKit.Localization
{
    /// <summary></summary>
    public class LocalizationSettings:ConfigurationSection
    {
       

        /// <summary></summary>
        /// <value></value>
        [ConfigurationProperty("ProviderTypeName", IsRequired = true)]
        public string LocalizationProviderTypeName
        {
            get { return (string)this["ProviderTypeName"]; }
            set { this["ProviderTypeName"] = value; }
        }

        /// <summary></summary>
        [ConfigurationProperty("ProviderAssemblyName", IsRequired = true)]
        public string LocalizationProviderAssemblyName
        {
            get { return (string)this["ProviderAssemblyName"]; }
            set { this["ProviderAssemblyName"] = value; }
        }

        private ILocalizationProvider providerInstance;
        /// <summary></summary>
        public ILocalizationProvider ProviderInstance
        {
            get
            {
                if (providerInstance == null)
                {
                    try
                    {
                        object currentProviderObj = CreateObjectByTypeName(
                                   LocalizationProviderTypeName,LocalizationProviderAssemblyName);
                        providerInstance = currentProviderObj as ILocalizationProvider;
                        
                    }
                    catch (Exception)
                    {
                        //should log exception
                        //SlkError.WriteToEventLog(ex);
                        return null;
                        
                    }
                }
                return providerInstance;
            }
        }


        private  object CreateObjectByTypeName(string typeName, string assemblyName)
        {
            Assembly targetAssembly = Assembly.Load(assemblyName);
            return targetAssembly.CreateInstance(typeName);
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
                //nothing configured

                //Do not log this exception
                
                return null;
                
            }
        }
    }
}
