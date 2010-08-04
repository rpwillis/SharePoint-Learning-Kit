/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.LearningComponents;
using System.Globalization;
using Microsoft.LearningComponents.Storage;
using Resources;
using System.Diagnostics.CodeAnalysis;

// This file includes code that is shared across multiple pages within both BasicWebPlayer and SLK framesets.

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Represents a helper object that provides useful functions to multiple pages within framesets. 
    /// </summary>
    public class FramesetPageHelper
    {
        // Error page display information
        private bool m_hasError;    // if true, there was an error to display
        private string m_errorTitle;    // the title of the error page
        private string m_errorMsg;      // the message of the error
        private bool m_errorAsInfo;

        private HttpRequest m_request; 

        /// <summary>Initializes a new instance of <see cref="FramesetPageHelper"/>.</summary>
        /// <param name="request">The current HttpRequest.</param>
        public FramesetPageHelper(HttpRequest request)
            : base()
        {
            m_request = request;
        }

        /// <summary>
        /// Register error page information to write to the response object at a later time.
        /// Only the first error that is registered is displayed.
        /// </summary>
        /// <param name="shortDescription">A short description (sort of a title) of the problem.</param>
        /// <param name="message">A longer description of the problem.</param>
        /// <param name="asInfo">If true, display as information message.</param>
        public void RegisterError(string shortDescription, string message, bool asInfo)
        {
            if (!m_hasError)
            {
                m_hasError = true;
                m_errorMsg = message;
                m_errorTitle = shortDescription;
                m_errorAsInfo = asInfo;
            }
        }

        /// <summary>
        /// Clear the current error state.
        /// </summary>
        public void ClearError()
        {
            m_hasError = false;
            m_errorMsg = null;
            m_errorTitle = null;
        }

        /// <summary>
        /// Returns true, and the value of the required parameter. If the parameter is not found or has no value,
        /// this method will display the error page.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter. The value should be ignored if false is returned.</param>
        /// <returns>True if the parameter existed.</returns>
        public bool TryGetRequiredParameter(string name, out string value)
        {
            return TryGetRequiredParameter(true, name, out value);
        }

        /// <summary>
        /// Returns true, and the value of the require parameter. If the <paramref name="showErrorPage"/> is true 
        /// and the parameter has no value, this method will register the error. 
        /// </summary>
        /// <returns>Returns true if the parameter had a value. If this is false, value should be ignored.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public bool TryGetRequiredParameter(bool showErrorPage, string name, out string value)
        {
            value = m_request.QueryString[name];
            if (String.IsNullOrEmpty(value))
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredTitle, name),
                        ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredMsg, name), false);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper function to process the View parameter. This method assumes the parameter is required. If it does not 
        /// exist or is not a valid value and showErrorPage=true, the error page is shown and the method returns false. 
        /// If false is returned, the caller should ignore the value of <paramref name="view"/>.    
        /// </summary>
        /// <param name="showErrorPage">TODO</param>
        /// <param name="view">TODO</param>
        public bool TryProcessViewParameter(bool showErrorPage, out SessionView view)
        {
            string viewParam;

            // Default value to make compiler happy
            view = SessionView.Execute;

            if (!TryGetRequiredParameter(FramesetQueryParameter.View, out viewParam))
                return false;

            try
            {
                // Get the view enum value
                view = (SessionView)Enum.Parse(typeof(SessionView), viewParam, true);
                if ((view < SessionView.Execute) || (view > SessionView.Review))
                {
                    if (showErrorPage)
                    {
                        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
                    }
                    return false;
                }
            }
            catch (ArgumentException)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper function to get the AttemptId query parameter. This method assumes the parameter is required. If 
        /// it does not exist or is not numeric, the error page is shown. This method does not check LearningStore.
        /// </summary>
        /// <param name="showErrorPage">If true, the error page is shown if the value is not provided.</param>
        /// <param name="attemptId">The attempt id.</param>
        /// <returns>If false, the value did not exist or was not valid. The application should not continue with 
        /// page processing.</returns>
        public bool TryProcessAttemptIdParameter(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            string attemptParam = null;
            bool isValid = true;

            attemptId = null;

            if (!TryGetRequiredParameter(showErrorPage, FramesetQueryParameter.AttemptId, out attemptParam))
                return false;


            // Try converting it to a long value. It must be positive.
            try
            {
                long attemptKey = long.Parse(attemptParam, NumberFormatInfo.InvariantInfo);

                if (attemptKey <= 0)
                    isValid = false;
                else
                    attemptId = new AttemptItemIdentifier(attemptKey);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            if (!isValid && showErrorPage)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.AttemptId),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.AttemptId, attemptParam), false);
            }

            return isValid;
        }

        /// <summary>Shows if the page has an error.</summary>
        public bool HasError
        {
            get { return m_hasError; }
        }

        /// <summary>
        /// If true, show the error page instead of the page
        /// </summary>
        public bool ShowError
        {
            get { return m_hasError; }
        }

        /// <summary>
        /// Return the short title of the error.
        /// </summary>
        public string ErrorTitle
        {
            get { return m_errorTitle; }
        }

        /// <summary>
        /// Return the long(er) error message.
        /// </summary>
        public string ErrorMsg
        {
            get { return m_errorMsg; }
        }

        /// <summary>If true show an error as information.</summary>
        public bool ErrorAsInfo
        {
            get { return m_errorAsInfo; }
        }
    }

    /// <summary>
    /// Valid values for the commands sent from the client. Note that the same strings are defined 
    /// at the top of the FramesetMgr.js file.
    /// </summary>
    public class Commands
    {
#pragma warning disable 1591
        public const string DoNext = "N";
        public const string DoPrevious = "P";
        public const string DoSave = "S";
        public const string DoTerminate = "T";
        public const string DoChoice = "C";
        public const string DoTocChoice = "TC"; // Force new attempt on the current activity
        public const string DoIsChoiceValid = "V";
        public const string DoIsNavigationValid = "NV";
        public const string DoSubmit = "DS";    // Do the submission or end the grading session
#pragma warning restore 1591
    }

    /// <summary>Names (and ids) of hidden fields in the posted data. </summary>
    /// <remarks>CAUTION: The value of the names is hard-coded in the *.js files. Don't change them without a thorough search.</remarks>
    ///
    public partial class HiddenFieldNames
    {
#pragma warning disable 1591
        public const string ActivityId = "hidActivityId";
        public const string AttemptId = "hidAttemptId";
        public const string Command = "hidCommand";
        public const string CommandData = "hidCommandData";
        public const string ContentHref = "hidContentHref";
        public const string DataModel = "hidDataModel";
        public const string ErrorMessage = "hidErrorMessage";
        public const string IsNavigationValidResponse = "hidIsNavValid";
        public const string PostFrame = "hidPostFrame";
        public const string ShowUI = "hidShowUI";
        public const string Title = "hidTitle";
        public const string View = "hidView";
        public const string ObjectiveIdMap = "hidObjectiveIdMap";
        public const string TocState = "hidTocState";
#pragma warning restore 1591
    }

    /// <summary>
    /// Resource Helper for frameset strings
    /// </summary>
    public static class ResHelper
    {

        /// <summary>
        /// Return a message formated for the current culture. resource is the string from the resource file which may have replacement
        /// tokens (e.g., {0}) in it. 
        /// </summary>
        public static string GetMessage(string resource, params string[] values)
        {
            return String.Format(CultureInfo.CurrentUICulture, resource, values);
        }

        /// <summary>
        /// Format a string using invariant culture.
        /// </summary>
        public static string FormatInvariant(string toFormat, params string[] values)
        {
            return String.Format(CultureInfo.InvariantCulture, toFormat, values);
        }

        /// <summary>
        /// Format a string using current culture. Does not access resources.
        /// </summary>
        public static string Format(string toFormat, params string[] values)
        {
            return String.Format(CultureInfo.CurrentCulture, toFormat, values);
        }
    }

    /// <summary>A utility class.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static class FramesetUtil
    {
        /// <summary>Gets the string value of the SessionView.</summary>
        public static string GetString(SessionView view)
        {
            return Convert.ToInt32(view, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Gets the string value of the LearningStoreItemIdentifier.</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")] // parameter is validated
        public static string GetString(LearningStoreItemIdentifier id)
        {
            ValidateNonNullParameter("id", id);
            return id.GetKey().ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Gets the string value of a long.</summary>
        public static string GetStringInvariant(long value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Checks that a value is not null.</summary>
        public static void ValidateNonNullParameter(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
