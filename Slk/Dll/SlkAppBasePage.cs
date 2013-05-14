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
                        throw new SafeToDisplayException(AppResources.SessionNotConfigured);
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
Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("Culture: OnInit before setting: Current UI {0} : Current {1} AppResources {2}", Thread.CurrentThread.CurrentUICulture, Thread.CurrentThread.CurrentCulture, AppResources.Culture == null ? "null" : AppResources.Culture.Name);
        PageCulture = new SlkCulture(SPWeb);
Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("Culture: OnInit before setting: Current UI {0} : Current {1} AppResources {2}", Thread.CurrentThread.CurrentUICulture, Thread.CurrentThread.CurrentCulture, AppResources.Culture == null ? "null" : AppResources.Culture.Name);
        base.OnInit(e);
    }

    /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnPreInit"/>.</summary>
    protected override void OnPreInit(EventArgs e)
    {
        try
        {
            if (SlkStore.Settings.UseMasterPageForApplicationPages)
            {
                if (OverrideMasterPage)
                {
                    MasterPageFile = SPWeb.CustomMasterUrl;
                }
            }
        }
        catch (SafeToDisplayException)
        {
            // Error will be handled elsewhere
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

    /// <summary>
    /// Creates a query based on a given <c>QueryDefinition</c> and values for standard SLK
    /// macros.
    /// </summary>
    ///
    /// <param name="queryDef">The query definition to use to create the query.</param>
    ///
    /// <param name="countOnly">If <c>true</c>, the query will include minimal output columns,
    ///     since it will be assumed that the purpose of executing the query is purely to count the
    ///     rows.  If <c>false</c>, all columns specified by the query definition are included
    ///     in the query.</param>
    ///
    /// <param name="spWebScopeMacro">The value of the "SPWebScope" macro, or null if none.</param>
    ///
    /// <param name="columnMap">See QueryDefinition.CreateQuery.</param>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
    protected LearningStoreQuery CreateStandardQuery(QueryDefinition queryDef, bool countOnly,
        Guid? spWebScopeMacro, out int[,] columnMap)
    {
        return queryDef.CreateQuery(SlkStore.LearningStore, countOnly, delegate(string macroName)
        {
            if (macroName == "SPWebScope")
            {
                // return the GUID of the SPWeb that query results will be limited to (i.e.
                // filtered by), or null for no filter
                return spWebScopeMacro;
            }
            else
            if (macroName == "CurrentUserKey")
            {
                // return the LearningStore user key string value of the current user
                return SlkStore.CurrentUserKey;
            }
            else
            if (macroName == "Now")
            {
                return DateTime.Now.ToUniversalTime();
            }
            else
            if (macroName == "StartOfToday")
            {
                // return midnight of today
                return DateTime.Today.ToUniversalTime();
            }
            else
            if (macroName == "StartOfTomorrow")
            {
                // return midnight of tomorrow
                return DateTime.Today.AddDays(1).ToUniversalTime();
            }
            else
            if (macroName == "StartOfThisWeek")
            {
                // return midnight of the preceding Sunday** (or "Today" if "Today" is Sunday**)
                return StartOfWeek(DateTime.Today).ToUniversalTime();
            }
            else
            if (macroName == "StartOfNextWeek")
            {
                // return midnight of the following Sunday**
                return StartOfWeek(DateTime.Today).AddDays(7).ToUniversalTime();
            }
            else
            if (macroName == "StartOfWeekAfterNext")
            {
                // return midnight of the Sunday** after the following Sunday**
                return StartOfWeek(DateTime.Today).AddDays(14).ToUniversalTime();
            }
            else
                return null;
            // ** Actually, it's only Sunday for regional setting for which Sunday is the first
            // day of the week.  For example, using Icelandic regional settings, the first day of
            // the week is Monday, and that's what's used above.
        }, out columnMap);
    }

    /// <summary>
    /// Returns midnight on the day that begins the week containing a given date/time, using the
    /// current culture settings.
    /// </summary>
    ///
    /// <param name="dateTime">The given date/time.</param>
    ///
    private static DateTime StartOfWeek(DateTime dateTime)
    {
        // set <cultureInfo> to information about the current user's culture
        CultureInfo cultureInfo = CultureInfo.CurrentCulture;

        // this method imagines that today is the day of <dateTime>
        DateTime today = dateTime.Date;
        DayOfWeek currentDayOfWeek = today.DayOfWeek;
        DayOfWeek firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
        int delta = (int) firstDayOfWeek - (int) currentDayOfWeek;
        if (delta <= 0)
            return today.AddDays(delta);
        else
            return today.AddDays(delta - 7);
    }
}
}
