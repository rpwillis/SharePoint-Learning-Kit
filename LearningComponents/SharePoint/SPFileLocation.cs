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
#region constructors
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
        public SharePointFileLocation(Guid siteId, Guid webId, Guid fileId, int versionId, DateTime timestamp) : this (siteId, webId, fileId, versionId, timestamp, null)
        {
        }

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
        /// <param name="extraInformation">Any extra information for the location.</param>
        public SharePointFileLocation(Guid siteId, Guid webId, Guid fileId, int versionId, DateTime timestamp, string extraInformation)
        {
            // No validation on parameters because if the values are not valid, it will become clear soon enough.
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            SiteId = siteId;
            WebId = webId;
            FileId = fileId;
            VersionId = versionId;
            ExtraInformation = extraInformation;
            if (timestamp != DateTime.MinValue)
            {
                Timestamp = timestamp;
            }
        }

        /// <summary>
        /// Create a SharePointFileLocation without providing a timestamp.
        /// </summary>
        /// <param name="siteId">The id of the site the file exists.</param>
        /// <param name="webId">The id of the SPWeb where the file exists.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <param name="versionId">The version of the file.</param>
        public SharePointFileLocation(Guid siteId, Guid webId, Guid fileId, int versionId) : this(siteId, webId, fileId, versionId, DateTime.MinValue)
        {
            Timestamp = GetTimeStamp(SiteId, WebId, FileId, VersionId);
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

            SiteId = web.Site.ID;
            WebId = web.ID;
            FileId = fileId;
            VersionId = versionId;
            Timestamp = GetTimeStamp(web, FileId, VersionId);
        }

        /// <summary>Create a SharePointFileLocation by providing the SPWeb and SPFile objects.</summary>
        /// <param name="web">The <see cref="SPWeb"/> containing the file.</param>
        /// <param name="file">The <see cref="SPFile"/>.</param>
        public SharePointFileLocation(SPWeb web, SPFile file)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("file", file);

            SiteId = web.Site.ID;
            WebId = web.ID;
            FileId = file.UniqueId;
            VersionId = file.UIVersion;
            Timestamp = file.TimeLastModified;
        }

        public SharePointFileLocation(string location)
        {
            string[] parts = location.Split('_');
            if (parts.Length < 5 || parts.Length > 6)
            {
                throw new ArgumentOutOfRangeException();
            }

            SiteId = new Guid(parts[0]);
            WebId = new Guid(parts[1]);
            FileId = new Guid(parts[2]);
            VersionId = Int32.Parse(parts[3], NumberFormatInfo.InvariantInfo);
            long ticks = long.Parse(parts[4], NumberFormatInfo.InvariantInfo);
            Timestamp = new DateTime(ticks);
            if (parts.Length == 6)
            {
                ExtraInformation = parts[5];
            }
        }

        /// <summary>
        /// Internal copy constructor.
        /// </summary>
        /// <param name="copyFrom">Package location to copy from.</param>
        internal SharePointFileLocation(SharePointFileLocation copyFrom)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("copyFrom", copyFrom);
            SiteId = copyFrom.SiteId;
            WebId = copyFrom.WebId;
            FileId = copyFrom.FileId;
            VersionId = copyFrom.VersionId;
            Timestamp = copyFrom.Timestamp;
        }
#endregion constructors

#region properties
        /// <summary>
        /// Gets the site id of the location.
        /// </summary>
        public Guid SiteId { get; private set; }

        /// <summary>
        /// Gets the identifier of the web portion of the location.
        /// </summary>
        public Guid WebId { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the file portion of the location.
        /// </summary>
        public Guid FileId { get; private set; }

        /// <summary>
        /// Gets the identifier of the version of the location.
        /// </summary>
        public int VersionId { get; private set; }

        /// <summary>
        /// Gets the timestamp associated with the location.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>Any extra information associated with the file location.</summary>
        public string ExtraInformation { get; private set; }
#endregion properties

#region public methods
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
                location = new SharePointFileLocation(locationValue);
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
            if (string.IsNullOrEmpty(ExtraInformation))
            {
                return String.Format(CultureInfo.CurrentCulture, "{0}_{1}_{2}_{3}_{4}",
                                        SiteId.ToString(), WebId.ToString(), FileId.ToString(),
                                        Convert.ToString(VersionId, CultureInfo.InvariantCulture),
                                        Convert.ToString(Timestamp.Ticks, CultureInfo.InvariantCulture));
            }
            else
            {
                return String.Format(CultureInfo.CurrentCulture, "{0}_{1}_{2}_{3}_{4}_{5}",
                                        SiteId.ToString(), WebId.ToString(), FileId.ToString(),
                                        Convert.ToString(VersionId, CultureInfo.InvariantCulture),
                                        Convert.ToString(Timestamp.Ticks, CultureInfo.InvariantCulture),
                                        ExtraInformation);
            }
        }

        /// <summary>Returns an <c>SPFile</c> represented by the <see cref="SharePointFileLocation"/>>.</summary>
        /// <remarks>
        /// Note that the returned <c>SPFile</c> represents the entire collection of versions
        /// associated with that location -- it is not version-specific.
        /// </remarks>
        public SPFile LoadFile()
        {
            bool isContextSite = false;
            bool isContextWeb = false;
            SPSite site = null;
            SPWeb web = null;

            if (SPContext.Current != null)
            {
                site = SPContext.Current.Site;
                web = SPContext.Current.Web;
            }

            if (site != null && site.ID == SiteId)
            {
                isContextSite = true;
            }
            else
            {
                site =  new SPSite(SiteId,SPContext.Current.Site.Zone);
            }

            try
            {
                try
                {
                    if (site != null && isContextSite && web.ID == WebId)
                    {
                        isContextWeb = true;
                    }
                    else
                    {
                        web = site.OpenWeb(WebId);
                    }

                    return web.GetFile(FileId);
                }
                finally
                {
                    if (isContextWeb == false)
                    {
                        web.Dispose();
                    }
                }
            }
            finally
            {
                if (isContextSite == false)
                {
                    site.Dispose();
                }
            }
        }
#endregion public methods

        /// <summary>Static helper function to get the timestamp.</summary>
        static DateTime GetTimeStamp(Guid siteId, Guid webId, Guid fileId, int versionId)
        {
            using (SPSite spSite = new SPSite(siteId))
            {
                using (SPWeb spWeb = spSite.OpenWeb(webId))
                {
                    return GetTimeStamp(spWeb, fileId, versionId);
                }
            }
        }

        static DateTime GetTimeStamp(SPWeb web, Guid fileId, int versionId)
        {
            SPFile file = web.GetFile(fileId);
            return GetTimeStamp(file, versionId);
        }

        static DateTime GetTimeStamp(SPFile file, int versionId)
        {
            if ((file.Versions.Count == 0) || file.UIVersion == versionId)
            {
                if (file.UIVersion != versionId)
                {
                    throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, file.Name));
                }

                return file.TimeLastModified;
            }
            else
            {
                // The specified version isn't the current one
                SPFileVersion spFileVersion = file.Versions.GetVersionFromID(versionId);

                if (spFileVersion == null)
                {
                    throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, file.Name));
                }

                return spFileVersion.Created;
            }
        }
    }

}
