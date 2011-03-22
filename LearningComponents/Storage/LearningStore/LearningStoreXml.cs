/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Xml;
using Microsoft.LearningComponents;

#endregion

/*
 * This file contains the LearningStoreXml class
 * 
 * Internal error numbers: 3000-3099
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents XML data contained in a store
    /// </summary>
    public class LearningStoreXml
    {
        /// <summary>
        /// Xml data
        /// </summary>
        private SqlXml m_sqlxml;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreXml</Typ> class.
        /// </summary>
        /// <param name="xml">Data to be contained in the instance.</param>
        internal LearningStoreXml(SqlXml xml)
        {
            // Check input parameters
            if (xml == null)
                throw new LearningComponentsInternalException("LSTR3000");

            m_sqlxml = xml;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreXml()
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <Typ>LearningStoreXml</Typ> class and loads some
        /// XML data into it.
        /// </summary>
        /// <returns>A new <Typ>LearningStoreXml</Typ></returns>
        /// <param name="reader"><Typ>/System.Xml.XmlReader</Typ> containing the XML
        ///     data to be read into the object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is a null reference.</exception>
        /// <example>
        /// The following code creates a <Typ>LearningStoreXml</Typ> object from
        /// an XmlDocument:
        /// <code language="C#">
        /// XmlNodeReader reader = new XmlNodeReader(document);
        /// LearningStoreXml xml = LearningStoreXml.CreateAndLoad(reader);
        /// </code>
        /// </example>
        public static LearningStoreXml CreateAndLoad(XmlReader reader)
        {
            // Check input parameters
            if (reader == null)
                throw new ArgumentNullException("reader");

            LearningStoreXml xml = new LearningStoreXml();
            xml.m_sqlxml = new SqlXml(reader);
            return xml;
        }

        /// <summary>
        /// Creates a new <Typ>/System.Xml.XmlReader</Typ> that can be used to read the
        /// XML data.
        /// </summary>
        /// <returns>A <Typ>/System.Xml.XmlReader</Typ> that can read the data
        ///     stored in this object.</returns>
        /// <example>
        /// The following code loads the data from a <Typ>LearningStoreXml</Typ>
        /// object into an XmlDocument:
        /// <code language="C#">
        /// XmlDocument document = new XmlDocument();
        /// document.Load(learningStoreXml.CreateReader());
        /// </code>
        /// </example>
        public XmlReader CreateReader()
        {
            return m_sqlxml.CreateReader();
        }

        /// <summary>
        /// The internal SqlXml object
        /// </summary>
        internal SqlXml SqlXml
        {
            get
            {
                return m_sqlxml;
            }
        }
    }
}
