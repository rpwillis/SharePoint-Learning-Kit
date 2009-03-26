using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

namespace CourseManagerConfCs
{
    public static class CourseManagerAdministrator
    {
        /// <summary>
        /// Returns an instance of the SharePoint Central Administration web service.
        /// </summary>
        ///
        internal static SPWebService GetAdminWebService()
        {
            SPAdministrationWebApplication adminWebApp = SPAdministrationWebApplication.Local;
            return (SPWebService)adminWebApp.Parent;
        }
    }
}
