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
using Microsoft.LearningComponents.DataModel;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Resources;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;    

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Abstract class to translate between RTE strings (e.g., "cmi.score.scaled") and 
    /// LearningDataModel values (e.g., LearningDataModel.Score.Scaled). One instance of this class 
    /// should be created for each activity required.
    /// </summary>
    /// <remarks>Derived classes support the version-specific features of SCORM 2004 and 1.2.</remarks>
    internal abstract class RteDataModelConverter
    {
        LearningDataModel m_dm;
        SessionView m_view;

        // Information required to do SetValue calls
        private delegate void DataModelSetDelegate(string subElementName, string value);
        
        // Changes to m_dm that are pending in this session. They are added to data model when FinishSetValue
        // is called.
        List<Objective> m_pendingObjectives;
        List<Interaction> m_pendingInteractions;

        string m_currentElementName;  // the name of the element currently being processed. This is used only for error reporting.

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        public static RteDataModelConverter Create(LearningSession session)
        {
            FramesetUtil.ValidateNonNullParameter("session", session);
            switch (session.PackageFormat)
            {
                case PackageFormat.V1p2:
                case PackageFormat.Lrm:
                    return new Rte1p2DataModelConverter(session.View, session.CurrentActivityDataModel);
                case PackageFormat.V1p3:                    
                    return new Rte2004DataModelConverter(session.View, session.CurrentActivityDataModel);
            }            
            return null;
        }

        /// <summary>
        /// Constructor. Create a converter based on a data model.
        /// </summary>
        protected RteDataModelConverter(SessionView view, LearningDataModel dataModel) 
        {
            m_dm = dataModel;
            m_view = view;
            m_pendingInteractions = new List<Interaction>();
            m_pendingObjectives = new List<Objective>();
        }      

        public LearningDataModel DataModel { get { return m_dm; } }

        protected SessionView View { get { return m_view; } }

        /// <summary>
        /// The current element being processed. Used only for error messages.
        /// </summary>
        protected string CurrentElementName 
        { 
            get { return m_currentElementName; }
            set { m_currentElementName = value; }
        }

        List<string> m_objectiveIndexer = new List<string>();  // index is "n" and value is id

        Dictionary<string, Objective> m_objectiveIdMap; // key = id, value is Objective

        // List in which the index is the 'n' in cmi.objectives.n.success_status. The value is the id of the
        // objective -- that is the 'id' in cmi.objectives.n.id.
        public List<string> ObjectiveIndexer
        {
            get { return m_objectiveIndexer; }
        }

        // Return dictionary of mapping between objective id and objective object
        protected Dictionary<string, Objective> ObjectiveIdMap
        {
            get
            {
                if (m_objectiveIdMap == null)
                {
                    m_objectiveIdMap = new Dictionary<string, Objective>(m_dm.Objectives.Count);
                    foreach (Objective o in m_dm.Objectives)
                    {
                        m_objectiveIdMap.Add(o.Id, o);
                    }
                }
                return m_objectiveIdMap;
            }
        }

        private Dictionary<int, Interaction> m_interactionsByIndex;

        /// <summary>
        /// Collection of interactions, including any pending interactions, keyed 
        /// by the 'n' in interactions.n.id.
        /// </summary>
        protected IDictionary<int, Interaction> InteractionsByIndex
        {
            get
            {
                if (m_interactionsByIndex == null)
                {
                    m_interactionsByIndex = new Dictionary<int, Interaction>(DataModel.Interactions.Count + 10);
                    int n = 0;
                    foreach (Interaction interaction in DataModel.Interactions)
                    {
                        m_interactionsByIndex.Add(n, interaction);
                        n++;
                    }
                }

                return m_interactionsByIndex;
            }
        }

        protected List<Interaction> PendingInteractions { get { return m_pendingInteractions; } }
        protected List<Objective> PendingObjectives { get { return m_pendingObjectives; } }

        #region SetValue delegates        

        /// <summary>
        /// The entry point for SetValue functions. Pass in the name (in SCORM terms) of the data model element
        /// and this method sets the appropriate value in the LearningDataModel class.
        /// </summary>
        /// <param name="inName">SCORM data model element name (e.g., "cmi.exit"). </param>
        /// <param name="inValue">The value of the element in SCORM terms (e.g., "logout").</param>
        /// <remarks>
        /// <para>It is not valid to call SetValue in Review view.</para></remarks>
        public abstract void SetValue(PlainTextString inName, PlainTextString inValue);

        // Check if there are the correct number of name tokens in the aray. If not, throw exception.
        // For instance, if the element name is "cmi.comments_from_learner.n.comment", then 4 tokens are required (cmi, comments_from_learner, n, comment).
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        protected void VerifyElementNameTokens(int numberRequired, string[] nameTokens)
        {
            FramesetUtil.ValidateNonNullParameter("nameTokens", nameTokens);

            if (nameTokens.Length < numberRequired)
                throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueInvalidName, m_currentElementName));
        }

        // Complete the process of setting values. Allows processing SetValue calls that required multiple, dependent 
        // values to be set (eg, comments)
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual Collection<string> FinishSetValue()
        {
            List<string> errors = new List<string>();

            foreach (Objective obj in m_pendingObjectives)
            {
                try
                {
                    m_dm.Objectives.Add(obj);
                }
                catch (Exception e)
                {
                    errors.Add(ResHelper.GetMessage(FramesetResources.CONV_SetValueObjective, e.Message));
                }
            }

            foreach (Interaction interaction in m_pendingInteractions)
            {
                try
                {
                    m_dm.Interactions.Add(interaction);
                }
                catch (Exception e)
                {
                    errors.Add(ResHelper.GetMessage(FramesetResources.CONV_SetValueInteraction, e.Message));
                }
            }

            return new Collection<string>(errors);
        }

        // SetValue called on any read-only element. subElementName is ignored.
        protected void SetReadOnlyValue()
        {
            throw new InvalidOperationException(ResHelper.GetMessage(FramesetResources.CONV_SetValueReadOnly, m_currentElementName));
        }      


        protected void SetSuspendData(string value)
        {
            m_dm.SuspendData = value;
        }

        
        // subElementName is e.g., 'raw' in cmi.score.raw
        protected void SetScore(string subElementName, string value)
        {
            SetScoreSubField(subElementName, value, m_dm.Score);
        }

        // Set the score field (eg, "raw") on a Score object.
        protected abstract void SetScoreSubField(string scoreField, string value, Score score);

        protected void SetSessionTime(string value)
        {
            m_dm.SessionTime = TimeSpanFromRte(value);
        }

        protected abstract TimeSpan TimeSpanFromRte(string rteTimeSpan);

        // Return the InteractionType enum value for the rteValue of interaction.type
        protected static InteractionType GetInteractionType(string rteValue)
        {
            switch (rteValue)
            {
                case "true-false":
                    return InteractionType.TrueFalse;
                case "choice":
                    return InteractionType.MultipleChoice;
                case "fill-in":
                    return InteractionType.FillIn;
                case "long-fill-in":
                    return InteractionType.LongFillIn;
                case "likert":
                    return InteractionType.Likert;
                case "matching":
                    return InteractionType.Matching;
                case "performance":
                    return InteractionType.Performance;
                case "sequencing":
                    return InteractionType.Sequencing;
                case "numeric":
                    return InteractionType.Numeric;
                default:
                    return InteractionType.Other;
            }
        }
       
        #endregion

        #region Translation from LearningDataModel types to RTE strings

        // Helper function to translate a float value into a string for the RTE. 
        public static string RteFloatValue(float value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static string RteIntValue(int value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        // Helper function to translate int value into string for RTE, using specified numeric format 
        // (e.g., "D4" for 4 digits).
        public static string RteIntValue(int value, string format)
        {
            return value.ToString(format, NumberFormatInfo.InvariantInfo);
        }

        // Helper function to translate from .NET DateTime to SCORM date format.
        public static string RteDateTimeValue(DateTime value)
        {
            return XmlConvert.ToString(value, XmlDateTimeSerializationMode.Utc);
        }

        /// <summary>
        /// Gets the RTE string that represents the credit value.
        /// </summary>
        public static string GetRteCredit(bool credit, SessionView view)
        {
            // It's only possible to return 'credit' in execute view
            if (credit && (view == SessionView.Execute))
            {
                return "credit";
            }
            return "no-credit";
        }

        /// <summary>
        /// Gets the RTE string that represents the cmi.entry value.
        /// </summary>
        /// <param name="entryMode"></param>
        /// <returns></returns>
        public static string GetRteEntry(EntryMode entryMode)
        {
            switch (entryMode)
            {
                case EntryMode.AbInitio:
                    return "ab-initio";
                case EntryMode.Resume:
                    return "resume";
                default:
                    return "";
            }
        }

        public static string GetRteInteractionType(InteractionType? type)
        {
            if (type == null)
                return String.Empty;

            switch (type)
            {
                case InteractionType.FillIn:
                    return "fill-in";
                case InteractionType.Likert:
                    return "likert";
                case InteractionType.LongFillIn:
                    return "long-fill-in";
                case InteractionType.Matching:
                    return "matching";
                case InteractionType.MultipleChoice:
                    return "choice";
                case InteractionType.Numeric:
                    return "numeric";
                case InteractionType.Other:
                    return "other";
                case InteractionType.Performance:
                    return "performance";
                case InteractionType.Sequencing:
                    return "sequencing";
                case InteractionType.TrueFalse:
                    return "true-false";
                default:
                    return "other";
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        public static string GetRteLearnerResponse(object response)
        {
            FramesetUtil.ValidateNonNullParameter("response", response);
            Type responseType = response.GetType();
            if ((responseType == Type.GetType("System.Boolean")) 
                        || (responseType == Type.GetType("bool")))
            {
                bool br = (bool)response;
                return (br ? "true" : "false");
            }
            else if (responseType == Type.GetType("float"))
            {
                float fr = (float)response;
                return fr.ToString(NumberFormatInfo.InvariantInfo);
            }

            return response.ToString();
        }

        /// <summary>
        /// Helper function to get the cmi.interactions.n.result value
        /// </summary>
        public abstract string GetRteResult(InteractionResult result);


        /// <summary>
        /// Gets the RTE string that represents time limit action.
        /// </summary>
        public static string GetTimeLimitAction(TimeLimitAction action)
        {
            switch (action)
            {
                
                case TimeLimitAction.ContinueWithMessage:
                    return "continue,message";
                case TimeLimitAction.ExitNoMessage:
                    return "exit,no message";
                case TimeLimitAction.ExitWithMessage:
                    return "exit,message";
                default:
                    return "continue,no message";
            }
        }

        /// <summary>
        /// Helper function to convert .NET TimeSpan to RTE string value;
        /// </summary>
        public abstract string GetRteTimeSpan(TimeSpan ts);

        public static string GetRteAudioCaptioning(AudioCaptioning caption)
        {
            switch (caption)
            {
                case AudioCaptioning.Off:
                    return "-1";
                case AudioCaptioning.On:
                    return "1";
                default:
                    return "0";
            }
        }

        public static string GetRteMode(SessionView view)
        {
            switch (view)
            {
                case SessionView.Review:
                    return "review";
                default:
                    return "normal";
            }
        }
        #endregion  // LearningDataModel to RTE string conversions

        #region GetValues - return data model values in string (RTE) form

        /// <summary>
        /// Delegate to add the name / value pair of data model values into the StringBuilder.
        /// </summary>
        public delegate void AddDataModelValue(StringBuilder db, string name, string value);

        /// <summary>
        /// Return the encoded string of all current data model values to pass to the client. 
        /// </summary>
        public abstract DataModelValues GetDataModelValues(AddDataModelValue addDataModelValue);

        #endregion
    }

    /// <summary>
    /// The data model values to send to the client
    /// </summary>
    public class DataModelValues
    {
        private PlainTextString m_dataModelValues;
        private PlainTextString m_objectiveIdMap;

        internal DataModelValues(PlainTextString dataModelValues, PlainTextString objectiveIdMap)
        {
            m_dataModelValues = dataModelValues;
            m_objectiveIdMap = objectiveIdMap;
        }

        internal DataModelValues(PlainTextString dataModelValues)
            : this(dataModelValues, "")
        {
        }

        /// <summary>
        /// Gets and sets the actual data model values string.
        /// </summary>
        public PlainTextString Values
        {
            get { return m_dataModelValues; }
            set { m_dataModelValues = value; }
        }

        /// <summary>The objective id map.</summary>
        public PlainTextString ObjectiveIdMap
        {
            get { return m_objectiveIdMap; }
            set { m_objectiveIdMap = value; }
        }
    }
}
