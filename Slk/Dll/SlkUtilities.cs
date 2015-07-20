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
        private const string baseUrlRegexString = @"\b(?:(?:https?|ftp|file|onenote:https?)://|www\.|ftp\.|mix\.office\.|sway\.)(?:\([-A-Z0-9+&@#/%=~_{}|$?!:;,.]*\)|[-A-Z0-9+&@#/%=~_{}|$?!:;,.])*(?:\([-A-Z0-9+&@#/%=~_{}|$?!:;,.]*\)|[A-Z0-9+&@#/%=~_{}|$])";
        // Markdown regex matches [display title](url). The display is in square brackets, the url is in parentheses
        private const string markdownRegexString = @"\[([^\]]+)\]\((" + baseUrlRegexString + @")\)";
        // url regex matches a url which is not preceeded by ]( i.e. the construct we would be expecting if it is a markdown url
        // Must also not be preceeded by :// or a string ofMarkdown format e.g. [title](http://www.codeplex.com) would match the www.codeplex.com bit, and not get
        // picked up as a markdown url
        private const string urlRegexString = @"(?<!\]\(|://|onenote:)" + baseUrlRegexString;

        static readonly Regex urlRegex = new Regex(urlRegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex markdownRegex = new Regex(markdownRegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex watchRegex = new Regex("watch", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #region Public Static Methods

        #region GetCurrentSPWeb

        /// <summary>
        /// Returns the <c>SPWeb</c> associated with the current <c>HttpRequest</c>.
        /// </summary>
        ///
        internal static SPWeb GetCurrentSPWeb()
        {
            return SPControl.GetContextWeb(HttpContext.Current);
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

        /// <summary>Does a case-insensitive contains search.</summary>
        /// <param name="input">The string to search.</param>
        /// <param name="value">The string to find.</param>
        /// <returns>True if value is in input.</returns>
        public static bool ContainsString(string input, string value)
        {
            return input.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Generates an Office Sway from an Office Sway</summary>
        /// <param name="link">The value of the link.</param>
        /// <returns>The proper embed url.</returns>
        public static string GenerateSwayUrl(string link)
        {
            string embedUrl = link;

            // Normal url is of form https://sway.com/aasJ_CLRUzF23dG2
            // Embed url is of form  https://sway.com/s/aasJ_CLRUzF23dG2/embed
            // So need to add the /s/ and the trailing embed

            if (ContainsString(link, "embed") == false)
            {
                // Re-jig the url to add the /s/ and the embed
                int index = embedUrl.IndexOf("sway.com", StringComparison.OrdinalIgnoreCase);
                embedUrl = "https://sway.com/s/" + embedUrl.Substring(index + 9);
                embedUrl = UrlCombine(embedUrl, "embed");
            }

            return embedUrl;
        }

        /// <summary>Generates an Office Mix embed url from an Office Mix url.</summary>
        /// <param name="link">The value of the link.</param>
        /// <returns>The proper embed url.</returns>
        public static string GenerateMixUrl(string link)
        {
            string embedUrl = watchRegex.Replace(link, "embed");
            if (embedUrl.Contains("://") == false)
            {
                embedUrl = "https://" + embedUrl;
            }

            return embedUrl;
        }

        /// <summary>Finds the links in the input.</summary>
        /// <param name="input">The text to find the links in.</param>
        /// <returns>A collection of matches.</returns>
        public static MatchCollection FindLinks(string input)
        {
            return urlRegex.Matches(input);
        }

        /// <summary>Turns urls into links.</summary>
        /// <param name="input">The text to change.</param>
        public static string ClickifyLinks(string input)
        {
            string intermediateValue = urlRegex.Replace(input, ClickfyLink);
            return markdownRegex.Replace(intermediateValue, ClickfyMarkdownLink);
        }

        private static string MakeLink(string title, string url, string defaultValue)
        {
            if (string.IsNullOrEmpty(url) == false)
            {
                if (url.Contains("://") == false)
                {
                    url = "http://" + url;
                }

                try
                {
                    // Make the url safe
                    // First unescape in case it's already been escaped e.g. OneNote urls
                    url = Uri.UnescapeDataString(url);
                    url = Uri.EscapeUriString(url);
                    return string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" class=\"slk-link\">{1}</a>", url, title);
                }
                catch (UriFormatException)
                {
                    // Invalid Url, don't replace
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        private static string ClickfyMarkdownLink(Match match)
        {
            if (string.IsNullOrEmpty(match.Value) == false)
            {
                string title = match.Groups[1].Value;
                string url = match.Groups[2].Value;
                if (string.IsNullOrEmpty(title))
                {
                    title = url;
                }

                return MakeLink(title, url, match.Value);
            }

            // If reached here something has gone wrong so don't replace
            return match.Value;
        }

        private static string ClickfyLink(Match match)
        {
            if (string.IsNullOrEmpty(match.Value) == false)
            {
                return MakeLink(match.Value, match.Value, match.Value);
            }

            // If reached here something has gone wrong so don't replace
            return match.Value;
        }

        #region GetLearnerAssignmentState
        /// <summary>
        /// Gets the localized string representation in text format of a LearnerAssignmentState value.
        /// </summary>
        /// <param name="learnerAssignmentState">The <c>LearnerAssignmentState</c> value.</param>
        public static string GetLearnerAssignmentState(LearnerAssignmentState? learnerAssignmentState)
        {
            SlkCulture culture = new SlkCulture();
            if (learnerAssignmentState == null)
            {
                return culture.Resources.LearnerAssignmentStatusNotStarted;
            }
            else
            {
                switch (learnerAssignmentState.Value)
                {
                    case LearnerAssignmentState.NotStarted:
                        return culture.Resources.LearnerAssignmentStatusNotStarted;
                        
                    case LearnerAssignmentState.Active:
                        return culture.Resources.LearnerAssignmentStatusActive;
                        
                    case LearnerAssignmentState.Completed:
                        return culture.Resources.LearnerAssignmentStatusCompleted;
                        
                    case LearnerAssignmentState.Final:
                        return culture.Resources.LearnerAssignmentStatusFinal;
                        
                    default:
                        return learnerAssignmentState.ToString();
                }
            }
        }
        #endregion       

        #endregion

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

        /// <summary>Determine if the browser is an iPad.</summary>
        public static bool IsIpad()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.UserAgent != null)
            {
                string upper = HttpContext.Current.Request.UserAgent.ToUpperInvariant();
                if (upper.Contains("IPAD"))
                {
                    return true;
                }
                else
                {
                    return upper.Contains("IPHONE");
                }
            }
            else
            {
                return false;
            }
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
        ///     </param>
        ///
        /// <param name="height">The value for the "height" attribute of the "&lt;img&gt;" element.
        ///     </param>
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
