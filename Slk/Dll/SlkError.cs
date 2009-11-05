/* Copyright (c) Microsoft Corporation. All rights reserved. */

//SlkError.cs
//
//Slk Error Messages 
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Resources.Properties;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.Data.SqlClient;

namespace Microsoft.SharePointLearningKit.WebControls // NOTE: SlkError isn't a control, but it's in this
                                                      // namespace to "hide" it (since it's undocumented)
{
    /// <summary>
    /// Types of Error messages that can be shown
    /// </summary>
    internal enum ErrorType
    {
        Info = 0,
        Warning,
        Error
    };
    /// <summary>
    /// Error messages to be rendered in the banner.
    /// </summary>
    internal class SlkError
    {

        #region Private Variables
        /// <summary>
        /// Holds the ErrorText 
        /// </summary>
        private string m_errorText;
        /// <summary>
        /// Holds the ErrorType
        /// </summary>
        private ErrorType m_errorType;

        #endregion

        #region Constructor
        /// <summary>
        /// Initialize the SlkError Object with passed errorType and errorText. 
        /// </summary> 
        internal SlkError(ErrorType errorType, string errorText)
        {
            // Initialize Object if ErrorText is not null or empty.
            if (!String.IsNullOrEmpty(SlkUtilities.Trim(errorText)))
            {
                m_errorType = errorType;
                m_errorText = errorText;
            }

        }
        #endregion

        #region Public Accessors

        /// <summary>
        /// Error messages to be rendered
        /// </summary>
        internal string ErrorText
        {
            get { return m_errorText; }
        }

        /// <summary>
        /// Error Message Type from allowed Type.
        /// </summary>
        internal ErrorType ErrorType
        {
            get { return m_errorType; }
        }

        #endregion

        #region WriteToEventLog
        /// <summary>
        /// Formats a message using <c>String.Format</c> and writes to the event
        /// log.
        /// </summary>    
        /// <param name="format">A string containing zero or more format items;
        ///     for example, "An exception occurred: {0}".</param>
        /// 
        /// <param name="args">Formatting arguments.</param>
        ///
        public static void WriteToEventLog(string format, params object[] args)
        {
            SlkUtilities.ImpersonateAppPool(delegate()
            {
                using (EventLog eventLog = new EventLog())
                {
#if true
                    // use SharePoint's event log source, since it already exists
                    eventLog.Source = AppResources.WssEventLogSource;
                    eventLog.WriteEntry(
                                String.Format(CultureInfo.CurrentCulture, 
                                              AppResources.AppError,
                                              String.Format(CultureInfo.CurrentCulture, 
                                              format, args)).Replace(@"\n", "\r\n"), 
                                              EventLogEntryType.Error);
#else
                    // use an SLK event log source -- but this requires the administrator to
                    // create a registry entry, since the application pool account typically
                    // won't have the ability to do so
                    eventLog.Source = AppResources.SlkEventLogSource;
                    try
                    {
                        eventLog.WriteEntry(String.Format(format, args).Replace(@"\n", "\r\n"), type);
                    }
                    catch (System.Security.SecurityException)
                    {
                        throw new SafeToDisplayException(AppResources.EventLogNotConfigured);
                        // AppResources.EventLogNotConfigured can contain this text:
                        //
                        // A serious error occurred.  Please contact your system administrator.
                        //
                        // Information about this error cannot be written to the system event log
                        // because the event log is not configured correctly.  To correct this problem,
                        // the system adminstrator must create a registry key named
                        // "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Application\
                        // SharePoint Learning Kit" and create a REG_EXPAND_SZ (Expandable String)
                        // value named "EventMessageFile" with value "C:\WINNT\Microsoft.NET\Framework\
                        // %FrameworkVersion%\EventLogMessages.dll" (without the quotes), on the web
                        // server(s).
                    }
#endif
                }
            });
        }
        #endregion

        #region WriteException
        /// <summary>
        /// writes the exeception to the event Log and outs the SlkError Object. 
        /// </summary>    
        /// <param name="ex">Exception</param>       
        /// <param name="slkerror">SlkError Object.</param>
        public static void WriteException(Exception ex, out SlkError slkError)
        {           
            //Check for Exception Type. For all Handled Exceptions
            //Add the Exception Message in Error 
            //Or Add the Standard Error Message  and 
            //log the exception in EventLog.            
            if (ex is SafeToDisplayException || 
                ex is UserNotFoundException ||
                ex is SlkNotConfiguredException ||
                ex is NotAnInstructorException)
            {
                slkError = new SlkError(ErrorType.Error, SlkUtilities.GetHtmlEncodedText(ex.Message));
            }
            else
            if (ex is UnauthorizedAccessException || 
                ex is LearningStoreSecurityException)
            {
                slkError = new SlkError(ErrorType.Error, SlkUtilities.GetHtmlEncodedText(AppResources.AccessDenied));
            }
            else
            {
                //Set the Standard Error text 
                string errorText = AppResources.SlkGenericError;

                SqlException sqlEx = ex as SqlException;
                if (sqlEx != null)
                {                    
                    //check whether deadlock occured
                    if (sqlEx.Number == 1205)
                    {
                        errorText = AppResources.SlkExWorkFlowSqlDeadLockError ;
                    }
                   
                }     
                //Add the Error to Error Collection.
                slkError = new SlkError(ErrorType.Error,
                    SlkUtilities.GetHtmlEncodedText(errorText));
                //log the exception in EventLog. 
                SlkError.WriteToEventLog(ex);
            }           
        }
        #endregion

        #region WriteToEventLog
        /// <summary>
        /// writes the exeception to the event Log 
        /// </summary>    
        /// <param name="ex">Exception</param>  
        public static void WriteToEventLog(Exception ex)
        {
            SlkError.WriteToEventLog("{0}", ex.ToString());
        }
        #endregion

    }
}

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Indicates an error message which may safely be displayed to the browser user.  Such messages
    /// may not include any sensitive information.
    /// </summary>
    [Serializable]
    public class SafeToDisplayException : Exception
    {
        /// <summary>
        /// Holds the value of the <c>ValidationResults</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ValidationResults m_validationResults;

        /// <summary>
        /// Validation results associated with the exception, or <n>null</n> if none.
        /// </summary>
        public ValidationResults ValidationResults
        {
            [DebuggerStepThrough]
            get
            {
                return m_validationResults;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SafeToDisplayException</Typ> class.
        /// </summary>
        public SafeToDisplayException()
            : base()
        {
        }

        /// <summary>
        /// Initializes an instance of this class, by formatting an error message.
        /// </summary>
        ///
        /// <param name="format">A <c>String.Format</c>-style format string.  If
        ///     <paramref name="args"/> is zero-length, <paramref name="format"/> is
        ///     returned without formatting.</param>
        ///
        /// <param name="args">Formatting arguments.</param>
        ///
        public SafeToDisplayException(string format, params object[] args)
            :
            base(
                    (args !=  null && args.Length == 0) ? 
                     format : String.Format(CultureInfo.CurrentCulture, format, args)
                )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SafeToDisplayException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SafeToDisplayException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SafeToDisplayException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SafeToDisplayException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SafeToDisplayException</Typ> class,
        /// including further information in a <r>ValidationResults</r> object.
        /// </summary>
        /// <param name="validationResults">Validation results associated with this exception,
        ///     or <n>null</n> if none.</param>
        /// <param name="message">The message that describes the error.</param>
        public SafeToDisplayException(ValidationResults validationResults, string message)
            : base(message)
        {
            m_validationResults = validationResults;
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SafeToDisplayException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SafeToDisplayException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            m_validationResults = (ValidationResults) info.GetValue("ValidationResults", typeof(ValidationResults));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("ValidationResults", m_validationResults, typeof(ValidationResults));
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Indicates an error message which is interal to Slk. Get the Internal ErrorMessage From 
    /// Resource and display.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal class InternalErrorException : Exception
    {
        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="internalErrorCode">An internal error code, e.g. "APP1001".</param>
        ///
        internal InternalErrorException(string internalErrorCode)
            :
            base(String.Format(CultureInfo.CurrentCulture, AppResources.InternalError, internalErrorCode))
        {
        }
    }

    /// <summary>
    /// Indicates an error message which may caused due to anonymous access/Not a SPUser.  
    /// </summary>
    [Serializable]
    public class UserNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>UserNotFoundException</Typ> class.
        /// </summary>
        public UserNotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>UserNotFoundException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UserNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>UserNotFoundException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UserNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>UserNotFoundException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected UserNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Indicates that SLK has not yet been configured in SharePoint Central Administraton.
    /// </summary>
    [Serializable]
    public class SlkNotConfiguredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>SlkNotConfiguredException</Typ> class.
        /// </summary>
        public SlkNotConfiguredException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkNotConfiguredException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SlkNotConfiguredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkNotConfiguredException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SlkNotConfiguredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkNotConfiguredException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SlkNotConfiguredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Indicates an error message which may caused due to SPUser is Not An Instructor in the Given SPWeb.  
    /// </summary>
    [Serializable]
    public class NotAnInstructorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>NotAnInstructorException</Typ> class.
        /// </summary>
        public NotAnInstructorException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>NotAnInstructorException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotAnInstructorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>NotAnInstructorException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public NotAnInstructorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>NotAnInstructorException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected NotAnInstructorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}


