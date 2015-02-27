using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.IO;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.LearningComponents.Storage;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A file in an assignment.</summary>
    public class AssignmentFile
    {

#region properties
        /// <summary>The file's Url.</summary>
        public string PermMask { get; private set; }

        /// <summary>The file's Url.</summary>
        public string Url { get; private set; }

        /// <summary>The file's ID.</summary>
        public int Id { get; private set; }

        /// <summary>The file's Name.</summary>
        public string Name { get; private set; }

        /// <summary>The extenstion of the file.</summary>
        public string Extension
        {
            get { return System.IO.Path.GetExtension(Url) ;}
        }

        /// <summary>Detemines if the file is an office file.</summary>
        public bool IsOfficeFile
        {
            get { return IsOfficeFileByExtension(Extension) ;}
        }

        /// <summary>Detemines if the file is an office file.</summary>
        public bool IsOwaCompatible
        {
            get
            {
                if (Extension == null)
                {
                    return false;
                }

                switch (Extension.ToUpperInvariant())
                {
                    case ".DOC":        // Strictly speaking not so, but will offer to convert
                    case ".DOCX":
                        return true;
                    case ".XLSX":
                        return true;
                    case ".PPTX":
                        return true;
                    case ".ONE":
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>Shows if the document is editable.</summary>
        public bool IsEditable
        {
            get
            {
                if (Extension == null)
                {
                    return false;
                }
                else if (IsOfficeFile)
                {
                    return true;
                }

                switch (Extension.ToUpperInvariant())
                {
                    case ".PDF":
                        return false;
                    case ".TXT":
                        return true;
                    case ".RTF":
                        return true;
                    default:
                        return false;
                }
            }
        }
#endregion properties

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentFile"/>.</summary>
        /// <param name="id">The id of the file.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="url">The file's url.</param>
        /// <param name="permMask">The permissions mask for the file.</param>
        public AssignmentFile(int id, string name, string url, string permMask)
        {
            Id = id;
            Name = name;
            Url = url;
            PermMask = permMask;
        }
#endregion constructors

#region public methods
        /// <summary>Generates the office protocol uri for iPads.</summary>
        /// <param name="web">The web being used.</param>
        /// <param name="sourceUrl">The source page.</param>
        /// <returns>The uri.</returns>
        public string GenerateOfficeProtocolUrl(SPWeb web, string sourceUrl)
        {
            string protocol = null;

            switch (Extension.ToUpperInvariant())
            {
                case ".DOC":
                case ".DOCX":
                    protocol = "ms-word";
                    break;
                case ".XLS":
                case ".XLSX":
                    protocol = "ms-excel";
                    break;
                case ".PPT":
                case ".PPTX":
                    protocol = "ms-powerpoint";
                    break;
                case ".ONE":
                    protocol = "onenote";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Extension.ToUpperInvariant());
            }

            string openMode = "ofe";
            string formatString = "{0}:{1}|u|{2}{3}|p|{2}{4}";

            Uri uri = new Uri(web.Url);
            string server = uri.GetLeftPart(UriPartial.Authority);

            return string.Format(CultureInfo.InvariantCulture, formatString, protocol, openMode, server, Url, sourceUrl);
        }

        /// <summary>Generate the edit url for office web apps.</summary>
        /// <param name="web">The web to open in.</param>
        /// <param name="sourceUrl">The source page.</param>
        /// <returns>The url.</returns>
        public string GenerateOfficeAppsEditUrl(SPWeb web, string sourceUrl)
        {
            string formatString = null;

#if SP2013
            formatString = "{0}/_layouts/15/WopiFrame.aspx?sourcedoc={1}&action=default";
#else
            switch (Extension.ToUpperInvariant())
            {
                case ".DOC":
                    // Not actually editing, but once in OWA can convert
                    formatString = "{0}/_layouts/WordViewer.aspx?id={1}&source={2}";
                    break;
                case ".DOCX":
                    formatString = "{0}/_layouts/WordEditor.aspx?id={1}&source={2}";
                    break;
                case ".XLSX":
                    formatString = "{0}/_layouts/xlviewer.aspx?DefaultItemOpen=1&Edit=1&id={1}&source={2}";
                    break;
                case ".PPTX":
                    formatString = "{0}/_layouts/PowerPoint.aspx?PowerPointView=EditView&PresentationId={1}&source={2}";
                    break;
                case ".ONE":
                    formatString = "{0}/_layouts/OneNote.aspx?id={1}&Edit=1&source={2}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Extension.ToUpperInvariant());
            }
#endif
            // formatString = "ms-word:ofe|u|http://wingtipserver{1}";

            // return string.Format(CultureInfo.InvariantCulture, formatString, web.Url, Url, HttpUtility.UrlEncode(sourceUrl));
            return string.Format(CultureInfo.InvariantCulture, formatString, web.Url, HttpUtility.UrlEncode(Url), HttpUtility.UrlEncode(sourceUrl));
        }

        /// <summary>Generate the view url for office web apps.</summary>
        /// <param name="web">The web to open in.</param>
        /// <param name="sourceUrl">The source page.</param>
        /// <returns>The url.</returns>
        public string GenerateOfficeAppsViewUrl(SPWeb web, string sourceUrl)
        {
            string formatString = null;

#if SP2013
            formatString = "{0}/_layouts/15/WopiFrame.aspx?sourcedoc={1}&action=default";
#else
            switch (Extension.ToUpperInvariant())
            {
                case ".DOC":
                case ".DOCX":
                    formatString = "{0}/_layouts/WordViewer.aspx?id={1}&source={2}";
                    break;
                case ".XLSX":
                    formatString = "{0}/_layouts/xlviewer.aspx?DefaultItemOpen=1&id={1}&source={2}";
                    break;
                case ".PPT":
                case ".PPTX":
                    formatString = "{0}/_layouts/PowerPoint.aspx?PowerPointView=ReadingView&PresentationId={1}&source={2}";
                    break;
                case ".ONE":
                    formatString = "{0}/_layouts/OneNote.aspx?id={1}&Edit=0&source={2}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Extension.ToUpperInvariant());
            }
#endif

            return string.Format(CultureInfo.InvariantCulture, formatString, web.Url, HttpUtility.UrlEncode(Url), HttpUtility.UrlEncode(sourceUrl));
        }

        /// <summary>Detemines if a file needs copying to the drop box.</summary>
        public static bool MustCopyFileToDropBox(string extension)
        {
            if (IsOfficeFileByExtension(extension))
            {
                return true;
            }
            else
            {
                switch (extension.ToUpperInvariant())
                {
                    case ".RTF":
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>Detemines if the file is an office file.</summary>
        public static bool IsOfficeFileByExtension(string extension)
        {
            if (extension == null)
            {
                return false;
            }

            switch (extension.ToUpperInvariant())
            {
                case ".DOC":
                    return true;
                case ".DOCX":
                    return true;
                case ".XLS":
                    return true;
                case ".XLSX":
                    return true;
                case ".PPT":
                    return true;
                case ".PPTX":
                    return true;
                case ".ONE":
                    return true;
                default:
                    return false;
            }
        }
#endregion public methods

#region private methods
#endregion private methods
    }
}
