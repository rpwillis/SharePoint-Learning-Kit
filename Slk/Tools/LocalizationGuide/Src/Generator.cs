/* Copyright (c) Microsoft Corporation. All rights reserved. */

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

namespace Loc
{
    class Generator
    {
        private ResourceDataCollection _data;
        private string _source;
        private AssemblyName _assemblyName;

        public Generator()
        {
            Init();
        }

        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        private void Init()
        {
            _data = new ResourceDataCollection();
            _assemblyName = new AssemblyName();
        }

        public void LoadXML()
        {
            LoadXML(_source);
        }

        public void LoadXML(string Source)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;

            XmlReader reader = null;
            try
            {
                reader = XmlReader.Create(Source, settings);
            }
            catch (Exception e)
            {
                Tools.Error("Can't open file " + Source, e.Message);
            }
            if (ReadHeader(reader))
            {
                try
                {
                    while (!reader.EOF)
                    {
                        reader.Read();
                        if (reader.Name == "NewDataSet")
                        {
                            string Xml = reader.ReadOuterXml();
                            // Xml = "<NewDataSet>" + Xml + "</NewDataSet>";
                            XmlDataDocument xmlDoc = new XmlDataDocument();
                            ResourceData.PrepareDataSet(xmlDoc.DataSet);
                            xmlDoc.LoadXml(Xml);

                            ResourceData rd = new ResourceData();
                            rd.DataSet = xmlDoc.DataSet.Copy();
                            rd.ResourceName = rd.DataSet.Tables["MetaData"].Rows[0]["Name"].ToString();
                            rd.ResourceType = rd.DataSet.Tables["MetaData"].Rows[0]["Type"].ToString();

                            _data.Add(rd);
                        }
                    }
                }
                catch (Exception e)
                {
                    Tools.Error("Corruption in resources XML", e.Message);
                }
            }



        }

        private bool ReadHeader(XmlReader reader)
        {
            try
            {
                bool done = false;
                while (!done && !reader.EOF)
                {
                    reader.Read();
                    if (reader.Name == "assembly")
                    {
                        _assemblyName.CultureInfo = new CultureInfo(reader.GetAttribute("culture"));
                        _assemblyName.Name = reader.GetAttribute("name");
                        _assemblyName.Version = new Version(reader.GetAttribute("version"));
                        _assemblyName.KeyPair = new StrongNameKeyPair(File.Open("..\\..\\..\\..\\src\\Shared\\SlkKey.snk", FileMode.Open, FileAccess.Read));
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



        public void Save()
        {

            AppDomain appDomain = System.Threading.Thread.GetDomain();

            AssemblyBuilder asmBuilder = appDomain.DefineDynamicAssembly(_assemblyName,
                AssemblyBuilderAccess.ReflectionOnly);

            string satteliteAssemblyFileName = _assemblyName.Name;
            satteliteAssemblyFileName += ".dll";

            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(satteliteAssemblyFileName, satteliteAssemblyFileName);

            foreach (ResourceData rd in _data)
            {

                MemoryStream stream = new MemoryStream();
                string resourceStreamExtension = Path.GetExtension(rd.ResourceName).ToLower();
                if (resourceStreamExtension == ".resources")
                {
                    ResourceWriter resourceWriter = new ResourceWriter(stream);
                    moduleBuilder.DefineManifestResource(LocResourceName(rd.ResourceName, _assemblyName.CultureInfo.ToString()), stream, ResourceAttributes.Private);
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
                        moduleBuilder.DefineManifestResource(LocResourceName(rd.ResourceName, _assemblyName.CultureInfo.ToString()), stream, ResourceAttributes.Private);
                        textWriter.Write(rd.DataSet.Tables["Resources"].Rows[0]["Translation"] as string);
                        textWriter.Flush();
                    }
                }



            }

            asmBuilder.Save(satteliteAssemblyFileName);
        }

        public string LocResourceName(string ResourceName, string Culture)
        {
            string ret;

            ret = ResourceName.Substring(0, ResourceName.Length - ".resources".Length);

            ret += "." + Culture + ".resources";

            return ret;
        }

 
    }
}
