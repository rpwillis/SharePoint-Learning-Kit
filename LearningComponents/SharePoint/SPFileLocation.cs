/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// Represents the location of a file in a SharePoint document library.This class stores and 
    /// parses location information.
    /// </summary>
    public class SharePointFileLocation
    {
        private Guid m_siteId;
        private Guid m_webId;
        private Guid m_fileId;
        private int m_versionId;
        private DateTime m_timestamp;

        /// <summary>
        /// Create a SharePointFileLocation with optional timestamp.
        /// </summary>
        /// <param name="siteId">The id of the site the file exists.</param>
        /// <param name="webId">The id of the SPWeb where the file exists.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <param name="versionId">The version of the file.</param>
        /// <param name="timestamp">The timestamp associated with the file. If versioning is turned on
        /// in the document library, this is the time the <paramref name="versionId"/> was created. If there 
        /// is no versioning, then it is the last modified time of the file.
        /// </param>
        public SharePointFileLocation(Guid siteId, Guid webId, Guid fileId, int versionId, DateTime timestamp)
        {
            // No validation on parameters because if the values are not valid, it will become clear soon enough.
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            m_siteId = siteId;
            m_webId = webId;
            m_fileId = fileId;
            m_versionId = versionId;
            m_timestamp = timestamp;
        }

        /// <summary>
        /// Create a SharePointFileLocation without providing a timestamp.
        /// </summary>
        /// <param name="siteId">The id of the site the file exists.</param>
        /// <param name="webId">The id of the SPWeb where the file exists.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <param name="versionId">The version of the file.</param>
        public SharePointFileLocation(Guid siteId, Guid webId, Guid fileId, int versionId)
        {
            // No validation on parameters because if the values are not valid, it will become clear soon enough.
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            
            m_siteId = siteId;
            m_webId = webId;
            m_fileId = fileId;
            m_versionId = versionId;
            m_timestamp = GetTimeStamp(m_siteId, m_webId, m_fileId, m_versionId);
        }

        /// <summary>
        /// Create a SharePointFileLocation by providing the SPWeb object.
        /// </summary>
        /// <param name="web">The web containing the file. This is only used to provide the 
        /// web and site ids. The web is not otherwise accessed by this class and should be disposed 
        /// by the caller.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <param name="versionId">The version of the file.</param>
        /// <remarks></remarks>
        public SharePointFileLocation(SPWeb web, Guid fileId, int versionId)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("web", web);

            m_siteId = web.Site.ID;
            m_webId = web.ID;
            m_fileId = fileId;
            m_versionId = versionId;
            m_timestamp = GetTimeStamp(m_siteId, m_webId, m_fileId, m_versionId);
        }

        /// <summary>
        /// Internal copy constructor.
        /// </summary>
        /// <param name="copyFrom">Package location to copy from.</param>
        internal SharePointFileLocation(SharePointFileLocation copyFrom)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("copyFrom", copyFrom);
            m_siteId = copyFrom.SiteId;
            m_webId = copyFrom.WebId;
            m_fileId = copyFrom.FileId;
            m_versionId = copyFrom.VersionId;
            m_timestamp = copyFrom.Timestamp;
        }

        /// <summary>
        /// Gets the site id of the location.
        /// </summary>
        public Guid SiteId { get { return m_siteId; } }

        /// <summary>
        /// Gets the identifier of the web portion of the location.
        /// </summary>
        public Guid WebId { get { return m_webId; } }

        /// <summary>
        /// Gets the unique identifier of the file portion of the location.
        /// </summary>
        public Guid FileId { get { return m_fileId; } }

        /// <summary>
        /// Gets the identifier of the version of the location.
        /// </summary>
        public int VersionId { get { return m_versionId; } }

        /// <summary>
        /// Gets the timestamp associated with the location.
        /// </summary>
        public DateTime Timestamp { get { return m_timestamp; } }

        /// <summary>
        /// Attempts to parse a location string into a SharePointFileLocation object. 
        /// </summary>
        /// <param name="locationValue">The string containing the location information. This must 
        /// be of the form provided by the <c>SharePointFileLocation.ToString()</c> method.</param>
        /// <param name="location">The location containing the parsed values. If the method returns 
        /// false, this object should be ignored.</param>
        /// <returns>True if the <paramref name="locationValue"/> was successfully parsed, otherwise 
        /// false. If true, the <paramref name="location"/> contains the parsed values.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // invalid values return false
        public static bool TryParse(string locationValue, out SharePointFileLocation location)
        {
            Utilities.ValidateParameterNonNull("locationValue", locationValue);
            Utilities.ValidateParameterNotEmpty("locationValue", locationValue);

            location = null;

            // if the location was valid, set all out parameter values and return true
            try
            {
                string[] parts = locationValue.Split('_');
                if (parts.Length != 5)
                    return false;

                Guid siteId = new Guid(parts[0]);
                Guid webId = new Guid(parts[1]);
                Guid fileId = new Guid(parts[2]);
                int versionId = Int32.Parse(parts[3], NumberFormatInfo.InvariantInfo);
                long ticks = long.Parse(parts[4], NumberFormatInfo.InvariantInfo);
                DateTime timestamp = new DateTime(ticks);
                location = new SharePointFileLocation(siteId, webId, fileId, versionId, timestamp);
            }
            catch
            {
                // if the location was not valid for any reason, return false
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the location formatted as a string.
        /// </summary>
        /// <returns>The string format of the location.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}_{1}_{2}_{3}_{4}",
                                    m_siteId.ToString(), m_webId.ToString(), m_fileId.ToString(),
                                    Convert.ToString(m_versionId, CultureInfo.InvariantCulture),
                                    Convert.ToString(m_timestamp.Ticks, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Static helper function to get the timestamp
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]    // handled by disposer
        private static DateTime GetTimeStamp(Guid siteId, Guid webId, Guid fileId, int versionId)
        {
            using (Disposer disposer = new Disposer())
            {
                SPSite spSite = new SPSite(siteId);
                disposer.Push(spSite);

                SPWeb spWeb = spSite.OpenWeb(webId);
                disposer.Push(spWeb);

                SPFile spFile = spWeb.GetFile(fileId);

                if ((spFile.Versions.Count == 0) || spFile.UIVersion == versionId)
                {
                    if (spFile.UIVersion != versionId)
                        throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, spFile.Name));

                    return spFile.TimeLastModified;
                }
                else
                {
                    // The specified version isn't the current one
                    SPFileVersion spFileVersion = spFile.Versions.GetVersionFromID(versionId);

                    if (spFileVersion == null)
                        throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, spFile.Name));

                    return spFileVersion.Created;
                }
            }
        }
    }

}
