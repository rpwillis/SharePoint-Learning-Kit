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
        ///  its value is retrieved and true is returned.  If the query string is
        ///  absent, null is retrieved and false is returned.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty.</param>  
        private static bool Get(string queryStringName, out string queryStringValue)
        {
            queryStringValue
                 = HttpContext.Current.Request.QueryString[queryStringName];

            return (!String.IsNullOrEmpty(SlkUtilities.Trim(queryStringValue)));
        }
        #endregion

        #region Get
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a string and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, null is retrieved and false is returned.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty and <paramref name="isOptional"/> is true.</param>  
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        public static bool Get(string queryStringName, out string queryStringValue, bool isOptional)
        {

            bool isValidQueryString = false;

            isValidQueryString = Get(queryStringName, out queryStringValue);

            if (!isValidQueryString && !isOptional)
            {
                throw new SafeToDisplayException(AppResources.SlkExQueryStringNotFound,
                                                 queryStringName);
            }

            return isValidQueryString;
        }
        #endregion

        #region Parse
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
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty and <paramref name="isOptional"/> is true.</param>  
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        public static bool Parse(string queryStringName, out int queryStringValue, bool isOptional)
        {
            string queryStringText;
            queryStringValue = 0;

            bool isValidQueryString = false;

            isValidQueryString = Get(queryStringName, out queryStringText, isOptional);

            if (isValidQueryString)
            {
                isValidQueryString = Int32.TryParse(queryStringText, out queryStringValue);
            }

            if (!isValidQueryString && !isOptional)
            {
                //The value is not valid for the given parameter.

                throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                 queryStringText, queryStringName);
            }

            return isValidQueryString;

        }
        #endregion

        #region Parse
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as an int and true is returned.  If the query string is
        ///  absent, then SafeToDisplayException is thrown.  If the query string is present
        ///  but its format is incorrect, a SafeToDisplayException is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="queryStringValue">The retrieved value.</param>  
        public static bool Parse(string queryStringName, out int queryStringValue)
        {
            return Parse(queryStringName, out queryStringValue, false);
        }

        #endregion

        #region Parse
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
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty and <paramref name="isOptional"/> is true.</param>  
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        public static bool Parse(string queryStringName, out int? queryStringValue, bool isOptional)
        {
            //check if value is in Right Format by Parsing the Value Return the Result.
            
            int value;
            queryStringValue = null;
            bool result = Parse(queryStringName, out value, isOptional);

            if (result)
            {
                queryStringValue = value;
            }

            return result;

        }
        #endregion

        #region Parse
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
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty and <paramref name="isOptional"/> is true.</param>  
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        public static bool Parse(string queryStringName, out long queryStringValue, bool isOptional)
        {
            string queryStringText;
            queryStringValue = 0;

            bool isValidQueryString = false;

            isValidQueryString = Get(queryStringName, out queryStringText, isOptional);

            if (isValidQueryString)
            {
                isValidQueryString = Int64.TryParse(queryStringText, out queryStringValue);
            }

            if (!isValidQueryString && !isOptional)
            {
                //The value is not valid for the given parameter.
                throw new SafeToDisplayException(AppResources.SlkExQueryStringFormatError,
                                                 queryStringText, queryStringName);
            }

            return isValidQueryString;

        }
        #endregion


        #region Parse
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a long and true is returned.  If the query string is
        ///  absent, then (a) if the caller specifies that the value is required, a 
        ///  SafeToDisplayException is thrown, or (b) if the caller specifies that
        ///  the value is optional, null is retrieved and false is returned.
        ///  If the query string is present but its format is incorrect, a SafeToDisplayException
        ///  is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="queryStringValue">The retrieved value. Set to null if the
        ///   query string is empty and <paramref name="isOptional"/> is true.</param>  
        /// <param name="isOptional">If false, SafeToDisplayException is thrown if
        ///   the query string is absent.</param>
        public static bool Parse(string queryStringName, out long? queryStringValue, bool isOptional)
        {
            //check if value is in Right Format by Parsing the Value Return the Result.
            long value;
            queryStringValue = null;
            bool result = Parse(queryStringName, out value, isOptional);

            if (result)
            {
                queryStringValue = value;
            }

            return result;

        }
        #endregion


        #region Parse
        /// <summary>
        ///  If a specified query string is present in the current HttpContext,
        ///  its value is retrieved as a Guid .  If the query string is
        ///  absent, then SafeToDisplayException is thrown.  If the query string is present
        ///  but its format is incorrect, a SafeToDisplayException is thrown.
        /// </summary>
        /// <param name="queryStringName">Name of the QueryString.</param>   
        /// <param name="queryStringValue">The retrieved value.</param>  
        public static void Parse(string queryStringName, out Guid queryStringValue)
        {
            string queryStringText;

            Get(queryStringName, out queryStringText, false);
            //check if the value is  valid for the given parameter.
            try
            {
                queryStringValue = new Guid(queryStringText);
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
        /// LearnerAssignmentGuidId
        /// </summary>
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";


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

        #endregion
    }
}
