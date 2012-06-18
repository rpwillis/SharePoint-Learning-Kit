/* Copyright (c) Microsoft Corporation. All rights reserved. */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Resources;
using System.Xml;
using System.Xml.XPath;


namespace SharePointLearningKit.Localization
{
    class Extractor
    {
        Dictionary<AssemblyName, ResourceDataCollection> resources = new Dictionary<AssemblyName, ResourceDataCollection>();
        List<string> crucial = new List<string>();
        List<string> dontLocalize = new List<string>();
        string culture;
        string fileCulture;

#region constructors
        public Extractor()
        {
            LoadSupportingFiles();
        }

        public Extractor(string assemblyPath) : this()
        {
            AssemblyPath = assemblyPath;
        }
#endregion constructors

#region properties
        public string AssemblyPath { get; set; }
#endregion properties

#region public methods
        public void Extract(string assemblyPath)
        {
            AssemblyPath = assemblyPath;
            Extract();
        }

        public void Extract()
        {
            if (File.Exists(AssemblyPath))
            {
                ProcessAssembly(AssemblyPath);
            }
            else if (Directory.Exists(AssemblyPath))
            {
                string[] files = Directory.GetFiles(AssemblyPath, "*.dll");

                foreach (string file in files)
                {
                    ProcessAssembly(file);
                }

            }
            else
            {
                Tools.Error("Invalid File name: " + AssemblyPath);
            }
        }

        void ProcessAssembly (string path)
        {
            string fullFileName = Path.GetFullPath(path);
            Console.WriteLine("Processing {0}", fullFileName);
            Assembly assembly = null;
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(fullFileName);
            }
            catch (Exception e)
            {
                Tools.Error("Can't open assembly: " + AssemblyPath,e.Message);
            }

            AssemblyName assemblyName = assembly.GetName();
            ResourceDataCollection data = new ResourceDataCollection();
            resources.Add(assemblyName, data);

            string[] ResourceNames = assembly.GetManifestResourceNames();

            foreach (string ResourceName in ResourceNames)
            {
                Stream ResourceStream = assembly.GetManifestResourceStream(ResourceName);

                string resourceStreamExtension = Path.GetExtension(ResourceName).ToLower();

                if (resourceStreamExtension == ".resources")
                {
                    ResourceReader resourceReader = new ResourceReader(ResourceStream);
                    ResourceData rd = new ResourceData(ResourceName, ResourceTypes.StringTable.ToString());

                    IDictionaryEnumerator dictionaryEnumerator = resourceReader.GetEnumerator();
                    while (dictionaryEnumerator.MoveNext())
                    {
                        bool extract = dictionaryEnumerator.Value is string;
                        if (extract)
                        {
                            string id = dictionaryEnumerator.Key.ToString();

                            if (dontLocalize.Contains(id) == false)
                            {
                                rd.Add(id, dictionaryEnumerator.Value.ToString(), crucial.Contains(id));
                            }
                        }
                    }

                    data.Add(rd);
                }

                if ((resourceStreamExtension == ".xsd") || (resourceStreamExtension == ".xml"))
                {
                    ResourceData rd = new ResourceData(ResourceName, ResourceTypes.XML.ToString());
                    TextReader textReader = new StreamReader(ResourceStream);
                    if (dontLocalize.Contains(ResourceName) == false)
                    {
                        rd.Add(ResourceName, textReader.ReadToEnd(), crucial.Contains(ResourceName));
                    }
                    data.Add(rd);
                }
            }
        }

        public void Save(string outputDirectory)
        {
            if (Directory.Exists(outputDirectory) == false)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "Invalid output directory {0}", outputDirectory));
            }

            string cultureFile = Path.Combine(outputDirectory, "culture.txt");
            if (File.Exists(cultureFile))
            {
                using (StreamReader reader = new StreamReader(cultureFile))
                {
                    culture = reader.ReadToEnd();
                }
            }

            foreach (KeyValuePair<AssemblyName, ResourceDataCollection> pair in resources)
            {
                AssemblyName assemblyName = pair.Key;
                ResourceDataCollection data = pair.Value;

                string fileName = Path.Combine(outputDirectory, assemblyName.Name + ".xml");

                InsertExistingTranslations(fileName, data);

                using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartElement("root");
                    writer.WriteStartElement("assembly");
                    writer.WriteAttributeString("culture", string.IsNullOrEmpty(fileCulture) ? culture : fileCulture);
                    writer.WriteAttributeString("name", assemblyName.Name + ".resources");
                    writer.WriteAttributeString("version", assemblyName.Version.ToString());
                    writer.WriteEndElement();

                    foreach (ResourceData rd in data)
                    {
                        writer.WriteStartElement("resources");
                        writer.WriteAttributeString("name",rd.ResourceName);
                        writer.WriteAttributeString("type", rd.ResourceType);
                        XmlDataDocument xml = new XmlDataDocument(rd.DataSet);
                        xml.Save(writer);
                        writer.WriteEndElement();

                    }

                    writer.WriteEndElement();
                    writer.Close();
                }
            }
        }
#endregion public methods

#region private methods
        void InsertExistingTranslations(string fileName, ResourceDataCollection data)
        {
            if (File.Exists(fileName))
            {
                XPathDocument document = new XPathDocument(fileName);
                XPathNavigator navigator = document.CreateNavigator();

                XPathNodeIterator iterator = navigator.Select("/root/resources");

                fileCulture = string.Empty;
                fileCulture = (string)navigator.Evaluate("string(/root/assembly/@culture)");

                while (iterator.MoveNext())
                {
                    string name = iterator.Current.GetAttribute("name", string.Empty);
                    ResourceData resource = data[name];
                    if (resource != null)
                    {
                        XPathNodeIterator resourceIterator = iterator.Current.Select("NewDataSet/Resources");

                        while (resourceIterator.MoveNext())
                        {
                            string id = (string)resourceIterator.Current.Evaluate("string(ID)");
                            string translation = (string)resourceIterator.Current.Evaluate("string(Translation)");
                            string source = (string)resourceIterator.Current.Evaluate("string(Source)");
                            if (translation != source)
                            {
                                resource.SetTranslation(id, translation);
                            }
                        }
                    }
                }
            }
        }

        void LoadSupportingFiles()
        {
            LoadSupportingFile("crucialStrings.txt", crucial);
            LoadSupportingFile("dontLocalize.txt", dontLocalize);
        }

        void LoadSupportingFile(string fileName, List<string> collection)
        {
            string path = Path.Combine("etc", fileName);
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        collection.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(string.Format("Could not load file {0}. [{1}]", path, e.Message));
            }
        }
#endregion private methods
    }
}
