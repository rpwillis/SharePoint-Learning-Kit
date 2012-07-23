/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkApi.cs
//
// Implements SlkStore and related types.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Library;
using Microsoft.SharePoint.Navigation;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Contains information about a SharePoint Learning Kit package.
    /// </summary>
    /// 
    public class PackageInformation
    {
        /// <summary>
        /// Holds the value of the <c>SPFile</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SPFile m_spFile;

        /// <summary>
        /// Holds the value of the <c>ManifestReader</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ManifestReader m_manifestReader;

        /// <summary>
        /// Holds the value of the <c>Title</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_title;

        /// <summary>
        /// Holds the value of the <c>Description</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_description;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// The <c>SPFile</c> of the package.
        /// </summary>
        public SPFile SPFile
        {
            [DebuggerStepThrough]
            get
            {
                return m_spFile;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_spFile = value;
            }
        }

        /// <summary>
        /// The manifest reader for the package
        /// </summary>
        public ManifestReader ManifestReader
        {
            [DebuggerStepThrough]
            get
            {
                return m_manifestReader;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_manifestReader = value;
            }
        }

        /// <summary>
        /// Title of the package
        /// </summary>
        public string Title
        {
            [DebuggerStepThrough]
            get
            {
                return m_title;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_title = value;
            }
        }

        /// <summary>
        /// Description for the package
        /// </summary>
        public string Description
        {
            [DebuggerStepThrough]
            get
            {
                return m_description;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_description = value;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        internal PackageInformation()
        {
        }
    }
}

