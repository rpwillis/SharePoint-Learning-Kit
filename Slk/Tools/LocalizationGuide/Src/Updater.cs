using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SharePointLearningKit.Localization
{
    class Updater
    {
        string newExtractFile;
        XmlDocument document;
        XPathNavigator newExtract;

#region constructors
        public Updater(string newExtractFile)
        {
            if (string.IsNullOrEmpty(newExtractFile))
            {
                throw new ArgumentOutOfRangeException("new Extract File is required.");
            }

            this.newExtractFile = newExtractFile;
        }
#endregion constructors

#region public methods
        public void Merge(string source, string target)
        {
            document = new XmlDocument();
            document.Load(source);

            XPathDocument newDocument = new XPathDocument(newExtractFile);
            newExtract = newDocument.CreateNavigator();
            Merge();

            document.Save(target);
        }

        public void Save(string destination)
        {
            document.Save(destination);
        }
#endregion public methods

#region private methods
        private void Merge()
        {
            XPathNodeIterator resources = newExtract.Select("/root/resources");

            while (resources.MoveNext())
            {
                XPathNavigator newResources = resources.Current;
                string name = (string)newResources.Evaluate("string(@name)");
                if (string.IsNullOrEmpty(name) == false)
                {
                    XmlElement currentResources = (XmlElement)document.SelectSingleNode("/root/resources[@name='" + name + "']");
                    if (currentResources == null)
                    {
                        AddResources(newResources);
                    }
                    else
                    {
                        MergeResourses(currentResources, newResources);
                    }
                }
            }
        }

        private void MergeResourses(XmlElement currentResources, XPathNavigator newResources)
        {
            Dictionary<string, Resource> currentItems = LoadResources(currentResources);
            Dictionary<string, Resource> newItems = LoadResources(newResources);
            XPathNavigator dataSet = currentResources.SelectSingleNode("NewDataSet").CreateNavigator();

            foreach (KeyValuePair<string, Resource> newItem in newItems)
            {
                Resource current = null;
                Resource newResource = newItem.Value;
                if (currentItems.TryGetValue(newItem.Key, out current))
                {
                    // Already in source xml. Update the crucial if required, otherwise leave.
                    if (current.Crucial != newResource.Crucial)
                    {
                        XmlElement crucial = (XmlElement)current.Element.SelectSingleNode("Crucial");
                        if (crucial != null)
                        {
                            crucial.InnerText = newResource.Crucial;
                        }
                    }
                }
                else
                {
                    // Not currently in so add
                    dataSet.AppendChild(newResource.Navigator);
                }
            }
        }

        private Dictionary<string, Resource> LoadResources(XPathNavigator items)
        {
            Dictionary<string, Resource> collection = new Dictionary<string, Resource>();

            XPathNodeIterator iterator = items.Select("NewDataSet/Resources");

            while (iterator.MoveNext())
            {
                Resource resource = new Resource(iterator.Current);
                collection[resource.Id] = resource;
            }

            return collection;
        }

        private Dictionary<string, Resource> LoadResources(XmlElement items)
        {
            Dictionary<string, Resource> collection = new Dictionary<string, Resource>();

            foreach (XmlNode node in items.SelectNodes("NewDataSet/Resources"))
            {
                Resource resource = new Resource(node);
                collection[resource.Id] = resource;
            }

            return collection;
        }

        /// <summary>Adds a new resources section to the document.</summary>
        /// <param name="newResources">The resources to add.</param>
        private void AddResources(XPathNavigator newResources)
        {
            XPathNavigator root = document.DocumentElement.CreateNavigator();
            root.AppendChild(newResources);
        }

        private static string Value(XmlNode item, string element)
        {
            XmlElement result = (XmlElement)item.SelectSingleNode(element);
            if (result == null)
            {
                return null;
            }
            else
            {
                return result.InnerText;
            }
        }
#endregion private methods

#region Resource class
        class Resource
        {
            public string Id { get; set; }
            public string Source { get; set; }
            public string Translation { get; set; }
            public string Crucial { get; set; }
            public XmlElement Element { get; set; }
            public XPathNavigator Navigator { get; set; }

            public Resource(XPathNavigator item)
            {
                Navigator = item.Clone();
                Id = (string)item.Evaluate("string(ID)");
                Source = (string)item.Evaluate("string(Source)");
                Translation = (string)item.Evaluate("string(Translation)");
                Crucial = (string)item.Evaluate("string(Crucial)");
            }

            public Resource(XmlNode item)
            {
                Element = (XmlElement)item;
                Id = Updater.Value(item, "ID");
                Source = Updater.Value(item, "Source");
                Translation = Updater.Value(item, "Translation");
                Crucial = Updater.Value(item, "Crucial");
            }

        }
#endregion Resource class
    }
}
