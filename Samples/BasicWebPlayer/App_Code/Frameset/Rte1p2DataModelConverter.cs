/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using Resources;
using Microsoft.LearningComponents.DataModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Converts data model elements from SCORM 1.2 content to LearningDataModel elements, and vice versa.
    /// </summary>
    internal class Rte1p2DataModelConverter : RteDataModelConverter
    {
        /// <summary>
        /// Constructor. Create a converter for SCORM 1.2 content.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="dataModel"></param>
        internal Rte1p2DataModelConverter(SessionView view, LearningDataModel dataModel)
            : base(view, dataModel)
        {

        }

        /// <summary>
        /// The entry point for SetValue functions. Pass in the name (in SCORM terms) of the data model element
        /// and this method sets the appropriate value in the LearningDataModel class.
        /// </summary>
        /// <param name="inName">SCORM data model element name (e.g., "cmi.exit"). </param>
        /// <param name="inValue">The value of the element in SCORM terms (e.g., "logout").</param>
        /// <remarks>Note: InteractionIndexer and ObjectiveIndexer must be set prior to calling this method.
        /// <para>It is not valid to call SetValue in Review view.</para>
        /// <p/>This method is relatively lax about throwing exceptions for invalid names. It assumes the caller 
        /// is passing in valid information.
        /// </remarks>
        public override void SetValue(PlainTextString inName, PlainTextString inValue)
        {
            // It's not valid to call in Review mode
            if (View == SessionView.Review)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_InvalidViewOnSetValue));

            CurrentElementName = inName.ToString();
            string[] nameParts = CurrentElementName.Split('.');

            string value = inValue.ToString();

            if (nameParts[0] == "cmi")
            {
                if (nameParts.Length < 2)
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                switch (nameParts[1])
                {
                    case "core":
                        {
                            SetCoreValues(nameParts, value);
                            break;
                        }

                    case "suspend_data":
                        {
                            DataModel.SuspendData = value;
                            break;
                        }
                    case "comments":
                        {
                            SetCommentsFromLearner(value);
                            break;
                        }
                    case "objectives":
                        {
                            VerifyElementNameTokens(4, nameParts);
                            SetObjectives(CurrentElementName.Substring(15) ,value);
                            break;
                        }
                    case "student_preference":
                        {
                            VerifyElementNameTokens(3, nameParts);
                            SetLearnerPreferences(nameParts[2], value);
                            break;
                        }
                    case "interactions":
                        {
                            SetInteractions(CurrentElementName.Substring(17), value);
                            break;
                        }

                    default:
                        // All other values are read-only (or invalid names). This will throw exception.
                        SetReadOnlyValue();
                        break;
                }

            }
            else
            {
                DataModel.ExtensionData[CurrentElementName] = value;
            }
        }

        // Set cmi.core.* values
        private void SetCoreValues(string[] nameParts, string value)
        {
            VerifyElementNameTokens(3, nameParts);

            switch (nameParts[2])
            {
                case "lesson_location":
                    DataModel.Location = value;
                    break;
                case "lesson_status":
                    SetLessonStatus(value);
                    break;
                case "exit":
                    SetExit(value);
                    break;
                case "session_time":
                    DataModel.SessionTime = TimeSpanFromRte(value);
                    break;
                case "score":
                    {
                        VerifyElementNameTokens(3, nameParts);
                        string scorePart = nameParts[3];

                        // If this is setting the raw score, then we need to also increment/decrement the values in 
                        // DataModel.EvaluationPoints by the change in the raw score
                        float? oldRawPoints = DataModel.Score.Raw;

                        // This sets the values in DataModel.Score.*
                        SetScore(scorePart, value);

                        // If this is setting the raw score, then we need to also increment/decrement the values in 
                        // DataModel.EvaluationPoints by the change in the raw score
                        if (scorePart == "raw")
                        {
                            // This will not fail, since it's already been parsed by SetScore(...).
                            float newRawPoints = float.Parse(value, NumberFormatInfo.InvariantInfo);
                            if (oldRawPoints == null)
                                DataModel.EvaluationPoints = newRawPoints;
                            else
                                DataModel.EvaluationPoints += (newRawPoints - oldRawPoints);
                        }
                    }
                    break;
                default:
                    // Any other valid value is read only.  This will throw exception.
                    SetReadOnlyValue();
                    break;
            }
        }

        /// <summary>
        /// Sets the LessonStatus value in the data model
        /// </summary>
        private void SetLessonStatus(string value)
        {
            switch (value)
            {
                case "passed":
                    DataModel.LessonStatus = LessonStatus.Passed;
                    break;
                case "completed":
                    DataModel.LessonStatus = LessonStatus.Completed;
                    break;
                case "failed":
                    DataModel.LessonStatus = LessonStatus.Failed;
                    break;
                case "incomplete":
                    DataModel.LessonStatus = LessonStatus.Incomplete;
                    break;
                case "browsed":
                    DataModel.LessonStatus = LessonStatus.Browsed;
                    break;
                case "not attempted":
                    DataModel.LessonStatus = LessonStatus.NotAttempted;
                    break;
                default: 
                    break;
            }
        }

        // Given an rte-formated timespan, return a TimeSpan object.
        protected override TimeSpan TimeSpanFromRte(string rteTimeSpan)
        {
            // Format is (HH)HH:MM:SS(.SS) where () represents optional elements
            
            MatchCollection matches = Regex.Matches(rteTimeSpan, @"^(\d{2,4}):(\d\d):(\d\d)(?:\.(\d{1,2}))?$");

            if (matches.Count != 1)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, rteTimeSpan, CurrentElementName));

            GroupCollection groups = matches[0].Groups;
            if (groups.Count != 5)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, rteTimeSpan, CurrentElementName));

            TimeSpan retVal;
            try
            {
                int hours = Int32.Parse(groups[1].Value, NumberFormatInfo.InvariantInfo);
                int mins = Int32.Parse(groups[2].Value, NumberFormatInfo.InvariantInfo);
                int secs = Int32.Parse(groups[3].Value, NumberFormatInfo.InvariantInfo);

                if (!String.IsNullOrEmpty(groups[4].Value))
                {
                    int ms = Int32.Parse(groups[4].Value, NumberFormatInfo.InvariantInfo);
                    retVal = new TimeSpan(0, hours, mins, secs, ms);
                }
                else
                {
                    retVal = new TimeSpan(hours, mins, secs);
                }

                return retVal; 
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, rteTimeSpan, CurrentElementName));
            }
        }

        // cmi.exit
        private void SetExit(string value)
        {
            ExitMode exitMode = ExitMode.Undetermined;
            switch (value)
            {
                case "time-out":
                    exitMode = ExitMode.TimeOut;
                    break;
                case "suspend":
                    exitMode = ExitMode.Suspended;
                    break;
                case "logout":
                    exitMode = ExitMode.Logout;
                    break;
                case "normal":
                    exitMode = ExitMode.Normal;
                    break;
                default:
                    break;
            }
            DataModel.NavigationRequest.ExitMode = exitMode;
        }

        
        private Dictionary<int, Objective> m_objectivesByIndex;

        /// <summary>
        /// Collection of interactions, including any pending objectives, keyed 
        /// by the 'n' in objectives.n.id.
        /// </summary>
        private IDictionary<int, Objective> ObjectivesByIndex
        {
            get
            {
                if (m_objectivesByIndex == null)
                {
                    m_objectivesByIndex = new Dictionary<int, Objective>(DataModel.Objectives.Count + 10);

                    int n = 0;
                    foreach (Objective objective in DataModel.Objectives)
                    {
                        m_objectivesByIndex.Add(n, objective);
                        n++;
                    }
                }

                return m_objectivesByIndex;
            }
        }

        /// <summary>
        /// SetValue("cmi.interactions.x.y.z")
        /// </summary>
        /// <param name="subElementName">The portion of the name following "cmi.interactions.". 
        /// In the example above, "x.y.z".</param>
        /// <param name="value">The value for the element</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // large switch statement
        private void SetInteractions(string subElementName, string value)
        {
            string[] elementParts = subElementName.Split('.');
            if (elementParts.Length < 2)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

            int index;
            if (!int.TryParse(elementParts[0], out index))
                return;

            Interaction interaction;
            if (index < InteractionsByIndex.Count)
            {
                // It's already in the list, so find the object
                interaction = InteractionsByIndex[index];
            }
            else
            {
                // It's a new Interaction. Add it to the list of pending Interactions and 
                // add it to the mapping table.
                interaction = DataModel.CreateInteraction();
                PendingInteractions.Add(interaction);
                InteractionsByIndex.Add(index, interaction);
            }

            switch (elementParts[1])
            {
                case "id":
                    {
                        if (String.CompareOrdinal(value, interaction.Id) != 0)
                            interaction.Id = value;
                    }
                    break;
                case "type":
                    {
                        interaction.InteractionType = GetInteractionType(value);
                    }
                    break;
                case "objectives":
                    {
                        // First find 'x' in the element name interactions.n.objectives.x.id.
                        if (elementParts.Length < 4)
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                        int objIndex;
                        if (!int.TryParse(elementParts[2], out objIndex))
                        {
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
                        }

                        InteractionObjective objective;
                        bool isNewObjective = false;
                        if (objIndex >= interaction.Objectives.Count)
                        {
                            objective = DataModel.CreateInteractionObjective();
                            isNewObjective = true;
                        }
                        else
                        {
                            objective = interaction.Objectives[objIndex];
                        }

                        if (String.CompareOrdinal(value, objective.Id) != 0)
                            objective.Id = value;

                        if (isNewObjective)
                        {
                            interaction.Objectives.Add(objective);
                        }
                    }
                    break;

                case "time":
                    {
                        interaction.Timestamp = value;
                    }
                    break;
                case "correct_responses":
                    {
                        // This is of the form: interactions.n.correct_responses.x.y

                        // First find 'x' in the element name interactions.n.correct_responses.x.y.
                        if (elementParts.Length < 4)
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                        int responseIndex;
                        if (!int.TryParse(elementParts[2], out responseIndex))
                        {
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
                        }

                        CorrectResponse response;
                        bool isNewResponse = false;
                        if (responseIndex >= interaction.CorrectResponses.Count)
                        {
                            // It's a new one
                            response = DataModel.CreateCorrectResponse();
                            isNewResponse = true;
                        }
                        else
                        {
                            // Note the assumption that the index is actually the index into the array.
                            // There's no other way to match them up (since there's no identifier).
                            response = interaction.CorrectResponses[responseIndex];
                        }

                        if (elementParts[3] != "pattern")
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                        response.Pattern = value;

                        if (isNewResponse)
                        {
                            interaction.CorrectResponses.Add(response);
                        }
                    }
                    break;
                case "weighting":
                    {
                        float weighting;
                        if (!float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out weighting))
                        {
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                        }
                        interaction.Weighting = weighting;
                    }
                    break;
                case "student_response":
                    {
                        // This must be set after InteractionType.

                        switch (interaction.InteractionType)
                        {
                            case InteractionType.TrueFalse:
                                {
                                    // Unfortunately, cannot use XmlConvert because it doesn't accept 't' and 'f'.
                                    if ((value == "t") || (value == "true") || (value == "1"))
                                        interaction.LearnerResponse = true;
                                    else if (((value == "f") || (value == "false") || (value == "0")))
                                        interaction.LearnerResponse = false;
                                    else
                                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                                }
                                break;
                            case InteractionType.Numeric:
                                {
                                    interaction.LearnerResponse = (float)XmlConvert.ToDouble(value);
                                }
                                break;
                            default:
                                interaction.LearnerResponse = value;
                                break;
                        }
                    }
                    break;
                case "result":
                    {
                        switch (value)
                        {
                            case "correct":
                                interaction.Result.State = InteractionResultState.Correct;
                                break;
                            case "wrong":
                                interaction.Result.State = InteractionResultState.Incorrect;
                                break;
                            case "neutral":
                                interaction.Result.State = InteractionResultState.Neutral;
                                break;
                            case "unanticipated":
                                interaction.Result.State = InteractionResultState.Unanticipated;
                                break;
                            default:
                                {
                                    // Should be a number. If it's not, then throw a basic error that says the 
                                    // value is not valid.
                                    float resultNumeric;
                                    try
                                    {
                                        resultNumeric = (float)XmlConvert.ToDouble(value);                                        
                                    }
                                    catch (FormatException)
                                    {
                                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                                    }
                                    interaction.Result.NumericResult = resultNumeric;
                                    interaction.Result.State = InteractionResultState.Numeric;
                                }
                                break;
                        }
                    }
                    break;
                case "latency":
                    {
                        interaction.Latency = TimeSpanFromRte(value);
                    }
                    break;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
            }

        }

        // Set the score field (eg, "raw") on a Score object.
        protected override void SetScoreSubField(string scoreField, string value, Score score)
        {

            float scoreValue;
            if (!float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out scoreValue))
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));

            switch (scoreField)
            {
                case "raw":
                    {
                        score.Raw = scoreValue;
                    }
                    break;
                case "min":
                    {
                        score.Minimum = scoreValue;
                    }
                    break;
                case "max":
                    {
                        score.Maximum = scoreValue;
                    }
                    break;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
            }
        }

        private void SetLearnerPreferences(string subElementName, string value)
        {
            switch (subElementName)
            {
                case "audio":
                    {
                        int level;
                        if (Int32.TryParse(value, out level))
                        {
                            DataModel.Learner.AudioLevel = (float)level;
                            return;
                        }
                        // couldn't parse it...
                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                    }
                case "language":
                    {
                        DataModel.Learner.Language = value;
                        return;
                    }
                case "speed":
                    {
                        int deliverySpeed;
                        if (Int32.TryParse(value, out deliverySpeed))
                        {
                            DataModel.Learner.DeliverySpeed = (float)deliverySpeed;
                            return;
                        }
                        // couldn't parse it
                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                    }
                case "text":
                    {
                        try
                        {
                            AudioCaptioning captioning;
                            captioning = (AudioCaptioning)Enum.Parse(typeof(AudioCaptioning), value);
                            DataModel.Learner.AudioCaptioning = captioning;
                            return;
                        }
                        catch
                        {
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                        }
                    }

            }

            // If we got here, the element name wasn't valid
            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
        }

        
        // In 1.2, there is just one string for comments
        private void SetCommentsFromLearner(string value)
        {
            // If there are no comments, then add a new one. Otherwise, update the existing one.
            if (DataModel.CommentsFromLearner.Count == 0)
            {
                Comment comment = DataModel.CreateComment();
                comment.CommentText = value;
                DataModel.CommentsFromLearner.Add(comment);
            }
            else
            {
                DataModel.CommentsFromLearner[0].CommentText = value;
            }
        }

        // Set cmi.objectives.n.<x> values. SubElementName is n.<x>
        private void SetObjectives(string subElementName, string value)
        {
            string[] elementParts = subElementName.Split('.');            

            if (elementParts.Length < 2)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
            
            int index;
            if (!int.TryParse(elementParts[0], out index))
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
            
            Objective objective;
            
            if (index < ObjectivesByIndex.Count)
            {
                // It's already in the list, so find the object
                objective = ObjectivesByIndex[index];
            }
            else
            {
                // It's a new objective. Add it to the list of pending objectives.
                objective = DataModel.CreateObjective();
                PendingObjectives.Add(objective);
                ObjectivesByIndex.Add(index, objective);
            }

            switch (elementParts[1])
            {
                case "id":
                    objective.Id = value;
                    break;
                case "score":
                    SetScoreSubField(elementParts[2], value, objective.Score);
                    break;
                case "status":
                    objective.Status = GetLessonStatus(value);
                    break;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

            }
        }

        // Translate the value (eg, "passed") into a LessonStatus value
        private LessonStatus GetLessonStatus(string value)
        {
            switch (value)
            {
                case "passed":
                    return LessonStatus.Passed;
                case "completed":
                    return LessonStatus.Completed;
                case "failed":
                    return LessonStatus.Failed;
                case "incomplete":
                    return LessonStatus.Incomplete;
                case "browsed":
                    return LessonStatus.Browsed;
                case "not attempted":
                    return LessonStatus.NotAttempted;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
            }
        }

        #region GetValue helper functions

        /// <summary>
        /// Helper function to get the cmi.interactions.n.result value
        /// </summary>
        public override string GetRteResult(InteractionResult result)
        {
            switch (result.State)
            {
                case InteractionResultState.Correct:
                    return "correct";
                case InteractionResultState.Incorrect:
                    return "wrong";
                case InteractionResultState.Neutral:
                    return "neutral";
                case InteractionResultState.Unanticipated:
                    return "unanticipated";
                case InteractionResultState.Numeric:
                    {
                        if (result.NumericResult != null) 
                            return RteFloatValue((float)result.NumericResult);
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Helper function to convert .NET TimeSpan to RTE string value;
        /// </summary>
        public static string GetRteTimeSpan(TimeSpan? ts)
        {
            if (ts == null)
                return "";

            TimeSpan ts2 = ts.Value;
            return GetRteTimeSpan(ts2.Hours, ts2.Minutes, ts2.Seconds, ts2.Milliseconds);
        }

        public override string GetRteTimeSpan(TimeSpan ts)
        {
            return GetRteTimeSpan(ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        private static string GetRteTimeSpan(int hours, int min, int sec, int ms)
        {
            return ResHelper.GetMessage("{0}:{1}:{2}.{3}", RteIntValue(hours, "D4"), RteIntValue(min, "D2"), RteIntValue(sec, "D2"), RteIntValue(ms, "D2"));
        }

        /// <summary>
        /// Helper function to get the comment text for the RTE.
        /// </summary>
        public static string GetRteComment(IList<Comment> comments)
        {
            // If there is a comment, return it. We only send one comment in 1.2.
            if ((comments.Count > 0) && (comments[0].CommentText != null))
                return comments[0].CommentText;

            return "";
        }

        /// <summary>
        /// Helper function to get the commentfromlms text for the RTE
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        public static string GetRteCommentFromLms(IList<CommentFromLms> comments)
        {
            // If there is a comment, return it. We only send one comment in 1.2.
            if ((comments.Count > 0) && (comments[0].CommentText != null))
                return comments[0].CommentText;

            return "";
        }

        /// <summary>
        /// Helper function to convert LessonStatus to the RTE string value
        /// </summary>
        public static string GetRteLessonStatus(LessonStatus status)
        {
            switch (status)
            {
                case LessonStatus.Browsed:
                    return "browsed";
                case LessonStatus.Completed:
                    return "completed";
                case LessonStatus.Failed:
                    return "failed";
                case LessonStatus.Incomplete:
                    return "incomplete";
                case LessonStatus.Passed:
                    return "passed";
                default: // case LessonStatus.NotAttempted:
                    return "not attempted";
            }
        }

        /// <summary>
        /// Helper function to convert float? to RTE form. Empty string is returned if 
        /// <paramref name="floatVal"/> is null.
        /// </summary>
        public static string GetRteFloat(float? value)
        {
            if (value == null)
                return "";

            return RteFloatValue(value.Value);
        }

        #endregion  // GetValue helper functions

        #region GetValues as RTE strings
        /// <summary>
        /// Return the encoded string of all current data model values to pass to the client. This method
        /// reinitializes the ObjectiveIndexer and InteractionIndexer values. 
        /// </summary>
        public override DataModelValues GetDataModelValues(AddDataModelValue addDataModelValue)
        {
            StringBuilder dataModelValuesBuffer = new StringBuilder(4096);  // data model values
            string n;

            LearningDataModel dm = this.DataModel;

            Learner learner = dm.Learner;
            addDataModelValue(dataModelValuesBuffer, "cmi.core.student_id", learner.Id);
            addDataModelValue(dataModelValuesBuffer, "cmi.core.student_name", learner.Name);

            // cmi.core values
            addDataModelValue(dataModelValuesBuffer, "cmi.core.lesson_location", dm.Location);
            addDataModelValue(dataModelValuesBuffer, "cmi.core.credit", GetRteCredit(dm.Credit, View));
            addDataModelValue(dataModelValuesBuffer, "cmi.core.lesson_status", GetRteLessonStatus(dm.LessonStatus));
            addDataModelValue(dataModelValuesBuffer, "cmi.core.entry", GetRteEntry(dm.Entry));

            addDataModelScore(addDataModelValue, dataModelValuesBuffer, "cmi.core.score", dm.Score);

            addDataModelValue(dataModelValuesBuffer, "cmi.core.total_time", GetRteTimeSpan(dm.TotalTime));
            addDataModelValue(dataModelValuesBuffer, "cmi.core.lesson_mode", GetRteMode(View));

            addDataModelValue(dataModelValuesBuffer, "cmi.suspend_data", dm.SuspendData);
            addDataModelValue(dataModelValuesBuffer, "cmi.launch_data", dm.LaunchData);
            addDataModelValue(dataModelValuesBuffer, "cmi.comments", GetRteComment(dm.CommentsFromLearner));
            addDataModelValue(dataModelValuesBuffer, "cmi.comments_from_lms", GetRteCommentFromLms(dm.CommentsFromLms));

            int objCountOrig = dm.Objectives.Count; // num objectives in data model
            int objCountToClient = 0;   // num objectives sent to client
            for (int i = 0; i < objCountOrig; i++)
            {
                Objective objective = dm.Objectives[i];

                n = XmlConvert.ToString(objCountToClient);
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.id", n), objective.Id);

                addDataModelScore(addDataModelValue, dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.score", n), objective.Score);
                
                if (objective.Status != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.status", n), GetRteLessonStatus(objective.Status.Value));

                objCountToClient++;
            }
            addDataModelValue(dataModelValuesBuffer, "cmi.objectives._count", RteIntValue(objCountToClient));

            // cmi.student_data
            addDataModelValue(dataModelValuesBuffer, "cmi.student_data.mastery_score", GetRteFloat(dm.MasteryScore));
            addDataModelValue(dataModelValuesBuffer, "cmi.student_data.max_time_allowed", GetRteTimeSpan(dm.MaxTimeAllowed));
            addDataModelValue(dataModelValuesBuffer, "cmi.student_data.time_limit_action", GetTimeLimitAction(dm.TimeLimitAction));

            // cmi.student_preference
            addDataModelValue(dataModelValuesBuffer, "cmi.student_preference.audio", GetRteFloat(learner.AudioLevel));
            addDataModelValue(dataModelValuesBuffer, "cmi.student_preference.language", learner.Language);
            addDataModelValue(dataModelValuesBuffer, "cmi.student_preference.speed", GetRteFloat(learner.DeliverySpeed));
            addDataModelValue(dataModelValuesBuffer, "cmi.student_preference.text", GetRteAudioCaptioning(learner.AudioCaptioning));

            // Interactions
            int numInterations = dm.Interactions.Count;
            addDataModelValue(dataModelValuesBuffer, "cmi.interactions._count", numInterations.ToString(NumberFormatInfo.InvariantInfo));

            int count = 0;
            foreach (Interaction interaction in dm.Interactions)
            {
                n = XmlConvert.ToString(count);
                
                // Not intuitive: interaction.n.id, time, weighting, student_response and type are write only in Scorm 1.2, so don't add them here.

                int numObjectives = interaction.Objectives.Count;
                for (int j = 0; j < numObjectives; j++)
                {
                    InteractionObjective obj = interaction.Objectives[j];
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.objectives.{1}.id", n, RteIntValue(j)), obj.Id);
                }
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.objectives._count", n), RteIntValue(numObjectives));
                
                int numResponses = interaction.CorrectResponses.Count;
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.correct_responses._count", n), RteIntValue(numResponses));
                for (int resI = 0; resI < numResponses; resI++)
                {
                    CorrectResponse response = interaction.CorrectResponses[resI];
                    if (response.Pattern != null)
                        addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.correct_responses.{1}.pattern", n, RteIntValue(resI)), response.Pattern);
                }

                count++;
            }   // end interactions

            return new DataModelValues(new PlainTextString(dataModelValuesBuffer.ToString()));
        }

        /// <summary>
        /// Adds the subfields of a score to the datamodel values to send to the client.
        /// </summary>
        private static void addDataModelScore(AddDataModelValue addDataModelValue, StringBuilder dataModelValuesBuffer, string cmiName, Score score)
        {
            string rteValue;
            float? tmpFloat;

            tmpFloat = score.Raw;
            rteValue = (tmpFloat == null) ? "" : RteDataModelConverter.RteFloatValue((float)tmpFloat);
            addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.raw", cmiName), rteValue);

            tmpFloat = score.Minimum;
            rteValue = (tmpFloat == null) ? "" : RteDataModelConverter.RteFloatValue((float)tmpFloat);
            addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.min", cmiName), rteValue);

            tmpFloat = score.Maximum;
            rteValue = (tmpFloat == null) ? "" : RteDataModelConverter.RteFloatValue((float)tmpFloat);
            addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.max", cmiName), rteValue);
        }
        #endregion // GetValues as RTE strings
    }
}