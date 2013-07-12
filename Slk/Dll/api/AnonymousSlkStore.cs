/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkApi.cs
//
// Implements SlkStore and related types.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Library;
using Microsoft.SharePoint.Navigation;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Microsoft.SharePointLearningKit
{

    /// <summary>
    /// Represents a connection to a SharePoint Learning Kit store.  Doesn't hold any information tied
    /// to the identity of the current user, so an <c>AnonymousSlkStore</c> can be cached on a Web site
    /// and shared by multiple users.
    /// </summary>
    ///
    internal class AnonymousSlkStore
    {
        /// <summary>
        /// Specifies for how many seconds <c>SlkStore</c> is cached within the current
        /// <c>HttpContext</c>.  This many seconds after being loaded, the next time you create an
        /// instance of <c>SlkStore</c>, information is reloaded from the SharePoint configuration
        /// database, and <c>SlkSettings</c> information is reloaded from the SharePoint Learning Kit
        /// database.
        /// </summary>
        internal const int HttpContextCacheTime = 60;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>SPSiteGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spSiteGuid;

        /// <summary>
        /// Holds the value of the <c>Mapping</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkSPSiteMapping m_mapping;

        /// <summary>
        /// Holds the value of the <c>Settings</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkSettings m_settings;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this <c>AnonymousSlkStore</c>.
        /// </summary>
        public Guid SPSiteGuid
        {
            [DebuggerStepThrough]
            get
            {
                return m_spSiteGuid;
            }
        }

        /// <summary>
        /// Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this
        /// <c>AnonymousSlkStore</c>.
        /// </summary>
        public SlkSPSiteMapping Mapping
        {
            [DebuggerStepThrough]
            get
            {
                return m_mapping;
            }
        }

        /// <summary>
        /// Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this
        /// <c>AnonymousSlkStore</c>.
        /// </summary>
        public SlkSettings Settings
        {
            [DebuggerStepThrough]
            get
            {
                return m_settings;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
        ///     </param>
        ///
        /// <param name="mapping">The value to use to initialize the <c>Mapping</c> property.
        ///     </param>
        ///
        /// <param name="settings">The value to use to initialize the <c>Settings</c> property.
        ///     </param>
        ///
        public AnonymousSlkStore(Guid spSiteGuid, SlkSPSiteMapping mapping, SlkSettings settings)
        {
            m_spSiteGuid = spSiteGuid;
            m_mapping = mapping;
            m_settings = settings;
        }

        /// <summary>Get the anonymous store.</summary>
        /// <param name="site">The site to get the settings for.</param>
        /// <returns></returns>
        public static AnonymousSlkStore GetStore(SPSite site)
        {
            Guid siteId = site.ID;
            // set <httpContext> to the current HttpContext (null if none)
            HttpContext httpContext = HttpContext.Current;

            // if an AnonymousSlkStore corresponding to <spSiteGuid> is cached, retrieve it, otherwise
            // create one
            string cacheItemName = null;
            AnonymousSlkStore anonymousSlkStore;
            if (httpContext != null)
            {
                    cacheItemName = String.Format(CultureInfo.InvariantCulture, "SlkStore_{0}", siteId);
                    anonymousSlkStore = (AnonymousSlkStore) httpContext.Cache.Get(cacheItemName);
                    if (anonymousSlkStore != null)
                    {
                        return anonymousSlkStore;
                    }
            }

            // set <mapping> to the SlkSPSiteMapping corresponding to <siteId>; if no such
            // mapping, exists, a SafeToDisplayException is thrown
            SlkSPSiteMapping mapping = SlkSPSiteMapping.GetRequiredMapping(site);

            // load "SlkSettings.xsd" from a resource into <xmlSchema>
            XmlSchema xmlSchema;
            using (StringReader schemaStringReader = new StringReader(AppResources.SlkSettingsSchema))
            {
                    xmlSchema = XmlSchema.Read(schemaStringReader,
                            delegate(object sender2, ValidationEventArgs e2)
                            {
                                    // ignore warnings (already displayed when SLK Settings file was uploaded)
                            });
            }

            // create a LearningStore. Read is in privileged scope so irrelevant what key is.
            // Cannot use current user as may be be called in a page PreInit event when it's not necessarily valid
            string learningStoreKey = "SHAREPOINT\\System";
            LearningStore learningStore = new LearningStore(mapping.DatabaseConnectionString, learningStoreKey, ImpersonationBehavior.UseOriginalIdentity);

            // read the SLK Settings file from the database into <settings>
            SlkSettings settings;
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                LearningStoreJob job = learningStore.CreateJob();
                LearningStoreQuery query = learningStore.CreateQuery(Schema.SiteSettingsItem.ItemTypeName);
                query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
                query.AddColumn(Schema.SiteSettingsItem.SettingsXmlLastModified);
                query.AddCondition(Schema.SiteSettingsItem.SiteGuid, LearningStoreConditionOperator.Equal, mapping.SPSiteGuid);
                job.PerformQuery(query);
                DataRowCollection dataRows = job.Execute<DataTable>().Rows;
                if (dataRows.Count != 1)
                {
                    throw new SafeToDisplayException(AppResources.SlkSettingsNotFound, site.Url);
                }
                DataRow dataRow = dataRows[0];
                string settingsXml = (string)dataRow[0];
                DateTime settingsXmlLastModified = ((DateTime)dataRow[1]);
                using (StringReader stringReader = new StringReader(settingsXml))
                {
                        XmlReaderSettings xmlSettings = new XmlReaderSettings();
                        xmlSettings.Schemas.Add(xmlSchema);
                        xmlSettings.ValidationType = ValidationType.Schema;
                        using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlSettings))
                        {
                            settings = new SlkSettings(xmlReader, settingsXmlLastModified);
                        }
                }
            }
            
            // create and (if possible) cache the new AnonymousSlkStore object
            anonymousSlkStore = new AnonymousSlkStore(siteId, mapping, settings);
            DateTime cacheExpirationTime = DateTime.Now.AddSeconds(HttpContextCacheTime);
            if (httpContext != null)
            {
                    httpContext.Cache.Add(cacheItemName, anonymousSlkStore, null, cacheExpirationTime,
                            System.Web.Caching.Cache.NoSlidingExpiration,
                            System.Web.Caching.CacheItemPriority.Normal, null);
            }

            return anonymousSlkStore;
        }
    }
}

