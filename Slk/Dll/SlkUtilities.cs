/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MockupHelper.cs
//
// Helper classes used by M2Mockup.
//

//#define REPRO_BUG_297

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using System.Reflection;
using System.Resources;

namespace Microsoft.SharePointLearningKit
{

    /// <summary>
    /// Contains static helper methods used in Slk.
    /// </summary>
    public static class SlkUtilities
    {
        #region Public Static Methods

        #region GetCurrentSPWeb

        /// <summary>
        /// Returns the <c>SPWeb</c> associated with the current <c>HttpRequest</c>.
        /// </summary>
        ///
        internal static SPWeb GetCurrentSPWeb()
        {
            /*
            // SharePoint can convert an URL to an SPWeb, but <httpRequest.Url> is the wrong URL
            // to use, because if it contains "_layouts" then the full path to the subsite (if any)
            // will be removed from <httpRequest.Url>; instead, we use <httpRequest.RawUrl> -- but
            // we need to (a) remove query parameters and (b) add the host name and port
            HttpRequest httpRequest = HttpContext.Current.Request;
            string rawPath = httpRequest.RawUrl;
            int index = rawPath.IndexOf('?');
            if (index >= 0)
                rawPath = rawPath.Substring(0, index);
            Uri originalUrl = new UriBuilder(httpRequest.Url.Scheme, httpRequest.Url.Host,
                httpRequest.Url.Port, rawPath).Uri;
            using (SPSite spSite = new SPSite(originalUrl.AbsoluteUri))
                return spSite.OpenWeb();
            */

            // In debug builds, look for the following in Web.config (example):
            //
            //     <configuration>
            //       <appSettings>
            //         <add key="debugSlkSPWebUrl" value="http://localhost/myweb"/>
            //     	 </appSettings>
            //       ...
            //     <configuration>
            //
            // This is useful when testing ASPX pages from within the Visual Studio ASP.NET
            // Development Server (i.e. outside the context of SharePoint).
            //
#if DEBUG
            string url =
                System.Web.Configuration.WebConfigurationManager.AppSettings["debugSlkSPWebUrl"];
            if (url != null)
            {
				using (SPSite spSite = new SPSite(url))
				{
					return spSite.OpenWeb();
				}
            }
#endif

            return SPControl.GetContextWeb(HttpContext.Current);
        }

        #endregion

        #region LogEvent

        /// <summary>
        /// Formats a message using <c>String.Format</c> and writes to the event
        /// log.
        /// </summary>
        ///
        /// <param name="type">The type of the event log entry.</param>
        ///
        /// <param name="format">A string containing zero or more format items;
        ///     for example, "An exception occurred: {0}". If there are no 
        ///     <paramref name="args"/> provided, the format string is used
        ///     without formatting.</param>
        /// 
        /// <param name="args">Formatting arguments.</param>
        ///
        public static void LogEvent(EventLogEntryType type, string format,
            params object[] args)
        {
            if((type != EventLogEntryType.Error) && (type != EventLogEntryType.FailureAudit) &&
               (type != EventLogEntryType.Information) && (type != EventLogEntryType.SuccessAudit) &&
               (type != EventLogEntryType.Warning))
               throw new ArgumentOutOfRangeException("type");
            if(format == null)
                throw new ArgumentNullException("format");
                
            ImpersonateAppPool(delegate()
            {
                using (EventLog eventLog = new EventLog())
                {
#if true
                    // use SharePoint's event log source, since it already exists
                    eventLog.Source = AppResources.WssEventLogSource;
                    string message;
                    format = format.Replace(@"\n", "\r\n");
                    if (args.Length > 0)
                    {
                        message = String.Format(format, args);
                    }
                    else
                    {
                        message = format;
                    }
                    eventLog.WriteEntry(String.Format(AppResources.AppError, message), type);
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

        #region ImpersonateAppPool
        /// <summary>
        /// Executes a supplied delegate while impersonating the application pool
        /// account.
        /// </summary>
        ///
        /// <param name="del">The delegate to execute.</param>
        ///
        internal static void ImpersonateAppPool(VoidDelegate del)
        {
            try
            {
                WindowsImpersonationContext m_context = null;
                try
                {
#if false
				m_context = AppPoolIdentity.Impersonate();
#else
                    m_context = WindowsIdentity.Impersonate(IntPtr.Zero);
#endif
                    del();
                }
                finally
                {
                    if (m_context != null)
                        m_context.Dispose();
                }
            }
            catch
            {
                // prevent exception filter exploits
                throw;
            }
        }

        #endregion

        #region Trim
        /// <summary>
        /// Trims the Spaces at both end of passed String if the value is not null
        /// Returns the Trimmed Value or Null
        /// </summary>
        /// <param name="value">A string containing space.</param>   
        /// <returns>string</returns>
        internal static string Trim(string value)
        {
            //check for the string is null or not if not trim the result 
            //else return the null string
            return (value != null) ? value.Trim() : value;
        }
        #endregion

        #region GetCrlfHtmlEncodedText
        /// <summary>
        /// Returns the HtmlEncoded Value for Carriage Return and LineFeed of the string 
        /// </summary>
        /// <param name="text">A string containing HTML".</param>   
        /// <returns>string</returns>
        internal static string GetCrlfHtmlEncodedText(string text)
        {
            text = GetHtmlEncodedText(text);
            //preserving spaces between words
            text = text.Replace(Constants.Space + Constants.Space,
                                Constants.NonBreakingSpace + Constants.Space);
            text = text.Replace(Constants.NonBreakingSpace + Constants.Space + Constants.Space,
                                Constants.NonBreakingSpace + Constants.Space + Constants.NonBreakingSpace);
            return text.Replace(Constants.CRLF,
                                Constants.LineBreak + Constants.CRLF);
        }
        #endregion

        #region GetHtmlEncodedText
        /// <summary>
        /// Trims the Spaces at both end of passed String if the value is not null
        /// Returns the HtmlEncoded Value of the string or empty
        /// </summary>
        /// <param name="text">A string containing HTML".</param>   
        /// <returns>string</returns>
        internal static string GetHtmlEncodedText(string text)
        {
            return String.IsNullOrEmpty(Trim(text)) ?
                   String.Empty : HttpContext.Current.Server.HtmlEncode(text);
        }
        #endregion

        #region GetLearnerAssignmentState
        /// <summary>
        /// Gets the localized string representation in text format of a LearnerAssignmentState value.
        /// </summary>
        /// <param name="learnerAssignmentState">The <c>LearnerAssignmentState</c> value.</param>
        public static string GetLearnerAssignmentState(LearnerAssignmentState learnerAssignmentState)
        {
            switch (learnerAssignmentState)
            {
                case LearnerAssignmentState.NotStarted:
                    return AppResources.LearnerAssignmentStatusNotStarted;
                    
                case LearnerAssignmentState.Active:
                    return AppResources.LearnerAssignmentStatusActive;
                    
                case LearnerAssignmentState.Completed:
                    return AppResources.LearnerAssignmentStatusCompleted;
                    
                case LearnerAssignmentState.Final:
                    return AppResources.LearnerAssignmentStatusFinal;
                    
                default:
                    return learnerAssignmentState.ToString();
            }
        }
        #endregion       

        #endregion

        /// <summary>
        /// Returns an <c>SPFile</c> given a <c>SharePointPackageStore</c>-style
        /// package location string.
        /// </summary>
        ///
        /// <param name="packageLocation">The package location string to parse.</param>
        ///
        /// <remarks>
        /// Note that the returned <c>SPFile</c> represents the entire collection of versions
        /// associated with that <paramref name="packageLocation"/> -- it is not version-specific.
        /// </remarks>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static SPFile GetSPFileFromPackageLocation(string packageLocation)
        {
            // Check parameters
            if(packageLocation == null)
                throw new ArgumentNullException("packageLocation");
                
            SharePointFileLocation fileLocation;

            if (SharePointFileLocation.TryParse(packageLocation, out fileLocation))
            {
                using (SPSite spSite = new SPSite(fileLocation.SiteId,SPContext.Current.Site.Zone))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(fileLocation.WebId))
                        return spWeb.GetFile(fileLocation.FileId);
                }
            }

            // If we got here, it's an error
            throw new ArgumentException(AppResources.IncorrectLocationStringSyntax,
                "packageLocation");
        }

		/// <summary>
		/// Combines url paths in a similar way to Path.Combine for file paths.
		/// Beginning and trailing slashes are not needed but will be accounted for
		/// if they are present.
		/// </summary>
		/// <param name="basePath">The start of the url. Beginning slashes will not be removed.</param>
		/// <param name="args">All other url segments to be added. Beginning and ending slashes will be
		/// removed and ending slashes will be added.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		internal static string UrlCombine(string basePath, params string[] args)
		{
			if (basePath == null)
		        throw new ArgumentNullException("basePath");
			if (args == null)
				throw new ArgumentNullException("args");

			if (basePath.EndsWith("/", StringComparison.Ordinal))
				basePath = basePath.Remove(basePath.Length - 1);

			StringBuilder sb = new StringBuilder(basePath);
			foreach (string path in args)
			{
				string tempPath = path;
				if (tempPath.EndsWith("/", StringComparison.Ordinal))
				{
					tempPath = tempPath.Remove(tempPath.Length - 1);
				}
				if (tempPath.StartsWith("/", StringComparison.Ordinal))
				{
					sb.AppendFormat("{0}", tempPath);
				}
				else
				{
					sb.AppendFormat("/{0}", tempPath);
				}
			}
			return sb.ToString();
		}

        /// <summary>
        /// Executes a delegate.  If a SQL Server deadlock occurs while executing the delegate,
        /// the delegate is re-executed, for a maximum of 5 attempts.
        /// </summary>
        ///
        /// <param name="del">The delegate to execute.  <b>Important:</b> since this delegate may
        ///     be executed more than once, it should not update any data outside the delegate
        ///     unless the update operation is safely repeatable.</param>
        ///
        public static void RetryOnDeadlock(VoidDelegate del)
        {
            if (del == null)
                throw new ArgumentNullException("del");
            int attemptsLeft = 5;
            while (attemptsLeft-- > 0)
            {
                try
                {
					del();

                    // if it was successful, return 
                    break;
                }
                catch (SqlException e)
                {
                    // try again if we're a deadlock victim AND we can try again
                    if ((e.Number == 1205) && (attemptsLeft > 0))
                        continue;
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Writes an opening HTML tag when the constructor is called and the corresponding closing HTML
    /// tag when <c>Dispose</c> is called.
    /// </summary>
    internal class HtmlBlock : IDisposable
    {

        #region Private Fields
        /// <summary>
        /// The <c>HtmlTextWriter</c> being written to.
        /// </summary>
        HtmlTextWriter m_htmlTextWriter;

        /// <summary>
        /// How many newlines to write after the end tag.
        /// </summary>
        int m_newLines;
        #endregion

        #region Constructor
        /// <summary>
        /// Writes an opening HTML tag.  The corresponding closing HTML tag is written when
        /// <c>Dispose</c> is called.
        /// </summary>
        ///
        /// <param name="hwTag">The name of the tag to write.</param>
        /// 
        /// <param name="newLines">How many newlines to write after the end tag.</param>
        ///
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        public HtmlBlock(HtmlTextWriterTag hwTag, int newLines, HtmlTextWriter htmlTextWriter)
        {
            m_htmlTextWriter = htmlTextWriter;
            m_newLines = newLines;
            m_htmlTextWriter.RenderBeginTag(hwTag);
        }
        #endregion

        #region Public Methods

        #region Dispose
        /// <summary>
        /// Writes the HTML closing tag corresponding to the opening tag written by the constructor.
        /// </summary>
        ///
        public void Dispose()
        {
            m_htmlTextWriter.RenderEndTag();
            for (int i = 0; i < m_newLines; i++)
                m_htmlTextWriter.WriteLine();
        }

        #endregion

        #region WriteFullTag
        /// <summary>
        /// Writes a full (i.e. self-closed) HTML tag.
        /// </summary>
        ///
        /// <param name="hwTag">The name of the tag to write.</param>
        /// 
        /// <param name="newLines">How many newlines to write after the end tag.</param>
        ///
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        public static void WriteFullTag(HtmlTextWriterTag hwTag, int newLines, HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.RenderBeginTag(hwTag);
            htmlTextWriter.RenderEndTag();
            for (int i = 0; i < newLines; i++)
                htmlTextWriter.WriteLine();
        }
        #endregion

        #region WriteBlankGif
        /// <summary>
        /// Writes HTML for a XxY-pixed blank GIF.
        /// </summary>
        ///
        /// <param name="width">The value for the "width" attribute of the "&lt;img&gt;" element.
        /// 	</param>
        ///
        /// <param name="height">The value for the "height" attribute of the "&lt;img&gt;" element.
        /// 	</param>
        ///
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        public static void WriteBlankGif(string width, string height, HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, Constants.BlankGifUrl);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, width);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, height);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Alt, "");
            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, htmlTextWriter);
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// A delegate with no parameters and no return value.
    /// </summary>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public delegate void VoidDelegate();

    

}
