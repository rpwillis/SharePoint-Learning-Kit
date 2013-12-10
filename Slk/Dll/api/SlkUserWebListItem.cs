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
    /// Represents an item in a SharePoint Learning Kit user web list, which is a list of SharePoint
    /// SPWebs that a given user has added to the list of Web sites in the E-Learning Actions page.
    /// Each user has zero or more associated <c>SlkUserWebListItem</c> objects stored in the
    /// SharePoint Learning Kit database.
    /// </summary>
    ///
    [DebuggerDisplay("SlkUserWebListItem {m_spWebGuid}, {m_lastAccessTime}")]
    public class SlkUserWebListItem
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>SPSiteGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spSiteGuid;

        /// <summary>
        /// Holds the value of the <c>SPWebGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spWebGuid;

        /// <summary>
        /// Holds the value of the <c>LastAccessTime</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DateTime m_lastAccessTime;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this user web list item.
        /// </summary>
        public Guid SPSiteGuid
        {
            [DebuggerStepThrough]
            get
            {
                return m_spSiteGuid;
            }
        }

        /// <summary>
        /// Gets the <c>Guid</c> of the <c>SPWeb</c> associated with this user web list item.
        /// </summary>
        public Guid SPWebGuid
        {
            [DebuggerStepThrough]
            get
            {
                return m_spWebGuid;
            }
        }

        /// <summary>
        /// Gets the <c>DateTime</c> that the Web site specified by this user web list item was
        /// accessed using SharePoint Learning Kit.  Unlike the related value stored in the SharePoint
        /// Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime LastAccessTime
        {
            [DebuggerStepThrough]
            get
            {
                return m_lastAccessTime;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
        ///     </param>
        ///
        /// <param name="spWebGuid">The value to use to initialize the <c>SPWebGuid</c> property.
        ///     </param>
        ///
        /// <param name="lastAccessTime">The value to use to initialize the <c>LastAccessTime</c>
        ///     property.</param>
        ///
        internal SlkUserWebListItem(Guid spSiteGuid, Guid spWebGuid, DateTime lastAccessTime)
        {
            m_spSiteGuid = spSiteGuid;
            m_spWebGuid = spWebGuid;
            m_lastAccessTime = lastAccessTime;
        }
    }
}

