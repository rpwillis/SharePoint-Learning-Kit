/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkAppBasePage
//
// Base class for all application .aspx pages.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;
using System.Threading;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{

/// <summary>
/// Helps implement this MLC web-based application.  ASP.NET web pages can be
/// based on this class.
/// </summary>
///
public class SlkAppBasePage : Microsoft.SharePoint.WebControls.LayoutsPageBase 
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    private string sourceUrl;
    private string rawSourceUrl;

    /// <summary>
    /// Holds the value of the <c>SPWeb</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPWeb m_spWeb;

    /// <summary>
    /// Holds the value of the <c>SlkStore</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkStore m_slkStore;

    /// <summary>
    /// Is true if the current user is an observer and false otherwise. Used to avoid repeated calls to
    /// SlkStore
    /// </summary>
    bool? m_isObserver;

    /// <summary>
    /// Holds the learner key which is used as the user for the LearningStore in case of the observer
    /// </summary>
    string m_observerRoleLearnerKey;

    //////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the current <c>SPWeb</c>.
    /// </summary>
    public SPWeb SPWeb
    {
        [DebuggerStepThrough]
        get
        {
            if (m_spWeb == null)
                m_spWeb = SlkUtilities.GetCurrentSPWeb();
            return m_spWeb;
        }
    }

    /// <summary>
    /// Gets the current <c>SlkStore</c>.
    /// </summary>
    public virtual SlkStore SlkStore
    {
        get { return LocalSlkStore; }
    }

    /// <summary>
    /// Gets the Current Culture Number Format Info used 
    /// to Format and display Numeric Values
    /// </summary>
    public static NumberFormatInfo NumberFormatInfo
    {
        get
        {
            return CultureInfo.CurrentCulture.NumberFormat;
        }
    }

    #region protected properties
    ///<summary>Gets the value of the "Source" query parameter, the URL of the source page.</summary>
    protected string RawSourceUrl
    {
        get
        {
            if (string.IsNullOrEmpty(rawSourceUrl))
            {
                rawSourceUrl = QueryString.ParseString("Source");
            }

            return rawSourceUrl;
        }
    }

    ///<summary>Gets the value of the "Source" query parameter, the URL of the source page.</summary>
    protected string SourceUrl
    {
        get
        {
            if (string.IsNullOrEmpty(sourceUrl))
            {
                sourceUrl = HttpUtility.UrlDecode(RawSourceUrl);
            }

            return sourceUrl;
        }
    }

    /// <summary>Indicates if should override master page.</summary>
    protected virtual bool OverrideMasterPage
    {
        get { return true; ;}
    }

    /// <summary>The culture to use for the page.</summary>
    protected SlkCulture PageCulture { get; private set; }

    /// <summary>
    /// Gets the learnerKey session parameter which is used as the LearningStore user
    /// if the user is an SlkObserver
    /// </summary>
    protected string ObserverRoleLearnerKey
    {
        get
        {
            if (m_observerRoleLearnerKey == null)
            {
                if (IsObserver)
                {
                    try
                    {
                        if (Session["LearnerKey"] != null)
                        {
                            m_observerRoleLearnerKey = Session["LearnerKey"].ToString();
                        }
                    }
                    catch (HttpException)
                    {
                        throw new SafeToDisplayException(PageCulture.Resources.SessionNotConfigured);
                    }
                }
            }
            return m_observerRoleLearnerKey;
        }
    }

    /// <summary>
    /// Returns true if the current user is an observer
    /// and false otherwise
    /// </summary>
    protected bool IsObserver
    {
        get
        {
            if (m_isObserver == null)
            {
                if (this.LocalSlkStore.IsObserver(SPWeb) == true)
                {
                    m_isObserver = true;
                }
                else if (this.LocalSlkStore.IsInstructor(SPWeb) == true && (Request.QueryString[QueryStringKeys.ForObserver] == "true"))
                {
                    m_isObserver = true;
                }
                else
                {
                    m_isObserver = false;
                }
            }
            return (bool)m_isObserver;
        }
    }

    #endregion protected properties

    #region private properties
    /// <summary>
    /// Gets the current <c>SlkStore</c>.
    /// </summary>
    public virtual SlkStore LocalSlkStore
    {

        get
        {
            if (m_slkStore == null)
            {
                m_slkStore = SlkStore.GetStore(SPWeb);
            }
            return m_slkStore;
        }
    }

    #endregion private properties


    //////////////////////////////////////////////////////////////////////////////////////////////
    // Protected Methods
    //

    /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnInit"/>.</summary>
    protected override void OnInit(EventArgs e)
    {
        PageCulture = new SlkCulture(SPWeb);
        base.OnInit(e);
    }

    /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnPreInit"/>.</summary>
    protected override void OnPreInit(EventArgs e)
    {
        try
        {
            if (OverrideMasterPage)
            {
                if (SPWeb != null)
                {
                    AnonymousSlkStore store = AnonymousSlkStore.GetStore(SPWeb.Site);
                    if (store.Settings.UseMasterPageForApplicationPages)
                    {
                        MasterPageFile = SPWeb.CustomMasterUrl;
                    }
                }
            }
        }
        catch (UserNotFoundException)
        {
            // No SPWeb.CurrentUser, so store need resetting
            m_slkStore = null;
        }
        catch (SafeToDisplayException)
        {
        }
        catch (Exception)
        {
            // Don't let an error stop the page rendering. Let the error happen during the actual rendering and be handled there.
            // Only issue this may cause is incorrect master page.
            m_slkStore = null;
        }

        base.OnPreInit(e);
    }


    /// <summary>
    /// Returns a copy of the current page's query string (beginning with "?") with a given
    /// query string parameter key changed to a different value, or added if it doesn't exist.
    /// </summary>
    ///
    /// <param name="replacementKey">The key (name) of the query string parameter to replace,
    ///     e.g. "Sort".</param>
    ///
    /// <param name="replacementValue">The new value for the query parameter.</param>
    ///
    /// <remarks>
    /// For example, if the query string is "?X=1&amp;Y=2" and <paramref name="replacementKey"/> is
    /// "X" and <paramref name="replacementValue"/> is "7", "?Y=2&amp;X=7" is returned (note that
    /// the order of query parameters may change).  If the query string is "?X=1&amp;Y=2" and
    /// <paramref name="replacementKey"/> is "Z" and <paramref name="replacementValue"/> is "7",
    /// "?X=1&amp;Y=2&amp;Z=7" is returned.
    /// </remarks>
    ///
    protected string GetAdjustedQueryString(string replacementKey, string replacementValue)
    {
        StringBuilder result = new StringBuilder(1000);
        foreach (string key in Request.QueryString.Keys)
        {
            if (String.Compare(key, replacementKey, true, CultureInfo.InvariantCulture) != 0)
            {
                result.Append((result.Length == 0) ? '?' : '&');
                result.Append(HttpUtility.UrlEncode(key));
                result.Append('=');
                result.Append(HttpUtility.UrlEncode(Request.QueryString[key]));
            }
        }
        result.Append((result.Length == 0) ? '?' : '&');
        result.Append(HttpUtility.UrlEncode(replacementKey));
        result.Append('=');
        result.Append(HttpUtility.UrlEncode(replacementValue));
        return result.ToString();
    }

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// 
    /// <param name="format">A <c>String.Format</c>-style formatting string.</param>
    /// 
    /// <param name="args">Formatting arguments.</param>
    /// 
    /// <remarks>
    /// The default implementation throws an exception with the given string as message text.
    /// </remarks>
    ///
    protected virtual void DisplayError(string format, params object[] args)
    {
        throw new SafeToDisplayException(format, args);
    }

}
}
