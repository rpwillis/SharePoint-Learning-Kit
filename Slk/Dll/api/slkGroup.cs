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
    /// Represents a collection of SharePoint Learning Kit learners and/or instructors contained
    /// within a SharePoint <c>SPGroup</c>.
    /// </summary>
    [DebuggerDisplay("SlkGroup: {Name}")]
    public class SlkGroup : IComparable<SlkGroup>
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>SPGroup</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SPGroup m_spGroup;

        /// <summary>
        /// Holds the value of the <c>DomainGroup</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SPUser m_domainGroup;

        /// <summary>
        /// Holds the value of the <c>Users</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<SlkUser> m_users;

        /// <summary>
        /// Holds the value of the <c>UserKeys</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<string> m_userKeys;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the SharePoint <c>SPGroup</c> that is represented by this <c>SlkGroup</c>, or
        /// <c>null</c> if this <c>SlkGroup</c> represents a domain group.
        /// </summary>
        public SPGroup SPGroup
        {
            [DebuggerStepThrough]
            get
            {
                return m_spGroup;
            }
        }

        /// <summary>
        /// Gets the SharePoint <c>SPUser</c> of the domain group that is represented by this
        /// <c>SlkGroup</c>, or <c>null</c> if this <c>SlkGroup</c> represents a SharePoint group
        /// instead of a domain group.
        /// </summary>
        public SPUser DomainGroup
        {
            [DebuggerStepThrough]
            get
            {
                return m_domainGroup;
            }
        }

        /// <summary>
        /// Gets the members of this group, represented as a collection of <c>SlkUser</c> objects.
        /// </summary>
        public ReadOnlyCollection<SlkUser> Users
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<SlkUser>(m_users);
            }
        }

        /// <summary>
        /// Gets or sets the members of this group, represented as a collection of SID strings.
        /// </summary>
        internal List<string> UserKeys
        {
            [DebuggerStepThrough]
            get
            {
                return m_userKeys;
            }
            [DebuggerStepThrough]
            set
            {
                m_userKeys = value;
            }
        }

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                if (m_spGroup != null)
                    return m_spGroup.Name;
                else
                    return m_domainGroup.Name;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="spGroup">The SharePoint <c>SPGroup</c> represented by this <c>SlkGroup</c>,
        ///     if this <c>SlkGroup</c> represents a SharePoint group; <c>null</c> otherwise.</param>
        /// 
        /// <param name="domainGroup">The SharePoint <c>SPUser</c> represented by this <c>SlkGroup</c>
        ///     if this <c>SlkGroup</c> represents a domain group; <c>null</c> otherwise.</param>
        ///
        internal SlkGroup(SPGroup spGroup, SPUser domainGroup)
        {
            m_spGroup = spGroup;
            m_domainGroup = domainGroup;
            m_users = new List<SlkUser>();
        }

        /// <summary>
        /// Adds a user to <c>Users</c>.
        /// </summary>
        ///
        /// <param name="slkUser">The user to add.</param>
        ///
        internal void AddUser(SlkUser slkUser)
        {
            m_users.Add(slkUser);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // IComparable<SlkGroup> Implementation
        //

        /// <summary>
        /// Compares this <r>SlkGroup</r> to another.
        /// </summary>
        ///
        /// <param name="other">The other <r>SlkGroup</r>.</param>
        ///
        /// <returns>
        /// A value less than, equal to, or greater than zero, depending on whether this
        /// <r>SlkGroup</r> is less than, equal to, or greather than <pr>other</pr>.
        /// </returns>
        ///
        int IComparable<SlkGroup>.CompareTo(SlkGroup other)
        {
            return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

