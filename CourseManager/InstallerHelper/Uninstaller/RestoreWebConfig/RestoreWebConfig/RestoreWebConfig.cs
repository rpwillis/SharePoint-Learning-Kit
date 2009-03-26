using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;

namespace RestoreWebConfig
{
    class RestoreWebConfig
    {
        /// <summary>
        /// Main Method of the Restore Web Config
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine(UninstallerResources.strErrorCommandSyntax);
                System.Console.WriteLine(UninstallerResources.strSharepointNotInstalled);
                return 1;
            }
            else if(args.Length > 0)
            {
                SPSite site = null;
                try
                {
                    site = new SPSite(args[0].ToString());
                }
                catch (Exception)
                {
                    System.Console.WriteLine(UninstallerResources.strErrorOpeningWeb);
                    return 1;
                }

                DeleteAppSetting(args[0].ToString(), "WebSiteGUID",
                    new SPSite(args[0].ToString()).ID.ToString());

                DeleteNode(args[0].ToString(),
                    "*[local-name()='dependentAssembly'][*/@name='System.Web.Extensions.Design'][*/@publicKeyToken='31bf3856ad364e35'][*/@culture='neutral']",
                    "configuration/runtime/*[local-name()='assemblyBinding' and namespace-uri()='urn:schemas-microsoft-com:asm.v1']",
                    "<dependentAssembly><assemblyIdentity name='System.Web.Extensions.Design' publicKeyToken='31bf3856ad364e35' culture='neutral' /><bindingRedirect oldVersion='1.0.0.0-1.1.0.0' newVersion='3.5.0.0' /></dependentAssembly>",
                    2/*, Modific*/);
                DeleteNode(args[0].ToString(),
                    "*[local-name()='dependentAssembly'][*/@name='System.Web.Extensions'][*/@publicKeyToken='31bf3856ad364e35'][*/@culture='neutral']",
                    "configuration/runtime/*[local-name()='assemblyBinding' and namespace-uri()='urn:schemas-microsoft-com:asm.v1']",
                    "<dependentAssembly><assemblyIdentity name='System.Web.Extensions' publicKeyToken='31bf3856ad364e35' culture='neutral' /><bindingRedirect oldVersion='1.0.0.0-1.1.0.0' newVersion='3.5.0.0' /></dependentAssembly>",
                    2/*, Modific*/);

                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "System.Data.DataSetExtensions", "3.5.0.0", "neutral", "B77A5C561934E089");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "System.Web.Extensions", "3.5.0.0", "neutral", "31BF3856AD364E35");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "System.Core", "3.5.0.0", "neutral", "B77A5C561934E089");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "Axelerate.DataAccessApplicationBlock", "1.0.0.2", "neutral", "8AECA469E4966909");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "Axelerate.SlkCourseManagerLogicalLayer", "1.0.0.0", "neutral", "5BBD900FCDE291A4");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "Axelerate.BusinessLayerFrameWork", "1.3.0.1", "neutral", "783DBD9B30AFB604");
                DeleteAddAssemblies(args[0].ToString(), "configuration/system.web/compilation/assemblies",
                    "System.Xml.Linq", "3.5.0.0", "neutral", "B77A5C561934E089");

                DeleteControls(args[0].ToString(), "configuration/system.web/pages/controls",
                    "asp", "System.Web.UI", "System.Web.Extensions", "3.5.0.0", "neutral", "31BF3856AD364E35");
                DeleteControls(args[0].ToString(), "configuration/system.web/pages/controls",
                    "asp", "System.Web.UI.WebControls", "System.Web.Extensions", "3.5.0.0", "neutral",
                        "31BF3856AD364E35");

                DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                    "roleService",
                    "System.Web.Configuration.ScriptingRoleServiceSection, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
                DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                    "authenticationService",
                    "System.Web.Configuration.ScriptingAuthenticationServiceSection, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
                DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                    "profileService",
                    "System.Web.Configuration.ScriptingProfileServiceSection, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
                DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                    "jsonSerialization",
                    "System.Web.Configuration.ScriptingJsonSerializationSection, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "Everywhere");
                DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']",
                    "scriptResourceHandler",
                    "System.Web.Configuration.ScriptingScriptResourceHandlerSection, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31bf3856ad364e35", "false", "MachineToApplication");
                /*DeleteSection(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='System.Workflow.ComponentModel.WorkflowCompiler']",
                    "authorizedTypes",
                    "System.Workflow.ComponentModel.Compiler.AuthorizedTypesSectionHandler, System.Workflow.ComponentModel",
                    "3.0.0.0", "neutral", "31bf3856ad364e35", "false", "Everywhere");*/

                DeleteSectionGroup(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']",
                    "webServices",
                    "System.Web.Configuration.ScriptingWebServicesSectionGroup, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35");
                DeleteSectionGroup(args[0].ToString(),
                    "configuration/configSections/sectionGroup[@name='system.web.extensions']",
                    "scripting",
                    "System.Web.Configuration.ScriptingSectionGroup, System.Web.Extensions", "3.5.0.0",
                    "neutral", "31BF3856AD364E35");
                DeleteSectionGroup(args[0].ToString(),
                    "configuration/configSections",
                    "system.web.extensions",
                    "System.Web.Configuration.SystemWebExtensionsSectionGroup, System.Web.Extensions",
                    "3.5.0.0", "neutral", "31BF3856AD364E35");
                /*DeleteSectionGroup(args[0].ToString(),
                    "configuration/configSections",
                    "System.Workflow.ComponentModel.WorkflowCompiler",
                    "System.Workflow.ComponentModel.Compiler.WorkflowCompilerConfigurationSectionGroup, System.Workflow.ComponentModel",
                    "3.0.0.0", "neutral", "31bf3856ad364e35");*/

                DeleteSafeControl(args[0].ToString(), "System.Web.Extensions", "3.5.0.0", "neutral",
                    "31BF3856AD364E35", "System.Web.UI", "*", "True");

                DeleteHttpHandlers(args[0].ToString(), "GET,HEAD,POST", "*/ImgCmp.asmx",
                    "Axelerate.BusinessLayerUITools.ImageProcessing.ImgCmpHandler, Axelerate.BusinessLayerUITools",
                    "1.0.0.0", "neutral", "4f9a37917b02d1a2", "false");
                DeleteHttpHandlers(args[0].ToString(), "GET,HEAD,POST", "*/Images.asmx",
                    "Axelerate.BusinessLayerUITools.ImageProcessing.ImageHandler, Axelerate.BusinessLayerUITools",
                    "1.0.0.0", "neutral", "4f9a37917b02d1a2", "false");
                DeleteHttpHandlers(args[0].ToString(), "GET,HEAD", "ScriptResource.axd",
                    "System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions", "3.5.0.0", "neutral",
                    "31BF3856AD364E35", "false");


                DeleteAppSetting(args[0].ToString(), "EnableSecurity", "false");
                DeleteAppSetting(args[0].ToString(), "AzmanApplication", "MyApplication");
                DeleteAppSetting(args[0].ToString(), "AzmanStore", "msldap://CN=AZMAN DEMO2,CN=Program Data,DC=guayabo");
                DeleteAppSetting(args[0].ToString(), "ReportViewerMessages", "Microsoft.SharePoint.Portal.Analytics.UI.ReportViewerMessages, Microsoft.SharePoint.Portal, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
                DeleteAppSetting(args[0].ToString(), "FeedXsl1", "/Style Library/Xsl Style Sheets/Rss.xsl");
                DeleteAppSetting(args[0].ToString(), "FeedPageUrl", "/_layouts/feed.aspx?");
                DeleteAppSetting(args[0].ToString(), "FeedCacheTime", "300");
            }
            return 0;
        }

        /// <summary>
        /// Delete the HttpHandlers of Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the HttpHandler is going to be erased</param>
        /// <param name="Verb">Verb of the HttpHandler that is going to be erased</param>
        /// <param name="path">Path of the HttpHandler that is going to be erased</param>
        /// <param name="TypeName">Type Name of the HttpHandler that is going to be erased</param>
        /// <param name="TypeVersion">Version of the HttpHandler that is going to be erased</param>
        /// <param name="TypeCulture"> Culture of the HttpHandler that is going to be erased</param>
        /// <param name="TypeToken">Public Key Token of the HttpHandler that is going to be erased</param>
        /// <param name="Validate">Boolean that indicates the validate of the HttpHandler that is going to be erased</param>
        public static void DeleteHttpHandlers(string Direction, string Verb, string path, string TypeName,
            string TypeVersion, string TypeCulture, string TypeToken, string Validate)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@verb='" + Verb + "'][@path='" + path + "']", "configuration/system.web/httpHandlers");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add verb=\"" + Verb + "\" path=\"" + path + "\" type=\"" + TypeName + ", Version=" + TypeVersion + ", Culture=" + TypeCulture + ", PublicKeyToken=" + TypeToken + "\" validate=\"" + Validate + "\" />";
            modification.Sequence = 1;
            Modific.Remove(modification);
            webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            //SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Delete the Safe Controls that where configured to use with Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the SafeControls is going to be erased</param>
        /// <param name="Assembly">Assembly of the SafeControls section that is going to be erased</param>
        /// <param name="Version">Version of the SafeControls section that is going to be erased</param>
        /// <param name="Culture">Culture of the SafeControls section that is going to be erased</param>
        /// <param name="PublicToken">Public Key Token of the SafeControls section that is going to be erased</param>
        /// <param name="NameSpace">Name Space of the SafeControls section that is going to be erased</param>
        /// <param name="TypeName">Type Name of the SafeControls section that is going to be erased</param>
        /// <param name="Safe">Boolean that indicates if the Safe Control is safe</param>
        public static void DeleteSafeControl(string Direction, string Assembly, string Version,
            string Culture, string PublicToken, string NameSpace, string TypeName, string Safe)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("SafeControl[@Assembly='" +
                Assembly + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicToken + "']",
                "configuration/SharePoint/SafeControls");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<SafeControl Assembly=\"" + Assembly + ", Version=" + Version + ", Culture=" +
                Culture + ", PublicKeyToken=" + PublicToken + "\" Namespace=\"" + NameSpace + "\" TypeName=\"" +
                TypeName + "\" Safe=\"" + Safe + "\" />";
            modification.Sequence = 1;
            Modific.Remove(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Delete the Section Group section of the web.config that was used by Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the SectionGroup is going to be erased</param>
        /// <param name="SectionGroupName">Name of the location of the SectionGroup that is going to be erased</param>
        /// <param name="Name">Name of the SectionGroup that is going to be erased</param>
        /// <param name="Type">Type of the SectionGroup that is going to be erased</param>
        /// <param name="Version">Version of the SectionGroup that is going to be erased</param>
        /// <param name="Culture">Culture of the SectionGroup that is going to be erased</param>
        /// <param name="Publickeytoken">Public Key Token of the SectionGroup that is going to be erased</param>
        public static void DeleteSectionGroup(string Direction, string SectionGroupName, string Name, string Type,
            string Version, string Culture, string Publickeytoken)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("sectionGroup[@name='" + Name + "']", SectionGroupName);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<sectionGroup name=\"" + Name + "\" type=\"" + Type + ", Version=" + Version +
                ", Culture=" + Culture + ", PublicKeyToken=" + Publickeytoken + "\"></sectionGroup>";
            modification.Sequence = 1;
            Modific.Remove(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Delete the Section element of the web.config that was used by Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the Section is going to be erased</param>
        /// <param name="SectionGroupName">Name of the location of the Section that is going to be erased</param>
        /// <param name="Name">Name of the Section that is going to be erased</param>
        /// <param name="Type">Type of the Section that is going to be erased</param>
        /// <param name="Version">Version of the Section that is going to be erased</param>
        /// <param name="Culture">Culture of the Section that is going to be erased</param>
        /// <param name="Publickeytoken">Public Key Token of the Section that is going to be erased</param>
        /// <param name="RequirePermission">Boolean that indicates if the Section require permission</param>
        /// <param name="AllowDefinition">Boolean that indicates if the definitions are allowed</param>
        public static void DeleteSection(string Direction, string SectionGroupName, string Name, string Type,
            string Version, string Culture, string Publickeytoken, string RequirePermission, string AllowDefinition)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("section[@name='" + Name + "']", SectionGroupName);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<section name=\"" + Name + "\" type=\"" + Type + ", Version=" + Version +
                ", Culture=" + Culture + ", PublicKeyToken=" + Publickeytoken + "\" requirePermission=\"" +
                RequirePermission + "\" allowDefinition=\"" + AllowDefinition + "\"/>";
            modification.Sequence = 1;
            Modific.Remove(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Delete the Page Controls section of the web.config that was used by Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the Page Control is going to be erased</param>
        /// <param name="ControlGroup">Name of the location of the Page Control element that is going to be erased</param>
        /// <param name="Prefix">Prefix of the Page Control element that is going to be erased</param>
        /// <param name="NameSpace">NameSpace of the Page Control element that is going to be erased</param>
        /// <param name="Assembly">Assembly of the Page Control element that is going to be erased</param>
        /// <param name="Version">Version of the Page Control element that is going to be erased</param>
        /// <param name="Culture">Culture of the Page Control element that is going to be erased</param>
        /// <param name="Publickeytoken">Public Key Token of the Page Control element that is going to be erased</param>
        public static void DeleteControls(string Direction, string ControlGroup, string Prefix, string NameSpace,
            string Assembly, string Version, string Culture, string Publickeytoken)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@namespace='" + NameSpace + "']", ControlGroup);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add namespace=\"" + NameSpace + "\" tagPrefix=\"" + Prefix + "\" assembly=\"" + Assembly +
                ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + Publickeytoken + "\"/>";
            modification.Sequence = 1;
            Modific.Remove(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }


        /// <summary>
        /// Delete AddAssemblies section of the web.config that was used by Course Manager
        /// </summary>
        /// <param name="Direction">Site from where the AddAssemblies is going to be erased</param>
        /// <param name="AssemblyControlGroup">Name of the location of the AddAssemblies element that is going to be erased</param>
        /// <param name="Assembly">Assembly of the AddAssemblies element that is going to be erased</param>
        /// <param name="Version">Version of the AddAssemblies element that is going to be erased</param>
        /// <param name="Culture">Culture of the AddAssemblies element that is going to be erased</param>
        /// <param name="Publickeytoken">Public Key Token of the AddAssemblies element that is going to be erased</param>
        public static void DeleteAddAssemblies(string Direction, string AssemblyControlGroup, string Assembly,
            string Version, string Culture, string Publickeytoken)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification
                ("add[@assembly='" + Assembly + ", Version=" + Version + ", Culture=" +
                Culture + ", PublicKeyToken=" + Publickeytoken + "']", AssemblyControlGroup);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add assembly=\"" + Assembly + ", Version=" + Version + ", Culture=" +
                Culture + ", PublicKeyToken=" + Publickeytoken + "\"/>";
            modification.Sequence = 1;
            Modific.Remove(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Delete a node or Section Child, in the web.config File
        /// </summary>
        /// <param name="Direction">Site from where the node is going to be erased</param>
        /// <param name="ElementName">Element name of the node that is going to be erased</param>
        /// <param name="ElementPosition">Element Position of the node that is going to be erased</param>
        /// <param name="Value">Value of the node that is going to be erased</param>
        /// <param name="Sequence">Sequence of the node that is going to be erased</param>
        public static void DeleteNode(string Direction, string ElementName, string ElementPosition, 
            string Value, uint Sequence/*, System.Collections.ObjectModel.Collection<SPWebConfigModification> Modifications*/)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modifications = webApp.WebConfigModifications;

            SPWebConfigModification Modification = new SPWebConfigModification(ElementName, ElementPosition);
            Modification.Value = Value;
            Modification.Sequence = Sequence;
            Modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            Modification.Owner = "";
            Modifications.Remove(Modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();

        }

        /// <summary>
        /// Delete an Appsetting from the web.config
        /// </summary>
        /// <param name="Direction">Site from where the AppSettings is going to be erased</param>
        /// <param name="Key">Key of the AppSetting element that is going to be erased</param>
        /// <param name="Value">Value of the AppSetting element that is going to be erased</param>
        public static void DeleteAppSetting(string Direction, string Key, string Value)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            
            
            SPWebConfigModification modification = new SPWebConfigModification("add[@key='" + Key + "']", "configuration/appSettings");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add key=\"" + Key + "\" value=\"" + Value + "\" />";
            modification.Sequence = 1;
            Modific.Remove(modification);

            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

    }
}
