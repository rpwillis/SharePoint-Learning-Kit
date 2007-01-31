/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace MigrationHelper
{
    public class ManifestParser
    {
        /// <summary>
        /// Parses index.xml manifest file. 
        /// Calls ParseIndexXml(XPathNavigator indexXmlNavigator) to get package identifier (stored in BranchID)
        /// </summary>
        /// <param name="filePath">file path that will be loaded into XmlDocument</param>
        /// <returns>BranchID as Guid</returns>
        public static Guid ParseIndexXml(string filePath)
        {
            Guid identifier = System.Guid.Empty;
            XPathDocument manifestDocument = new XPathDocument(filePath);
            XPathNavigator manifestNavigator = manifestDocument.CreateNavigator();
            identifier = ParseIndexXml(manifestNavigator);
            return identifier;
        }

        /// <summary>
        /// Returns BranchID attribute value at LearningResource/PackageDescription/General/LearningResourceData/ID
        /// </summary>
        /// <param name="indexXmlNavigator">XPathNavigator for index xml document</param>
        /// <returns>BranchID as Guid</returns>
        public static Guid ParseIndexXml(XPathNavigator indexXmlNavigator)
        {
            Guid identifier = System.Guid.Empty;
            XPathNodeIterator allNodes = indexXmlNavigator.Select("//*");
            XPathNodeIterator allNodesCopy = allNodes.Clone();
            while (allNodes.MoveNext())
            {
                if (allNodes.Current.Name == "LearningResource")
                {
                    indexXmlNavigator.MoveTo(allNodes.Current);
                    break;
                }
            }
            string nameSpace = indexXmlNavigator.NamespaceURI;
            XmlNamespaceManager nsManager = new XmlNamespaceManager(indexXmlNavigator.NameTable);
            //assigning a dummy prefix aa to force XPath query to use learning resources namespace
            nsManager.AddNamespace("aa", nameSpace);
            XPathNavigator idNode = indexXmlNavigator.SelectSingleNode("//aa:LearningResource/aa:PackageDescription/aa:General/aa:LearningResourceData/aa:ID", nsManager);
            string identifierString = String.Empty;
            if (idNode != null)
            {
                identifierString = idNode.GetAttribute("BranchID", "");
            }
            else
            {
                //loop through all nodes, looking for any ID node having LearningResourceData parent
                while (allNodesCopy.MoveNext())
                {
                    if (allNodesCopy.Current.LocalName == "ID")
                    {
                        indexXmlNavigator.MoveTo(allNodesCopy.Current);
                        indexXmlNavigator.MoveToParent();
                        if (indexXmlNavigator.LocalName == "LearningResourceData")
                        {
                            //thats the right identity node
                            identifierString = allNodesCopy.Current.GetAttribute("BranchID", "");
                        }
                        break;
                    }
                }
            }
            if (identifierString != String.Empty)
            {
                identifier = FormatGuid(identifierString);
            }
            return identifier;
        }

        /// <summary>
        /// Loops through all the nodes in the XML document
        /// and returns true if finds "License" node
        /// </summary>
        /// <param name="filePath">XML file to process</param>
        /// <returns>true if "License" node is found.</returns>
        public static bool ManifestIncludesLicense(string filePath)
        {
            XPathDocument manifestDocument = new XPathDocument(filePath);
            XPathNavigator LearningResourceData = manifestDocument.CreateNavigator();
            if (LearningResourceData != null)
            {
                XPathNodeIterator allChildren = LearningResourceData.SelectDescendants(XPathNodeType.Element,true);
                while (allChildren.MoveNext())
                {
                    if (String.CompareOrdinal(allChildren.Current.Name, "License") == 0)
                    {
                        return true;
                    }
                }

            }    
            return false;
        }



        /// <summary>
        /// Parses imsmanifest.xml manifest file. 
        /// Calls ParseImsManifestXml(XPathNavigator manifestNavigator) to get package identifier 
        /// (either "identifier" attribute value of the /manifest node 
        /// or value in metadata/lom/general/identifier)
        /// </summary>
        /// <param name="filePath">file path that will be loaded into XmlDocument</param>
        /// <returns>identifier value as Guid</returns>
        public static Guid ParseImsManifestXml(string filePath)
        {
            Guid identifier = System.Guid.Empty;
            XPathDocument manifestDocument = new XPathDocument(filePath);
            XPathNavigator manifestNavigator = manifestDocument.CreateNavigator();
            identifier = ParseImsManifestXml(manifestNavigator);
            return identifier;

        }

        /// <summary>        
        /// Returns "identifier" attribute value of the /manifest node
        /// or if its not found, looks for value in metadata/lom/general/identifier
        /// </summary>
        /// <param name="manifestNavigator">XPathNavigator for imsmanifest document</param>
        /// <returns>identifier value as Guid</returns>
        public static Guid ParseImsManifestXml(XPathNavigator manifestNavigator)
        {
            Guid identifier = System.Guid.Empty;
            XPathNodeIterator allNodes = manifestNavigator.Select("//*");
            XPathNodeIterator allNodesCopy = allNodes.Clone();
            while (allNodes.MoveNext())
            {
                if (allNodes.Current.Name == "manifest")
                {
                    string identifierString = allNodes.Current.GetAttribute("identifier", "");
                    if (identifierString != String.Empty)
                    {
                        identifier = FormatGuid(identifierString);
                    }
                    break;
                }
            }
            if (identifier == System.Guid.Empty)
            {
                while (allNodesCopy.MoveNext())
                {
                    if (allNodesCopy.Current.LocalName == "identifier")
                    {
                        manifestNavigator.MoveTo(allNodesCopy.Current);
                        manifestNavigator.MoveToParent();
                        if (manifestNavigator.LocalName == "general")
                        {
                            //thats the right identity node
                            identifier = FormatGuid(allNodesCopy.Current.Value);
                        }
                        break;
                    }
                }
            }
            return identifier;
        }

        /// <summary>
        /// executes new Guid(string) and re-wraps FormatException to include more information
        /// </summary>
        /// <param name="guidString">String to be converted to Guid</param>
        /// <returns>Guid created from the string</returns>
        public static Guid FormatGuid(string guidString)
        {
            try
            {
                return new Guid(guidString);
            }
            catch(System.FormatException ex)
            {
                throw(new Exception("The string " + guidString + " is not a valid guid.",ex));
            }
        }

    }
}
