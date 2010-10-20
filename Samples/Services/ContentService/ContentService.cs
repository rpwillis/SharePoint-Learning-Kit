using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.SharePoint;

namespace SharePointLearningKit.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ContentService : GetContentContract, PutContentContract
    {
        #region PutContentContract
        // Data used throughout into PutFile
        private string _fileName;
        private Uri _documentLibrary;
        private SPWeb _spWeb;
        private SPFolder _spFolderDocumentLibrary;
        private string _cacheFileName;
        private FileStream _cacheFile;

        /// <summary>
        /// Verify that the DocumentLibrary exists and open the Content Service cache file to be written
        /// </summary>
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public void BeginPutFile(Uri documentLibrary, string fileName)
        {
            _fileName = fileName;
            _documentLibrary = documentLibrary;

            Trace.WriteLine(String.Format("Uploading {0}/{1}", _documentLibrary.ToString(), _fileName));

            _spWeb = new SPSite(documentLibrary.ToString()).OpenWeb();
            if (!_spWeb.Exists)
                throw new DirectoryNotFoundException("SPWeb: " + documentLibrary.ToString());

            // Assume the last segment of the Uri is the DocumentLibrary's folder name
            _spFolderDocumentLibrary = _spWeb.GetFolder(Uri.UnescapeDataString(documentLibrary.Segments[documentLibrary.Segments.Length - 1]));
            if (!_spFolderDocumentLibrary.Exists)
                throw new DirectoryNotFoundException("SPFolder: " + documentLibrary.ToString());

            // Generate the name of the file to be stored in the Content Service cache.  This consists of the unique identifier (Guid) of the SPFolder combined with
            // the name of the package so we get guaranteed uniqueness in the case that the same file name is used in different DocumentLibraries
            _cacheFileName = _spFolderDocumentLibrary.UniqueId + "_" + fileName;

            // TODO: Potential attack here -- overwriting a file in the cache that the user has rights to when she doesn't have writes to the SharePoint Document Library.

            Directory.CreateDirectory(ServiceConfiguration.Default.ContentPath); // Ensure the destination directory exists
            _cacheFile = File.Create(Path.Combine(ServiceConfiguration.Default.ContentPath, _cacheFileName));
        }

        /// <summary>
        /// Upload a series of chunks of data to be written to the Content Service cache file
        /// </summary>
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public void PutFileChunk(byte[] chunk)
        {
            if (_cacheFile == null)
                throw new InvalidOperationException("You must call BeginPutFile before PutFileChunk");

            if (chunk == null)
                throw new ArgumentNullException("chunk");

            if (chunk.Length == 0 || chunk.Length > 64 * 1024)
                throw new ArgumentOutOfRangeException("chunk.Length");

            _cacheFile.Write(chunk, 0, chunk.Length);
        }

        /// <summary>
        /// Generate a new Html Redirect file, delete the old one, and add the new one to the target SharePoint Document Library
        /// </summary>
        [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        public void EndPutFile()
        {
            if (_cacheFile == null)
                throw new InvalidOperationException("You must call BeginPutFile before EndPutFile");

            #region Close out the file write
            _cacheFile.Close();
            _cacheFile.Dispose();
            _cacheFile = null;
            #endregion

            #region Find/generate parameters to put into the redirect file
            // Generate the Uri to fetch the file out fo the Content Service cache.
            StringBuilder packageLocation = new StringBuilder();
            packageLocation.Append("/Services/Content.svc/Get/");
            packageLocation.Append(_cacheFileName);

            // Create the full Uri to the redirect file to be stored
            string documentLibraryString = _documentLibrary.ToString();
            if (!documentLibraryString.EndsWith("/")) // Add the trailing / if needed to make the Uri combine work, otherwise the last segment is overwritten
                documentLibraryString += "/";
            string sharePointFileLocation = new Uri(new Uri(documentLibraryString), _fileName + ".html").ToString();
            #endregion

            #region Generate & Add Redirect File
            using (Stream htmlStream = new MemoryStream())
            {
                string launchUrl = null;
                string contentType = null;
                if (String.CompareOrdinal(Path.GetExtension(_fileName), ".gxml") == 0)
                {
                    // Play Lesson -- TODO, rename this parameter from PackageLocation to Play
                    launchUrl = "/Services/Player/Player.application?" +
                        "Play=" + Uri.EscapeDataString(packageLocation.ToString());
                    contentType = "Grava Player";
                }
                else if (String.CompareOrdinal(Path.GetExtension(_fileName), ".grproj") == 0)
                {
                    // Edit Lesson
                    launchUrl = "/Services/Authoring/Authoring.application?" +
                        "DocumentLibrary=\" + documentLibraryUrl + \"" + // Javascript on the client HTML does a substitution here from its launch context
                        "&Edit=" + Uri.EscapeDataString(packageLocation.ToString());
                    contentType = "Grava Authoring";
                }

                if (launchUrl != null) // Only generate a stub file for recognized content types
                {
                    using (StreamWriter sw = new StreamWriter(htmlStream))
                    {
                        sw.Write(GenerateRedirectStub(_fileName, contentType, launchUrl));
                        sw.Flush();
                        htmlStream.Position = 0;

                        #region Delete any existing file since we're not allowed to overwrite
                        // Enumerating is silly, I know, but the only way I could figure out how to find out whether a document library contained a file...
                        foreach (SPFile spFile in _spFolderDocumentLibrary.Files)
                        {
                            if (spFile.Name == _fileName + ".html")
                            {
                                spFile.Delete();
                                break;
                            }
                        }
                        #endregion

                        _spWeb.Files.Add(sharePointFileLocation, htmlStream);

                        // TODO: Roll back the cached file if the SharePoint Redirect write fails (especially due to permissions issues).
                    }
                }
            }
            #endregion

            Trace.WriteLine(String.Format("Upload completed: {0}/{1}", _documentLibrary.ToString(), _fileName));
        }
        #endregion

        #region GetContentContract
        public void GetFileHeaders(string name)
        {
            Trace.WriteLine(String.Format("Retrieving Headers {0}", name));
            FileInfo fileInfo = new FileInfo(Path.Combine(ServiceConfiguration.Default.ContentPath, name));

            try
            {
                WebOperationContext.Current.OutgoingResponse.LastModified = fileInfo.LastWriteTime;
                WebOperationContext.Current.OutgoingResponse.ContentLength = fileInfo.Length;
                //WebOperationContext.Current.OutgoingResponse.ContentType = ...; // Disabled for now...
            }
            catch (FileNotFoundException)
            {
                Trace.WriteLine(String.Format("File Not Found Retrieving Headers {0}", name));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
            }
            catch (IOException ioe)
            {
                Trace.WriteLine(String.Format("IO Error Retrieving Headers {0} : {1}", name, ioe.Message));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (UnauthorizedAccessException uae)
            {
                Trace.WriteLine(String.Format("Unauthorized Error Retrieving Headers {0} : {1}", name, uae.Message));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            }
        }

        public Stream GetFile(string name)
        {
            Trace.WriteLine(String.Format("Retrieving Content {0}", name));
            FileInfo fileInfo = new FileInfo(Path.Combine(ServiceConfiguration.Default.ContentPath, name));

            try
            {
                WebOperationContext.Current.OutgoingResponse.LastModified = fileInfo.LastWriteTime;
                WebOperationContext.Current.OutgoingResponse.ContentLength = fileInfo.Length;
                //WebOperationContext.Current.OutgoingResponse.ContentType = ...; // Disabled for now...

                return fileInfo.OpenRead();
            }
            catch (FileNotFoundException)
            {
                Trace.WriteLine(String.Format("File Not Found Retrieving Content {0}", name));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
            }
            catch (IOException ioe)
            {
                Trace.WriteLine(String.Format("IO Error Retrieving Content {0} : {1}", name, ioe.Message));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = ioe.Message;
            }
            catch (UnauthorizedAccessException uae)
            {
                Trace.WriteLine(String.Format("Unauthorized Error Retrieving Content {0} : {1}", name, uae.Message));

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            }

            return null;
        }
        #endregion

        private void CopyStream(Stream sourceStream, Stream destinationStream)
        {
            byte[] buffer = new byte[4096];
            int cBytes = 0;

            using (destinationStream)
            {
                do
                {
                    cBytes = sourceStream.Read(buffer, 0, buffer.Length);
                    destinationStream.Write(buffer, 0, cBytes);
                }
                while (cBytes > 0);
            }
        }

        /// <summary>
        /// Creates an HTML Redirection Stub that will launch a ClickOnce application for Content by reading
        /// a template in from a resource stream and substituting strings
        /// </summary>
        public static string GenerateRedirectStub(string pageTitle, string contentType, string launchUrl)
        {
            string redirectHtml;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SharePointLearningKit.Services.RedirectTemplate.html"))
            using (StreamReader sr = new StreamReader(stream))
                redirectHtml = sr.ReadToEnd();

            redirectHtml = redirectHtml.Replace("{PageTitle}", pageTitle);
            redirectHtml = redirectHtml.Replace("{ContentType}", contentType);
            redirectHtml = redirectHtml.Replace("{LaunchUrl}", launchUrl);

            return redirectHtml;
        }
    }
}