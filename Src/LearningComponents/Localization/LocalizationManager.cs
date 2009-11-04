using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;
using Microsoft.SharePoint;
using System.Collections;
using System.Threading;

namespace Microsoft.SharePointLearningKit.Localization
{
    public static class LocalizationManager
    {
       // private static int defaultLcid = 1033;
        private static object sync = new object();
        private static CultureInfo defaultCulture;
        private static Hashtable localizationProviders;

        
        /// <summary>
        /// Initializing static members
        /// </summary>
        static LocalizationManager()
        {
            localizationProviders = new Hashtable();
        }

        /// <summary>
        /// Gets the current localozation provider for the current web-application
        /// </summary>
        private static ILocalizationProvider CurrentProvider
        {
            get
            {
                ILocalizationProvider current = null;
                string currentWebApplicationName = CurrentWebApplication;
                try
                {
                    current = localizationProviders[currentWebApplicationName] as ILocalizationProvider;
                }
                catch
                {
                    
                }
                if (current == null)
                {
                    ILocalizationProvider currentProvider = CreateCurrentProvider();
                    if (currentProvider != null)
                    {
                        lock (sync)
                        {
                            localizationProviders.Add(currentWebApplicationName, currentProvider);
                        }
                    }
                }

                return current;
            }
        }

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
        public static CultureInfo GetCurrentCulture()
        {
            CultureInfo currentCulture;
            if (CurrentProvider == null)
            {
                currentCulture = CreateDefaultCulture();
            }
            else
            {
                currentCulture = CurrentProvider.CurrentCulture;
            }
            //Thread.CurrentThread.CurrentCulture = currentCulture;
            //Thread.CurrentThread.CurrentUICulture = currentCulture;
            return currentCulture;
        }

        /// <summary>
        /// Creates the localization provider that is configured in configuration file
        /// </summary>
        /// <returns>refrence to configured localization provider object or null if nothing configured</returns>
        private static ILocalizationProvider CreateCurrentProvider()
        {
            
            LocalizationSettings providerSettings = LocalizationSettingsReader.ReadLocalizationSettings();
            if (providerSettings == null)
                return null;
            else
            {
                return providerSettings.ProviderInstance;
            }
           
        }

        /// <summary>
        /// Creates default english culture
        /// </summary>
        /// <returns>Refrence to default english culture object</returns>
        private static CultureInfo CreateDefaultCulture()
        {
            //int currentLcid = (int)SPContext.Current.Web.Language;
            CultureInfo defaultCulture = Thread.CurrentThread.CurrentCulture;
            return defaultCulture;
        }

      

        private static string CurrentWebApplication
        {
            get
            {
                return SPContext.Current.Web.Site.WebApplication.Name;
            }
        }
    }
}
