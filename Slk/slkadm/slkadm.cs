/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Program.cs
//
// Implementation of slkadm.exe.
//
// NOTE: Use "-opause" instead of "-o" to display "Press any key..." before quitting -- useful
// for F5 debugging.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.LearningComponents;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePointLearningKit;
using AdmResources = slkadm.Properties.Resources;

class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    ///
    /// <param name="args">Command-line parameters.</param>
    /// 
    /// <returns>
    /// Application exit code.
    /// </returns>
    ///
    static int Main(string[] args)
    {
        // execute command-line options; set <pause> to true if the command-line options specified
        // pausing before quitting
        bool pause = false;
        int exitCode;
        try
        {
            new Program(args, out pause);
            exitCode = 0;
        }
        catch (UsageException)
        {
            Console.Error.WriteLine(AdmResources.CommandLineError);
            Console.Error.WriteLine(AdmResources.AppUsage);
            exitCode = 1;
        }
        catch (SafeToDisplayException ex)
        {
            Console.Error.WriteLine(AdmResources.ErrorMessage, ex.Message);
            exitCode = 1;
        }
        catch (SlkNotConfiguredException ex)
        {
            Console.Error.WriteLine(AdmResources.ErrorMessage, ex.Message);
            exitCode = 1;
        }
#if false
        // sometimes SharePoint throws weird UnauthorizedAccessExceptions -- this code is
        // #if'd out so that "catch (Exception)" below can display the entire stack trace
        // to help the user diagnose the problem
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(AdmResources.ErrorMessage, ex.Message);
            exitCode = 1;
        }
#endif
        catch (Exception ex)
        {
            Console.Error.WriteLine(AdmResources.ErrorMessage, ex.ToString());
            exitCode = 1;
        }

        // if specified in command-line options, pause before quitting
        if (pause)
        {
            Console.Error.WriteLine(AdmResources.Pause);
            Console.ReadKey();
        }

        return exitCode;
    }

    /// <summary>
    /// Executes the application.
    /// </summary>
    ///
    /// <param name="args">Command-line parameters.</param>
    /// 
    /// <param name="pause">Set to <c>true</c> if the caller should pause and
    ///     display "Press any key..." before exiting.</param>
    /// 
    /// <returns>
    /// Application exit code.
    /// </returns>
    ///
    Program(string[] args, out bool pause)
    {
        // parse command-line parameters
        pause = false;
        System.Collections.IEnumerator argEnum = args.GetEnumerator();
        string argLower;
        switch (argLower = GetNextArg(argEnum).ToLower())
        {

        case "-o":
        case "-opause":

            // -opause is used for development: it displays "Press any key..." at end
            if (argLower == "-opause")
                pause = true;

            switch (GetNextArg(argEnum))
            {

            case "configuresite":

                ConfigureSite(argEnum);
                break;

            case "getsiteconfiguration":

                GetSiteConfiguration(argEnum);
                break;

            case "enummappings":

                EnumMappings(argEnum);
                break;

            case "deletemapping":

                DeleteMapping(argEnum);
                break;

            default:

                throw new UsageException();

            }
            break;

        case "-help":

            if (argEnum.MoveNext())
            {
                switch (((string) argEnum.Current).ToLower())
                {

                case "configuresite":

                    Console.Error.WriteLine(AdmResources.ConfigureSiteHelp);
                    break;

                case "getsiteconfiguration":

                    Console.Error.WriteLine(AdmResources.GetSiteConfigurationHelp);
                    break;

                case "enummappings":

                    Console.Error.WriteLine(AdmResources.EnumMappingsHelp);
                    break;

                case "deletemapping":

                    Console.Error.WriteLine(AdmResources.DeleteMappingHelp);
                    break;

                default:

                    throw new UsageException();

                }
            }
            else
                Console.Error.WriteLine(AdmResources.AppUsage);
            break;

        default:

            throw new UsageException();

        }

        // there should be no further command-line parameters
        if (argEnum.MoveNext())
            throw new UsageException();
    }

    /// <summary>
    /// Implements the "configuresite" operation.
    /// </summary>
    ///
    /// <param name="argEnum">Command-line argument enumerator.</param>
    ///
    void ConfigureSite(System.Collections.IEnumerator argEnum)
    {
        // parse parameters
        string url = null;
        string databaseServer = null;
        string databaseName = null;
        bool createDatabase = false;
        string instructorPermission = null;
        string learnerPermission = null;
        string observerPermission = null;
        bool createPermissions = false;
        string settingsFileName = null;
        bool defaultSlkSettings = false;
        while (argEnum.MoveNext())
        {
            switch (argEnum.Current.ToString().ToLower())
            {
            case "-url":
                url = GetNextArg(argEnum);
                break;
            case "-databaseserver":
                databaseServer = GetNextArg(argEnum);
                break;
            case "-databasename":
                databaseName = GetNextArg(argEnum);
                break;
            case "-createdatabase":
                createDatabase = true;
                break;
            case "-instructorpermission":
                instructorPermission = GetNextArg(argEnum);
                break;
            case "-learnerpermission":
                learnerPermission = GetNextArg(argEnum);
                break;
            case "-observerpermission":
                observerPermission = GetNextArg(argEnum);
                break;
            case "-createpermissions":
                createPermissions = true;
                break;
            case "-uploadslksettings":
                settingsFileName = GetNextArg(argEnum);
                break;
            case "-defaultslksettings":
                defaultSlkSettings = true;
                break;
            default:
                Console.Error.WriteLine(AdmResources.CommandLineError);
                Console.Error.WriteLine(AdmResources.ConfigureSiteHelp);
                return;
            }
        }

        // check command-line usage:
        //   -- -url is required;
        //   -- can't specify both -uploadslksettings and -defaultslksettings
        if ((url == null) ||
            ((settingsFileName != null) && defaultSlkSettings))
        {
            Console.Error.WriteLine(AdmResources.CommandLineError);
            Console.Error.WriteLine(AdmResources.ConfigureSiteHelp);
            return;
        }

        // perform the operation
        using (SPSite spSite = new SPSite(url))
        {
            AdministrationConfiguration configuration = SlkAdministration.LoadConfiguration(spSite.ID);
            if (databaseServer == null)
            {
                databaseServer = configuration.DatabaseServer;
            }

            if (databaseName == null)
            {
                databaseName = configuration.DatabaseName;
            }

            if (instructorPermission == null)
            {
                instructorPermission = configuration.InstructorPermission;
            }

            if (learnerPermission == null)
            {
                learnerPermission = configuration.LearnerPermission;
            }

            if (observerPermission == null)
            {
                observerPermission = configuration.ObserverPermission;
            }



            // set <exeDirPath> to the full path of the directory containing this .exe
            string exeDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // load SlkSchema.sql if the parameters specify creating the database;
            // SlkSchema.sql is in the same directory as this .exe file
            string schemaFileContents;
            if (createDatabase)
                schemaFileContents = File.ReadAllText(Path.Combine(exeDirPath, "SlkSchema.sql"));
            else
                schemaFileContents = null;

            // load the default SLK Settings file into <defaultSettingsFileContents>
            string defaultSettingsFileContents =
                File.ReadAllText(Path.Combine(exeDirPath, "SlkSettings.xml"));

            // load the SLK Settings file to upload into <settingsFileContents> if specified by the
            // parameters
            string settingsFileContents;
            if (settingsFileName != null)
                settingsFileContents = File.ReadAllText(settingsFileName);
            else
            if (defaultSlkSettings)
                settingsFileContents = defaultSettingsFileContents;
            else
                settingsFileContents = null;

            // save the SLK configuration for this SPSite
            Console.WriteLine(AdmResources.ConfiguringSlk, spSite.Url, spSite.ID);
            SlkAdministration.SaveConfiguration(spSite.ID, databaseServer, databaseName,
                schemaFileContents, instructorPermission, learnerPermission, observerPermission, createPermissions,
                settingsFileContents, defaultSettingsFileContents,
                spSite.WebApplication.ApplicationPool.Username);
        }
    }

    /// <summary>
    /// Implements the "getsiteconfiguration" operation.
    /// </summary>
    ///
    /// <param name="argEnum">Command-line argument enumerator.</param>
    ///
    void GetSiteConfiguration(System.Collections.IEnumerator argEnum)
    {
        // parse parameters
        string url = null;
        while (argEnum.MoveNext())
        {
            switch (argEnum.Current.ToString().ToLower())
            {
            case "-url":
                url = GetNextArg(argEnum);
                break;
            default:
                Console.Error.WriteLine(AdmResources.CommandLineError);
                Console.Error.WriteLine(AdmResources.GetSiteConfigurationHelp);
                return;
            }
        }

        // check command-line usage:
        //   -- -url is required;
        if (url == null)
        {
            Console.Error.WriteLine(AdmResources.CommandLineError);
            Console.Error.WriteLine(AdmResources.GetSiteConfigurationHelp);
            return;
        }

        // perform the operation
        using (SPSite spSite = new SPSite(url))
        {
            AdministrationConfiguration configuration = SlkAdministration.LoadConfiguration(spSite.ID);
            if (configuration.IsNewConfiguration)
            {
                Console.WriteLine(AdmResources.SiteConfig_ConfigNotFound, spSite.Url, spSite.ID);
            }
            else
            {
                Console.WriteLine(AdmResources.SiteConfig_FoundConfig, spSite.Url, spSite.ID);
            }
            Console.WriteLine(AdmResources.SiteConfig_DatabaseServer, configuration.DatabaseServer);
            Console.WriteLine(AdmResources.SiteConfig_DatabaseName, configuration.DatabaseName);
            Console.WriteLine(AdmResources.SiteConfig_CreateDatabase, configuration.CreateDatabase);
            Console.WriteLine(AdmResources.SiteConfig_InstructorPermission, configuration.InstructorPermission);
            Console.WriteLine(AdmResources.SiteConfig_LearnerPermission, configuration.LearnerPermission);
            Console.WriteLine(AdmResources.SiteConfig_CreatePermissions, configuration.CreatePermissions);
        }
    }

    /// <summary>
    /// Implements the "enummappings" operation.
    /// </summary>
    ///
    /// <param name="argEnum">Command-line argument enumerator.</param>
    ///
    void EnumMappings(System.Collections.IEnumerator argEnum)
    {
        // parse parameters
        while (argEnum.MoveNext())
        {
            switch (argEnum.Current.ToString().ToLower())
            {
            // no parameters defined
            default:
                throw new UsageException();
            }
        }

        // enumerate mappings
        foreach (SlkSPSiteMapping mapping in SlkSPSiteMapping.GetMappings())
        {
            // set <siteLabel> to a label representing the site in <mapping>
            string siteLabel;
            try
            {
                using (SPSite spSite = new SPSite(mapping.SPSiteGuid))
                    siteLabel = spSite.Url;
            }
            catch (FileNotFoundException)
            {
                siteLabel = AdmResources.SiteNotFoundLabel;
            }

            // display mapping information
            Console.WriteLine("{0} ({1}) --> Server={2};Database={3}", siteLabel,
                mapping.SPSiteGuid, mapping.DatabaseServer, mapping.DatabaseName);
        }
    }

    /// <summary>
    /// Implements the "deletemapping" operation.
    /// </summary>
    ///
    /// <param name="argEnum">Command-line argument enumerator.</param>
    ///
    void DeleteMapping(System.Collections.IEnumerator argEnum)
    {
        // parse parameters
        Guid? guid = null;
        while (argEnum.MoveNext())
        {
            switch (argEnum.Current.ToString().ToLower())
            {
            case "-guid":
                guid = new Guid(GetNextArg(argEnum));
                break;
            default:
                Console.Error.WriteLine(AdmResources.CommandLineError);
                Console.Error.WriteLine(AdmResources.DeleteMappingHelp);
                return;
            }
        }

        // check command-line usage:
        //   -- -guid is required;
        if (guid == null)
        {
            Console.Error.WriteLine(AdmResources.CommandLineError);
            Console.Error.WriteLine(AdmResources.DeleteMappingHelp);
            return;
        }

        // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
        // information for that SPSite
        SlkSPSiteMapping mapping= SlkSPSiteMapping.GetMapping(guid.Value);
        if (mapping == null)
        {
            throw new Exception(String.Format(AdmResources.MappingNotFound, guid));
        }

        // delete the mapping
        mapping.Delete();
        Console.WriteLine(AdmResources.MappingDeleted, guid);
    }

    /// <summary>
    /// Returns the next command-line argument.
    /// </summary>
    ///
    /// <param name="argEnum">Command-line argument enumerator.</param>
    ///
    string GetNextArg(System.Collections.IEnumerator argEnum)
    {
        if (!argEnum.MoveNext())
            throw new UsageException();
        return (string)argEnum.Current;
    }
}

/// <summary>
/// Indicates a command-line usage exception.
/// </summary>
///
class UsageException : Exception
{
}

