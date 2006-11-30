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
    /// Converts data model elements from 2004 content to LearningDataModel elements, and vice versa.
    /// </summary>
    internal class Rte2004DataModelConverter : RteDataModelConverter
    {
        Dictionary<int, Comment> m_pendingLearnerComments;
        
        /// <summary>
        /// Constructor. Create a converter for 2004 content.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="dataModel"></param>
        internal Rte2004DataModelConverter(SessionView view, LearningDataModel dataModel)
            :base(view, dataModel)
        {
            m_pendingLearnerComments = new Dictionary<int, Comment>();
        }

        // LearnerComments that have not yet been added to the data model.
        private Dictionary<int, Comment> PendingLearnerComments { get { return m_pendingLearnerComments; } }

        // Complete the process of setting values. Allows processing SetValue calls that required multiple, dependent 
        // values to be set (eg, comments)
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] 
        public override Collection<string> FinishSetValue()
        {
            Collection<string> errors;
            
            // The base class creates the list
            errors = base.FinishSetValue();

            foreach (KeyValuePair<int, Comment> kvPair in m_pendingLearnerComments)
            {
                try
                {
                    DataModel.CommentsFromLearner.Add(kvPair.Value);
                }
                catch (Exception e)
                {
                    errors.Add(ResHelper.GetMessage(FramesetResources.CONV_SetValueComment, e.Message));
                }
            }

            return errors;
        }

        /// <summary>
        /// The entry point for SetValue functions. Pass in the name (in SCORM terms) of the data model element
        /// and this method sets the appropriate value in the LearningDataModel class.
        /// </summary>
        /// <param name="inName">SCORM data model element name (e.g., "cmi.exit"). </param>
        /// <param name="inValue">The value of the element in SCORM terms (e.g., "logout").</param>
        /// <remarks>Note: ObjectiveIndexer must be set prior to calling this method.
        /// <para>It is not valid to call SetValue in Review view.</para></remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // large switch statement
        public override void SetValue(PlainTextString inName, PlainTextString inValue)
        {
            // It's not valid to call in Review mode
            if (View == SessionView.Review)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_InvalidViewOnSetValue));

            if (ObjectiveIndexer == null)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_MappingRequired));

            CurrentElementName = inName.ToString();
            string[] nameParts = CurrentElementName.Split('.');

            string value = inValue.ToString();

            if (nameParts[0] == "cmi")
            {
                if (nameParts.Length < 2)
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                switch (nameParts[1])
                {
                    case "comments_from_learner":
                        {
                            VerifyElementNameTokens(4, nameParts);
                            SetCommentsFromLearner(nameParts[2], nameParts[3], value);
                        }
                        break;
                    case "completion_status":
                        {
                            SetCompletionStatus(value);
                        }
                        break;
                    case "exit":
                        {
                            SetExit(value);
                        }
                        break;
                    case "interactions":
                        {
                            SetInteractions(CurrentElementName.Substring(17), value);
                        }
                        break;
                    case "learner_preference":
                        {
                            VerifyElementNameTokens(3, nameParts);
                            SetLearnerPreferences(nameParts[2], value);
                        }
                        break;
                    case "location":
                        {
                            DataModel.Location = value;
                        }
                        break;
                    case "objectives":
                        {
                            SetObjectives(CurrentElementName.Substring(15), value);
                        }
                        break;
                    case "progress_measure":
                        {
                            SetProgressMeasure(value);
                        }
                        break;
                    case "score":
                        {
                            VerifyElementNameTokens(3, nameParts);
                            SetScore(nameParts[2], value);
                        }
                        break;
                    case "session_time":
                        {
                            SetSessionTime(value);
                        }
                        break;
                    case "success_status":
                        {
                            SetSuccessStatus(value);
                        }
                        break;
                    case "suspend_data":
                        {
                            SetSuspendData(value);
                        }
                        break;

                    default:
                        // All other values are read-only. This will throw exception.
                        SetReadOnlyValue();
                        break;
                }

            }
            else if (nameParts[0] == "adl")
            {
                if (nameParts.Length < 3)
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

                if (nameParts[1] != "nav")
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));


                switch (nameParts[2])
                {
                    case "request":
                        {
                            SetNavRequest(value);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));
                }
            }
            else
            {
                DataModel.ExtensionData[CurrentElementName] = value;
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

        /// <summary>
        /// SetValue("cmi.interactions.x.y.z")
        /// </summary>
        /// <param name="subElementName">The portion of the name following "cmi.interactions.". 
        /// In the example above, "x.y.z".</param>
        /// <param name="value">The value for the element</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
                // It's a new Interaction. Add it to the list of pending Interactions.
                interaction = DataModel.CreateInteraction();
                PendingInteractions.Add(interaction);
                InteractionsByIndex.Add(index, interaction);
            }


            switch (elementParts[1])
            {
                case "id":
                    // Only set it if they are different, just to save some processing time.
                    if (String.CompareOrdinal(interaction.Id, value) != 0)
                        interaction.Id = value;
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
                            objective = interaction.Objectives[index];
                        }
                        objective.Id = value;

                        if (isNewObjective)
                        {
                            interaction.Objectives.Add(objective);
                        }
                    }
                    break;

                case "timestamp":
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
                case "learner_response":
                    {
                        // This must be set after InteractionType.

                        switch (interaction.InteractionType)
                        {
                            case InteractionType.TrueFalse:
                                {
                                    interaction.LearnerResponse = XmlConvert.ToBoolean(value);
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
                            case "incorrect":
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
                                    // Should be a number. 
                                    try
                                    {
                                        float resultNumeric = (float)XmlConvert.ToDouble(value);
                                        interaction.Result.NumericResult = resultNumeric;
                                        interaction.Result.State = InteractionResultState.Numeric;
                                    }
                                    catch (Exception)
                                    {
                                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                                    }
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
                case "description":
                    {
                        interaction.Description = value;
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
                case "scaled":
                    {
                        score.Scaled = scoreValue;
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
                case "audio_level":
                    {
                        float level;
                        if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out level))
                        {
                            DataModel.Learner.AudioLevel = level;
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
                case "delivery_speed":
                    {
                        float deliverySpeed;
                        if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out deliverySpeed))
                        {
                            DataModel.Learner.DeliverySpeed = deliverySpeed;
                            return;
                        }
                        // couldn't parse it
                        throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                    }
                case "audio_captioning":
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

        private void SetNavRequest(string value)
        {
            // The value here is either of the form "continue" or "{target=intro}choice". So, look for the 
            // actual tokens without delimiters, then check if "choice" is the final value.

            switch (value)
            {
                case "continue":
                    DataModel.NavigationRequest.Command = NavigationCommand.Continue;
                    break;
                case "previous":
                    DataModel.NavigationRequest.Command = NavigationCommand.Previous;
                    break;
                case "exit":
                    DataModel.NavigationRequest.Command = NavigationCommand.UnqualifiedExit;
                    break;
                case "exitAll":
                    DataModel.NavigationRequest.Command = NavigationCommand.ExitAll;
                    break;
                case "abandon":
                    DataModel.NavigationRequest.Command = NavigationCommand.Abandon;
                    break;
                case "abandonAll":
                    DataModel.NavigationRequest.Command = NavigationCommand.AbandonAll;
                    break;
                case "_none_":
                    DataModel.NavigationRequest.Command = null;
                    break;
                default:
                    {
                        // Check if it's a choice request:
                        Match match = Regex.Match(value, "^{target=(.*)}choice$");
                        if (match.Success)
                        {
                            // Get destination, which is the selected activity. In the example, "{target=intro}choice", the destination
                            // is "intro".
                            string destination = match.Groups[1].Captures[0].Value;

                            DataModel.NavigationRequest.Command = NavigationCommand.Choose;
                            DataModel.NavigationRequest.Destination = destination;
                        }
                        else
                            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
                    }
                    break;
            }
        }
        private void SetCommentsFromLearner(string n, string field, string value)
        {
            int index;  // the 'n' in "cmi.comments_from_learner.n.location"

            if (!int.TryParse(n, out index))
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

            Comment comment;

            if (index < DataModel.CommentsFromLearner.Count)
            {
                // It's referring to an existing comment in the data model
                comment = DataModel.CommentsFromLearner[index];
            }
            else
            {
                // It's not in the data model, so check if it's one that is already pending
                index = index - DataModel.CommentsFromLearner.Count;
                if (!PendingLearnerComments.TryGetValue(index, out comment))
                {
                    comment = DataModel.CreateComment();
                    PendingLearnerComments.Add(index, comment);
                }
            }

            switch (field)
            {
                case "comment":
                    {
                        comment.CommentText = value;
                    }
                    break;
                case "location":
                    {
                        comment.Location = value;
                    }
                    break;
                case "timestamp":
                    {
                        comment.Timestamp = value;
                    }
                    break;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

            }
        }

        // Set cmi.objectives.n.<x> values
        private void SetObjectives(string subElementName, string value)
        {
            string[] elementParts = subElementName.Split('.');
            if (elementParts.Length < 2)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));


            int index;
            if (!int.TryParse(elementParts[0], out index))
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));


            Objective objective;
            if (index < ObjectiveIndexer.Count)
            {
                // It's already in the list, so find the object
                string objectiveId = ObjectiveIndexer[index];
                objective = ObjectiveIdMap[objectiveId];
            }
            else
            {
                // It's a new objective. Add it to the list of pending objectives. When the id is set (which it 
                // should be on this SetValue call), add it to the mapping tables.
                objective = DataModel.CreateObjective();
                PendingObjectives.Add(objective);
            }

            switch (elementParts[1])
            {
                case "id":
                    if (String.IsNullOrEmpty(objective.Id))
                    {
                        // This is the first time it's being set
                        ObjectiveIndexer.Insert(index, value);
                        ObjectiveIdMap.Add(value, objective);
                    }
                    objective.Id = value;
                    break;
                case "score":
                    SetScoreSubField(elementParts[2], value, objective.Score);
                    break;
                case "success_status":
                    objective.SuccessStatus = GetSuccessStatus(value);
                    break;
                case "completion_status":
                    objective.CompletionStatus = GetCompletionStatus(value);
                    break;
                case "progress_measure":
                    objective.ProgressMeasure = GetProgressMeasure(value);
                    break;
                case "description":
                    objective.Description = value;
                    break;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, CurrentElementName));

            }
        }

        private float? GetProgressMeasure(string value)
        {
            float pm;
            if (!float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out pm))
            {
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));

            }
            return pm;
        }

        private void SetProgressMeasure(string value)
        {
            DataModel.ProgressMeasure = GetProgressMeasure(value);
        }

        private void SetSuccessStatus(string value)
        {
            DataModel.SuccessStatus = GetSuccessStatus(value);
        }
        // Translate the value (eg, "passed") into a SuccessStatus value
        private SuccessStatus GetSuccessStatus(string value)
        {
            switch (value)
            {
                case "passed":
                    return SuccessStatus.Passed;
                case "failed":
                    return SuccessStatus.Failed;
                case "unknown":
                    return SuccessStatus.Unknown;

                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
            }
        }

        // Translate the value (eg, "completed") into a CompletionStatus value
        private CompletionStatus GetCompletionStatus(string value)
        {
            switch (value)
            {
                case "completed":
                    return CompletionStatus.Completed;
                case "incomplete":
                    return CompletionStatus.Incomplete;
                case "not attempted":
                    return CompletionStatus.NotAttempted;
                case "unknown":
                    return CompletionStatus.Unknown;
                default:
                    throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidValue, value, CurrentElementName));
            }
        }

        private void SetCompletionStatus(string value)
        {
            DataModel.CompletionStatus = GetCompletionStatus(value);
        }
        
        // Given an rte-formated timespan, return a TimeSpan object.
        protected override TimeSpan TimeSpanFromRte(string rteTimeSpan)
        {
            return XmlConvert.ToTimeSpan(rteTimeSpan);
        }

        #region GetValue helper functions

        /// <summary>
        /// Gets the RTE string that represents the completion status.
        /// </summary>
        public static string GetRteCompletionStatus(CompletionStatus status)
        {
            switch (status)
            {
                case CompletionStatus.Completed:
                    return "completed";
                case CompletionStatus.Incomplete:
                    return "incomplete";
                case CompletionStatus.NotAttempted:
                    return "not attempted";
                default:
                    return "unknown";
            }
        }

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
                    return "incorrect";
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
        /// Gets the RTE string that represents the success status.
        /// </summary>
        public static string GetRteSuccessStatus(SuccessStatus status)
        {
            switch (status)
            {
                case SuccessStatus.Failed:
                    return "failed";
                case SuccessStatus.Passed:
                    return "passed";
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Helper function to convert .NET TimeSpan to RTE string value;
        /// </summary>
        public override string GetRteTimeSpan(TimeSpan ts)
        {
            return TimeSpanToStringScormV1p3(ts);
        }

        /// <summary>
        /// Converts a TimeSpan into a SCORM 2004 Timespan string. 
        /// *** WARNING: This method exists in the SLS unit tests!! If you change this you must also change the 
        /// real code!!!*************
        /// </summary>
        /// <remarks>
        /// Returns a length of time in hours, minutes and seconds shown in the following 
        /// format: P[yY][mM][dD][T[hH][mM][s[.s]S]] with a precision of 0.01 seconds.
        /// 
        /// <para>
        /// XmlConvert.ToTimeSpan() will not work for converting SCORM 2004 values (which are also ISO 
        /// 8601 values) since XmlConvert uses an incorrect value for days per year and days per month.
        /// XmlConvert.ToString(TimeSpan) will produce a correct equivalent, however.</para>
        /// <para>
        /// 1 year ~ (365*4+1)/4*60*60*24*100 = 3155760000 centiseconds
        /// 1 month ~ (365*4+1)/48*60*60*24*100 = 262980000 centiseconds
        /// 1 day = 8640000 centiseconds
        /// 1 hour = 360000 centiseconds
        /// 1 minute = 6000 centiseconds
        /// </para>
        /// </remarks>
        private static string TimeSpanToStringScormV1p3(TimeSpan value)
        {
            bool hasTime = false;
            double num;
            double remainingCentiseconds = 0;

            // Oddly valid
            if (value.CompareTo(TimeSpan.Zero) == 0)
                return "PT0H0M0S";

            StringBuilder retTimeSpan = new StringBuilder(50);
            retTimeSpan.Append("P");

            remainingCentiseconds = value.Ticks / 100000;

            // If there is at least enough time for 1 year...
            if (remainingCentiseconds >= 3155760000)
            {
                num = (int)(remainingCentiseconds / 3155760000);
                retTimeSpan.AppendFormat("{0}Y", num.ToString(NumberFormatInfo.InvariantInfo));
                remainingCentiseconds -= (num * 3155760000);
            }

            // If there is at least enough time for 1 month...
            if (remainingCentiseconds >= 262980000)
            {
                num = (int)(remainingCentiseconds / 262980000);
                retTimeSpan.AppendFormat("{0}M", num.ToString(NumberFormatInfo.InvariantInfo));
                remainingCentiseconds = remainingCentiseconds - (num * 262980000);
            }

            // If there is at least enough time for 1 day...
            if (remainingCentiseconds >= 8640000)
            {
                num = (int)(remainingCentiseconds / 8640000);
                retTimeSpan.AppendFormat("{0}D", num.ToString(NumberFormatInfo.InvariantInfo));
                remainingCentiseconds -= num * 8640000;
            }

            // If there is at least enough time for 1 hour...
            if (remainingCentiseconds >= 360000)
            {
                hasTime = true;
                num = (int)(remainingCentiseconds / 360000);
                retTimeSpan.AppendFormat("T{0}H", num.ToString(NumberFormatInfo.InvariantInfo));
                remainingCentiseconds -= num * 360000;
            }

            // If there is at least enough time for 1 minute
            if (remainingCentiseconds >= 6000)
            {
                if (!hasTime)
                {
                    retTimeSpan.Append("T");
                    hasTime = true;
                }
                num = (int)(remainingCentiseconds / 6000);
                retTimeSpan.AppendFormat("{0}M", num.ToString(NumberFormatInfo.InvariantInfo));
                remainingCentiseconds -= num * 6000;
            }

            // If there is at least enough time for .01 second
            if (remainingCentiseconds > 0)
            {
                if (!hasTime)
                {
                    retTimeSpan.Append("T");
                    hasTime = true;
                }
                num = remainingCentiseconds / 100;
                retTimeSpan.AppendFormat("{0}S", num.ToString("#.##", NumberFormatInfo.InvariantInfo));
            }

            return retTimeSpan.ToString();
        }

        #endregion  // GetValue helper functions

        #region GetValues as RTE strings
        /// <summary>
        /// Return the encoded string of all current data model values to pass to the client. This method
        /// reinitializes the ObjectiveIndexer value. 
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override DataModelValues GetDataModelValues(AddDataModelValue addDataModelValue)
        {
            StringBuilder dataModelValuesBuffer = new StringBuilder(4096);  // data model values
            StringBuilder objectiveIdMapBuffer = new StringBuilder(1000);
            string dmValue;
            float? tmpFloat;
            string n;
            
            LearningDataModel dm = this.DataModel;

            int numComments = dm.CommentsFromLearner.Count;
            addDataModelValue(dataModelValuesBuffer, "cmi.comments_from_learner._count", numComments.ToString(NumberFormatInfo.InvariantInfo));
            for (int i = 0; i < numComments; i++)
            {
                Comment comment = dm.CommentsFromLearner[i];
                n = XmlConvert.ToString(i);

                if (comment.CommentText != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_learner.{0}.comment", n), comment.CommentText);

                if (comment.Location != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_learner.{0}.location", n), comment.Location);

                if (comment.Timestamp != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_learner.{0}.timestamp", n), comment.Timestamp);
            }
            numComments = dm.CommentsFromLms.Count;
            addDataModelValue(dataModelValuesBuffer, "cmi.comments_from_lms._count", numComments.ToString(NumberFormatInfo.InvariantInfo));
            for (int i = 0; i < numComments; i++)
            {
                CommentFromLms comment = dm.CommentsFromLms[i];
                n = XmlConvert.ToString(i);
                if (comment.CommentText != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_lms.{0}.comment", n), comment.CommentText);

                if (comment.Location != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_lms.{0}.location", n), comment.Location);

                if (comment.Timestamp != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.comments_from_lms.{0}.timestamp", n), comment.Timestamp);
            }
            addDataModelValue(dataModelValuesBuffer, "cmi.completion_status", GetRteCompletionStatus(dm.CompletionStatus));
            if (dm.CompletionThreshold != null)
            {
                addDataModelValue(dataModelValuesBuffer, "cmi.completion_threshold", RteDataModelConverter.RteFloatValue((float)dm.CompletionThreshold));
            }
            addDataModelValue(dataModelValuesBuffer, "cmi.credit", RteDataModelConverter.GetRteCredit(dm.Credit, View));
            addDataModelValue(dataModelValuesBuffer, "cmi.entry", RteDataModelConverter.GetRteEntry(dm.Entry));

            if (dm.ScaledPassingScore != null)
            {
                addDataModelValue(dataModelValuesBuffer, "cmi.scaled_passing_score", RteDataModelConverter.RteFloatValue((float)dm.ScaledPassingScore));
            }

            int numInterations = dm.Interactions.Count;
            addDataModelValue(dataModelValuesBuffer, "cmi.interactions._count", numInterations.ToString(NumberFormatInfo.InvariantInfo));

            int count = 0;
            foreach (Interaction interaction in dm.Interactions)
            {
                n = XmlConvert.ToString(count);

                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.id", n), interaction.Id);

                if (interaction.InteractionType != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.type", n), RteDataModelConverter.GetRteInteractionType(interaction.InteractionType));

                if (interaction.Timestamp != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.timestamp", n), interaction.Timestamp);

                int numObjectivesOrig = interaction.Objectives.Count;
                int numObjectivesToClient = 0;
                for (int j = 0; j < numObjectivesOrig; j++)
                {
                    InteractionObjective obj = interaction.Objectives[j];

                    // If there is no objective id, skip it
                    if (String.IsNullOrEmpty(obj.Id))
                        continue;

                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.objectives.{1}.id", n, XmlConvert.ToString(numObjectivesToClient)), obj.Id);

                    numObjectivesToClient++;
                }
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.objectives._count", n), numObjectivesToClient.ToString(NumberFormatInfo.InvariantInfo));


                int numResponses = interaction.CorrectResponses.Count;
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.correct_responses._count", n), numResponses.ToString(NumberFormatInfo.InvariantInfo));
                for (int resI = 0; resI < numResponses; resI++)
                {
                    CorrectResponse response = interaction.CorrectResponses[resI];
                    if (response.Pattern != null)
                        addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.correct_responses.{1}.pattern", n, XmlConvert.ToString(resI)), response.Pattern);
                }

                if (interaction.Weighting != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.weighting", n), RteDataModelConverter.RteFloatValue((float)interaction.Weighting));

                if (interaction.LearnerResponse != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.learner_response", n), RteDataModelConverter.GetRteLearnerResponse(interaction.LearnerResponse));

                dmValue = GetRteResult(interaction.Result);
                if (dmValue != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.result", n), dmValue);

                if (interaction.Latency != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.latency", n), GetRteTimeSpan(interaction.Latency));

                if (interaction.Description != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.interactions.{0}.description", n), interaction.Description);

                count++;
            }   // end interactions

            if (dm.LaunchData != null)
                addDataModelValue(dataModelValuesBuffer, "cmi.launch_data", dm.LaunchData);

            Learner learner = dm.Learner;
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_id", learner.Id);
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_name", learner.Name);
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_preference.audio_level", learner.AudioLevel.ToString(DateTimeFormatInfo.InvariantInfo));
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_preference.delivery_speed", learner.DeliverySpeed.ToString(NumberFormatInfo.InvariantInfo));
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_preference.language", learner.Language);
            addDataModelValue(dataModelValuesBuffer, "cmi.learner_preference.audio_captioning", RteDataModelConverter.GetRteAudioCaptioning(learner.AudioCaptioning));

            dmValue = dm.Location;
            if (!String.IsNullOrEmpty(dmValue))
                addDataModelValue(dataModelValuesBuffer, "cmi.location", dmValue);

            if (dm.MaxTimeAllowed != null)
                addDataModelValue(dataModelValuesBuffer, "cmi.max_time_allowed", GetRteTimeSpan((TimeSpan)dm.MaxTimeAllowed));

            addDataModelValue(dataModelValuesBuffer, "cmi.mode", RteDataModelConverter.GetRteMode(View));
            tmpFloat = dm.ProgressMeasure;
            if (tmpFloat != null)
                addDataModelValue(dataModelValuesBuffer, "cmi.progress_measure", RteDataModelConverter.RteFloatValue((float)tmpFloat));

            int objCountOrig = dm.Objectives.Count; // num objectives in data model
            int objCountToClient = 0;   // num objectives sent to client
            for (int i = 0; i < objCountOrig; i++)
            {
                Objective objective = dm.Objectives[i];

                // If the objective does not have an id, don't send it to client.
                if (String.IsNullOrEmpty(objective.Id))
                    continue;

                n = XmlConvert.ToString(objCountToClient);
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.id", n), objective.Id);
                addDataModelValue(objectiveIdMapBuffer, n, objective.Id);

                addDataModelScore(addDataModelValue, dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.score", n), objective.Score);
                if (objective.Description != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.description", n), objective.Description);

                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.completion_status", n), GetRteCompletionStatus(objective.CompletionStatus));
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.success_status", n), GetRteSuccessStatus(objective.SuccessStatus));
                tmpFloat = objective.ProgressMeasure;
                if (tmpFloat != null)
                    addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("cmi.objectives.{0}.progress_measure", n), RteDataModelConverter.RteFloatValue((float)tmpFloat));

                objCountToClient++;

            }
            addDataModelValue(dataModelValuesBuffer, "cmi.objectives._count", objCountToClient.ToString(NumberFormatInfo.InvariantInfo));

            // Add all the cmi.score values
            addDataModelScore(addDataModelValue, dataModelValuesBuffer, "cmi.score", dm.Score);

            if (dm.SuspendData != null)
                addDataModelValue(dataModelValuesBuffer, "cmi.suspend_data", dm.SuspendData);

            addDataModelValue(dataModelValuesBuffer, "cmi.success_status", GetRteSuccessStatus(dm.SuccessStatus));

            addDataModelValue(dataModelValuesBuffer, "cmi.time_limit_action", GetTimeLimitAction(dm.TimeLimitAction));

            addDataModelValue(dataModelValuesBuffer, "cmi.total_time", GetRteTimeSpan(dm.TotalTime));

            foreach (KeyValuePair<string, object> kvPair in dm.ExtensionData)
            {
                string name = kvPair.Key;
                string value;
                object keyValue = kvPair.Value;
                Type type = keyValue.GetType();
                if (type == typeof(double))
                {
                    double dValue = (double)keyValue;
                    value = dValue.ToString(NumberFormatInfo.InvariantInfo);
                }
                else if (type == typeof(DateTime))
                {
                    DateTime vDate = (DateTime)keyValue;
                    value = RteDataModelConverter.RteDateTimeValue(vDate);
                }
                else
                {
                    value = keyValue.ToString();
                }
                addDataModelValue(dataModelValuesBuffer, name, value);
            }

            return new DataModelValues(new PlainTextString(dataModelValuesBuffer.ToString()),
                                        new PlainTextString(objectiveIdMapBuffer.ToString()));
        }

        private static void addDataModelScore(AddDataModelValue addDataModelValue, StringBuilder dataModelValuesBuffer, string cmiName, Score score)
        {
            float? tmpFloat = score.Raw;
            if (tmpFloat != null)
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.raw", cmiName), RteDataModelConverter.RteFloatValue((float)tmpFloat));

            tmpFloat = score.Scaled;
            if (tmpFloat != null)
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.scaled", cmiName), RteDataModelConverter.RteFloatValue((float)tmpFloat));

            tmpFloat = score.Minimum;
            if (tmpFloat != null)
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.min", cmiName), RteDataModelConverter.RteFloatValue((float)tmpFloat));

            tmpFloat = score.Maximum;
            if (tmpFloat != null)
                addDataModelValue(dataModelValuesBuffer, ResHelper.FormatInvariant("{0}.max", cmiName), RteDataModelConverter.RteFloatValue((float)tmpFloat));

        }
        #endregion // GetValues as RTE strings
    }
}