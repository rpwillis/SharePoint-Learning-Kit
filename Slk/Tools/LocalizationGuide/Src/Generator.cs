using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.IO;
using System.Data;
using System.Globalization;

namespace SharePointLearningKit.Localization
{
    class Generator
    {
        Dictionary<AssemblyName, ResourceDataCollection> resources = new Dictionary<AssemblyName, ResourceDataCollection>();
        string slkKeyPath;

#region constructors
        public Generator()
        {
        }
#endregion constructors

#region properties
        /// <summary>The source file.</summary>
        public string Source {get; set; }
#endregion properties

#region public methods
        public void LoadXML()
        {
            LoadXML(Source);
        }

        public void LoadXML(string source)
        {
            if (File.Exists(source))
            {
                ProcessAssembly(source);
            }
            else if (Directory.Exists(source))
            {
                string[] files = Directory.GetFiles(source, "*.xml");

                foreach (string file in files)
                {
                    ProcessAssembly(file);
                }
            }
            else
            {
                Tools.Error("Invalid File name: " + source);
            }
        }

        public void ProcessAssembly(string source)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;

            XmlReader reader = null;
            try
            {
                reader = XmlReader.Create(source, settings);
            }
            catch (Exception e)
            {
                Tools.Error("Can't open file " + source, e.Message);
            }

            ResourceDataCollection data = new ResourceDataCollection();
            AssemblyName assemblyName = new AssemblyName();
            resources.Add(assemblyName, data);

            using (reader)
            {
                if (ReadHeader(assemblyName, reader))
                {
                    try
                    {
                        while (!reader.EOF)
                        {
                            reader.Read();
                            if (reader.Name == "NewDataSet")
                            {
                                string xml = reader.ReadOuterXml();
                                XmlDataDocument xmlDoc = new XmlDataDocument();
                                ResourceData.PrepareDataSet(xmlDoc.DataSet);
                                xmlDoc.LoadXml(xml);

                                ResourceData rd = new ResourceData();
                                rd.DataSet = xmlDoc.DataSet.Copy();
                                rd.ResourceName = rd.DataSet.Tables["MetaData"].Rows[0]["Name"].ToString();
                                rd.ResourceType = rd.DataSet.Tables["MetaData"].Rows[0]["Type"].ToString();

                                data.Add(rd);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Tools.Error("Corruption in resources XML", e.Message);
                    }
                }
            }
        }

        public void Save()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;

            foreach (KeyValuePair<AssemblyName, ResourceDataCollection> pair in resources)
            {
                AssemblyName assemblyName = pair.Key;
                ResourceDataCollection data = pair.Value;
                AssemblyBuilder asmBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.ReflectionOnly);

                string satelliteAssemblyFileName = assemblyName.Name + ".dll";

                ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(satelliteAssemblyFileName, satelliteAssemblyFileName);

                foreach (ResourceData rd in data)
                {

                    MemoryStream stream = new MemoryStream();
                    string resourceStreamExtension = Path.GetExtension(rd.ResourceName).ToLower();
                    if (resourceStreamExtension == ".resources")
                    {
                        ResourceWriter resourceWriter = new ResourceWriter(stream);
                        moduleBuilder.DefineManifestResource(LocResourceName(rd.ResourceName, assemblyName.CultureInfo.ToString()), stream, ResourceAttributes.Private);
                        foreach (DataRow dr in rd.DataSet.Tables["Resources"].Rows)
                        {
                            if (dr["Translation"].ToString() != dr["Source"].ToString())
                            {
                                resourceWriter.AddResource(dr["ID"].ToString(), dr["Translation"].ToString());
                            }
                        }
                        resourceWriter.Generate();
                    }

                    if ((resourceStreamExtension == ".xsd") || (resourceStreamExtension == ".xml"))
                    {
                        if (!Convert.IsDBNull(rd.DataSet.Tables["Resources"].Rows[0]["Translation"]))
                        {
                            TextWriter textWriter = new StreamWriter(stream);
                            moduleBuilder.DefineManifestResource(LocResourceName(rd.ResourceName, assemblyName.CultureInfo.ToString()), stream, ResourceAttributes.Private);
                            textWriter.Write(rd.DataSet.Tables["Resources"].Rows[0]["Translation"] as string);
                            textWriter.Flush();
                        }
                    }
                }

                asmBuilder.Save(satelliteAssemblyFileName);
            }
        }

        public string LocResourceName(string ResourceName, string Culture)
        {
            string ret;

            ret = ResourceName.Substring(0, ResourceName.Length - ".resources".Length);

            ret += "." + Culture + ".resources";

            return ret;
        }

#endregion public methods

#region private methods
        private bool ReadHeader(AssemblyName assemblyName, XmlReader reader)
        {
            try
            {
                bool done = false;
                while (!done && !reader.EOF)
                {
                    reader.Read();
                    if (reader.Name == "assembly")
                    {
                        assemblyName.CultureInfo = new CultureInfo(reader.GetAttribute("culture"));
                        assemblyName.Name = reader.GetAttribute("name");
                        assemblyName.Version = new Version(reader.GetAttribute("version"));
                        using (FileStream stream =File.Open(FindKeyFile(), FileMode.Open, FileAccess.Read)) 
                        {
                            assemblyName.KeyPair = new StrongNameKeyPair(stream);
                        }

                        done = true;
                    }
                }
            }
            catch (Exception e)
            {
                Tools.Error("Error reading XML header", e.Message);
                return false;
            }

            return true;
        }

        private string FindKeyFile()
        {
            if (slkKeyPath == null)
            {
                DirectoryInfo directory = Directory.GetParent(".");
                ProcessDirectory(directory);
                if (slkKeyPath == null)
                {
                    throw new InvalidOperationException("Cannot find SLK key file.");
                }
            }

            return slkKeyPath;
        }

        private void ProcessDirectory(DirectoryInfo directory)
        {
            if (directory != null)
            {
                if (CheckDirectory(directory))
                {
                    slkKeyPath = Path.Combine(directory.FullName, "LearningComponents\\Shared\\SlkKey.snk");
                }
                else
                {
                    ProcessDirectory(directory.Parent);
                }
            }
        }

        bool CheckDirectory(DirectoryInfo directory)
        {
            return (directory.GetDirectories("LearningComponents").Length == 1);
        }
#endregion private methods



 
    }
}
