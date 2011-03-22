/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Microsoft.LearningComponents.DataModel;
using System.Globalization;
using System.IO;
using System.Web;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Represents a processor that can save posted form data in the approriate format.
    /// FormDataProcessors read and process data based on Interactions.
    /// </summary>
    internal abstract class FormDataProcessor
    {
        private RloProcessFormDataContext m_context;
        private Interaction m_interaction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Context in which data from a posted form is processed.</param>
        public FormDataProcessor(RloProcessFormDataContext context)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            m_context = context;
        }

        /// <summary>
        /// Gets the current context of form data processing.
        /// </summary>
        internal RloProcessFormDataContext Context
        {
            get { return m_context; }
            set { m_context = value; }
        }

        /// <summary>
        /// Gets and sets the interaction associated with the current data being procesed.
        /// </summary>
        /// <remarks>This is set by AssessmentItemManager. Used by subclasses. </remarks>
        public Interaction Interaction
        {
            get { return m_interaction; }
            set { m_interaction = value; }
        }
        
        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public abstract void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files);

        /// <summary>
        /// Random Access view version of ProcessFormData.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        internal void ProcessRandomAccessView(NameValueCollection formData)
        {
            // Update the evaluation points with the intructor's posted score.
            Interaction.Evaluation.Points = GetPostedEvaluationPointsForInteraction(formData);

            // Only Essay and Attachment interaction types may have rubrics, and only these two types have instructor comments
            // output by the assessment item renderers.
            if (Interaction.InteractionType == InteractionType.Essay
                || Interaction.InteractionType == InteractionType.Attachment)
            {
                // Update the instructor comments.
                Interaction.Evaluation.Comments.Clear();
                // There is a teacher comment per response option
                if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
                {
                    int itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                    for (int i = 0; i < itemCount; i++)
                    {
                        Comment comment = Interaction.DataModel.CreateComment();
                        // Convert occurrances of %0d%0a to <br> tags - this is mirrored in AssessmentItemRenderer.InstructorComments().
                        comment.CommentText = FormDataProcessor.ConvertPostedTextAreaDataToDatabaseFormat(
                            formData[AssessmentItemRenderer.RenderInstructorCommentsFormFieldName(Interaction.Id, i.ToString(CultureInfo.InvariantCulture))]);
                        Interaction.Evaluation.Comments.Add(comment);
                    }
                }

                // Update the rubrics
                for (int i = 0; i < Interaction.Rubrics.Count; i++)
                {
                    Rubric rubric = Interaction.Rubrics[i];
                    if (formData[RubricAssessmentRenderer.RenderFormFieldName(Interaction.Id, i.ToString(CultureInfo.InvariantCulture))] == null)
                    {
                        rubric.IsSatisfied = false;
                    }
                    else
                    {
                        rubric.IsSatisfied = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public abstract void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files);

        /// <summary>
        /// Validates that item scores, rubrics, and instructor comments are valid.
        /// </summary>
        /// <param name="formData">Form data containing the responses.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        internal void ValidateRandomAccessView(NameValueCollection formData)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            // Validate the score field.
            // only the "0" ordinal score is used
            string formFieldName = ItemScoreAssessmentRenderer.RenderFormFieldName(Interaction.Id, "0");
            string postedScore = formData[formFieldName];
            if (!String.IsNullOrEmpty(postedScore))
            {
                postedScore = postedScore.Trim();
                try
                {
                    double score = Convert.ToDouble(postedScore, CultureInfo.CurrentUICulture);
                    // according to the LRDG, a score should be from -1000 to 1000, but the only
                    // real restriction here needs to be that it fit in a float.
                    if (score > float.MaxValue || score < float.MinValue)
                    {
                        throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                            AIResources.InvalidFormFieldValue, postedScore, formFieldName));
                    }
                }
                catch (InvalidCastException)
                {
                    throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                        AIResources.InvalidFormFieldValue, postedScore, formFieldName));
                }
            }

            // In RandomAccess view, Essay and Attachment InteractionType's (which correspond to TextArea and
            // File assessment items) generate a textarea form field for instructor comments for each TextArea
            // assessment item, and a checkbox form field for each associated rubric.)
            if (Interaction.InteractionType == InteractionType.Essay
                || Interaction.InteractionType == InteractionType.Attachment)
            {
                // Validate teacher comments: there is a teacher comment per response option
                if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
                {
                    // validate intructor comments.  There should be form data for each - a blank entry still submits
                    // String.Empty.
                    int itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                    for (int i = 0; i < itemCount; i++)
                    {
                        formFieldName = AssessmentItemRenderer.RenderInstructorCommentsFormFieldName(Interaction.Id, i.ToString(CultureInfo.InvariantCulture));
                        if (formData[formFieldName] == null)
                        {
                            throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                                AIResources.MissingFormField, formFieldName));
                        }
                    }
                }

                // There is essentially nothing to validate for rubrics.  Since they are check box form fields, they
                // are only posted if checked.  The absence of a form field value means the check box wasn't checked.
            }
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public abstract void ProcessSessionEnd(RloDataModelContext context);

        /// <summary>
        /// Helper function to append one learner response string (which is not encoded)
        /// to the buffer that will be assigend to Interaction.LearnerResponse.
        /// </summary>
        /// <param name="sb">Buffer to add the encoded learner response data to.</param>
        /// <param name="learnerResponse">The response from the learner.</param>
        /// <returns>The encoded value that can be added to Interaction.LearnerResponse. Does not include
        /// initial separator.</returns>
        internal static void AppendLearnerResponse(StringBuilder sb, string learnerResponse)
        {
            // encode the single learner response 
            string response = LrmRloHandler.EncodePattern(learnerResponse);
            sb.Append(response);

            // Append the [,] token to separate other learner responses.
            sb.Append(comma[0]);
        }

        private static string[] comma = { "[,]" };
        /// <summary>
        /// Helper function to extract one learner response out of an Interaction.LearnerResponse, by ordinal.
        /// </summary>
        /// <param name="learnerResponse">Interaction.LearnerResponse</param>
        /// <param name="ordinal">Ordinal of response to obtain.</param>
        /// <returns>The learner response string at the ordinal requested, or String.Empty if none exists.</returns>
        internal static string ExtractLearnerResponse(object learnerResponse, int ordinal)
        {
            string response = String.Empty;
            // Split the learner response into individual learner responses
            string responseString = learnerResponse as string;
            if (!String.IsNullOrEmpty(responseString))
            {
                string[] responses = responseString.Split(comma, StringSplitOptions.None);
                if (ordinal < responses.Length)
                {
                    response = LrmRloHandler.DecodePattern(responses[ordinal]);
                }
            }
            return response;
        }

        /// <summary>
        /// Returns the posted instructor evaluation poitns for the interaction.  If there is no posted value, returns null.
        /// </summary>
        /// <exception cref="InvalidFormDataException">The posted score isn't a valid float.</exception>
        internal float? GetPostedEvaluationPointsForInteraction(NameValueCollection formData)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            // only the "0" ordinal score is used
            string formFieldName = ItemScoreAssessmentRenderer.RenderFormFieldName(Interaction.Id, "0");
            string postedScore = formData[formFieldName];
            float? score = null;
            if (!String.IsNullOrEmpty(postedScore))
            {
                try
                {
                    score = (float)Convert.ToDouble(postedScore, CultureInfo.CurrentUICulture);
                }
                catch (InvalidCastException)
                {
                    throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                        AIResources.InvalidFormFieldValue, postedScore, formFieldName));
                }
            }
            return score;
        }

        /// <summary>
        /// Converts a posted value for a textarea into a format to be stored in the database.
        /// </summary>
        internal static string ConvertPostedTextAreaDataToDatabaseFormat(string p)
        {
            return p.Replace("%0d%0a", "\r\n");
        }

        /// <summary>
        /// Converts a value stored in the database that originally came from a posted textarea 
        /// into a value for posting to an html page inside a textarea.
        /// </summary>
        internal static string ConvertDatabaseFormatToTextArea(string p)
        {
            return p; // no conversion needed
        }

        /// <summary>
        /// Converts a value stored in the database that originally came from a posted textarea 
        /// into a value for posting to an html page (but not inside a textarea.)
        /// </summary>
        internal static HtmlString ConvertDatabaseFormatToHtml(string p)
        {
            return new HtmlString(new PlainTextString(p).ToHtmlString().ToString().Replace("\r\n", "<br>"));
        }
    }

    /// <summary>
    /// Represents processor to multiple-choice interactions, which corresponds to the <c>AssessmentItemType.Radio</c>
    /// or <c>AssessmentItemType.Checkbox</c>.
    /// </summary>
    internal class MultipleChoiceFormDataProcessor : FormDataProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal MultipleChoiceFormDataProcessor(RloProcessFormDataContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Helper method.
        /// </summary>
        private void SetExecuteViewResponses(float? score, InteractionResultState resultState, string learnerResponse)
        {
            Interaction.LearnerResponse = learnerResponse;
            Interaction.Score.Raw = score;
            Interaction.Evaluation.Points = score;
            Interaction.Result.State = resultState;
        }

        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile> files)
        {
            if (Context.View == SessionView.Execute)
            {
                float? score;
                InteractionResultState resultState;
                string learnerResponse;
                GetExecuteViewResults(formData, out score, out resultState, out learnerResponse);
                SetExecuteViewResponses(score, resultState, learnerResponse);
            }
            else if (Context.View == SessionView.RandomAccess)
            {
                ProcessRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            SetExecuteViewResponses(0, InteractionResultState.Incorrect, String.Empty);
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public override void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            // Execute View:
            // The multiple choice interaction will contain either checkboxes or radio buttons.
            // No validation needs to be performed.

            // RandomAccess View:
            if (Context.View == SessionView.RandomAccess)
            {
                ValidateRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Returns the extension data's maxpoints value for the ordinal, or 0 if it doesn't exist.
        /// </summary>
        internal static float MaxPoints(Interaction interaction, string ordinal)
        {
            if (interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.MaxPoints(ordinal)))
            {
                return (float)(double)interaction.ExtensionData[InteractionExtensionDataKeys.MaxPoints(ordinal)];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the score, InteractionResultState, and learner response for the posted form data for this interaction.
        /// </summary>
        /// <remarks>
        /// For a radio button, the learner response is the form field name of the radio button.  For a checkbox, the
        /// learner response is a "[,]" separated list of the form field names.
        /// </remarks>
        private void GetExecuteViewResults(NameValueCollection formData, out float? score, out InteractionResultState result,
            out string learnerResponse)
        {
            // Note: The calculations here are based on 'LRM to IMS.xls', the InteractionScore tab.
            score = 0;
            learnerResponse = String.Empty;
            result = InteractionResultState.None;

            if (Context.View == SessionView.Execute)
            {
                // If Execute View: do autoscoring 
                // Extension data contains maxpoints values for each possible response, by ordinal.
                // The number of possible ordinals are indicated by the response option count.
                if ((AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType] == AssessmentItemType.Radio)
                {
                    // radio type allows only a single response.  The value is the ordinal into the maxpoints extension data
                    string formFieldName = RadioAssessmentRenderer.RenderFormFieldName(Interaction.Id);
                    string ordinal = formData[formFieldName];
                    if (ordinal != null)
                    {
                        learnerResponse = ordinal;
                        score += MaxPoints(Interaction, ordinal);
                    }
                }
                else
                {
                    // checkbox type allows multiple responses
                    StringBuilder bld = new StringBuilder();
                    int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                    for (int ordinal = 0; ordinal < responseOptionCount; ordinal++)
                    {
                        string ordinalAsString = ordinal.ToString(CultureInfo.InvariantCulture);
                        // if the form data contains a value for the checkbox named for this interaction and ordinal,
                        // add the maxpoints to the score.
                        string formFieldName = CheckboxAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalAsString);
                        if (formData[formFieldName] != null)
                        {
                            // checkbox input fields are only posted when checked, so there is no need to check
                            // the value.
                            AppendLearnerResponse(bld, formFieldName);
                            score += MaxPoints(Interaction, ordinalAsString);
                        }
                    }
                    learnerResponse = bld.ToString();
                }
                if (score > 0)
                {
                    result = InteractionResultState.Correct;
                }
                else
                {
                    result = InteractionResultState.Incorrect;
                }
                return;
            }
        }
    }

    /// <summary>
    /// Represents processor to FileAttachment interactions, which correspond to 
    /// File attachment AssessmentItemType.
    /// </summary>
    internal class FileAttachmentFormDataProcessor : FormDataProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal FileAttachmentFormDataProcessor(RloProcessFormDataContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Delegate to pass to the internal versions of ProcessFormData() and ValidateFormData(), which allows for
        /// easier unit testing.
        /// </summary>
        /// <param name="fieldName">Form field that identifies which file to retrieve from the posted files.</param>
        /// <param name="stream">Stream for the file, or null if not found.</param>
        /// <param name="filename">File name for the file, or String.Empty if not found.</param>
        internal delegate void GetFileDelegate(string fieldName, out Stream stream, out string filename);

        /// <summary>
        /// Internal version of <c>ProcessFormData</c> for execute view.
        /// </summary>
        internal void ExecuteViewProcessFormData(NameValueCollection formData, GetFileDelegate getFileDelegate)
        {
            int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];


            // For each form field name that corresponds to this interaction, store the attachment keys in the
            // LearnerResponse.
            // E.g. attachmentKey = ms.learningcomponents.fileAttachment.<ordinal>
            // If there is a file, 
            //      store it as ExtensionData[attachmentKey] = byte[] byte array of file
            //      ExtensionData[attachmentKey + ".fileExtension"] = file extension of attached file
            // Else
            //      set ExtensionData[attachmentKey] = (bool)false;
            //      remove ExtensionData[attachmentKey + "fileExtension"] if it exists
            string hidDetach = formData["hidDetach"]; // form field name of the file to detach
            if (hidDetach == null) hidDetach = String.Empty; // makes string comparison easier
            StringBuilder bld = new StringBuilder(); // for learner response
            for (int ordinal = 0; ordinal < responseOptionCount; ordinal++)
            {
                string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                string formFieldName = FileAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalString);
                string filename;

                // For each form field name that corresponds to this interaction, store the attachment keys in the
                // LearnerResponse.
                // E.g. attachmentKey = ms.learningcomponents.fileAttachment.<ordinal>
                AppendLearnerResponse(bld, InteractionExtensionDataKeys.FileAttachment(ordinalString));

                // If the hidDetach field has a value that indicates one of the files should be detached, do that.
                // Note that the Detach button calls Submit, so there will only ever be one file to be detached at a time.
                if (0 == String.Compare(formData["hidDetach"], formFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove any file extension and set file attachment to false.
                    if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileExtension(ordinalString)))
                    {
                        Interaction.ExtensionData.Remove(InteractionExtensionDataKeys.FileExtension(ordinalString));
                    }
                    if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileAttachment(ordinalString)))
                    {
                        Interaction.ExtensionData[InteractionExtensionDataKeys.FileAttachment(ordinalString)] = false;
                    }
                    else
                    {
                        Interaction.ExtensionData.Add(InteractionExtensionDataKeys.FileAttachment(ordinalString), false);
                    }
                }
                else
                {
                    // Get the file stream and name
                    Stream stream;
                    getFileDelegate(formFieldName, out stream, out filename);
                    
                    // If the filename is empty, or stream is null or its length is zero, no file was chosen for upload.  
                    // However, one may already be attached for this form field.
                    if ((stream != null) && !String.IsNullOrEmpty(filename) && stream.Length != 0)
                    {
                        // Add the file data to the extension data.  The file data is guaranteed to exist since it was validated 
                        // in the ValidateFormData method.
                        // If a file attachment was previously stored, remove it.  It will be replaced.
                        if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileAttachment(ordinalString)))
                        {
                            Interaction.ExtensionData.Remove(InteractionExtensionDataKeys.FileAttachment(ordinalString));
                        }
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        Interaction.ExtensionData.Add(InteractionExtensionDataKeys.FileAttachment(ordinalString), buffer);

                        // Add or replace the file extension, which is the all-caps version of the letters after the last
                        // "." in the file name.  If no "." exists in the file name, the file extension is String.Empty.
                        string fileExtension = Path.GetExtension(filename).ToUpperInvariant();
                        if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileExtension(ordinalString)))
                        {
                            Interaction.ExtensionData[InteractionExtensionDataKeys.FileExtension(ordinalString)] = fileExtension;
                        }
                        else
                        {
                            Interaction.ExtensionData.Add(InteractionExtensionDataKeys.FileExtension(ordinalString), fileExtension);
                        }
                    }
                    else
                    {
                        // If there is not a form field value for this ordinal, create the attachment key if it doesn't
                        // exist already, and set its value to (bool)false, but leave it alone if it already exists.
                        if (!Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileAttachment(ordinalString)))
                        {
                            Interaction.ExtensionData.Add(InteractionExtensionDataKeys.FileAttachment(ordinalString), false);
                        }
                    }
                }
            }
            Interaction.LearnerResponse = bld.ToString();
        }

        /// <summary>
        /// Internal version of <c>ProcessFormData</c> for easier unit testing purposes.
        /// </summary>
        internal void ProcessFormData(NameValueCollection formData, GetFileDelegate getFileDelegate)
        {
            if (Context.View == SessionView.Execute)
            {
                ExecuteViewProcessFormData(formData, getFileDelegate);
            }
            else if (Context.View == SessionView.RandomAccess)
            {
                ProcessRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Internal version of ValidateFormData that takes a delegate.
        /// </summary>
        /// <param name="getFileDelegate">Delegate to get the file stream and name.</param>
        internal void ValidateFormData(GetFileDelegate getFileDelegate)
        {
            if (Context.View == SessionView.Execute)
            {
                // Ensure that there is an HttpPostedFile for every expected field name by calling
                // getFileDelegate for each expected field name, based on the interaction response option count.
                // getFileDelegate will throw InvalidFormDataException if asked for a file that isn't in
                // m_files.
                int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                for (int ordinal = 0; ordinal < responseOptionCount; ordinal++)
                {
                    string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                    string formFieldName = FileAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalString);
                    string filename;
                    Stream stream;

                    // Get the file stream and name
                    getFileDelegate(formFieldName, out stream, out filename);
                }
            }
        }

        /// <summary>
        /// Set at the beginning of ProcessFormData() and ValidateFormData() and used by the GetStream() delegate.
        /// </summary>
        private IDictionary<string, HttpPostedFile>  m_files;
        /// <summary>
        /// Returns a stream from the m_files state data.  If the file is not in m_files, stream is returned as null.
        /// The length of the stream will be 0 if no file was chosen for upload, and the filename will be String.Empty.
        /// </summary>
        private void GetFile(string fieldName, out Stream stream, out string filename)
        {
            stream = null;
            filename = String.Empty;
            if (m_files != null)
            {
                if (m_files.ContainsKey(fieldName))
                {
                    HttpPostedFile file = m_files[fieldName];
                    if (file != null)
                    {
                        stream = file.InputStream;
                        filename = file.FileName;
                    }
                }
            }
        }

        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            m_files = files; // temporary state data
            ProcessFormData(formData, GetFile);
            m_files = null; // remove temporary state data
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            // Call ExecuteViewProcessFormData with empty parameters.
            ExecuteViewProcessFormData(new NameValueCollection(0), delegate(string formFieldName, out Stream stream, out string filename)
            {
                stream = null;
                filename = null;
                return;
            });
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public override void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            // Execute View: validate posted files
            if (Context.View == SessionView.Execute)
            {
                m_files = files; // temporary state data
                ValidateFormData(GetFile);
                m_files = null; // remove temporary state data
            }

            // RandomAccess View:
            else if (Context.View == SessionView.RandomAccess)
            {
                ValidateRandomAccessView(formData);
            }
        }
    }

    /// <summary>
    /// Represents processor to Essay interactions, which corresponds to the <c>AssessmentItemType.TextArea</c>.
    /// </summary>
    internal class EssayFormDataProcessor : FormDataProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal EssayFormDataProcessor(RloProcessFormDataContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Execute view version of ProcessFormData.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses, or null if called from ProcessSessionEnd.</param>
        private void ExecuteViewProcessFormData(NameValueCollection formData)
        {
            StringBuilder bld = new StringBuilder();
            int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
            for (int ordinal = 0; ordinal < responseOptionCount; ordinal++)
            {
                if (formData == null)
                {
                    // If formData is null, this was called from ProcessSessionEnd, and means there are no
                    // learner responses.
                    AppendLearnerResponse(bld, String.Empty);
                }
                else
                {
                    AppendLearnerResponse(bld, FormDataProcessor.ConvertPostedTextAreaDataToDatabaseFormat(
                        formData[TextAreaAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture))]));
                }
            }
            Interaction.LearnerResponse = bld.ToString();
        }

        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            if (Context.View == SessionView.Execute)
            {
                ExecuteViewProcessFormData(formData);
            }
            else if (Context.View == SessionView.RandomAccess)
            {
                ProcessRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            ExecuteViewProcessFormData(null);
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public override void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            if (Context.View == SessionView.Execute)
            {
                // There must be form data for every text area
                int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                for (int ordinal = 0; ordinal < responseOptionCount; ordinal++)
                {
                    string ordinalAsString = ordinal.ToString(CultureInfo.InvariantCulture);
                    // if the form data is absent, it is an error
                    string formFieldName = TextAreaAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalAsString);
                    if (formData[formFieldName] == null)
                    {
                        throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                            AIResources.MissingFormField, formFieldName));
                    }
                }
            }

            // RandomAccess View:
            else if (Context.View == SessionView.RandomAccess)
            {
                ValidateRandomAccessView(formData);
            }
        }
    }

    /// <summary>
    /// Represents processor to fill in the blank interactions, which corresponds to the <c>AssessmentItemType.Text</c>.
    /// </summary>
    internal class FillInFormDataProcessor : FormDataProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal FillInFormDataProcessor(RloProcessFormDataContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Execute view version of ProcessFormData.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        private void ExecuteViewProcessFormData(NameValueCollection formData)
        {
            // This interaction is associated with the text assessment item type.
            // The Interaction.CorrectResponse field looks like this:
            // "{case_matters=false}{order_matters={0}}encoded answer 1[,]encoded answer 2[,]etc."
            // where {0} may be either "true" or "false", and the encoded answers were encoded
            // by calling LrmRloHandler.EncodePattern().
            // The learner submitted form fields will not be encoded, so encode them with 
            // LrmRloHandler.EncodePattern() to compare
            // with the encoded answers in the CorrectResponse field, and in order to add them
            // to the LearnerResponse field.
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            List<string> correctResponses;
            bool ignoreCase;
            bool orderMatters;
            float perFieldScore = 0;
            float firstFieldScore = 0;

            // set the result state to correct.  It will be set to incorrect if any answers are incorrect.
            Interaction.Result.State = InteractionResultState.Correct;

            GetCorrectResponses(Interaction, out correctResponses, out ignoreCase, out orderMatters);
            // There must be a correct response at index 0 to do autograding.
            if (Interaction.CorrectResponses.Count > 0)
            {
                // For scoring, the first ordinal gets a different score than the others, to account for rounding errors.
                if (Interaction.Score.Maximum.HasValue)
                {
                    perFieldScore = Interaction.Score.Maximum.Value / correctResponses.Count;
                    if (correctResponses.Count == 1)
                    {
                        firstFieldScore = perFieldScore;
                    }
                    else
                    {
                        firstFieldScore = Interaction.Score.Maximum.Value - (perFieldScore * (correctResponses.Count - 1));
                    }
                }
            }
            // Go through all the correct responses and see if the learner posted the correct answers.
            float score = 0;
            StringBuilder learnerResponses = new StringBuilder();
            if (orderMatters)
            {
                // correctResponses.Length is the maximum ordinal
                for (int ordinal = 0; ordinal < correctResponses.Count; ordinal++)
                {
                    string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                    // Remove any previously added comment extension data.
                    if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinalString)))
                    {
                        Interaction.ExtensionData.Remove(InteractionExtensionDataKeys.AutogradeResponse(ordinalString));
                    }

                    string learnerResponse = formData[TextAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalString)];
                    if (learnerResponse == null)
                    {
                        // If the form field isn't submitted, the learner didn't type anything in the text field.
                        // Use String.Empty 
                        learnerResponse = String.Empty;
                    }
                    else
                    {
                        learnerResponse = LrmRloHandler.EncodePattern(AssessmentItem.LrdgDecodeString(learnerResponse));
                    }
                    // see if this is the correct response for the ordinal
                    if (IsCorrectResponse(correctResponses[ordinal], learnerResponse, ignoreCase))
                    {
                        // score is incremented for each correct answer.
                        if (ordinal == 0) score += firstFieldScore;
                        else score += perFieldScore;
                    }
                    else
                    {
                        // the answer was incorrect.  Add the correct answer extension data.
                        Interaction.Result.State = InteractionResultState.Incorrect;
                        Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AutogradeResponse(ordinalString), correctResponses[ordinal]);
                    }
                    AppendLearnerResponse(learnerResponses, learnerResponse);
                }
            }
            else
            {
                // if order does not matter, use the formData key to find the match.
                // Copy the pertinant formData into a new List
                // and convert to all lower case if case does not matter.
                List<string> responseList = new List<string>(correctResponses.Count);
                for (int ordinal = 0; ordinal < correctResponses.Count; ordinal++)
                {
                    string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                    // Remove any previously added comment extension data.
                    if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinalString)))
                    {
                        Interaction.ExtensionData.Remove(InteractionExtensionDataKeys.AutogradeResponse(ordinalString));
                    }

                    string key = TextAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinalString);
                    string learnerResponse = AssessmentItem.LrdgDecodeString(formData[key]);
                    if (learnerResponse != null)
                    {
                        learnerResponse = LrmRloHandler.EncodePattern(learnerResponse);
                        AppendLearnerResponse(learnerResponses, learnerResponse);
                        if (ignoreCase)
                        {
                            responseList.Add(learnerResponse.ToLower(CultureInfo.CurrentUICulture));
                            correctResponses[ordinal] = correctResponses[ordinal].ToLower(CultureInfo.CurrentUICulture);
                        }
                        else
                        {
                            responseList.Add(learnerResponse);
                        }
                    }
                }
                // responseList is now filled with the learner responses (both responseList and correctResponses are
                // now all lower case if case doesn't matter.)
                // Find correct matches, and keep track of the ordinals that don't match.
                List<int> incorrectResponseOrdinals = new List<int>(correctResponses.Count);
                for (int ordinal = 0; ordinal < responseList.Count; ordinal++)
                {
                    bool correct = false;
                    foreach (string correctResponseString in correctResponses)
                    {
                        if (IsCorrectResponse(correctResponseString, responseList[ordinal], false))
                        {
                            correct = true;
                            // score is incremented for each correct answer.
                            if (ordinal == 0) score += firstFieldScore;
                            else score += perFieldScore;
                            // remove the correct response once it has been matched
                            correctResponses.RemoveAt(correctResponses.IndexOf(correctResponseString));
                            break;
                        }
                    }
                    if (!correct)
                    {
                        // keep track of the ordinals for incorrect answers.
                        Interaction.Result.State = InteractionResultState.Incorrect;
                        incorrectResponseOrdinals.Add(ordinal);
                    }
                }

                // Add comment extension data for all incorrect answers.  The comment string is the remaining
                // correct answers, separated by the resource separator.
                if (correctResponses.Count > 0)
                {
                    string comment = correctResponses[0];
                    for (int ordinal = 1; ordinal < correctResponses.Count; ordinal++)
                    {
                        comment += AIResources.CorrectAnswerSeparatorHtml + correctResponses[ordinal];
                    }
                    for (int i = 0; i < incorrectResponseOrdinals.Count; i++)
                    {
                        Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AutogradeResponse(
                            incorrectResponseOrdinals[i].ToString(CultureInfo.InvariantCulture)),
                            comment);
                    }
                }
            }
            Interaction.LearnerResponse = learnerResponses.ToString();
            Interaction.Score.Raw = score;
            Interaction.Evaluation.Points = score;
        }

        private char[] m_correctResponseSeparater = new char[] { ';' };
        /// <summary>
        /// Returns true if the supplied <paramref name="correctResponsePattern"/> contains the
        /// <paramref name="learnerResponse"/> according to the casing rules of
        /// <paramref name="ignoreCase"/>
        /// </summary>
        private bool IsCorrectResponse(string correctResponsePattern, string learnerResponse, bool ignoreCase)
        {
            // if the correctResponsePattern contains ; characters it must be split into multiple correct response patterns
            if (correctResponsePattern.Contains(";"))
            {
                string[] correctResponses = correctResponsePattern.Split(m_correctResponseSeparater);
                foreach (string correctResponse in correctResponses)
                {
                    if (String.Compare(correctResponse.Trim(), learnerResponse, ignoreCase, CultureInfo.CurrentUICulture) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return String.Compare(correctResponsePattern, learnerResponse, ignoreCase, CultureInfo.CurrentUICulture) == 0;
            }
        }

        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            if (Context.View == SessionView.Execute)
            {
                ExecuteViewProcessFormData(formData);
            }
            else if (Context.View == SessionView.RandomAccess)
            {
                ProcessRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            // Call ExecuteViewProcessFormData with empty parameters.
            ExecuteViewProcessFormData(new NameValueCollection(0));            
        }

        /// <summary>
        /// Returns the correct responses for the interaction.
        /// </summary>
        internal static void GetCorrectResponses(Interaction interaction, out List<string> correctResponses, out bool ignoreCase, out bool orderMatters)
        {
            if (interaction.CorrectResponses.Count > 0)
            {
                // Split on the "[,]" string.
                string[] patterns = LrmRloHandler.SplitPattern(interaction.CorrectResponses[0].Pattern);
                // The first correct response has the "{case_matters=false}{order_matters={0}}" text,
                // so parse it out.
                string[] firstResponse = patterns[0].Split(new char[] { '}' }, 3);
                // Even though the current TryAddDataModel() method always sets case_matters=false,
                // parse it here anyway in case that ever changes.
                ignoreCase = firstResponse[0].EndsWith("true", StringComparison.Ordinal) ? false : true;
                orderMatters = firstResponse[1].EndsWith("true", StringComparison.Ordinal) ? true : false;
                patterns[0] = firstResponse[2]; // get rid of the prefix stuff
                correctResponses = new List<string>(patterns.Length);
                for (int i = 0; i < patterns.Length; i++)
                {
                    correctResponses.Add(patterns[i]);
                }
            }
            else
            {
                correctResponses = new List<string>(0);
                ignoreCase = true;
                orderMatters = false;
            }
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public override void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            // Nothing to validate in Execute view.

            // RandomAccess View:
            if (Context.View == SessionView.RandomAccess)
            {
                ValidateRandomAccessView(formData);
            }
        }
    }

    /// <summary>
    /// Represents processor to matching interactions, which corresponds to the <c>AssessmentItemType.Select</c>.
    /// </summary>
    internal class MatchingFormDataProcessor : FormDataProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal MatchingFormDataProcessor(RloProcessFormDataContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Process form data sent from client for the current interaction for execute view.
        /// </summary>
        /// <param name="values">String array returned from GetPostedValues, or null if called from ProcessSessionEnd.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        private void ExecuteViewProcessFormData(string[] values)
        {
            // set the result state to correct.  It will be set to incorrect if any answers are incorrect.
            Interaction.Result.State = InteractionResultState.Correct;

            // The options were rendered in alphabetical order, in order to match the value to a correct response
            // create an array to map them.
            List<string> alphabeticalOptions = SelectAssessmentRenderer.CreateAlphabeticalList(Interaction.ExtensionData);
            List<string> ordinalOptions = SelectAssessmentRenderer.CreateOrdinalList(Interaction.ExtensionData);
            StringBuilder bld = new StringBuilder(); // for the learner response

            // if values is null, there was no posted data - all questions are unanswered.  Most likely ProcessSessionEnd
            // has been called.
            if (values == null)
            {
                // For each option, append a learner response of "0" (unanswered) and add the correct response
                // to the autograde response.
                for (int ordinal = 0; ordinal < ordinalOptions.Count; ordinal++)
                {
                    AppendLearnerResponse(bld, "0");
                    Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AutogradeResponse(ordinal.ToString(CultureInfo.InvariantCulture)),
                        ordinalOptions[ordinal]);
                }
                Interaction.LearnerResponse = bld.ToString();
                Interaction.Score.Raw = 0;
                Interaction.Evaluation.Points = 0;
                Interaction.Result.State = InteractionResultState.Incorrect;
            }
            else
            {
                int ordinal = 0;
                float score = 0;
                float perOptionScore = Interaction.Score.Maximum.GetValueOrDefault() / values.Length;
                float firstOptionScore = Interaction.Score.Maximum.GetValueOrDefault() - (perOptionScore * (values.Length - 1));
                foreach (string value in values)
                {
                    // Remove any previously added comment extension data.
                    string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                    if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinalString)))
                    {
                        Interaction.ExtensionData.Remove(InteractionExtensionDataKeys.AutogradeResponse(ordinalString));
                    }
                    // value is the value of the posted data, which corresponds to the order of the alphabetized options.
                    // However, a value of 0 always indicates no selection. Subtract one to get the index into the list of options.
                    int selectedOption = Convert.ToInt32(value, CultureInfo.InvariantCulture) - 1;
                    string learnerResponse = "0";
                    bool correct = false;
                    if (selectedOption >= 0)
                    {
                        learnerResponse = alphabeticalOptions[selectedOption];
                        // If the learner response matches the ordinal value, the answer is correct.
                        if (String.CompareOrdinal(learnerResponse, ordinalOptions[ordinal]) == 0)
                        {
                            correct = true;
                        }
                    }
                    AppendLearnerResponse(bld, value);
                    if (correct)
                    {
                        // the response is correct, so increment the score.
                        if (ordinal == 0)
                        {
                            score += firstOptionScore;
                        }
                        else
                        {
                            score += perOptionScore;
                        }
                    }
                    else
                    {
                        // the answer was incorrect, so add the correct answer extension data.
                        Interaction.Result.State = InteractionResultState.Incorrect;
                        Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AutogradeResponse(ordinalString),
                            ordinalOptions[ordinal]);
                    }
                    ordinal++;
                }
                Interaction.LearnerResponse = bld.ToString();
                Interaction.Score.Raw = score;
                Interaction.Evaluation.Points = score;
            }
        }

        /// <summary>
        /// Process form data sent from client for the current interaction.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <remarks>Interaction must be set prior to calling this method.</remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            if (Context.View == SessionView.Execute)
            {
                ExecuteViewProcessFormData(GetPostedValues(formData));
            }
            else if (Context.View == SessionView.RandomAccess)
            {
                ProcessRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Requests the form data processor to do whatever is required to exit from the current activity.
        /// This request may only be issued when the session is in Execute view and is not active -- it is 
        /// either Completed or Abandoned.
        /// </summary>
        /// <param name="context">The context within which the command is processed</param>
        public override void ProcessSessionEnd(RloDataModelContext context)
        {
            // Call ExecuteViewProcessFormData with empty data
            ExecuteViewProcessFormData(null);
        }

        /// <summary>
        /// Returns if the posted form data is valid for the current Interaction, otherwise
        /// throws <Typ>InvalidFormDataException</Typ>.
        /// </summary>
        /// <param name="formData">Form data containing the learner responses.</param>
        /// <param name="files">File collection from the <Typ>HttpRequest</Typ>.  E.g. <c>HttpRequest.Files</c>.</param>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        public override void ValidateFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile>  files)
        {
            if (Context.View == SessionView.Execute)
            {
                GetPostedValues(formData);
            }

            // RandomAccess View:
            else if (Context.View == SessionView.RandomAccess)
            {
                ValidateRandomAccessView(formData);
            }
        }

        /// <summary>
        /// Returns the posted values, by ordinal, for the &lt;select&gt; that represents this interaction.
        /// Returns null if there are no response options in the interaction extension data.
        /// Otherwise, returns a string[] with a length equal to the response option count.  Each array member
        /// will be the option value, where "0" represents
        /// the blank choice and other values are the other selections.
        /// </summary>
        /// <exception cref="InvalidFormDataException">The <paramref name="formData"/> did not have
        /// the correct name/value pairs expected by the interaction.</exception>
        private string[] GetPostedValues(NameValueCollection formData)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            string[] values = null;
            // For each select tag, check the submitted option and ensure it exists and is within the legal range.
            if(Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                int responseOptionCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount];
                values = new string[responseOptionCount];
                for(int ordinal = 0; ordinal < responseOptionCount; ordinal++)
                {
                    string formFieldName = SelectAssessmentRenderer.RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture));
                    values[ordinal] = formData[formFieldName];
                    if (String.IsNullOrEmpty(values[ordinal]))
                    {
                        throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                            AIResources.MissingFormField, formFieldName));
                    }
                    try
                    {
                        int i = Convert.ToInt32(values[ordinal], CultureInfo.InvariantCulture);
                        // The value can be one greater than the response option count, since "0" corresponds to the blank option.
                        if ( i >= responseOptionCount + 1 || i < 0 )
                        {
                            throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                                AIResources.InvalidOptionValue, values[ordinal], formFieldName));
                        }
                    }
                    catch (FormatException)
                    {
                        throw new InvalidFormDataException(String.Format(CultureInfo.InvariantCulture,
                            AIResources.InvalidOptionValue, values[ordinal], formFieldName));
                    }
                }
            }
            return values;
        }
    }
}
