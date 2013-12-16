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
    /// Specifies a role that a user may have in SLK, with respect to a Web site or an assignment.
    /// A user may have multiple roles.
    /// </summary>
    public enum SlkRole
    {
        /// <summary>
        /// No role
        /// </summary>
        None = 0,
        
        /// <summary>
        /// The user is an instructor on the Web site or assignment.
        /// </summary>
        Instructor = 1,

        /// <summary>
        /// The user is a learner on the Web site or assignment.
        /// </summary>
        Learner = 2,

        /// <summary>
        /// The user is an observer on the Web site or assignment.
        /// </summary>
        Observer = 3,
    }
}

