/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents.DataModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{
    internal static class ImageFileName
    {
        internal static string GetFullPath(Uri embeddedUIResourcePath, string imageFileName)
        {
            return Path.Combine(embeddedUIResourcePath.OriginalString, imageFileName).Replace('\\', '/');
        }

        /// <summary>
        /// An image showing a check mark.
        /// </summary>
        internal const string Correct = "Correct.gif";

        /// <summary>
        /// An image showing an 'x'.
        /// </summary>
        internal const string Incorrect = "Incorrect.gif";

        /// <summary>
        /// A checkbox unchecked.
        /// </summary>
        internal const string BoxOff = "BoxOff.gif";

        /// <summary>
        /// A checkbox correctly unchecked.
        /// </summary>
        internal const string BoxOffCorrect = "BoxOffCorrect.gif";

        /// <summary>
        /// A checked checkbox.
        /// </summary>
        internal const string BoxOn = "BoxOn.gif";

        /// <summary>
        /// A correctly checked checkbox.
        /// </summary>
        internal const string BoxOnCorrect = "BoxOnCorrect.gif";

        /// <summary>
        /// An incorrectly checked checkbox.
        /// </summary>
        internal const string BoxOnWrong = "BoxOnWrong.gif";

        /// <summary>
        /// An image showing a radio button that has been selected and determined to be correct.
        /// </summary>
        internal const string ButtonOnCorrect = "ButtonOnCorrect.gif";

        /// <summary>
        /// An image showing a radio button that has been selected and determined to be incorrect.
        /// </summary>
        internal const string ButtonOnWrong = "ButtonOnWrong.gif";

        /// <summary>
        /// An image showing a radio button that was not selected but should have been the correct response.
        /// </summary>
        internal const string ButtonOffCorrect = "ButtonOffCorrect.gif";

        /// <summary>
        /// An image showing a radio button that was not selected.
        /// </summary>
        internal const string ButtonOff = "ButtonOff.gif";
    }

    /// <summary>
    /// Strings used for extension data keys in interactions.
    /// </summary>
    internal static class InteractionExtensionDataKeys
    {
        /// <summary>
        /// The number of response options contributing to this Interaction.  Only used for some assessment item types.
        /// </summary>
        internal const string ResponseOptionCount = "ms.learningcomponents.responseOptionCount";

        /// <summary>
        /// Keeps track of the first non-score assessment item type contributing to this Interaction.
        /// </summary>
        internal const string AssessmentItemType = "ms.learningcomponents.assessmentItemType";

        /// <summary>
        /// Key used by the autograding to add comments (e.g. correct answers) to the review view.  The lack of a comment
        /// of this type for a particular ordinal indicates the answer to the corresponding ordinal was a correct one.
        /// </summary>
        /// <param name="ordinal">ordinal of the assessment item.</param>
        /// <returns>Key.</returns>
        internal static string AutogradeResponse(string ordinal)
        {
            return "ms.learningcomponents.autogradeResponse" + "." + ordinal;
        }

        /// <summary>
        /// Key for an assessment item value, based on the ordinal.
        /// </summary>
        /// <param name="ordinal">ordinal of the assessment item.</param>
        /// <returns>Key.</returns>
        internal static string MaxPoints(string ordinal)
        {
            return "ms.learningcomponents.maxpoints" + "." + ordinal;
        }

        /// <summary>
        /// Key for a file attachment.  Value will contain either (bool)false if there is no file,
        /// or byte[] file.
        /// </summary>
        /// <param name="ordinal">ordinal of the assessment item.</param>
        /// <returns>Key.</returns>
        internal static string FileAttachment(string ordinal)
        {
            return "ms.learningcomponents.fileAttachment" + "." + ordinal;
        }

        /// <summary>
        /// File extension for the file attachment.
        /// </summary>
        /// <param name="ordinal">ordinal of the assessment item.</param>
        /// <returns>File extension.</returns>
        internal static string FileExtension(string ordinal)
        {
            return FileAttachment(ordinal) + ".fileExtension";
        }

        /// <summary>
        /// Indicates the page has been rendered at least once if the key exists in the learning data model extension data.
        /// </summary>
        internal const string PageRendered = "ms.learningcomponents.page_rendered";

        /// <summary>
        /// Indicates the page has been autograded at least once if the key exists in the learning data model extension data.
        /// </summary>
        internal const string PageAutograded = "ms.learningcomponents.page_autograded";

        private const string MatchingOptionValueBaseKey = "ms.learningcomponents.matchingOptionValue";
        /// <summary>
        /// Holds the value for a matching type interaction for the given ordinal.
        /// </summary>
        /// <param name="itemCount"></param>
        /// <returns></returns>
        internal static string MatchingOptionValue(int itemCount)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", MatchingOptionValueBaseKey, itemCount);
        }
    }

    /// <summary>
    /// Represents a handler to render assessment items in various views. This includes 
    /// adding initial data into the data model for the assessment item.
    /// </summary>
    internal abstract class AssessmentItemRenderer
    {
        private AssessmentItem m_assessmentItem;
        private RloDataModelContext m_context;
        private RloRenderContext m_renderContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal AssessmentItemRenderer(RloDataModelContext context)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            m_context = context;
            m_renderContext = context as RloRenderContext;
        }

        /// <summary>
        /// The context given to the constructor, as a <Typ>RloRenderContext.</Typ>
        /// </summary>
        /// <exception cref="LearningComponentsInternalException">The context given to the contructor is not a <Typ>RloRenderContext</Typ>.</exception>
        internal RloRenderContext RenderContext
        {
            get
            {
                if (m_renderContext == null)
                {
                    throw new LearningComponentsInternalException("AIR01");
                }
                else
                {
                    return m_renderContext;
                }
            }
        }

        /// <summary>
        /// Sets the current context.
        /// </summary>
        internal RloDataModelContext DataModelContext
        {
            set
            {
                m_context = value;
                m_renderContext = value as RloRenderContext;
            }
        }

        /// <summary>
        /// Returns true if the assessment item's type is the same as that contained within the interaction
        /// stored in the learning data model associated with the assessment item's id.
        /// </summary>
        internal bool IsSameAsInteractionType
        {
            get
            {
                LearningDataModel learningDataModel = m_context.LearningDataModel;
                //if(learningDataModel.Interactions.Contains(AssessmentItem.Id))
                if(learningDataModel.InteractionListContains(AssessmentItem.Id))
                {
                    //Interaction interaction = learningDataModel.Interactions[AssessmentItem.Id];
                    Interaction interaction = learningDataModel.InteractionListElement(AssessmentItem.Id);
                    if(interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
                    {
                        if (AssessmentItem.Type == (AssessmentItemType)interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">An ordinal indicating the number of times the same assessment item type has 
        /// previously appeared with the same assessment item id, during the current rendering process.
        /// </param>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        public abstract HtmlString Render(int ordinal);

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false may be returned in an error case, for instance when
        /// attempting to add a checkbox type when the interaction has already been added with a radio button
        /// type.</returns>
        public abstract bool TryAddToDataModel();

        /// <summary>
        /// Gets and sets the assessment item currently being processed. This is the &lt;img&gt; assessment item
        /// included in the raw LRM content.
        /// </summary>
        /// <remarks>
        /// This is set by AssessmentItemManager. Used by subclasses.
        /// </remarks>
        public AssessmentItem AssessmentItem 
        { 
            get 
            {
                if (m_assessmentItem == null) throw new LearningComponentsInternalException("AIR003");
                else return m_assessmentItem; 
            }
            set 
            {
                // out with the old, in with the new
                m_assessmentItem = value; 
            }
        }

        /// <summary>
        /// Return the Interaction that is associated with the assessment item that is currently being rendered.
        /// </summary>
        /// <remarks>
        /// If an interacation does not already exist for this assessment item id, the returned interaction is
        /// a newly created one, containing extension data for assessment item count equal to 0, but no
        /// extension data for assessment item type.
        /// </remarks>
        protected Interaction Interaction
        {
            get
            {
                // Use AssessmentItem Id to get the Interaction from the context.LearningDataModel.
                LearningDataModel learningDataModel = m_context.LearningDataModel;
                //if(learningDataModel.Interactions.Contains(AssessmentItem.Id))
                if(learningDataModel.InteractionListContains(AssessmentItem.Id))
                {
                    //return learningDataModel.Interactions[AssessmentItem.Id];
                    return learningDataModel.InteractionListElement(AssessmentItem.Id);
                }

                // no interaction found, so create it
                // if the interaction hasn't been added yet, create a new one.
                Interaction interaction = learningDataModel.CreateInteraction();
                interaction.Id = AssessmentItem.Id;

                // Add the interaction to the data model as long as this is execute view
                if (m_context.View == SessionView.Execute)
                {
                    learningDataModel.Interactions.Add(interaction);
                }
                return interaction;
            }
        }

        /// <summary>
        /// Returns the instructor comments for the requested ordinal, or String.Empty if none.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        protected string InstructorComments(int ordinal)
        {
            if (Interaction.Evaluation.Comments.Count > ordinal)
            {
                return Interaction.Evaluation.Comments[ordinal].CommentText;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Returns the <Typ>CorrectResponse</Typ> for the interaction associated with this renderer.
        /// </summary>
        /// <remarks>
        /// For these renderer's, there is only ever one correct response although cmi.interactions.n.correct_responses
        /// supports multiple values.  This property simply returns the first correct response in the list of
        /// correct responses in the Interaction.  To override this behavior, access <c>Interaction.CorrectResponses</c>
        /// directly.
        /// </remarks>
        protected CorrectResponse CorrectResponse
        {
            get
            {
                CorrectResponse response;
                if (Interaction.CorrectResponses.Count > 0)
                {
                    response = Interaction.CorrectResponses[0];
                }
                else
                {
                    response = Interaction.DataModel.CreateCorrectResponse();
                    Interaction.CorrectResponses.Add(response);
                }
                return response;
            }
        }

        /// <summary>
        /// Returns a name for use in the instructor comments form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderInstructorCommentsFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "TA*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Renders the instructor comments.
        /// </summary>
        protected void RenderInstructorComments(StringBuilder bld, int ordinal)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            // RandomAccess view is the same as Review view, except that there is an intructor comments field in
            // RandomAccess view.
            if (RenderContext.View == SessionView.RandomAccess)
            {
                // Render the instructor comments field.  The conversion of <br> to %0d%0a is mirrored in
                // FormDataProcessor.ProcessRandomAccessView.
                bld.AppendLine(String.Format(CultureInfo.InvariantCulture,
                    "<br><br>Instructor comments:<br><textarea style=\"width: 100%; font-family:Arial\"" +
                    " name=\"{0}\" id=\"{0}\" rows=\"3\" onblur=\"mlc_ValidateComment(this)\">{1}</textarea>",
                    RenderInstructorCommentsFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture)),
                    FormDataProcessor.ConvertDatabaseFormatToTextArea(new PlainTextString(InstructorComments(ordinal)).ToHtmlString())
                ));
            }
            else
            {
                // Output any instructor comments.
                HtmlString instructorComments = FormDataProcessor.ConvertDatabaseFormatToHtml(InstructorComments(ordinal));
                if (!String.IsNullOrEmpty(instructorComments))
                {
                    bld.Append("<BR><BR>");
                    bld.Append(AIResources.InstructorCommentsColonHtml);
                    bld.Append("<BR><I><FONT COLOR=\"MAROON\">");
                    bld.Append(instructorComments.ToString());
                    bld.Append("</FONT></I>");
                }
            }
        }
    }

    /// <summary>
    ///  An assessment item from raw LRM content. 
    /// </summary>
    internal class AssessmentItem
    {
        // These are all the possible attributes of assessment items in LRM format.
        private AssessmentItemType m_type;
        private string m_id;
        private int? m_cols;
        private int? m_rows;
        private string m_akey;
        private bool? m_uak;
        private float? m_maxPts;
        private string m_text;

        // Note that this class must parse the encoding rules described in LRDG for assessment items.
        // E.g., akey=Blue%20Sky or akey=Blue+Sky means that the argument value for akey=  is "Blue Sky." 
        // See LRDG section 3.4.3
        private AssessmentItem()
        {
            // nothing to do - values are assigned in the Parse() method.
        }

        /// <summary>
        /// Used by IsLegalId().
        /// </summary>
        private static Regex regexBadId = new Regex(@"[^0-9a-zA-Z\x2E\x2D\x3A\x5F]");

        /// <summary>
        /// Returns true if id is a legal syntactic id of an assessment markup.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static bool IsLegalId(string id)
        {
            // The id= value can not be longer than 31 characters. The characters are limited to digits (0-9), 
            // letters (a-z and A-Z), periods (.), hyphens (-), colons (:), and underscores (_).
            if (String.IsNullOrEmpty(id) || id.Length > 31 || regexBadId.IsMatch(id)) return false;
            return true;
        }

        /// <summary>
        /// Returns the "cols" value or a default of 40 if the value isn't in the dictionary.
        /// </summary>
        /// <param name="parsedValues"></param>
        /// <returns></returns>
        private static int GetCols(Dictionary<string, string> parsedValues)
        {
            int result;
            if (parsedValues != null
                && parsedValues.ContainsKey(AiStrings.Cols)
                && Int32.TryParse(parsedValues[AiStrings.Cols], NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            else return 40;
        }

        /// <summary>
        /// Returns the "maxpts" value, or a default value of 0 if the value isn't in the dictionary, or 1000 if greater
        /// than 1000, or -1000 if less than -1000.
        /// </summary>
        /// <param name="parsedValues"></param>
        /// <returns></returns>
        private static float GetMaxPts(Dictionary<string, string> parsedValues)
        {
            float result;
            if (parsedValues != null
                && parsedValues.ContainsKey(AiStrings.Maxpts)
                && float.TryParse(parsedValues[AiStrings.Maxpts], NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                if (result > 1000) return 1000;
                if (result < -1000) return -1000;
                return result;
            }
            else return 0F;
        }


        /// <summary>
        /// Used by DecodeHiByteChars - finds "~xxxx" where 'x' is a hex character.
        /// </summary>
        private static Regex regexHiByteChars = new Regex(@"~[0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]");

        /// <summary>
        /// The LRE authoring app encodes hibyte chars as ~xxxx where xxxx is the hex representation of the unicode character
        /// value. This function decodes them and returns the decoded string.
        /// </summary>
        /// <param name="text">Text to decode.  This should already by decoded by HttpUtility.UrlDecode(), in order to convert
        /// e.g. %7e into ~.</param>
        private static string DecodeHiByteChars(string text)
        {
            StringBuilder bld = new StringBuilder(text.Length);
            // Find each occurance of the ~ character.  If the 4 characters following it are hexidecimal digits,
            // convert those 5 characters into the single unicode character.
            Match m = regexHiByteChars.Match(text);
            int index = 0;
            while (m.Success)
            {
                // copy over any characters before the match
                if(m.Index > index)
                {
                    bld.Append(text.Substring(index, m.Index - index));
                }
                // append the converted character and advance to the index past the four hex digits
                bld.Append(Convert.ToChar(Convert.ToInt32(text.Substring(m.Index + 1, 4), 16)));
                index = m.Index + 5;
                m = regexHiByteChars.Match(text, index);
            }
            // copy over any remaining characters
            if (index < text.Length)
            {
                bld.Append(text.Substring(index));
            }
            return bld.ToString();
        }

        private static Regex regexCollapseWhitespace = new Regex(@"\s\s");

        /// <summary>
        /// Collapse any whitespace between words in the answer into one space.
        /// </summary>
        private static string CollapseWhitespace(string text)
        {
            StringBuilder bld = new StringBuilder(text.Length);
            // Find each occurance of at least two whitespace characters side-by-side.
            Match m = regexCollapseWhitespace.Match(text);
            int index = 0;
            while (m.Success)
            {
                // copy over any characters before the match
                if (m.Index > index)
                {
                    bld.Append(text.Substring(index, m.Index - index));
                }
                // append a space character and advance to the next non-whitespace character
                bld.Append(" ");
                index = m.Index;
                while (Char.IsWhiteSpace(text, index))
                {
                    if (++index >= text.Length) break;
                }
                m = regexCollapseWhitespace.Match(text, index);
            }
            // copy over any remaining characters
            if (index < text.Length)
            {
                bld.Append(text.Substring(index));
            }
            return bld.ToString();
        }

        /// <summary>
        /// Decodes the string p according to the rules in the LRDG regarding assessment markup argument value rules.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static string LrdgDecodeString(string p)
        {
            if (String.IsNullOrEmpty(p)) return String.Empty;
            // Assessment markup argument values, for example, the string after the equal sign (=) in akey=Canada, can contain 
            // arbitrary characters (except ASCII 0 to 31, inclusive), but they need to be encoded. For example, 
            //     akey=Blue%20Sky or akey=Blue+Sky means that the argument value for akey=  is "Blue Sky." The rules of 
            // character encoding are as follows:
            // The following characters need to be encoded as "%XX", where XX is two hexadecimal digits (for example, 
            //     "%3D" for "="):  " # % & + , \ ~
            // The space character may be encoded as "+" or "%20", or left as is.
            // Characters above ASCII 127 should be UTF-8 encoded. 
            //
            // The Learning Resource Editor (LRE) encodes these characters 
            // in the following way, but it is not recommended for other utilities.  We support it here in order to
            // support LR's created by the LRE.
            // o	Characters greater than ASCII 127 and less than or equal to ASCII 255 are encoded using the same "%XX" 
            // format described above.
            // o	Characters above ASCII 255 are encoded in the form ~XXXX, where XXXX is four hexadecimal digits (for 
            //     example, "~898B" for the Kanji character representing "mi").
            // o	Leading and trailing white spaces are removed. 
            // o	Multiple white spaces are compressed into one space.
            // Decode any high byte characters that were encoded using the LRE
            return CollapseWhitespace(DecodeHiByteChars(HttpUtility.UrlDecode(p.Trim())));
        }

        /// <summary>
        /// Common strings used during AssessmentItem parsing
        /// </summary>
        private class AiStrings
        {
            private AiStrings() { }

            internal const string Akey = "AKEY";
            internal const string Cols = "COLS";
            internal const string Id = "ID";
            internal const string Maxpts = "MAXPTS";
            internal const string Rows = "ROWS";
            internal const string Text = "TEXT";
            internal const string Type = "TYPE";
            internal const string Uak = "UAK";
        }

        /// <summary>
        /// Use an assessmentItem from an LRM page.htm to create an AssessmentItem object. 
        /// </summary>
        /// <param name="assessmentItem">The src attribute value of the img tag that corresponds to the assessment item.</param>
        /// <returns>The object representing the parsed value.</returns>
        /// <remarks>
        /// If more than one of the same name/value pairs exists with the same name, only the first is used.
        /// </remarks>
        /// <exception cref="FormatException">Thrown if the <paramref name="assessmentItem"/> cannot be parsed.</exception>
        public static AssessmentItem Parse(string assessmentItem)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            // assessmentItem contains the value of the src attribute, which is of the form:
            // "http://localhost:3535/mslamrk,type=1,id=3,cols=10,akey=over,uak=0".
            // Split this string by the ',' character.
            string[] commaSplit = assessmentItem.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> parsedValues = new Dictionary<string, string>(10);
            foreach (string pair in commaSplit)
            {
                string[] equalSplit = pair.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                if (equalSplit.Length == 2) // gets rid of the "protocol://server/mslamrk" portion, and any non-value markup
                {
                    // convert id to all upper case
                    string id = equalSplit[0].ToUpperInvariant();
                    if (!parsedValues.ContainsKey(id))
                    {
                        parsedValues.Add(id, equalSplit[1]);
                    }
                }
            }

            // if (id contains invalid values per LRDG) throw FormatException.  id is required.
            if (!parsedValues.ContainsKey(AiStrings.Id) || !IsLegalId(parsedValues[AiStrings.Id]))
            {
                throw new FormatException(String.Format(CultureInfo.InvariantCulture, AIResources.InvalidId, assessmentItem));
            }

            // if (there is no type value) throw FormatException.  see switch statement below.
            if (!parsedValues.ContainsKey(AiStrings.Type))
            {
                throw new FormatException(String.Format(CultureInfo.InvariantCulture, AIResources.InvalidType, assessmentItem));
            }

            // if other values are not correct (eg, rows is not numeric) then just ignore the invalid values 
            // and 'be nice' about returning defaults that are appropriate for the type.
            AssessmentItem item = new AssessmentItem();
            item.m_id = parsedValues[AiStrings.Id]; // id is valid because of above check
            switch(parsedValues[AiStrings.Type]) // type is valid because of above check
            {
                default:
                    // If the type is not one of the cases, it is an invalid value.
                    throw new FormatException(String.Format(CultureInfo.InvariantCulture, AIResources.InvalidType, assessmentItem));
                case "1":
                    item.m_type = AssessmentItemType.Text;
                    item.m_cols = GetCols(parsedValues);
                    if (parsedValues.ContainsKey(AiStrings.Akey)) // optional argument
                    {
                        item.m_akey = LrdgDecodeString(parsedValues[AiStrings.Akey]); // optional argument
                        // according the LRDG, uak can be "0" or "1", and if it exists then
                        // akey must exist.  Here, if uak exists but akey doesn't, uak is never
                        // parsed.  Also, if uak exists but is anything other than "0" it is
                        // assumed to be "1" by default.
                        if (parsedValues.ContainsKey(AiStrings.Uak))
                        {
                            if (parsedValues[AiStrings.Uak] == "0") item.m_uak = false;
                            else item.m_uak = true;
                        }
                    }
                    else
                    {
                        item.m_akey = String.Empty;
                    }
                    break;
                case "2":
                    item.m_type = AssessmentItemType.TextArea;
                    item.m_cols = GetCols(parsedValues);
                    int rows;
                    if (parsedValues.ContainsKey(AiStrings.Rows)
                        && Int32.TryParse(parsedValues[AiStrings.Rows], NumberStyles.Integer, CultureInfo.InvariantCulture, out rows))
                    {
                        item.m_rows = rows;
                    }
                    else item.m_rows = 5;
                    break;
                case "3":
                    item.m_type = AssessmentItemType.Radio;
                    item.m_maxPts = GetMaxPts(parsedValues);
                    break;
                case "4":
                    item.m_type = AssessmentItemType.Checkbox;
                    item.m_maxPts = GetMaxPts(parsedValues);
                    break;
                case "5":
                    item.m_type = AssessmentItemType.ItemScore;
                    item.m_maxPts = GetMaxPts(parsedValues);
                    break;
                case "7":
                    item.m_type = AssessmentItemType.Select;
                    if (parsedValues.ContainsKey(AiStrings.Text))
                    {
                        item.m_text = LrdgDecodeString(parsedValues[AiStrings.Text]);
                    }
                    else
                    {
                        item.m_text = String.Empty;
                    }
                    break;
                case "8":
                    item.m_type = AssessmentItemType.Rubric;
                    item.m_maxPts = GetMaxPts(parsedValues);
                    break;
                case "9":
                    item.m_type = AssessmentItemType.File;
                    item.m_cols = GetCols(parsedValues);
                    break;
            }
            return item;
        }

        /// <summary>
        /// Specifies the assessment markup type. 
        /// </summary>
        public AssessmentItemType Type
        { 
            get { return m_type; }
        }

        /// <summary>
        /// Specifies the question (assessment item) number.
        /// </summary>
        /// <remarks>
        /// All sub-items within a given question must have the same id= value. The id= value can not be 
        /// longer than 31 characters. The characters are limited to digits (0-9), letters (a-z and A-Z), 
        /// periods (.), hyphens (-), colons (:), and underscores (_).
        /// </remarks>
        public string Id
        { 
            get { return m_id; }
        }

        /// <summary>
        /// Used by LrmRloHandler during rendering as a dictionary key.  It combines the Id and Type
        /// to create the key.
        /// </summary>
        internal string RenderKey
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}_{1}", Id, (int)Type);
            }
        }

        /// <summary>
        /// Specifies the width in characters of the associated form element.
        /// </summary>
        /// <remarks>
        /// Depending on the element, this value is used as the value for the cols or size attribute of the HTML element.
        /// </remarks>
        public int? Cols
        { 
            get { return m_cols; }
        }

        /// <summary>
        /// Specifies the height (in lines) of the associated form element.
        /// </summary>
        public int? Rows
        { 
            get { return m_rows; }
        }

        /// <summary>
        /// Answer key.
        /// </summary>
        /// <remarks>
        /// For Text assessment markup, akey= (if provided) specifies an answer key that indicates the correct 
        /// answer for automatic grading. Semicolons are used to separate multiple correct answers (for example, 
        /// "akey=color;colour"). Autograding matching is case-insensitive. If one student types "Color" in the 
        /// previous example, and another student types "colour," both answers will be graded as correct. During 
        /// grading, if akey= is not provided, the teacher must grade the question manually, either by entering 
        /// a grade in the space provided by the Item Score assessment markup, or by selecting the appropriate 
        /// check boxes provided by Rubric assessment markup.
        /// </remarks>
        public string AKey { 
            get { return m_akey; }
        }

        /// <summary>
        /// Unordered answer key.  If true, student answers may appear in any order.
        /// </summary>
        /// <remarks>
        /// This argument is used by Text assessment markup to indicate that the student answers may appear 
        /// in any order (among all sub-items in a question). For example, if the question is "name the three 
        /// primary colors," and three Text sub-items are provided, one sub-item can have "akey=red", the second 
        /// can have "akey=green", and the third can have "akey=blue", and all can have Uak == true, which means the 
        /// student will get full credit for entering "red", "green", and "blue" in the three text boxes in any order.
        /// </remarks>
        public bool? Uak { 
            get { return m_uak; }
        }

        /// <summary>
        /// Maximum points.
        /// </summary>
        /// <remarks>
        /// A floating-point number (positive, negative, or zero) that specifies the maximum number of points 
        /// for an item. maxpts= is used in Item Score assessment markup to indicate the score for a given 
        /// item. It is also used in Radio, Check Box, and Rubric assessment markup to specify a number 
        /// of points (positive or negative) for that particular sub-item. The points must be within the 
        /// range: -1000 &lt;= maxpts &gt;= 1000.
        /// </remarks>
        public float? MaxPoints
        { 
            get { return m_maxPts; }
        }

        /// <summary>
        /// For a Select assessment markup, text= specifies the text of one of the lines in the drop-down 
        /// list box and the correct answer for this sub-item.
        /// </summary>
        /// <remarks>
        /// For example, if a question has three Select sub-items, with "text=Beta" on the first, "text=Gamma" 
        /// on the second, and "text=Alpha" on the third, then in Assigned view (that is, while the student 
        /// is working on the learning resource) each of the three sub-items will appear as a drop-down list 
        /// box with these four lines: "" (blank), "Alpha", "Beta", and "Gamma" (sorted alphabetically, 
        /// with "" selected by default).
        /// </remarks>
        public string Text
        { 
            get { return m_text; }
        }
    }

    internal enum AssessmentItemType
    {
        // These values do not correspond to the type value in the assessment item tag, since 
        // those values are not contiguous.
        Text = 0,
        TextArea,
        Radio,
        Checkbox,
        ItemScore,
        Select,
        Rubric,
        File
    }

    /// <summary>
    /// AssessmentItemRenderer for the text assessment type.
    /// </summary>
    internal class TextAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal TextAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns the currently submitted answer for the given ordinal
        /// </summary>
        internal PlainTextString LearnerAnswer(int ordinal)
        {
            return new PlainTextString(FormDataProcessor.ExtractLearnerResponse(Interaction.LearnerResponse, ordinal)).ToString();
        }

        /// <summary>
        /// Returns an html encoded string to be used as the correct answer comment text, or String.Empty if none.
        /// </summary>
        private HtmlString CorrectAnswer(string ordinal)
        {
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinal)))
            {
                return new PlainTextString((string)Interaction.ExtensionData[InteractionExtensionDataKeys.AutogradeResponse(ordinal)]).ToHtmlString();
            }
            else
            {
                return new HtmlString(String.Empty);
            }
        }

        /// <summary>
        /// Returns true if the learner's answer was correct.
        /// </summary>
        private bool LearnerAnswerCorrect(string ordinal)
        {
            // the learner's answer was correct if there is no comment extension data
            return !Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinal));
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "T*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            HtmlString html = new HtmlString(String.Empty);
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return html;

            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    html = new HtmlString(String.Format(CultureInfo.InvariantCulture,
                        "<INPUT TYPE=\"text\" NAME=\"{0}\" SIZE=\"{1}\" VALUE=\"{2}\" />",
                        RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture)),
                        AssessmentItem.Cols,
                        LearnerAnswer(ordinal).ToHtmlString()
                        ));
                    break;
                case SessionView.RandomAccess:
                    // RandomAccess view is identical to Review view
                case SessionView.Review:
                    // Renders differently based on whether ShowCorrectAnswers is true or false, and whether or not
                    // the learner got the answer correct.
                    string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
                    StringBuilder bld = new StringBuilder(200);
                    bool correct = LearnerAnswerCorrect(ordinalString);
                    bld.AppendLine("<FONT FACE = \"Times\" SIZE=\"2\" COLOR=\"BLACK\"><B>");
                    if (correct)
                    {
                        bld.Append(LearnerAnswer(ordinal).ToHtmlString());
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(LearnerAnswer(ordinal)))
                        {
                            // no learner answer - output "<Unanswered>"
                            bld.Append(new PlainTextString(AIResources.Unanswered).ToHtmlString());
                        }
                        else
                        {
                            bld.Append("<STRIKE>");
                            bld.Append(LearnerAnswer(ordinal).ToHtmlString());
                            bld.Append("</STRIKE>");
                        }
                    }
                    bld.Append("</B></FONT>&nbsp;<IMG ALIGN=\"absmiddle\" BORDER=\"0\" SRC=\"");
                    if (correct)
                    {
                        bld.Append(ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, ImageFileName.Correct));
                    }
                    else
                    {
                        bld.Append(ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, ImageFileName.Incorrect));
                    }
                    bld.Append("\" ALT = \"");
                    if (correct)
                    {
                        bld.Append(AIResources.CorrectAnswerHtml);
                    }
                    else
                    {
                        bld.Append(AIResources.IncorrectAnswerHtml);
                    }
                    bld.Append("\" WIDTH=\"18\" HEIGHT=\"16\">");
                    // If the answer was incorrect and ShowCorrectAnswers is true, show the correct answers.
                    // RandomAccess view is identical to Review view.
                    if (!correct && RenderContext.ShowCorrectAnswers)
                    {
                        bld.Append("<FONT FACE=\"Arial,Helvetica,sans-serif\" SIZE=\"2\" COLOR=\"MAROON\"><I>");
                        bld.Append(AIResources.CorrectAnswerColonHtml);
                        bld.Append("&nbsp;");
                        bld.Append(CorrectAnswer(ordinalString));
                        bld.Append("</I></FONT>");
                    }
                    html = new HtmlString(bld.ToString());
                    break;
            }

            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this Interaction.
                    return false;
                }
            }
            else
            {
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // since the assessment item type is text, this is fill-in-the-blank type
            Interaction.InteractionType = InteractionType.FillIn;

            // add the akey to the correct response pattern.  akey will not be null for the text assessment item type, but it may be String.Empty.
            // Therefore, it is unneccessary to save extension data response option count since it is implicitly one plus the number of
            // "[,]" substrings in the non-null correct response pattern.
            if (String.IsNullOrEmpty(CorrectResponse.Pattern))
            {
                // only the first assessment item to add a pattern affects the "order_matters" attribute.
                // Notice the use of the "{{" and "}}" escape sequences in the format string.
                CorrectResponse.Pattern = String.Format(CultureInfo.InvariantCulture,
                    "{{case_matters=false}}{{order_matters={0}}}{1}", AssessmentItem.Uak == true ? "false" : "true",
                     LrmRloHandler.EncodePattern(AssessmentItem.AKey));
            }
            else
            {
                CorrectResponse.Pattern += "[,]" + LrmRloHandler.EncodePattern(AssessmentItem.AKey);
            }
            // note that Score.Maximum is set for this interaction type in the ItemScoreAssessmentRenderer.TryAddToDataModel()
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the text area assessment type.
    /// </summary>
    internal class TextAreaAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal TextAreaAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns the currently submitted answer, as stored, for the given ordinal.
        /// </summary>
        /// <remarks>
        /// For this text area type, note that occurances of the string "&lt;br&gt;" should be converted to "%0d%0a"
        /// for Execute view.
        /// </remarks>
        internal PlainTextString LearnerAnswer(int ordinal)
        {
            return new PlainTextString(FormDataProcessor.ExtractLearnerResponse(Interaction.LearnerResponse, ordinal));
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "TA*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            HtmlString html = new HtmlString(String.Empty);
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return html;

            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    html = new HtmlString(String.Format(CultureInfo.InvariantCulture,
                        "<TEXTAREA NAME=\"{0}\" ROWS=\"{1}\" COLS=\"{2}\">{3}</TEXTAREA>",
                        RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture)),
                        AssessmentItem.Rows,
                        AssessmentItem.Cols,
                        FormDataProcessor.ConvertDatabaseFormatToTextArea(new PlainTextString(LearnerAnswer(ordinal)).ToHtmlString())
                        ));
                    break;
                case SessionView.RandomAccess:
                    // RandomAccess view is the same as Review view, except that there is an intructor comments field in
                    // RandomAccess view.
                case SessionView.Review:
                    StringBuilder bld = new StringBuilder(200);
                    bld.Append("<FONT FACE=\"Times\" SIZE=\"2\" COLOR=\"BLACK\"><B>");
                    HtmlString learnerAnswer = FormDataProcessor.ConvertDatabaseFormatToHtml(LearnerAnswer(ordinal));
                    if (String.IsNullOrEmpty(learnerAnswer.ToString()))
                    {
                        bld.Append(new PlainTextString(AIResources.Unanswered).ToHtmlString());
                    }
                    else
                    {
                        bld.Append(learnerAnswer.ToString());
                    }
                    bld.AppendLine("</B></FONT>");

                    RenderInstructorComments(bld, ordinal);

                    html = new HtmlString(bld.ToString());
                    break;
            }

            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this Interaction.
                    return false;
                }
            }
            else
            {
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // since the assessment item type is text, this is an essay type
            Interaction.InteractionType = InteractionType.Essay;

            // this assessment item type needs to keep track of the number of text area fields
            int itemCount;
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                // increment the item count
                itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] + 1;
                Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] = itemCount;
            }
            else
            {
                itemCount = 1;
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.ResponseOptionCount, itemCount);
            }

            // note that Score.Maximum is set for this interaction type in the ItemScoreAssessmentRenderer.TryAddToDataModel()
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the radio button assessment type.
    /// </summary>
    internal class RadioAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal RadioAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns the Interaction.LearnerResponse, or String.Empty if none.  This value corresponds to the string
        /// ordinal of the chosen radio button.
        /// </summary>
        private string LearnerResponse
        {
            get
            {
                if (Interaction.LearnerResponse == null) return String.Empty;
                else return (string)Interaction.LearnerResponse;
            }
        }

        /// <summary>
        /// Returns true if the learner checked this ordinal's response.
        /// </summary>
        internal bool IsLearnerAnswer(int ordinal)
        {
            string ordinalAsString = ordinal.ToString(CultureInfo.InvariantCulture);
            bool value = false;
            // if the learner response is the same as the form field value, this is the selected radio button.
            // Note there is no need to call LrmRloHandler.DecodePattern() since a form field value won't have
            // the string "[,]" in it.
            if (0 == String.Compare(ordinalAsString, LearnerResponse, StringComparison.OrdinalIgnoreCase))
            {
                value = true;
            }
            return value;
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId)
        {
            return String.Format(CultureInfo.InvariantCulture, "R*{0}", interactionId);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the radio button.  E.g. the "index" of the radio
        /// button in a group of radio buttons, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a radio assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            HtmlString html = new HtmlString(String.Empty);
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return html;

            switch(RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    string check;
                    if (RenderContext.View == SessionView.Execute && IsLearnerAnswer(ordinal))
                    {
                        check = "CHECKED";
                    }
                    else
                    {
                        check = String.Empty;
                    }
                    html = new HtmlString(String.Format(CultureInfo.InvariantCulture,
                        "<INPUT TYPE=\"radio\" NAME=\"{0}\" VALUE=\"{1}\" {2} />",
                        RenderFormFieldName(Interaction.Id),
                        ordinal,
                        check
                        ));
                    break;
                case SessionView.RandomAccess:
                    // RandomAccess view is identical to Review view.
                case SessionView.Review:
                    float maxPoints = AssessmentItem.MaxPoints.GetValueOrDefault(0);
                    StringBuilder bld = new StringBuilder(200);
                    bld.Append("<IMG ALIGN=\"absmiddle\" BORDER=\"0\" SRC=\"");

                    string imageFileName;
                    string altText;
                    if (IsLearnerAnswer(ordinal))
                    {
                        // The learner selected this ordinal's radio button.  If the item's maxpoints is positive,
                        // it is considered a correct answer.
                        if (maxPoints > 0F)
                        {
                            // The learner answer was correct.
                            imageFileName = ImageFileName.ButtonOnCorrect;
                            altText = AIResources.StudentSelectedCorrectHtml;
                        }
                        else
                        {
                            // The learner answer was incorrect.
                            imageFileName = ImageFileName.ButtonOnWrong;
                            altText = AIResources.StudentSelectedIncorrectHtml;
                        }
                    }
                    else
                    {
                        // The learner did not select this ordinal's radio button.  If the item's maxpoints is positive,
                        // it is considered a correct answer.
                        if (maxPoints > 0F)
                        {
                            if (RenderContext.ShowCorrectAnswers)
                            {
                                // This ordinal represents a correct answer and ShowCorrectAnswers is true. Show
                                // a radio that indicates it was correct but unchecked.
                                imageFileName = ImageFileName.ButtonOffCorrect;
                                altText = AIResources.StudentNotSelectedIncorrectHtml;
                            }
                            else
                            {
                                // If this ordinal represents a correct answer and ShowCorrectAnswers is false.
                                // Show a normal unchecked radio.
                                imageFileName = ImageFileName.ButtonOff;
                                altText = AIResources.StudentNotSelectedHtml;
                            }
                        }
                        else
                        {
                            // This ordinal represents an incorrect or neutral answer that was not selected.  Show a normal
                            // unchecked radio.
                            imageFileName = ImageFileName.ButtonOff;
                            altText = AIResources.StudentNotSelectedHtml;
                        }
                    }
                    bld.Append(ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, imageFileName));
                    bld.Append("\" ALT=\"");
                    bld.Append(altText);

                    bld.Append("\" WIDTH=\"16\" HEIGHT=\"16\"");
                    html = new HtmlString(bld.ToString());
                    break;
            }

            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            Interaction interaction = Interaction;

            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this interaction.
                    return false;
                }
            }
            else
            {
                interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // since the assessment item type is radio, this a multiple-choice
            Interaction.InteractionType = InteractionType.MultipleChoice;

            // this assessment item type needs to keep track of the number of radio buttons for the correct response pattern
            int itemCount;
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                // increment the item count
                itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] + 1;
                Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] = itemCount;
            }
            else
            {
                itemCount = 1;
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.ResponseOptionCount, itemCount);
            }

            // add the assessment item's score to the extension data by ordinal
            Interaction.ExtensionData.Add(
                InteractionExtensionDataKeys.MaxPoints(String.Format(CultureInfo.CurrentCulture, "{0}", itemCount - 1)), (double)AssessmentItem.MaxPoints.Value);

            // for the radio type, the minimum score is the lesser of 0 or the lowest negative maxpts values.
            if (AssessmentItem.MaxPoints < Interaction.Score.Minimum)
            {
                Interaction.Score.Minimum = AssessmentItem.MaxPoints;
            }
            else if (AssessmentItem.MaxPoints > 0)
            {
                // A positive maxpts value makes this item considered "correct".
                string pattern;
                if (String.IsNullOrEmpty(CorrectResponse.Pattern))
                {
                    pattern = "{0}";
                }
                else
                {
                    pattern = "[,]{0}";
                }
                CorrectResponse.Pattern += String.Format(CultureInfo.InvariantCulture, pattern, itemCount - 1);
            }
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the checkbox assessment type.
    /// </summary>
    internal class CheckboxAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal CheckboxAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns true if this is the currently submitted answer.
        /// </summary>
        internal bool IsLearnerAnswer(string ordinal)
        {
            bool value = false;
            // if the learner response contains the form field, this is the selected radio button.
            // Note there is no need to call LrmRloHandler.DecodePattern() since a form field name won't have
            // the string "[,]" in it.  A direct comparison suffices.
            if (Interaction.LearnerResponse != null)
            {
                string response = (string)Interaction.LearnerResponse;
                if(response.Contains(RenderFormFieldName(Interaction.Id, ordinal)))
                {
                    value = true;
                }
            }
            return value;
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "C*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Template for review view html. {0} is the img src and {1} is the alt text.
        /// </summary>
        private static string reviewHtml =
            "<IMG ALIGN=\"absmiddle\" BORDER=\"0\" SRC=\"{0}\" ALT=\"{1}\" WIDTH=\"16\" HEIGHT=\"16\">";

        /// <summary>
        /// Convenience method to format the review HTML.
        /// </summary>
        private HtmlString GetReviewHtml(string imageFileName, string altText)
        {
            return new HtmlString(String.Format(CultureInfo.InvariantCulture,
                reviewHtml, 
                ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, imageFileName),
                altText));
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            HtmlString html = new HtmlString(String.Empty);
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return html;

            string stringOrdinal = ordinal.ToString(CultureInfo.InvariantCulture);
            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    string check;
                    if (RenderContext.View == SessionView.Execute && IsLearnerAnswer(stringOrdinal))
                    {
                        check = "CHECKED";
                    }
                    else
                    {
                        check = String.Empty;
                    }
                    html = new HtmlString(String.Format(CultureInfo.InvariantCulture,
                        "<INPUT TYPE=\"checkbox\" NAME=\"{0}\" {1} />",
                        RenderFormFieldName(Interaction.Id, stringOrdinal),
                        check
                        ));
                    break;
                case SessionView.RandomAccess:
                    // RandomAccess view is identical to Review view.
                case SessionView.Review:
                    float maxPoints = AssessmentItem.MaxPoints.GetValueOrDefault(0);
                    if (IsLearnerAnswer(stringOrdinal))
                    {
                        // The learner selected this ordinal's checkbox.  If the item's maxpoints is positive,
                        // it is considered a correct answer.
                        if (maxPoints > 0F)
                        {
                            // This ordinal represents a correct answer that was selected by the learner.
                            // Show a correctly checked checkbox.
                            html = GetReviewHtml(ImageFileName.BoxOnCorrect, AIResources.StudentSelectedCorrectHtml);
                        }
                        else
                        {
                            // This ordinal represents an incorrect answer that was selected by the learner.
                            html = GetReviewHtml(ImageFileName.BoxOnWrong, AIResources.StudentSelectedIncorrectHtml);
                        }
                    }
                    else
                    {
                        // The learner did not select this ordinal's checkbox.  If the item's maxpoints is positive,
                        // it is considered a correct answer.
                        if (maxPoints > 0F)
                        {
                            if (RenderContext.ShowCorrectAnswers)
                            {
                                // This ordinal represents a correct answer and ShowCorrectAnswers is true. Show
                                // a checkbox that indicates it was correct but unchecked.
                                html = GetReviewHtml(ImageFileName.BoxOffCorrect, AIResources.StudentNotSelectedIncorrectHtml);
                            }
                            else
                            {
                                // If this ordinal represents a correct answer and ShowCorrectAnswers is false.
                                // Show a normal unchecked checkbox.
                                html = GetReviewHtml(ImageFileName.BoxOff, AIResources.StudentNotSelectedHtml);
                            }
                        }
                        else
                        {
                            // This ordinal represents an incorrect or neutral answer that was not selected.  Show a normal
                            // unchecked checkbox.
                            html = GetReviewHtml(ImageFileName.BoxOff, AIResources.StudentNotSelectedHtml);
                        }
                    }
                    break;
            }

            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this Interaction.
                    return false;
                }
            }
            else
            {
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // since the assessment item type is checkbox, this is a multiple-choice.
            Interaction.InteractionType = InteractionType.MultipleChoice;

            // this assessment item type needs to keep track of the number of checkboxes
            int itemCount;
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                // increment the item count
                itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] + 1;
                Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] = itemCount;
            }
            else
            {
                itemCount = 1;
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.ResponseOptionCount, itemCount);
            }

            // add the assessment item's score to the extension data by ordinal
            Interaction.ExtensionData.Add(
                InteractionExtensionDataKeys.MaxPoints(String.Format(CultureInfo.CurrentCulture, "{0}", itemCount - 1)), (double)AssessmentItem.MaxPoints.Value);

            // for the checkbox type, the minimum score is the lesser of 0 or the sum of all negative maxpts values.
            if (AssessmentItem.MaxPoints < 0)
            {
                Interaction.Score.Minimum += AssessmentItem.MaxPoints;
            }
            else if (AssessmentItem.MaxPoints > 0)
            {
                // A positive maxpts value makes this item considered "correct".
                string pattern;
                if (String.IsNullOrEmpty(CorrectResponse.Pattern))
                {
                    pattern = "{0}";
                }
                else
                {
                    pattern = "[,]{0}";
                }
                // "itemCount - 1" corresponds to the Value (the ordinal) rendered for the checkbox.
                CorrectResponse.Pattern += String.Format(CultureInfo.InvariantCulture, pattern, itemCount - 1);
            }
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the item score assessment type.
    /// </summary>
    internal class ItemScoreAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal ItemScoreAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns the max score of the Interaction, or zero if none.
        /// </summary>
        private float MaxScore
        {
            get
            {
                return Interaction.Score.Maximum.GetValueOrDefault(0F);
            }
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "S*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.</remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            HtmlString html = new HtmlString(String.Empty);
            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    html = new HtmlString(String.Format(CultureInfo.CurrentUICulture,
                        "<FONT FACE=\"Arial,Helvetica,sans-serif\" SIZE=\"2\"><nobr><B>{0}</B></nobr></FONT>",
                        MaxScore));
                    break;
                case SessionView.RandomAccess:
                    html = new HtmlString(String.Format(CultureInfo.CurrentUICulture,
                        "<nobr><B><FONT COLOR=\"MAROON\">" + 
                        "<INPUT TYPE=\"text\" NAME=\"{0}\" id=\"{0}\" SIZE=\"5\" VALUE=\"{1}\" TITLE=\"\" STYLE=\"text-align:right\"" +
                        " maxlength=\"7\" autocomplete=\"off\" onblur=\"mlc_ValidateScore(this)\">" +
                        "</FONT><FONT COLOR=\"BLACK\"> / {2}</FONT></B></nobr>",
                        RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture)),
                        LearnerScore,
                        MaxScore));
                    break;
                case SessionView.Review:
                    html = new HtmlString(String.Format(CultureInfo.CurrentUICulture,
                        "<nobr><B><FONT COLOR=\"MAROON\">{0}</FONT><FONT COLOR=\"BLACK\"> / {1}</FONT></B></nobr>",
                        LearnerScore,
                        MaxScore));
                    break;
            }

            return html;
        }

        /// <summary>
        /// Gets the learner's score (as a string in the CurrentUICulture) from the evaluation, 
        /// or "--" (two dashes) or String.Empty (if the view is RandomAccess) if there is no score.
        /// </summary>
        private string LearnerScore
        {
            get
            {
                if (Interaction.Evaluation.Points.HasValue)
                {
                    return Convert.ToString(Interaction.Evaluation.Points.Value, CultureInfo.CurrentUICulture);
                }
                else
                {
                    AIResources.Culture = LocalizationManager.GetCurrentCulture();
                    if (RenderContext.View == SessionView.RandomAccess) return String.Empty;
                    else return AIResources.NoScoreHtml;
                }
            }
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // maxScore should be set when this score assessment item is parsed.
            Interaction.Score.Maximum = AssessmentItem.MaxPoints;

            // since the assessment item type is a score, it doesn't set Interaction.InteractionType nor increment the
            // item count.
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the select assessment type.
    /// </summary>
    internal class SelectAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal SelectAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Create an alphabetical list of values, discarding duplicates (case sensitive).
        /// </summary>
        internal static List<string> CreateAlphabeticalList(IDictionary<string, object> extensionData)
        {
            List<string> values = new List<string>(20);
            int index = 1;
            while (extensionData.ContainsKey(InteractionExtensionDataKeys.MatchingOptionValue(index)))
            {
                string value = extensionData[InteractionExtensionDataKeys.MatchingOptionValue(index)].ToString();
                if (!values.Contains(value))
                {
                    values.Add(value);
                }
                index++;
            }
            values.Sort();
            return values;
        }

        /// <summary>
        /// Create a list of options from ExtensionData in the same order they appear in the original file.
        /// </summary>
        internal static List<string> CreateOrdinalList(IDictionary<string, object> extensionData)
        {
            List<string> values = new List<string>(20);
            int index = 1;
            while (extensionData.ContainsKey(InteractionExtensionDataKeys.MatchingOptionValue(index)))
            {
                values.Add(extensionData[InteractionExtensionDataKeys.MatchingOptionValue(index)].ToString());
                index++;
            }
            return values;
        }

        /// <summary>
        /// Add &lt;option&gt; tags, sorted by alphabetical order of the option values.
        /// </summary>
        private void RenderOptions(StringBuilder bld, int ordinal)
        {
            List<string> options = CreateAlphabeticalList(Interaction.ExtensionData);
            // Output the options, and make sure the one that is currently selected is chosen
            int val = 1;
            int response = 0;
            string learnerResponse = FormDataProcessor.ExtractLearnerResponse(Interaction.LearnerResponse, ordinal);
            if (!String.IsNullOrEmpty(learnerResponse))
            {
                response = Int32.Parse(learnerResponse, CultureInfo.InvariantCulture);
            }
            foreach (string option in options)
            {
                string selected;
                if (val == response)
                {
                    selected = " SELECTED";
                }
                else
                {
                    selected = String.Empty;
                }
                bld.AppendLine(String.Format(CultureInfo.InvariantCulture, "<OPTION VALUE=\"{0}\"{1}>{2}</OPTION>",
                    val.ToString(CultureInfo.InvariantCulture),
                    selected,
                    new PlainTextString(option).ToHtmlString()));
                val++;
            }
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "SE*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Returns the currently submitted answer for the given ordinal.
        /// Also returns String.Empty if the learner didn't answer.
        /// </summary>
        internal string LearnerAnswer(int ordinal)
        {
            string learnerAnswer = String.Empty;
            string answerOrdinalString = FormDataProcessor.ExtractLearnerResponse(Interaction.LearnerResponse, ordinal);
            if (!String.IsNullOrEmpty(answerOrdinalString))
            {
                List<string> options = CreateAlphabeticalList(Interaction.ExtensionData);
                int answerOrdinal = Convert.ToInt32(answerOrdinalString, CultureInfo.InvariantCulture) - 1;
                if (answerOrdinal >= 0)
                {
                    learnerAnswer = options[answerOrdinal];
                }
            }
            return new PlainTextString(learnerAnswer);
        
        }

        /// <summary>
        /// Returns an html encoded string to be used as the correct answer comment text, or String.Empty if none.
        /// </summary>
        private HtmlString CorrectAnswer(string ordinal)
        {
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinal)))
            {
                return new PlainTextString((string)Interaction.ExtensionData[InteractionExtensionDataKeys.AutogradeResponse(ordinal)]).ToHtmlString();
            }
            else
            {
                return new HtmlString(String.Empty);
            }
        }

        /// <summary>
        /// Returns true if the learner's answer was correct.
        /// </summary>
        private bool LearnerAnswerCorrect(string ordinal)
        {
            // the learner's answer was correct if there is no comment extension data
            return !Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AutogradeResponse(ordinal));
        }

        /// <summary>
        /// HTML for review view. 
        /// {0} is the learner answer - for incorrect answers this should include the &lt;STRIKE&gt; tag.
        /// {1} is the image src.
        /// {2} is the alt text.
        /// </summary>
        private string m_reviewHtml = "<FONT FACE=\"Times\" SIZE=\"2\" COLOR=\"BLACK\"><B>{0}</B></FONT>&nbsp;" +
            "<IMG ALIGN=\"absmiddle\" BORDER=\"0\" SRC=\"{1}\" ALT=\"{2}\" WIDTH=\"18\" HEIGHT=\"16\">";

        /// <summary>
        /// HTML for the correct answer text to include after m_reviewHtml when ShowCorrectAnswers is true.
        /// {0} is the text "Correct answer:".
        /// {1} is the correct answer text.
        /// </summary>
        private string m_correctAnswerHtml =
            "<FONT FACE=\"Arial,Helvetica,sans-serif\" SIZE=\"2\" COLOR=\"MAROON\"><I>{0}&nbsp;{1}</I></FONT>";

        /// <summary>
        /// Returns the review HTML depending on the correctness of the learner response and whether
        /// ShowCorrectAnswers is true.
        /// </summary>
        private string GetReviewHtml(int ordinal)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();
            // RandomAccess view is identical to Review view.
            string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
            StringBuilder bld = new StringBuilder(200);
            string learnerAnswer = LearnerAnswer(ordinal);
            string learnerAnswerHtml;
            string imageSrc;
            string altText;

            if (LearnerAnswerCorrect(ordinalString))
            {
                learnerAnswerHtml = new PlainTextString(learnerAnswer).ToHtmlString();
                imageSrc = ImageFileName.Correct;
                altText = AIResources.CorrectAnswerHtml;
            }
            else
            {
                // if answer is incorrect, learner answer is surrounded by <STRIKE></STRIKE> unless unanswered.
                if (String.IsNullOrEmpty(learnerAnswer))
                {
                    learnerAnswerHtml = new PlainTextString(AIResources.Unanswered).ToHtmlString();
                }
                else
                {
                    learnerAnswerHtml = "<STRIKE>" + new PlainTextString(learnerAnswer).ToHtmlString() + "</STRIKE>";
                }
                imageSrc = ImageFileName.Incorrect;
                altText = AIResources.IncorrectAnswerHtml;
            }
            bld.AppendLine(String.Format(CultureInfo.InvariantCulture, m_reviewHtml,
                learnerAnswerHtml, ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, imageSrc), altText));
            if (!LearnerAnswerCorrect(ordinalString))
            {
                if (RenderContext.ShowCorrectAnswers)
                {
                    bld.AppendLine(String.Format(CultureInfo.InvariantCulture, m_correctAnswerHtml,
                        AIResources.CorrectAnswerColonHtml, CorrectAnswer(ordinalString)));
                }
            }
            return bld.ToString();
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return new HtmlString(String.Empty);

            string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
            StringBuilder bld = new StringBuilder(200);
            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    bld.AppendLine(String.Format(CultureInfo.InvariantCulture,
                        "<SELECT NAME=\"{0}\" SIZE=\"1\">",
                        RenderFormFieldName(Interaction.Id, ordinalString)
                        ));
                    bld.AppendLine("<OPTION VALUE=\"0\" />");    
                    RenderOptions(bld, ordinal);
                    bld.AppendLine("</SELECT>");
                    break;
                case SessionView.RandomAccess:
                    // identical to Review view
                case SessionView.Review:
                    bld.Append(GetReviewHtml(ordinal));
                    break;
            }

            return new HtmlString(bld.ToString());
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this Interaction.
                    return false;
                }
            }
            else
            {
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // this assessment item type needs to keep track of the number of OPTION matches.
            int itemCount;
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                // increment the item count
                itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] + 1;
                Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] = itemCount;
            }
            else
            {
                itemCount = 1;
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.ResponseOptionCount, itemCount);
            }

            // add the correct answer text to the extension data for this item
            Interaction.ExtensionData.Add(InteractionExtensionDataKeys.MatchingOptionValue(itemCount), AssessmentItem.Text);

            // since the assessment item type is select, this is a matching type
            Interaction.InteractionType = InteractionType.Matching;
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the rubric assessment type.
    /// </summary>
    internal class RubricAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal RubricAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// HTML to use in a String.Format when rendering.
        /// {0} is the img src.
        /// {1} is the img alt text.
        /// {2} is the point value.
        /// </summary>
        private static string htmlFormat =
            "<nobr><IMG ALIGN=\"top\" BORDER=\"0\" SRC=\"{0}\" ALT=\"{1}\" WIDTH=\"16\" HEIGHT=\"16\">" +
            "<FONT FACE=\"Arial,Helvetica,sans-serif\" SIZE=\"2\">{2}</FONT></nobr>";

        /// <summary>
        /// Returns a string value for max points that is in the current culture's ui.
        /// </summary>
        private string MaxPoints
        {
            get
            {
                return Convert.ToString(AssessmentItem.MaxPoints, CultureInfo.CurrentUICulture);
            }
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "R*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.</remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            AIResources.Culture = LocalizationManager.GetCurrentCulture();

            HtmlString html = new HtmlString(String.Empty);
            string imgSrc;
            string altText;
            SessionView view = RenderContext.View;
            
            // rubrics are only valid for Essay and Attachment interactions.  If the Interaction isn't one
            // of these types, show the Execute view version of the rubric no matter which view - e.g.
            // in RandomAccess view the rubric won't be a form field, it will only be an image of an
            // unchecked box.
            if (Interaction.InteractionType != InteractionType.Essay
                && Interaction.InteractionType != InteractionType.Attachment)
            {
                view = SessionView.Execute;
            }
            switch (view)
            {
                default:
                case SessionView.Execute:
                    // In Execute view, the rubrics are always unchecked box images.
                    imgSrc = ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, ImageFileName.BoxOff);
                    altText = String.Empty;
                    break;
                case SessionView.RandomAccess:
                    return new HtmlString(String.Format(CultureInfo.InvariantCulture,
                        "<nobr><INPUT TYPE=\"checkbox\" {3}NAME=\"{0}\" onclick=\"mlc_RubricClick(this, '{1}')\" VALUE=\"{2}\">" +
                        "<FONT FACE=\"Arial,Helvetica,sans-serif\" SIZE=\"2\">{2}</FONT></nobr>",
                        RenderFormFieldName(Interaction.Id, ordinal.ToString(CultureInfo.InvariantCulture)),
                        ItemScoreAssessmentRenderer.RenderFormFieldName(Interaction.Id, "0"), // only supports the first item score of the interaction
                        MaxPoints,
                        Interaction.Rubrics[ordinal].IsSatisfied.GetValueOrDefault(false) ? "CHECKED " : String.Empty
                        ));
                case SessionView.Review:
                    // In Review view, rubrics are either checked or unchecked box images depending on if the rubric is satisfied.
                    Rubric rubric = Interaction.Rubrics[ordinal];
                    if (rubric.IsSatisfied.GetValueOrDefault(false))
                    {
                        imgSrc = ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, ImageFileName.BoxOn);
                        altText = AIResources.StudentSelectedHtml;
                    }
                    else
                    {
                        imgSrc = ImageFileName.GetFullPath(RenderContext.EmbeddedUIResourcePath, ImageFileName.BoxOff);
                        altText = AIResources.StudentNotSelectedHtml;
                    }
                    break;
            }

            html = new HtmlString(String.Format(CultureInfo.InvariantCulture, htmlFormat,
                imgSrc,
                altText,
                MaxPoints));
            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // Add a new rubric to the Interaction.
            Rubric rubric = Interaction.DataModel.CreateRubric();
            // The Id is simply the ordinal of the rubric, which also happens to be the current rubric count.
            //rubric.Id = Convert.ToString(Interaction.Rubrics.Count, CultureInfo.InvariantCulture);
            rubric.Points = AssessmentItem.MaxPoints;
            Interaction.Rubrics.Add(rubric);
            return true;
        }
    }

    /// <summary>
    /// AssessmentItemRenderer for the file assessment type.
    /// </summary>
    internal class FileAssessmentRenderer : AssessmentItemRenderer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context"></param>
        internal FileAssessmentRenderer(RloDataModelContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Returns the currently uploaded file extension for outputting to HTML, or null if none.
        /// </summary>
        internal string CurrentFileExtension(string ordinal)
        {
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.FileExtension(ordinal)))
            {
                string str = (string)Interaction.ExtensionData[InteractionExtensionDataKeys.FileExtension(ordinal)];
                return str.ToUpperInvariant();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the current file path for outputting file attachment references in HTML.
        /// </summary>
        internal string CurrentFilePath(string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "./~RLO/{0}/{1}", Interaction.Id, ordinal);
        }

        /// <summary>
        /// Returns a name for use in the form field.
        /// </summary>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="ordinal">Ordinal.</param>
        /// <returns>Name</returns>
        internal static string RenderFormFieldName(string interactionId, string ordinal)
        {
            return String.Format(CultureInfo.InvariantCulture, "F*{0}*{1}", interactionId, ordinal);
        }

        /// <summary>
        /// Gets the HtmlString that replaces the assessment item in the content.
        /// </summary>
        /// <param name="ordinal">The ordinal of the item.  E.g. the "index" of the item
        /// in a group of items, beginning with 0 and incrementing from there.</param>
        /// <remarks>This method doesn't check for the validity of <paramref name="ordinal"/>.  E.g.
        /// one could pass a negative value and the method would work, but this would most likely
        /// produce undesireable results.
        /// <para>If this assessment doesn't match the type expected by the Interaction, 
        /// this method returns an empty string.</para>
        /// </remarks>
        /// <returns>HTML that should be inserted in place of a assessment item IMG tag.</returns>
        public override HtmlString Render(int ordinal)
        {
            HtmlString html = new HtmlString(String.Empty);
            // If this assessment doesn't match the expected type, return an empty string.
            if (!IsSameAsInteractionType) return html;

            string ordinalString = ordinal.ToString(CultureInfo.InvariantCulture);
            // Note that if currentFileExtension is null, there is no currently attached file.
            string currentFileExtension = CurrentFileExtension(ordinalString);
            switch (RenderContext.View)
            {
                default:
                case SessionView.Execute:
                    if (currentFileExtension == null)
                    {
                        html = new HtmlString(String.Format(CultureInfo.InvariantCulture,
                            "<BR>{0}<BR><INPUT TYPE=\"file\" ID=\"{1}\" NAME=\"{1}\" SIZE=\"30\" style=\"font-size:10pt\">",
                            AIResources.ChooseFileToAttachHtml,
                            RenderFormFieldName(Interaction.Id, ordinalString)));
                    }
                    else
                    {
                        StringBuilder bld = new StringBuilder(200);
                        bld.AppendFormat(CultureInfo.InvariantCulture, 
                            "<BR>{0}<BR>",
                            String.Format(CultureInfo.InvariantCulture, AIResources.AttachedFileHtml, currentFileExtension));
                        bld.AppendFormat(CultureInfo.InvariantCulture,
                            "<INPUT TYPE='button' style='font-size:10pt' name='View File' value='{0}' OnClick='window.open(\"{1}\",\"_blank\")'>&nbsp",
                            AIResources.ViewFileHtml,
                            CurrentFilePath(ordinalString));
                        bld.AppendFormat(CultureInfo.InvariantCulture,
                        "<INPUT TYPE='button' style='font-size:10pt' name='Detach File' value='{0}' OnClick='OnDetach(\"{1}\")'>",
                            AIResources.DetachFileHtml,
                            RenderFormFieldName(Interaction.Id, ordinalString));
                        bld.AppendFormat(CultureInfo.InvariantCulture,
                            "<BR>{0}<BR><INPUT TYPE=\"file\" ID=\"{1}\" NAME=\"{1}\" SIZE=\"30\" style=\"font-size:10pt\">",
                            AIResources.ReplaceFileHtml,
                            RenderFormFieldName(Interaction.Id, ordinalString));
                        html = new HtmlString(bld.ToString());
                    }
                    break;
                case SessionView.RandomAccess:
                    // RandomAccess view is the same as Review view, except that there is an intructor comments field in
                    // RandomAccess view.
                case SessionView.Review:
                    {
                        StringBuilder bld = new StringBuilder(200);
                        if (currentFileExtension == null)
                        {
                            // There is no file attached, so show the text "No file attached."
                            bld.AppendLine(String.Format(CultureInfo.InvariantCulture, "<BR>{0}", AIResources.NoFileAttachedHtml));
                        }
                        else
                        {
                            // Show a link to "View the attached file."
                            bld.AppendLine(String.Format(CultureInfo.InvariantCulture,
                                "<A HREF=\"{0}\" TARGET=\"_blank\">{1}</A>",
                                CurrentFilePath(ordinalString),
                                AIResources.ViewTheAttachedFileHtml));
                        }

                        RenderInstructorComments(bld, ordinal);

                        html = new HtmlString(bld.ToString());
                    }
                    break;
            }

            return html;
        }

        /// <summary>
        /// Adds the current Interaction to the data model.
        /// </summary>
        /// <remarks>AssessmentItem must be set before calling this method.</remarks>
        /// <returns>True if successfully added.  A false is returned if the interaction is not of the correct type.</returns>
        public override bool TryAddToDataModel()
        {
            // if the assessment item type has already been set on the interaction, only accept adding
            // assessment items of the same type.
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.AssessmentItemType))
            {
                if (AssessmentItem.Type != (AssessmentItemType)Interaction.ExtensionData[InteractionExtensionDataKeys.AssessmentItemType])
                {
                    // can't add the assessment item because the type doesn't match the first one added to this Interaction.
                    return false;
                }
            }
            else
            {
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.AssessmentItemType, (int)AssessmentItem.Type);
            }

            // this assessment item type needs to keep track of the number of response options
            int itemCount;
            if (Interaction.ExtensionData.ContainsKey(InteractionExtensionDataKeys.ResponseOptionCount))
            {
                // increment the item count
                itemCount = (int)Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] + 1;
                Interaction.ExtensionData[InteractionExtensionDataKeys.ResponseOptionCount] = itemCount;
            }
            else
            {
                itemCount = 1;
                Interaction.ExtensionData.Add(InteractionExtensionDataKeys.ResponseOptionCount, itemCount);
            }

            // since the assessment item type is File, this is an "Attachment" type
            Interaction.InteractionType = InteractionType.Attachment;
            // note that Score.Maximum is set for this interaction type in the ItemScoreAssessmentRenderer.TryAddToDataModel()
            return true;
        }
    }

}
