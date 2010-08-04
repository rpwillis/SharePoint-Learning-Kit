/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkQueryString.cs
//
// Handles Request QueryString Validation and Parsing.
//


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using System.Web;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    internal static class QueryString
    {
        #region Get
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a string and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, null is retrieved and false is returned.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        static string Get(string queryStringName, bool isOptional)
        {

            string queryStringValue = HttpContext.Current.Request.QueryString[queryStringName];
            queryStringValue = SlkUtilities.Trim(queryStringValue);

            if (queryStringValue == string.Empty)
            {
                queryStringValue = null;
            }

            bool isValidQueryString = (String.IsNullOrEmpty(queryStringValue) == false);

            if (!isValidQueryString && !isOptional)
            {
                throw new SafeToDisplayException(AppResources.SlkExQueryStringNotFound, queryStringName);
            }

            return queryStringValue;
        }
        #endregion

#region Parse String
        /// <summary>Finds the string value from the query string.</summary>
        /// <param name="queryStringName">The name of the item.</param>
        /// <returns>The string value.</returns>
        public static string ParseString(string queryStringName)
        {
            return Get(queryStringName, false);
        }

        /// <summary>Finds the string value from the query string. If not present returns null.</summary>
        /// <param name="queryStringName">The name of the item.</param>
        /// <returns>The string value or null if not present.</returns>
        public static string ParseStringOptional(string queryStringName)
        {
            return Get(queryStringName, true);
        }
#endregion Parse String

        #region Parse int
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as an int and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, 0 is retrieved and false is returned.
        ///  If the query string is present but its format is incorrect, a SafeToDisplayException
        ///  is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>
        /// <param name="defaultValue">The default value if not present.</param>
        /// <returns></returns>
        static int ParseIntOptional(string queryStringName, int defaultValue)
        {
            string queryStringText = Get(queryStringName, true);

            if (string.IsNullOrEmpty(queryStringText))
            {
                return defaultValue;
            }
            else
            {
                int value;
                if (Int32.TryParse(queryStringText, out value))
                {
                    return value;
                }
                else
                {
                    //The value is not valid for the given parameter.

                    throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                     queryStringText, queryStringName);
                }
            }
        }

        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as an int and true is returned.  If the query string is
        ///  absent, then SafeToDisplayException is thrown.  If the query string is present
        ///  but its format is incorrect, a SafeToDisplayException is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        public static int Parse(string queryStringName)
        {
            string queryStringText = Get(queryStringName, true);

            int value;
            if (Int32.TryParse(queryStringText, out value))
            {
                return value;
            }
            else
            {
                //The value is not valid for the given parameter.

                throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                 queryStringText, queryStringName);
            }
        }

        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as an int and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, null is retrieved and false is returned.
        ///  If the query string is present but its format is incorrect, a SafeToDisplayException
        ///  is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        public static int? ParseIntOptional(string queryStringName)
        {
            string queryStringText = Get(queryStringName, true);

            if (string.IsNullOrEmpty(queryStringText))
            {
                return null;
            }
            else
            {
                int value;
                if (Int32.TryParse(queryStringText, out value))
                {
                    return value;
                }
                else
                {
                    //The value is not valid for the given parameter.

                    throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                     queryStringText, queryStringName);
                }
            }
        }
        #endregion

        #region Parse long
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a long and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, 0 is retrieved and false is returned.
        ///  If the query string is present but its format is incorrect, a SafeToDisplayException
        ///  is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>
        /// <param name="defaultValue">The default value to use if not present.</param>
        public static long? ParseLong(string queryStringName, long? defaultValue)
        {
            string queryStringText = Get(queryStringName, true);

            if (string.IsNullOrEmpty(queryStringText))
            {
                return defaultValue;
            }
            else
            {
                long value;
                if (long.TryParse(queryStringText, out value))
                {
                    return value;
                }
                else
                {
                    throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                     queryStringText, queryStringName);
                }
            }
        }
        #endregion


        #region Parse Guid
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a Guid .  If the query string is
        ///  absent, then SafeToDisplayException is thrown.  If the query string is present
        ///  but its format is incorrect, a SafeToDisplayException is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        public static Guid ParseGuid(string queryStringName)
        {
            string queryStringText = Get(queryStringName, false);
            //check if the value is  valid for the given parameter.
            try
            {
                return new Guid(queryStringText);
            }
            catch
            {
                throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                 queryStringText, queryStringName);
            }

        }

        #endregion
    }

    internal static class QueryStringKeys
    {
        #region Query String Keys

        /// <summary>
        /// Name of querystring parameter for passing QuerySet to load AssignmentList Webpart
        /// </summary>
        public const string QuerySet = "QuerySet";

        /// <summary>
        /// Name of querystring parameter for passing Query to load AssignmentList Webpart
        /// </summary>
        public const string Query = "Query";

        /// <summary>
        /// Name of querystring parameter for passing id of iFrame to be loaded
        /// </summary>
        public const string FrameId = "FrameId";

        /// <summary>
        /// Name of querystring parameter for passing scope of web based visibility
        /// </summary>
        public const string SPWebScope = "SPWebScope";

        /// <summary>
        /// Name of querystring parameter for passing learner user key for the observer mode
        /// </summary>
        public const string LearnerKey = "LearnerKey";

        /// <summary>
        /// Name of querystring parameter for Sorting Order
        /// </summary>
        public const string Sort = "Sort";

        /// <summary>
        /// Category Parameter
        /// </summary>
        public const string CategoryName = "Assignment List";

        /// <summary>
        /// RootActivityId
        /// </summary>
        public const string RootActivityId = "RootActivityId";

        /// <summary>
        /// ListId
        /// </summary>
        public const string ListId = "ListId";

        /// <summary>
        /// ItemId
        /// </summary>
        public const string ItemId = "ItemId";

        /// <summary>
        /// LearnerAssignmentId
        /// </summary>
        public const string LearnerAssignmentId = "LearnerAssignmentId";

        /// <summary>
        ///  Name of Location querystring parameter for APP
        /// </summary>

        public const string Location = "Location";
        /// <summary>
        /// Name of Organization Index querystring parameter for APP 
        /// </summary>
        public const string OrgIndex = "OrgIndex";

        /// <summary>
        /// Name of AssignmentId querystring parameter for APP 
        /// </summary>
        public const string AssignmentId = "AssignmentId";

        /// <summary>The Action query string key.</summary>
        public const string Action = "Action";

        #endregion
    }
}
