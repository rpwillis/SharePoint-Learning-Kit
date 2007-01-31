/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Resources;
using System.Xml;


namespace Loc
{
    class Extractor
    {
        private string _assemblyPath = "";

        private AssemblyName _assemblyName;

        ResourceDataCollection _Data;

        public Extractor()
        {
            Init();
        }

        public Extractor(string AssemblyPath)
        {

            Init();
            _assemblyPath = AssemblyPath;
        }

        private void Init()
        {
            _Data = new ResourceDataCollection();
        }

        public string AssemblyPath
        {
            get
            {
                return _assemblyPath;
            }
            set
            {
                _assemblyPath = value;
            }
        }

        public void Extract(string AssemblyPath)
        {
            _assemblyPath = AssemblyPath;
            Extract();
        }
        

        public void Extract()
        {
            string fullFileName = "";
            try
            {
                fullFileName = Path.GetFullPath(_assemblyPath);
            }
            catch (Exception e)
            {
                Tools.Error("Invalid File name: " + _assemblyPath, e.Message);
            }
            Assembly assembly = null;
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(fullFileName);
            }
            catch (Exception e)
            {
                Tools.Error("Can't open assembly: " + _assemblyPath,e.Message);
            }
            _assemblyName = assembly.GetName();
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
                            rd.Add(dictionaryEnumerator.Key.ToString(), dictionaryEnumerator.Value.ToString());
                        }

                    }

                    _Data.Add(rd);
                }

                if ((resourceStreamExtension == ".xsd") || (resourceStreamExtension == ".xml"))
                {
                    ResourceData rd = new ResourceData(ResourceName, ResourceTypes.XML.ToString());
                    TextReader textReader = new StreamReader(ResourceStream);
                    rd.Add(ResourceName, textReader.ReadToEnd());
                    _Data.Add(rd);
                }

            }
            
        }

        public void Save(string XMLFile)
        {
            XmlTextWriter writer = new XmlTextWriter(XMLFile, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartElement("root");
            writer.WriteStartElement("assembly");
            writer.WriteAttributeString("culture", _assemblyName.CultureInfo.ToString());
            writer.WriteAttributeString("name", _assemblyName.Name + ".resources");
            writer.WriteAttributeString("version", _assemblyName.Version.ToString());
            writer.WriteStartElement("key");
            writer.WriteValue(_assemblyName.GetPublicKey());
            writer.WriteEndElement();
            writer.WriteStartElement("keytoken");
            writer.WriteValue(_assemblyName.GetPublicKeyToken());
            writer.WriteEndElement();
            writer.WriteEndElement();
            foreach (ResourceData rd in _Data)
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
