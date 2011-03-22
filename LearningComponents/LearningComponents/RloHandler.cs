/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.DataModel;
using System.Globalization;
using System.Web;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// RloHandler represents the RLO that may be involved in rendering and processing the current 
    /// LearningSession. 
    /// </summary>
    /// <remarks>
    /// </remarks>
    internal abstract class RloHandler
    {
        internal RloHandler()
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
        }

        /// <summary>
        /// Requests the RloHandler to process information received from the client.
        /// </summary>
        /// <param name="context">The context within which to process the form data.</param>
        /// <param name="formData">The data to process.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        public abstract void ProcessFormData(RloProcessFormDataContext context, NameValueCollection formData, IDictionary<string, HttpPostedFile> files); 

        /// <summary>
        /// Requests the RloHandler to modify the <c>RloHandlerContext.InputStream</c> and render the requested 
        /// view to the <c>RloHandlerContext.OuputStream</c>
        /// </summary>
        /// <param name="context">The context within which to render the content.</param>
        public abstract void Render(RloRenderContext context);

        /// <summary>
        /// Requests the RloHandler to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public abstract void ProcessSessionEnd(RloDataModelContext context);

        /// <summary>
        /// Requests the RloHandler to process a reactivation request and clear the appropriate data 
        /// values from the data model. The request may only be issued when the session is in RandomAccess 
        /// view and is not active -- it is either Completed or Abandoned.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Reactivate(RloReactivateContext context);

    }

    /// <summary>
    /// The context within which RloHandlers process a session exiting.
    /// </summary>
    internal class RloDataModelContext
    {
        protected LearningSession m_session;
        internal RloDataModelContext(LearningSession session)
        {
            m_session = session;
        }

        /// <summary>
        /// The view of the session.
        /// </summary>
        public SessionView View
        {
            get { return m_session.View; }
        }

        /// <summary>
        /// Gets the data model of the current activity in the session.
        /// </summary>
        public LearningDataModel LearningDataModel
        {
            get
            {
                return m_session.CurrentActivityDataModel;
            }
        }

        /// <summary>
        /// Gets the entry point of the current activity. This may be an absolute Uri, indicating 
        /// the file does not exist in the package and cannot be returned from <Mth>GetInputStream()</Mth>.
        /// </summary>
        public Uri CurrentActivityEntryPoint
        {
            get
            {
                return m_session.CurrentActivityEntryPoint;
            }
        }

        /// <summary>
        /// Get the input stream containing the primary file from the resource associated with the 
        /// current activity in the session. 
        /// </summary>
        /// <returns>The stream containing the current activity's primary file.</returns>
        /// <remarks>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the <Prp>CurrentActivityEntryPoint</Prp> indicates 
        /// the current activity does not have a resource in the package.</exception>
        public virtual Stream GetInputStream()
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if (CurrentActivityEntryPoint.IsAbsoluteUri)
                throw new InvalidOperationException(Resources.DRLO_CurrentActivityEntryPointIsAbsolute);

            return m_session.GetInputStream(CurrentActivityEntryPoint.OriginalString);
        }
    }

    /// <summary>
    /// Context used for Reactivate process.
    /// </summary>
    internal class RloReactivateContext : RloDataModelContext
    {
        private bool m_resetPoints;
        private bool m_resetComments;

        internal RloReactivateContext(LearningSession session, ReactivateSettings settings)
            : base(session)
        {
            m_resetPoints = ((settings & ReactivateSettings.ResetEvaluationPoints) != 0);
            m_resetComments = ((settings & ReactivateSettings.ResetEvaluationComments) != 0);
        }

        internal bool ResetEvaluationPoints
        {
            get { return m_resetPoints; }
        }

        internal bool ResetEvaluationComments
        {
            get { return m_resetComments; }
        }
    }


    /// <summary>
    /// Represents processing of Rlo data when there is no other custom handler available.
    /// </summary>
    internal class DefaultRloHandler : RloHandler
    {
        internal DefaultRloHandler()
        {
            // Nothing to do
        }

        /// <summary>
        /// Requests the RloHandler to process information received from the client.
        /// </summary>
        /// <remarks>
        /// This method does not take action on the posted data
        /// </remarks>
        /// <exception cref="InvalidFormDataException">Thrown if posted data contains invalid data.</exception>
        public override void ProcessFormData(RloProcessFormDataContext context, NameValueCollection formData, IDictionary<string, HttpPostedFile> files)
        {
            // do nothing
        }

        /// <summary>
        /// Requests the RloHandler to render the requested 
        /// view to the <c>RloHandlerContext.OuputStream</c>
        /// </summary>
        /// <param name="context"></param>
        public override void Render(RloRenderContext context)
        {
            // This rlo handler does nothing to process the content, so just send the file 
            // to the response.
            context.WriteFileToResponse(context.RelativePath);            
        }

        /// <summary>
        /// Process the end of the session.
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            // nothing to do
        }

        /// <summary>
        /// Requests the RloHandler to process a reactivation request and clear the appropriate data 
        /// values from the data model. The request may only be issued when the session is in RandomAccess 
        /// view and is not active -- it is either Completed or Abandoned.
        /// </summary>
        /// <param name="context"></param>
        public override void Reactivate(RloReactivateContext context)
        {
            // nothing to do
        }
    }

     /// <summary>
    /// Class that represents the context in which data from a posted form is processed. An instance of this 
    /// class is passed to the RloHandler when ProcessFormData is called.
    /// </summary>
    internal class RloProcessFormDataContext 
    {
        private LearningSession m_session;
        private SessionView m_view; // only used if m_session is null
        private LearningDataModel m_learningDataModel; // only used if m_session is null

        /// <summary>
        /// Create a context to send to an RloHandler.
        /// </summary>
        /// <param name="session">The session that is being rendered.</param>
        internal RloProcessFormDataContext(LearningSession session)
        {
            m_session = session;
        }

        /// <summary>
        /// Create a context to send to an RloHandler.
        /// </summary>
        /// <param name="view">The view that will be rendered from the form data.</param>
        /// <param name="learningDataModel">The data model of the current activity in the session.</param>
        internal RloProcessFormDataContext(SessionView view, LearningDataModel learningDataModel)
        {
            m_view = view;
            m_learningDataModel = learningDataModel;
        }

        /// <summary>
        /// Gets the view that will be rendered from the form data.
        /// </summary>
        public SessionView View
        {
            get
            {
                if (m_session == null) return m_view;
                else return m_session.View;
            }
        }

        /// <summary>
        /// Gets the data model of the current activity in the session.
        /// </summary>
        /// <remarks>Note that in <c>SessionView.Review</c> the returned data model may not be modified
        /// by the RloHandler.
        /// </remarks>
        public LearningDataModel LearningDataModel
        {
            get
            {
                if (m_session == null) return m_learningDataModel;
                else return m_session.CurrentActivityDataModel;
            }
        }
    }

    /// <summary>
    /// Class that represents the context in which the RloHandler is called to Render.
    /// An instance of this class is passed to the RloHandler when rendering 
    /// is requested by the RloHandler.
    /// </summary>
    internal class RloRenderContext : RloDataModelContext
    {
        private RenderContext m_context;

        /// <summary>
        /// Create a context to send to an RloHandler when calling <c>RloHandler.Render</c>.
        /// </summary>
        /// <param name="session">The session that is being rendered.</param>
        /// <param name="renderContext">Information required to render the current file. 
        /// </param>
        internal RloRenderContext(LearningSession session, RenderContext renderContext) : base(session)
        {
            m_context = renderContext;
        }

        /// <summary>
        /// Gets the output stream that the RloHandler should write to, when requested to render a view.
        /// </summary>
        internal Stream OutputStream
        {
            get
            {
                Stream toReturn = m_context.OutputStream;
                
                if ((toReturn == null) && (Response != null))
                    toReturn = Response.OutputStream;

                return toReturn;
            }
        }

        /// <summary>
        /// Gets the httpResponse that the RloHandler may write to. In some cases (particularly test code), this 
        /// value may be null.
        /// </summary>
        public HttpResponse Response
        {
            get
            {
                return m_context.Response;
            }
        }

        /// <summary>
        /// Set the file extension for the output stream. If there is a 
        /// Response object, the mime type is set on the response.
        /// </summary>
        /// <param name="fileExtension">The extension of the filename
        /// that is being rendered. This is of the form ".htm", including the 
        /// period.</param>
        public void SetOutputStreamExtension(string fileExtension)
        {
            IDictionary<string, string> mimeMapping = m_context.MimeTypeMapping;

            if (mimeMapping.ContainsKey(fileExtension))
            {
                m_context.MimeType = mimeMapping[fileExtension];
            }
            else
            {
                m_context.MimeType = "application/octet-stream";
            }

            if (Response != null)
            {
                Response.ContentType = m_context.MimeType;
            }
        }

        /// <summary>
        /// The package-relative path of the file to be rendered. If the file exists in the package, 
        /// the contents of this file is returned by <Mth>GetInputStream()</Mth>.
        /// </summary>
        public string RelativePath
        {
            get { return m_context.RelativePath; }
        }

        /// <summary>
        /// Returns true if the requested <Prp>RelativePath</Prp> is the entry point for the current activity
        /// </summary>
        public bool IsResourceEntryPoint
        {
            get
            {
                try
                {
                    return (Uri.Compare(new Uri(RelativePath, UriKind.Relative), m_session.CurrentActivityEntryPoint, UriComponents.PathAndQuery, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0);
                }
                catch (UriFormatException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Writes the file from the package directly to the response and set the appropriate 
        /// content type in the response. If the response is provided and the path extension is not
        /// in the list of files to use IIS compatibility mode, this is much faster than 
        /// copying the file contents to the output stream and should be used whenever possible.
        /// If there is no response object, the method reverts to copying the file between streams.
        /// </summary>
        /// <param name="relativePath">The package-relative path to the file to render.</param>
        internal void WriteFileToResponse(string relativePath)
        {
            // Set the mime type on the response
            string pathExtension = Path.GetExtension(relativePath);
            SetOutputStreamExtension(pathExtension);

            using(Disposer disposer = new Disposer())
            {
                PackageReader pkgReader = m_session.GetPackageReader();
                disposer.Push(pkgReader);

                // If the response was provided (which it will be, in most non-test cases), then 
                // do the TransmitFile or WriteFile. Otherwise, copy the streams to the response output stream.
                if (Response != null) 
                {                
                    // If we are to use Compatibility mode, then write the file
                    if (UseCompatibilityMode(pathExtension))
                    {
                        Stream packageFile = pkgReader.GetFileStream(relativePath);
                        WriteIisCompatibilityModeToResponse(packageFile);
                    }
                    else
                    {
                        pkgReader.TransmitFile(relativePath, m_context.Response);
                    }
                }
                else
                {
                    DetachableStream outputDS = new DetachableStream(OutputStream);
                    disposer.Push(outputDS);

                    Stream packageFile = pkgReader.GetFileStream(relativePath);
                    disposer.Push(packageFile);

                    Utilities.CopyStream(packageFile, ImpersonationBehavior.UseImpersonatedIdentity, outputDS, ImpersonationBehavior.UseImpersonatedIdentity);

                    outputDS.Detach();
                }
            }
        }

        /// <summary>
        /// Writes the file from a byte array directly to the response. The output stream
        /// extension must be set prior to calling this method.
        /// If the response is provided and the path extension is not
        /// in the list of files to use IIS compatibility mode, this is much faster than 
        /// copying the file contents to the output stream and should be used whenever possible.
        /// If there is no response object, the method reverts to copying the file between streams.
        /// </summary>
        /// <param name="fileBytes">The file to render.</param>
        internal void WriteFileToResponse(byte[] fileBytes)
        {
            using (Disposer disposer = new Disposer())
            {
                // Open a stream on the file. For some odd reason fxcop does not acknowledge the ability of disposer
                // to actually dispose fileStream, so we give it its own using block.
                using (Stream fileStream = new MemoryStream(fileBytes))
                {
                    // If the response was provided (which it will be, in most non-test cases), then 
                    // do the TransmitFile or WriteFile. Otherwise, copy the streams to the response output stream.
                    if (Response != null)
                    {
                        // IisCompatibilityMode is not considered here, because in no case do we have a file 
                        // path that can be read by TransmitFile. 
                        WriteIisCompatibilityModeToResponse(fileStream); 
                    }
                    else
                    {
                        DetachableStream outputDS = new DetachableStream(OutputStream);
                        disposer.Push(outputDS);

                        Utilities.CopyStream(fileStream, ImpersonationBehavior.UseImpersonatedIdentity, outputDS, ImpersonationBehavior.UseImpersonatedIdentity);

                        outputDS.Detach();
                    }
                }
            }
        }

        private const int BUFFER_SIZE = 100000;
        /// <summary>
        /// Write the stream to the response, using WriteBinary. This method closes the stream.
        /// </summary>
        /// <param name="packageStream"></param>
        private void WriteIisCompatibilityModeToResponse(Stream packageStream)
        {
            // Create buffer big enough to handle many files in one chunk, but small enough to not thrash
            // the response.
            byte[] buffer = new Byte[BUFFER_SIZE];

            // Length of the file
            int length;

            // Total bytes to read
            long dataToRead;

            // Total bytes to read:
            dataToRead = packageStream.Length;

            Response.AppendHeader("Content-length", dataToRead.ToString(CultureInfo.InvariantCulture));

            // Read the bytes.
            while (dataToRead > 0)
            {
                // Verify that the client is connected.
                if (Response.IsClientConnected)
                {
                    // Read the data in buffer.
                    length = packageStream.Read(buffer, 0, BUFFER_SIZE);

                    // Write the data to the current output stream.
                    Response.OutputStream.Write(buffer, 0, length);

                    // Flush the data to the HTML output.
                    Response.Flush();

                    buffer = new Byte[BUFFER_SIZE];
                    dataToRead = dataToRead - length;
                }
                else
                {
                    //prevent infinite loop if user disconnects
                    dataToRead = -1;
                }
            }
        }

        /// <summary>
        /// Returns true if IIS compatibility mode should be used for sending this file. 'Compatibility 
        /// mode' indicates the code should not use TransmitFile to send the requested file.
        /// </summary>
        /// <param name="pathExtension">The extension, including preceding period, of a file to render</param>
        /// <returns>True if IIS compatiblity mode should be used for this file type.</returns>
        private bool UseCompatibilityMode(string pathExtension)
        {
            foreach (string compatExtension in m_context.IisCompatibilityModeExtensions)
            {
                if ((String.CompareOrdinal(compatExtension, pathExtension) == 0) ||
                    (String.CompareOrdinal(compatExtension, ".*") == 0)) // meaning 'all'
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the input stream containing the current requested page for the RloHandler
        /// to operate on.
        /// </summary>
        /// <returns>The stream containing the currently requested file.</returns>
        /// <remarks>
        /// </remarks>
        /// <exception cref="FileNotFoundException">If the requested input stream does not exist 
        /// within the package.</exception>
        public override Stream GetInputStream()
        {
            return m_session.GetInputStream(m_context.RelativePath); 
        }

        /// <summary>
        /// Get the location of resources embedded in the rendered content that are 
        /// not contained within the content. 
        /// This will not be null.
        /// </summary>
        /// <remarks>
        /// The value will be either an absolute path or a relative path.  If relative, the path
        /// must begin with "./" and end without a "/".  E.g. "./", "./path" are legal values.
        /// If not set explicitly, will be the relative path "./".
        /// </remarks>
        public Uri EmbeddedUIResourcePath
        {
            get
            {
                if (m_context.EmbeddedUIResourcePath == null)
                {
                    return new Uri("./", UriKind.Relative);
                }
                else
                {
                    return m_context.EmbeddedUIResourcePath;
                }
            }
        }

        /// <summary>
        /// Gets the list of hidden controls to render in the form. The key is 
        /// the id of the control, the value is the value of the control.
        /// Both strings are provided in plain text (not HTML). 
        /// </summary>
        public ReadOnlyDictionary<string, string> FormHiddenControls
        {
            get
            {
                // Issue: RloHandler has to ensure that the names provided here are unique in the page. It can only do that 
                // when the page is being rendered, so what happens at that point? 

                return new ReadOnlyDictionary<string, string>(m_context.FormHiddenControls);
            }
        }

        /// <summary>
        /// Gets the jscript to render in the page. The returned string will not include the &lt;script&gt; delimiters. 
        /// The rendered page should ensure this script is rendered in a way that will cause it to run before the user can interact with the 
        /// page but after the page has been processed by the browser for rendering. For instance, the script should be added to 
        /// an onload handler or in-page script at the end of the file.
        /// </summary>
        /// <remarks>The value of this property is valid jscript 
        /// and can be rendered directly in the page without further encoding.</remarks>
        public string ScriptToRender
        {
            get
            {
                return m_context.Script;
            }
        }

        /// <summary>
        /// Get a value indicating whether correct answers should be 
        /// displayed in the content, if possible. Regardless of this value, answers may only be shown if 
        /// it is supported in the current activity format for the current <c>LearningSession.View</c>.
        /// </summary>
        /// <remarks>
        /// This setting only affects content which is dynamically rendered, such as Lrm content, and only
        /// in Review and Grading views. In other views and content formats, the setting is ignored.
        /// </remarks>
        public bool ShowCorrectAnswers
        {
            get { return m_context.ShowCorrectAnswers; }
        }

        /// <summary>
        /// Gets a value indicating whether sections of the content intended for a reviewer of a session
        /// (and not the learner)
        /// should be displayed in the rendered content. 
        /// </summary>
        /// <remarks>
        /// This setting only affects content which is dynamically rendered, such as Lrm content, and only
        /// in Review and Grading views. In other views and content formats, the setting is ignored.
        /// </remarks>
        public bool ShowReviewerInformation
        {
            get { return m_context.ShowReviewerInformation; }
        }
    }
}
