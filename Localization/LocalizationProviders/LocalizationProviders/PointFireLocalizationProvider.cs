using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePointLearningKit.Localization;
using System.Globalization;
using System.Configuration;
using System.Web.Configuration;
using Microsoft.SharePoint;
using System.Web;
using System.Collections;

namespace Itworx.Localization.LocalizationProviders
{
    public class PointFireLocalizationProvider:ILocalizationProvider
    {
        #region Constants
        private const string pointFireLanguageCookieNameKey = "PFLanguageCookieKeyName";
        private const string languageSessionName = "PF_LCID__";
        private const int emptyLcid = -1;
        private const int defaultLCID = 1033;
        #endregion

        #region Static Memebers
        private static Hashtable culturesTable = new Hashtable();
        private static object sync = new object();
        #endregion

        #region ILocalizationProvider Members

        CultureInfo ILocalizationProvider.CurrentCulture
        {
            get { return CreateCulture(GetPointFireLCID()); }
        }

        private CultureInfo CreateCulture(int lcid)
        {
            if (culturesTable.ContainsKey(lcid))
            {
                return culturesTable[lcid] as CultureInfo;
            }
            else
            {
                CultureInfo culture = new CultureInfo(lcid);
                lock (sync)
                {   
                    culturesTable.Add(lcid, culture);
                }
                return culture;
            }
        }

        #endregion


        #region PointFire Cluture Logic 

        private int GetPointFireLCID()
        { 
           
            string pointFireCookieName = GetCookieName();
            //first , get query string value for the key
            int queryStringLCID = GetQueryStringLCIDValue(pointFireCookieName);

            if (queryStringLCID.Equals(emptyLcid))
            {
                //nothing set explicitly, try the session
                int sessionLCID = GetSessionValue();
                if (sessionLCID.Equals(emptyLcid))
                {
                    //nothing is set before, try the cookie
                    int cookieLCID = GetCookieValue(pointFireCookieName);
                    if (cookieLCID.Equals(emptyLcid))
                    {
                        //nothing is totally set, seems first login, set session with default
                        SetSessionValue(defaultLCID);
                        return defaultLCID;
                    }
                    else
                    { 
                        //cookie has value, update the session
                        SetSessionValue(cookieLCID);
                        return cookieLCID;
                    }
                }
                else
                {
                    return sessionLCID;
                }
            }
            else
            { 
                //something set explicitly, update the session and return it
                SetSessionValue(queryStringLCID);
                return queryStringLCID;
            }
        }

       
        /// <summary>
        /// Return Cookie Name Used By Point Fire PFLanguageCookieKeyName
        /// </summary>
        /// <returns></returns>
        private string GetCookieName()
        {
            try
            {
                return ConfigurationManager.AppSettings[pointFireLanguageCookieNameKey];
            }
            catch (Exception ex)
            {
                throw new PointFireException("Error in reading PointFire CookieName key value", ex);
            }
        }

        /// <summary>
        /// Return Lcid from QueryString Used By Point Fire
        /// </summary>
        /// <returns></returns>
        private int GetQueryStringLCIDValue(string queryStringName)
        {
            try
            {
                string lcidString = HttpContext.Current.Request.QueryString[queryStringName];
                return GetValidLCID(lcidString);
            }
            catch (Exception ex)
            {
                throw new PointFireException("Error in reading PointFire Query string value for the LCID query string parameter: "+queryStringName, ex);
            }
        }

        /// <summary>
        /// Gets integer value of LCID on a string, if not valid, gets default LCID
        /// </summary>
        /// <param name="targetLCID">String contains LCID</param>
        /// <returns>integer value of LCID on the string if valid, if not valid, returns default LCID</returns>
        private int GetValidLCID(string targetLCID)
        {
            int lcid;
            bool isValidLCID = int.TryParse(targetLCID, out lcid);
            return (isValidLCID) ? lcid : emptyLcid;
        }


        private void SetSessionValue(int lcid)
        {
            try
            {
                HttpContext.Current.Session[languageSessionName] = lcid;
            }
            catch (Exception ex)
            {
                throw new PointFireException("Error while setting session LCID with value: "+lcid, ex); ;
            }
        }


        /// <summary>
        /// Return Lcid From Cookie Used By Point Fire
        /// </summary>
        /// <returns></returns>
        private int GetCookieValue(string cookieName)
        {
            try
            {
                HttpCookie languageCookie = HttpContext.Current.Request.Cookies[cookieName];
                if (languageCookie != null)
                {
                    string cookieValue = languageCookie.Value;
                    //Itworx.IctQatar.MLG.BackEnd.LoggingService.LogInformation("LCID: " + val);
                    string[] cookieValueTokens = cookieValue.Split('=');
                    string lcidString = cookieValueTokens.Length > 1 ? cookieValueTokens[1] : cookieValue;
                    return GetValidLCID(lcidString);
                }
                else
                {
                    //create the cookie

                    HttpCookie PointFire = new HttpCookie(cookieName);
                    PointFire[cookieName] = cookieName + "=" + defaultLCID;
                    PointFire.Expires = DateTime.Now.AddYears(1);
                    HttpContext.Current.Response.Cookies.Add(PointFire);
                    return defaultLCID;
                }
            }
            catch (Exception ex)
            {
                throw new PointFireException("Error while reading PointFire cookie value for cookie name: "+cookieName, ex);
            }
        }

        /// <summary>
        /// Gets user session LCID value
        /// </summary>
        /// <returns></returns>
        private int GetSessionValue()
        {
            object sesionValue = HttpContext.Current.Session[languageSessionName];
            return sesionValue != null ? (int)sesionValue : emptyLcid;
        }


        /// <summary>
        /// This function is used to get keya values from appsettings section 
        /// </summary>
        /// <param name="site">target site</param>
        /// <param name="key">key</param>
        /// <returns>key value</returns>
        //public static String GetValueFromConfigSection(SPSite site, string key)
        //{
        //    string keyValue = string.Empty;
        //    Configuration config = WebConfigurationManager.OpenWebConfiguration("/", site.WebApplication.Name);
        //    AppSettingsSection configSection = config.AppSettings;

        //    if (configSection.Settings[key] != null)
        //        keyValue = configSection.Settings[key].Value;

        //    return keyValue;
        //}

        #endregion
    }
}
