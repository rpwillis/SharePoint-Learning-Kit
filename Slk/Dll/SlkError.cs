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
using Microsoft.SharePoint;
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
                string message = String.Format(CultureInfo.InvariantCulture, AppResources.AppError, String.Format(CultureInfo.InvariantCulture, format, args));
                message = message.Replace(@"\n", "\r\n");
                WriteEvent(message);
            });
        }

        static string source;
        static object lockObject = new object();

        static void WriteEvent(string message)
        {
            WriteEvent(message, new string[] {AppResources.SlkEventLogSource, AppResources.WssEventLogSource, AppResources.SharePoint2010LogSource});
        }

        static void WriteEvent(string message, string[] possibleSources)
        {
            using (EventLog eventLog = new EventLog())
            {
                if (string.IsNullOrEmpty(source))
                {
                    eventLog.Source = possibleSources[0];
                }
                else
                {
                    eventLog.Source = source;
                }

                bool errorOccurred = false;

                try
                {
                    eventLog.WriteEntry(message, EventLogEntryType.Error);

                    if (string.IsNullOrEmpty(source))
                    {
                        lock (lockObject)
                        {
                            source = eventLog.Source;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    errorOccurred = true;
                }
                catch (ArgumentException)
                {
                    errorOccurred = true;
                }
                catch (System.Security.SecurityException)
                {
                    errorOccurred = true;
                }

                if (errorOccurred)
                {
                    if (possibleSources.Length > 1)
                    {
                        string[] newPossible = new string[possibleSources.Length - 1];
                        Array.Copy(possibleSources, 1, newPossible, 0, possibleSources.Length - 1);
                        WriteEvent(message, newPossible);
                    }
                }
            }
        }
        #endregion

        #region WriteException
        /// <summary>
        /// writes the exeception to the event Log and outs the SlkError Object. 
        /// </summary>
        /// <param name="store">The ISlkStore to write exceptions to.</param>
        /// <param name="ex">Exception</param>
        /// <returns></returns>
        public static SlkError WriteException(ISlkStore store, Exception ex)
        {           
            SlkError slkError = null;
            //Check for Exception Type. For all Handled Exceptions
            //Add the Exception Message in Error 
            //Or Add the Standard Error Message  and 
            //log the exception in EventLog.            
            if (ex is SafeToDisplayException || ex is UserNotFoundException || ex is SlkNotConfiguredException || ex is NotAnInstructorException)
            {
                slkError = new SlkError(ErrorType.Error, SlkUtilities.GetHtmlEncodedText(ex.Message));
            }
            else if (ex is UnauthorizedAccessException || ex is LearningStoreSecurityException)
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


                //log the exception in EventLog. 
                store.LogException(ex);
                slkError = new SlkError(ErrorType.Error, SlkUtilities.GetHtmlEncodedText(errorText));
            }           

            return slkError;
        }
        #endregion

        public static void Debug (Exception e)
        {
            Debug(e.ToString());
        }

        public static void Debug (string message, params object[] arguments)
        {
            try
            {
                /*
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter("c:\\temp\\slkDebug.txt", true))
                {
                    writer.WriteLine(message, arguments);
                }
                */
            }
            catch
            {
            }
        }

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
                     format : String.Format(SlkCulture.GetCulture(), format, args)
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

        /// <summary>Support serialization.</summary>
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
            base(String.Format(SlkCulture.GetCulture(), AppResources.InternalError, internalErrorCode))
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


