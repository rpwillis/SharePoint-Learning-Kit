using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Configuration;
using Microsoft.Win32;

namespace InstallerHelper
{
    public class InstallerHelperWebConf
    {

        /// <summary>
        /// Add to the Web.config the machine's id
        /// </summary>
        /// <param name="pName">Machine ID</param>
        /// <param name="pConnectionString">Connection String</param>
        /// <param name="pProviderName">Provider</param>
        /// <returns></returns>
        public static ConnectionStringSettings SetConnectionString(
            string pName, string pConnectionString, string pProviderName)
        {
            RegistryKey masterKey = Registry.LocalMachine.CreateSubKey(InstallerHelperResource.Connection_RegistrySettingsLocation);
            if (masterKey == null) {
                throw new Exception(InstallerHelperResource.Connection_ExceptionSubKeyCreation);
            }
            else {
                try
                {
                    masterKey.SetValue(pName, pConnectionString);
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
                finally {
                    masterKey.Close();
                }
            }
            return new ConnectionStringSettings(pName, pConnectionString, InstallerHelperResource.Connection_DataProvider);
        }



        /// <summary>
        /// Add and Edit new elements in the ConnectionString section, in the web.config File.
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="Name">Name of the Connection String</param>
        /// <param name="DataSource">Data source of the Connection String</param>
        /// <param name="InitialCatalog">Data Base of the Connection String</param>
        /// <param name="User">User of the Connection String</param>
        /// <param name="Pwd">PassWord of the Connection String</param>
        /// <param name="TimeOut">TimeOut of the Connection String</param>
        /// <param name="ProviderName">Provider of the Connection String</param>
        public static void EditConnString(string Direction, string Name, string DataSource, string InitialCatalog,
            string User, string Pwd, string TimeOut, string ProviderName)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@name='" + Name + "']", "configuration/connectionStrings");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add name=\"" + Name + "\" connectionString=\"Data Source=" + DataSource + ";Initial Catalog=" + InitialCatalog + ";User ID=" + User + ";Password=" + Pwd + ";Timeout=" + TimeOut + "\" providerName=\"" + ProviderName + "\" />";
            modification.Sequence = 1;
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add and Edit new elements in the ConnectionString section with Integrated Security
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="Name">Name of the Connection String</param>
        /// <param name="DataSource">Data Source string of the connection string element</param>
        /// <param name="InitialCatalog">Initial Catalog of the connection string element</param>
        /// <param name="TimeOut">Time Out of the connection string element</param>
        /// <param name="ProviderName">Provider Name of the connection string element</param>
        public static void EditConnStringIntegratedSecurity(string Direction, string Name, string DataSource,
            string InitialCatalog, string TimeOut, string ProviderName)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@name='" + Name + "']", "configuration/connectionStrings");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add name=\"" + Name + "\" connectionString=\"Data Source=" + DataSource + ";Initial Catalog=" + InitialCatalog + ";Integrated Security=SSPI;Timeout=" + TimeOut + "\" providerName=\"" + ProviderName + "\" />";
            modification.Sequence = 1;
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Create a new Section in the web.config File.
        /// </summary>
        /// <param name="SectionName">Name of the new section</param>
        /// <param name="SectionPosition">Position or Section in the web.config file, where the new section will be added</param>
        /// <param name="Sequence">Insertion order of the modification</param>
        /// <param name="Modifications">Collection with all the modifications</param>
        public static void CreateSection(string SectionName, string SectionPosition, uint Sequence, System.Collections.ObjectModel.Collection<SPWebConfigModification> Modifications)
        {
            SPWebConfigModification Modification = new SPWebConfigModification(SectionName, SectionPosition);
            Modification.Value = "<" + SectionName + "></" + SectionName + ">";
            Modification.Sequence = Sequence;
            Modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureSection;
            Modification.Owner = "";
            Modifications.Add(Modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();

        }

        /// <summary>
        /// Delete de Web Config Modification Cache
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        public static void DeleteWebConfigModificationsCache(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            Modific.Clear();
            webApp.Update();
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
        }

        /// <summary>
        /// Add and Edit new elements in the AppSetting section, in the web.config File.
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="Key">Key of the new AppSetting element</param>
        /// <param name="Value">Value of the new AppSetting element</param>
        public static void EditAppSetting(string Direction, string Key, string Value)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@key='" + Key + "']", "configuration/appSettings");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add key=\"" + Key + "\" value=\"" + Value + "\" />";
            modification.Sequence = 1;
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add and Edit new elements in the HttpHandlers section
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="Verb">The verb of the HttpHandler element</param>
        /// <param name="path">Path of the HttpHandler element</param>
        /// <param name="TypeName">Type of the HttpHandler element</param>
        /// <param name="TypeVersion">Version of the HttpHandler element</param>
        /// <param name="TypeCulture">Culture of the HttpHandler element</param>
        /// <param name="TypeToken">PublicKeyToken of the HttpHandler element</param>
        /// <param name="Validate">A boolean indicating the validation state of the HttpHandler element</param>
        public static void EditHttpHandlers(string Direction, string Verb, string path, string TypeName,
            string TypeVersion, string TypeCulture, string TypeToken, string Validate)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@verb='" + Verb + "'][@path='" + path + "']", "configuration/system.web/httpHandlers");
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add verb=\"" + Verb + "\" path=\"" + path + "\" type=\"" + TypeName + ", Version=" + TypeVersion + ", Culture=" + TypeCulture + ", PublicKeyToken=" + TypeToken + "\" validate=\"" + Validate + "\" />";
            modification.Sequence = 1;
            Modific.Add(modification);
            webApp.Farm.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            //SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add a SafeControl Tag to the web.config
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="Assembly">Assembly of the SafeControl element</param>
        /// <param name="Version">Version of the SafeControl element</param>
        /// <param name="Culture">Culture of the element</param>
        /// <param name="PublicToken">Public Token of the SafeControl element</param>
        /// <param name="NameSpace">Namespace of the SafeControl element</param>
        /// <param name="TypeName">TypeName of the SafeControl element</param>
        /// <param name="Safe">A boolean that indicates if the SafeControl element is safe</param>
        public static void EditSafeControl(string Direction, string Assembly, string Version,
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
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add a SectionGroup to the web.config
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="SectionGroupName">The name of the Section Group where the element is going to be placed</param>
        /// <param name="name">Name of the Section Group</param>
        /// <param name="type">Type of the Section Group</param>
        /// <param name="version">Version of the Section Group</param>
        /// <param name="culture">Culture of the Section Group</param>
        /// <param name="publickeytoken">Public Key Token of the Section Group</param>
        public static void EditSectionGroup(string Direction, string SectionGroupName, string Name, string Type,
            string Version, string Culture, string Publickeytoken)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("sectionGroup[@name='" + Name + "']", SectionGroupName);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<sectionGroup name=\"" + Name + "\" type=\"" + Type + ", Version=" + Version +
                ", Culture=" + Culture + ", PublicKeyToken=" + Publickeytoken + "\"></sectionGroup>";
            modification.Sequence = 1;
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add a Section to the web.config
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="SectionGroupName">Name of the Section Group where the element is going to be placed</param>
        /// <param name="name">Name of the Section element</param>
        /// <param name="type">Type of the Section element</param>
        /// <param name="version">Version of the Section element</param>
        /// <param name="culture">Culture of the Section element</param>
        /// <param name="publickeytoken">Public Key Token of the Section element</param>
        public static void EditSection(string Direction, string SectionGroupName, string Name, string Type,
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
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add a System.web/pages/controls section to the web.config
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="ControlGroup">Section of the web.config were the Control element will be installed</param>
        /// <param name="Prefix">Prefix of the Control element</param>
        /// <param name="NameSpace">Namespace of the Control element</param>
        /// <param name="Assembly">Assembly of the Control element</param>
        /// <param name="Version">Version of the Control element</param>
        /// <param name="Culture">Culture of the Control element</param>
        /// <param name="Publickeytoken">Public Key Token of the Control element</param>
        public static void EditControls(string Direction, string ControlGroup, string Prefix, string NameSpace,
            string Assembly, string Version, string Culture, string Publickeytoken)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            SPWebConfigModification modification = new SPWebConfigModification("add[@namespace='" + NameSpace + "']", ControlGroup);
            modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            modification.Value = "<add namespace=\"" + NameSpace + "\" tagPrefix=\"" + Prefix + "\" assembly=\"" + Assembly +
                ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + Publickeytoken + "\"/>";
            modification.Sequence = 1;
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Add a System.web/compilation/assemblies to the web.config
        /// </summary>
        /// <param name="Direction">A string that specifies the absolute URL for the site Collection</param>
        /// <param name="AssemblyControlGroup">Section of the Web.config where assemblies is goint to be installed</param>
        /// <param name="Assembly">Name of the Assembly element</param>
        /// <param name="Version">Version of the Assembly element</param>
        /// <param name="Culture">Culture of the Assembly element</param>
        /// <param name="Publickeytoken">Public Key Token of the Assembly element</param>
        public static void EditAddAssemblies(string Direction, string AssemblyControlGroup, string Assembly,
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
            Modific.Add(modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();
        }

        /// <summary>
        /// Create a new Node or Section Child, in the web.config File
        /// </summary>
        /// <param name="ElementName">Name of the new Node</param>
        /// <param name="ElementPosition">Position or Section in the web.config file, where the new Node will be added</param>
        /// <param name="Value">Value of the new Node</param>
        /// <param name="Sequence">Insertion order of the modification</param>
        /// <param name="Modifications">Collection with all the modifications</param>
        public static void EditNode(string ElementName, string ElementPosition, string Value, uint Sequence, System.Collections.ObjectModel.Collection<SPWebConfigModification> Modifications)
        {
            SPWebConfigModification Modification = new SPWebConfigModification(ElementName, ElementPosition);
            Modification.Value = Value;
            Modification.Sequence = Sequence;
            Modification.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            Modification.Owner = "";
            Modifications.Add(Modification);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();

        }


    }
}
