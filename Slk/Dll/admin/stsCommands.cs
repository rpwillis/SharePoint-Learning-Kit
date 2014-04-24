using System;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.StsAdmin;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The STS commands.</summary>
    public class StsCommands : ISPStsadmCommand
    {
        const string commandConfigure = "slk-configure";
        const string commandDeleteMapping = "slk-deletemapping";
        const string commandEnumMappings = "slk-enummappings";
        const string commandConfiguration = "slk-getconfiguration";
        StringDictionary keyValues;
        SlkCulture culture = new SlkCulture();
#region constructors
#endregion constructors

#region properties
#endregion properties

#region public methods
        /// <summary>See <see cref="ISPStsadmCommand.GetHelpMessage"/>.</summary>
        public string GetHelpMessage(string command)
        {
            switch (command.ToLower(CultureInfo.InvariantCulture))
            {
                case commandConfigure:
                    return culture.Resources.StsHelpConfigure;

                case commandConfiguration:
                    return culture.Resources.StsHelpGetConfiguration;

                case commandEnumMappings:
                    return culture.Resources.StsHelpEnumMappings;

                case commandDeleteMapping:
                    return culture.Resources.StsHelpDeleteMapping;

                default:
                    throw new ArgumentOutOfRangeException("command");
            }
        }

        /// <summary>See <see cref="ISPStsadmCommand.Run"/>.</summary>
        public int Run(string command, StringDictionary keyValues, out string output)
        {
            this.keyValues = keyValues;

            switch (command.ToLower(CultureInfo.InvariantCulture))
            {
                case commandConfigure:
                    return Configure(out output);

                case commandDeleteMapping:
                    return DeleteMapping(out output);

                case commandEnumMappings:
                    return EnumMappings(out output);

                case commandConfiguration:
                    return GetConfiguration(out output);

                default:
                    Console.WriteLine(command);
                    throw new ArgumentOutOfRangeException("command");
            }
        }

#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        int GetConfiguration(out string output)
        {
            string guidValue = Value("guid");
            string url = Value("url");
            Guid id;

            if (string.IsNullOrEmpty(guidValue) && string.IsNullOrEmpty(url))
            {
                output = FormatString(culture.Resources.StsMissingParameters, "guid", "url");
                return (int)ErrorCodes.SyntaxError;
            }
            else if (string.IsNullOrEmpty(url))
            {
                id = new Guid(guidValue);
            }
            else
            {
                try
                {
                    using (SPSite site = new SPSite(url))
                    {
                        id = site.ID;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    SPWebApplication application = SPWebApplication.Lookup(new Uri(url));
                    id = application.Id;
                }
            }

            return GetSiteConfiguration(id, out output);
        }

        int GetSiteConfiguration(Guid guid, out string output)
        {
                AdministrationConfiguration configuration = SlkAdministration.LoadConfiguration(guid);
                if (configuration.IsNewConfiguration)
                {
                    output = FormatString(culture.Resources.StsInvalidGuid, guid);
                    return (int)ErrorCodes.GeneralError;
                }
                else
                {
                    output = FormatString(culture.Resources.StsGetSiteConfiguration,
                                            configuration.DatabaseServer,
                                            configuration.DatabaseName,
                                            configuration.InstructorPermission,
                                            configuration.LearnerPermission);
                    return 0;
                }
        }
        

        int EnumMappings(out string output)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            foreach (SlkSPSiteMapping mapping in SlkSPSiteMapping.GetMappings())
            {
                string siteLabel = null;
                try
                {
                    using (SPSite spSite = new SPSite(mapping.SPSiteGuid))
                    {
                        siteLabel = spSite.Url;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    SPFarm farm = SPFarm.Local;
                    SPWebApplication webApp = farm.GetObject(mapping.SPSiteGuid) as SPWebApplication;
                    siteLabel = webApp.Name;
                }

                builder.AppendFormat(culture.Resources.StsEnumMappingLine, siteLabel, mapping.SPSiteGuid, mapping.DatabaseServer, mapping.DatabaseName);
                builder.AppendLine();
            }

            output = builder.ToString();
            return 0;
        }

        int DeleteMapping(out string output)
        {
            string guidValue = Value("guid");

            if (string.IsNullOrEmpty(guidValue))
            {
                output = FormatString(culture.Resources.StsMissingParameter, "guid");
                return (int)ErrorCodes.SyntaxError;
            }

            Guid guid = new Guid(guidValue);

            // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection information for that SPSite
            SlkSPSiteMapping mapping= SlkSPSiteMapping.GetMapping(guid);
            if (mapping == null)
            {
                output = FormatString(culture.Resources.StsInvalidGuid, guid);
                return (int)ErrorCodes.GeneralError;
            }
            else
            {
                mapping.Delete();
                output = FormatString(culture.Resources.StsMappingDeleted, guid);
                return 0;
            }
        }

        int Configure(out string output)
        {
            // parse parameters
            bool forApplication = BooleanValue("application");
            string url = Value("url");
            string databaseServer = Value("databaseServer");
            string databaseName = Value("databaseName");
            bool createDatabase = BooleanValue("createDatabase");
            string instructorPermission = Value("instructorPermission");
            string learnerPermission = Value("learnerPermission");
            string observerPermission = Value("observerPermission");
            bool createPermissions = BooleanValue("createPermissions");
            string settingsFileName = Value("settingsFileName");
            if (string.IsNullOrEmpty(settingsFileName))
            {
                settingsFileName = Value("uploadSlkSettings");
            }

            bool defaultSlkSettings = BooleanValue("defaultSlkSettings");

            if (string.IsNullOrEmpty(url))
            {
                output = FormatString(culture.Resources.StsMissingParameter, "url");
                return (int)ErrorCodes.SyntaxError;
            }

            //   -- can't specify both -uploadslksettings and -defaultslksettings
            if (settingsFileName != null && defaultSlkSettings)
            {
                output = FormatString(culture.Resources.StsSettingsError, "settingsFileName", "defaultSlkSettings");
                return (int)ErrorCodes.SyntaxError;
            }

            Guid id;
            string userName;

            // perform the operation
            if (forApplication)
            {
                SPWebApplication application = SPWebApplication.Lookup(new Uri(url));
                id = application.Id;
                userName = application.ApplicationPool.Username;
            }
            else
            {
                using (SPSite site = new SPSite(url))
                {
                    id = site.ID;
                    url = site.Url;
                    userName = site.WebApplication.ApplicationPool.Username;
                }
            }

            AdministrationConfiguration configuration = SlkAdministration.LoadConfiguration(id);
            databaseServer = ValueIfMissing(databaseServer, configuration.DatabaseServer);
            databaseName = ValueIfMissing(databaseName, configuration.DatabaseName);
            instructorPermission = ValueIfMissing(instructorPermission, configuration.InstructorPermission);
            learnerPermission = ValueIfMissing(learnerPermission, configuration.LearnerPermission);
            observerPermission = ValueIfMissing(observerPermission, configuration.ObserverPermission);

            // load SlkSchema.sql if the parameters specify creating the database;
            // SlkSchema.sql is in the same directory as this .exe file
            string schemaFileContents = null;
            if (createDatabase)
            {
                schemaFileContents = culture.Resources.SlkSchemaSql;
            }

            // load the default SLK Settings file into <defaultSettingsFileContents>
            string defaultSettingsFileContents = culture.Resources.SlkSettingsFile;

            // load the SLK Settings file to upload into <settingsFileContents> if specified by the parameters.
            string settingsFileContents = null;
            if (settingsFileName != null)
            {
                settingsFileContents = System.IO.File.ReadAllText(settingsFileName);
            }
            else if (defaultSlkSettings)
            {
                settingsFileContents = defaultSettingsFileContents;
            }

            // Save the configuration
            SlkAdministration.SaveConfiguration(id, databaseServer, databaseName,
                schemaFileContents, instructorPermission, learnerPermission, observerPermission, createPermissions,
                settingsFileContents, defaultSettingsFileContents, userName);

            if (forApplication)
            {
                output = FormatString(culture.Resources.StsConfiguringSlkForApplication, url, id);
            }
            else
            {
                output = FormatString(culture.Resources.StsConfiguringSlkForSite, url, id);
            }

            return 0;
        }

        string Value(string key)
        {
            if (keyValues.ContainsKey(key))
            {
                return keyValues[key];
            }
            else
            {
                return null;
            }
        }

        bool BooleanValue(string key)
        {
            if (keyValues.ContainsKey(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        string ValueIfMissing(string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }
#endregion private methods

#region static members
        static string FormatString(string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentUICulture, format, args);
        }

#endregion static members
    }
}

