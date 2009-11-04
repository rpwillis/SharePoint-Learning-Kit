using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Microsoft.LearningComponents.Localization
{
    internal static class LocalizationManager
    {
        private const int defaultLcid = 1033;
        private static object sync = new object();
        private static CultureInfo defaultCulture;
        private static ILocalizationProvider currentProvider;
        /// <summary>
        /// Gets the default culture object 
        /// </summary>
        private static CultureInfo DefaultCulture
        {
            get
            { 
                if(defaultCulture==null)
                    lock (sync)
                    {
                        defaultCulture = CreateDefaultCulture();
                    }
                return defaultCulture;
            }
        }

        /// <summary>
        /// Creates the current cluture from the current localization provider
        /// </summary>
        /// <returns>Current culture if a localization provider is configured or the english default culture</returns>
        internal static CultureInfo GetCurrentCulture()
        {
            ILocalizationProvider currentLocalizationProvider = CreateCurrentProvider();
            if (currentLocalizationProvider == null)
                return CreateDefaultCulture();
            else
                return currentLocalizationProvider.CurrentCulture;
        }

        /// <summary>
        /// Creates the localization provider that is configured in configuration file
        /// </summary>
        /// <returns>refrence to configured localization provider object or null if nothing configured</returns>
        private static ILocalizationProvider CreateCurrentProvider()
        {
            if (currentProvider != null)
                return currentProvider;
            else
            {
                LocalizationSettings providerSettings = LocalizationSettingsReader.ReadLocalizationSettings();
                if (providerSettings == null)
                    return null;
                else
                {
                    try
                    {
                        object currentProviderObj = CreateObjectByTypeName(
                                    providerSettings.LocalizationProviderTypeName,
                                    providerSettings.LocalizationProviderAssemblyName);
                        currentProvider = currentProviderObj as ILocalizationProvider;
                        return currentProvider;
                    }
                    catch (Exception)
                    {
                        //should log exception
                        return null;
                        //throw;
                    }

                }
            }
        }

        /// <summary>
        /// Creates default english culture
        /// </summary>
        /// <returns>Refrence to default english culture object</returns>
        private static CultureInfo CreateDefaultCulture()
        {
            CultureInfo defaultCulture = new CultureInfo(defaultLcid);
            return defaultCulture;
        }

        private static object CreateObjectByTypeName(string typeName, string assemblyName)
        {
            Assembly targetAssembly = Assembly.Load(assemblyName);
            return targetAssembly.CreateInstance(typeName);
        }
    }
}
