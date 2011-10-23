/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Configure.aspx.cs
//
// Code-behind for Configure.aspx (used in SharePoint Central Administration to configure SLK).
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using System.Threading;
using Schema = Microsoft.SharePointLearningKit.Schema;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;

namespace Microsoft.SharePointLearningKit.AdminPages
{

    /// <summary>
    /// Code-behind for DownloadSettings.aspx.
    /// </summary>
    public class DownloadSettingsPage : System.Web.UI.Page
    {

        /// <summary>The OnInit event.</summary>
        protected override void OnInit(EventArgs e)
        {
            AppResources.Culture = Thread.CurrentThread.CurrentCulture;
            base.OnInit(e);
        }

        /// <summary>Initializes a new instance of <see cref="DownloadSettingsPage"/>.</summary>
        public DownloadSettingsPage()
        {
            Load += new EventHandler(DownloadSettingsPage_Load);
        }

        /// <summary>The page load event.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void DownloadSettingsPage_Load(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    // The page URL is of the following two forms:
                    //   1. http://.../DownloadSettings.aspx/<guid>/SlkSettings.xml
                    //   2. http://.../DownloadSettings.aspx/Default/SlkSettings.xml
                    // The following code parses <guid> into <siteGuid> (for case 1), or sets
                    // <siteGuid> to null (for case 2).
                    Uri uri = Request.Url;
                    if ((uri.Segments.Length < 3) ||
                        !String.Equals(uri.Segments[uri.Segments.Length - 1], "SlkSettings.xml",
                            StringComparison.OrdinalIgnoreCase))
                        throw new SafeToDisplayException(AppResources.DownloadSettingsIncorrectUrl);
                    string siteGuidOrDefault = uri.Segments[uri.Segments.Length - 2];
                                    siteGuidOrDefault = siteGuidOrDefault.Substring(0, siteGuidOrDefault.Length - 1);
                                    Guid? spSiteGuid;
                    if (String.Equals(siteGuidOrDefault, "Default",
                                                StringComparison.OrdinalIgnoreCase))
                                            spSiteGuid = null;
                                    else
                                    {
                        try
                        {
                            spSiteGuid = new Guid(siteGuidOrDefault);
                        }
                        catch (FormatException)
                        {
                            throw new SafeToDisplayException(
                                AppResources.DownloadSettingsIncorrectUrl);
                        }
                        catch (OverflowException)
                        {
                            throw new SafeToDisplayException(
                                AppResources.DownloadSettingsIncorrectUrl);
                        }
                    }

                    // set <settingXml> to the SLK Settings XML for <spSiteGuid> -- use the default
                    // SLK Settings XML if <spSiteGuid> is null or <spSiteGuid> is not configured for
                    // use with SLK
                    string settingsXml = null;
                    if (spSiteGuid != null)
                        settingsXml = SlkAdministration.GetSettingsXml(spSiteGuid.Value);
                    if (settingsXml == null)
                    {
                        // load the default SLK Settings
                        settingsXml = File.ReadAllText(Server.MapPath("SlkSettings.xml.dat"));
                    }

                    // write the XML to the browser
                    Response.ContentType = "text/xml";
                                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    // if the following line is un-commented, clicking "Open" in the IE File Download
                                    // dialog gives an error: "Cannot find 'C:\Documents and Settings\<user>\Local
                                    // Settings\Temporary Internet Files\Content.IE5\<path>\SlkSettings[1].xml'"
                    //Response.AddHeader("content-disposition", "attachment");
                    Response.Write(settingsXml);
                }
                catch (SafeToDisplayException ex)
                {
                    // an expected exception occurred
                    Response.Clear();
                    Response.ContentType = "text/html";
                                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(String.Format(CultureInfo.CurrentCulture, AppResources.AdminErrorPageHtml, ex.Message));
                    Response.End();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // thrown by Response.End above
                throw;
            }
            catch (Exception ex)
            {
                // an unexpected exception occurred
                Response.Clear();
                Response.ContentType = "text/html";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Write(String.Format(CultureInfo.CurrentCulture, AppResources.AdminErrorPageHtml, Server.HtmlEncode(AppResources.SeriousErrorInEventLog)));
                Microsoft.SharePointLearningKit.WebControls.SlkError.WriteToEventLog(ex);
                Response.End();
            }
        }
    }

}

