/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents.DataModel;
using System.Xml;
using System.Xml.XPath;
using System.Collections.ObjectModel;
using Microsoft.LearningComponents.Manifest;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Represents a SCORM activity.
    /// </summary>
    /// <remarks>
    /// This class contains all information necessary to represent an activity, including the 
    /// <Typ>LearningDataModel</Typ>.  Activities are stored in a tree structure and are a superset
    /// of the <Typ>TableOfContentsElement</Typ> class.
    /// </remarks>
    internal class Activity : TableOfContentsElement
    {
        /// <summary>
        /// The Navigator object that owns this activity.
        /// </summary>
        private Navigator m_owner;

        /// <summary>
        /// The parent of this activity.  This is null for the root activity.
        /// </summary>
        private Activity m_parent;

        /// <summary>
        /// The next sibling activity.  This is null for a right-most activity.
        /// </summary>
        private Activity m_next;

        /// <summary>
        /// The previous sibling activity.  This is null for a left-most activity.
        /// </summary>
        private Activity m_previous;

        /// <summary>
        /// A list of the children of this activity.
        /// </summary>
        private List<TableOfContentsElement> m_children = new List<TableOfContentsElement>();
        
        /// <summary>
        /// An xml block in Static Activity XML format.
        /// </summary>
        private XPathNavigator m_rawStaticXml;

        /// <summary>
        /// The activity's data model.
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// A unique identifier for this activity.
        /// </summary>
        private long m_activityId;

        /// <summary>
        /// Random placement of this activity within its parent, or -1 if the original placement is to be used.
        /// </summary>
        private int m_randomPlacement;

        /// <summary>
        /// True if objectives are global to the system for this activity tree.  This is only valid for the root
        /// activity, it should be ignored for all other activities.
        /// </summary>
        private bool m_objectivesGlobalToSystem;

        /// <summary>
        /// Gets whether or not objectives are global to the system for this activity tree.  This is only valid for the root
        /// activity, it should be ignored for all other activities.
        /// </summary>
        public bool ObjectivesGlobalToSystem
        {
            get
            {
                Utilities.Assert(m_parent == null);
                return m_objectivesGlobalToSystem;
            }
        }

        /// <summary>
        /// Static sequencing data.
        /// </summary>
        private SequencingNodeReader m_sequencing;

        /// <summary>
        /// Whether or not this activity is valid to navigate to.
        /// </summary>
        private bool m_isValidToNavigateTo;

        /// <summary>
        /// Internal activity id, used for saving data to the database.
        /// </summary>
        private long m_internalActivityId;

        /// <summary>
        /// Initializes an Activity object.
        /// </summary>
        /// <param name="owner">The owner of this actitity tree.</param>
        /// <param name="activityId">The unique identifier of this activity.</param>
        /// <param name="rawStaticData">Refers to an xml block in Static Activity XML format.</param>
        /// <param name="rawSequencingData">Refers to an xml block in Sequencing Activity XML format.</param>
        /// <param name="rawDynamicData">Refers to an xml block in Dynamic Activity XML format.</param>
        /// <param name="commentsFromLms">Refers to an xml block in LMS Comments XML format.</param>
        /// <param name="wrap">Delegate to wrap attachments.</param>
        /// <param name="wrapGuid">Delegate to wrap guids that represent attachments.</param>
        /// <param name="randomPlacement">Random placement of this activity within its parent, or -1 if the original placement is to be used.</param>
        /// <param name="objectivesGlobalToSystem">Whether or not objectives are global to the system within this activity tree.</param>
        /// <param name="writeValidationMode">Validation mode to determine if the data model is writable.</param>
        /// <param name="learnerId">The unique identifier of the learner.</param>
        /// <param name="learnerName">The name of the learner.</param>
        /// <param name="learnerLanguage">The language code for the learner.</param>
        /// <param name="learnerCaption">The AudioCaptioning setting for the learner.</param>
        /// <param name="learnerAudioLevel">The audio level setting for the learner.</param>
        /// <param name="learnerDeliverySpeed">The delivery speed setting for the learner.</param>
        internal Activity(Navigator owner, long activityId, XPathNavigator rawStaticData, XPathNavigator rawSequencingData, 
            XPathNavigator rawDynamicData, XPathNavigator commentsFromLms, 
            LearningDataModel.WrapAttachmentDelegate wrap, LearningDataModel.WrapAttachmentGuidDelegate wrapGuid,
            int randomPlacement, bool objectivesGlobalToSystem, DataModelWriteValidationMode writeValidationMode, string learnerId, string learnerName, string learnerLanguage, 
            AudioCaptioning learnerCaption, float learnerAudioLevel, float learnerDeliverySpeed)
        {
            m_owner = owner;
            m_activityId = activityId;
            m_rawStaticXml = rawStaticData.SelectSingleNode("/item");
            PackageFormat format;
            if(owner.PackageFormat == PackageFormat.V1p2)
            {
                if(GetResourceType(m_rawStaticXml) == ResourceType.Lrm)
                {
                    format = PackageFormat.Lrm;
                }
                else
                {
                    format = PackageFormat.V1p2;
                }
            }
            else
            {
                format = owner.PackageFormat;
            }
            m_dataModel = new LearningDataModel(format, m_rawStaticXml, rawSequencingData,
                rawDynamicData, commentsFromLms, wrap, wrapGuid, writeValidationMode, learnerId, learnerName, learnerLanguage, 
                learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
            m_dataModel.DataChange = OnDataModelChanged;
            m_randomPlacement = randomPlacement;
            m_objectivesGlobalToSystem = objectivesGlobalToSystem;
            // only call the following after m_rawStaticXml is set.
            m_dataModel.Tracked = Sequencing.Tracked;
            if (IsLeaf)
            {
                m_dataModel.CompletionSetByContent = Sequencing.CompletionSetByContent;
                m_dataModel.ObjectiveSetByContent = Sequencing.ObjectiveSetByContent;
            }
            m_dataModel.UpdateScore = UpdateScore;
        }

        /// <summary>
        /// Updates the total score for the entire activity tree.
        /// </summary>
        /// <param name="oldScore">The previous score for this activity.</param>
        /// <param name="newScore">The new score for this activity.</param>
        /// <remarks>
        /// This method must only be called for SCORM 1.2 or LRM packages.
        /// The value passed is intended to be added to the previous
        /// value of Navigator.TotalPoints.
        /// </remarks>
        internal void UpdateScore(float? oldScore, float? newScore)
        {
            float add;

            if(oldScore.HasValue && newScore.HasValue)
            {
                // do this to avoid overflow problems instead of just subtracting old from new
                if(!m_owner.TotalPoints.HasValue)
                {
                    m_owner.TotalPoints = -oldScore.Value;
                }
                else
                {
                    m_owner.TotalPoints -= oldScore.Value;
                }
                add = newScore.Value;
            }
            else if(newScore.HasValue)
            {
                add = newScore.Value;
            }
            else
            {
                // load the entire tree if it isn't loaded already
                m_owner.LoadActivityTree();
                bool clearTotal = true;
                foreach(Activity activity in m_owner.Traverse)
                {
                    if(activity.DataModel.EvaluationPoints.HasValue)
                    {
                        clearTotal = false;
                        break;
                    }
                }
                if(clearTotal)
                {
                    m_owner.TotalPoints = null;
                    return;
                }
                add = -oldScore.Value;
            }
            if(!m_owner.TotalPoints.HasValue)
            {
                m_owner.TotalPoints = add;
            }
            else
            {
                m_owner.TotalPoints += add;
            }
        }

        /// <summary>
        /// Initializes an Activity object for cloning.
        /// </summary>
        /// <param name="owner">The owner of this actitity tree.</param>
        /// <param name="activityId">The unique identifier of this activity.</param>
        /// <param name="rawStaticData">Refers to an xml block in Static Activity XML format.</param>
        /// <param name="rawSequencingData">Refers to an xml block in Sequencing Activity XML format.</param>
        /// <param name="randomPlacement">Random placement of this activity within its parent, or -1 if the original placement is to be used.</param>
        internal Activity(Navigator owner, long activityId, XPathNavigator rawStaticData, 
            XPathNavigator rawSequencingData, int randomPlacement)
        {
            m_owner = owner;
            m_activityId = activityId;
            m_rawStaticXml = rawStaticData;
            PackageFormat format;
            if(owner.PackageFormat == PackageFormat.V1p2)
            {
                if(GetResourceType(m_rawStaticXml) == ResourceType.Lrm)
                {
                    format = PackageFormat.Lrm;
                }
                else
                {
                    format = PackageFormat.V1p2;
                }
            }
            else
            {
                format = owner.PackageFormat;
            }
            m_dataModel = new LearningDataModel(format, m_rawStaticXml, rawSequencingData,
                null, null, null, null, DataModelWriteValidationMode.AlwaysAllowWrite, String.Empty, String.Empty, String.Empty, 
                AudioCaptioning.NoChange, (float)1.0, (float)1.0);
            m_randomPlacement = randomPlacement;
            // only call the following after m_rawStaticXml is set.
            m_dataModel.Tracked = Sequencing.Tracked;
            m_dataModel.CompletionSetByContent = Sequencing.CompletionSetByContent;
            m_dataModel.ObjectiveSetByContent = Sequencing.ObjectiveSetByContent;
        }

        /// <summary>
        /// Clones this activity by copying only the data needed for sequencing/navigation.
        /// </summary>
        /// <returns>A new activity with data similar enough for sequencing/navigation purposes.</returns>
        internal Activity CloneForNavigationTest()
        {
            XPathNavigator seq = ((XmlNode)m_dataModel.SequencingData.UnderlyingObject).OwnerDocument.CloneNode(true).CreateNavigator();
            Activity clone = new Activity(m_owner, m_activityId, m_rawStaticXml, 
                seq, m_randomPlacement);
            return clone;
        }

        /// <summary>
        /// Called via delegate when the data model changes
        /// </summary>
        /// <remarks>
        /// This method merely adds this activity to the collection of dirty activities in the 
        /// <Typ>Navigator</Typ> that owns this activity.  Later, it is the responsibility of that 
        /// <Typ>Navigator</Typ> to save these dirty activities if the view mode supports saving.
        /// </remarks>
        private void OnDataModelChanged()
        {
            m_owner.DirtyActivities[m_activityId] = this;
        }

        #region TableOfContentsElement members

        /// <summary>
        /// Gets the title for this table of contents element.
        /// </summary>
        public override string Title
        {
            get
            {
                return m_rawStaticXml.GetAttribute("title", String.Empty);
            }
        }

        /// <summary>
        /// Gets the children of this table of contents element.
        /// </summary>
        public override ReadOnlyCollection<TableOfContentsElement> Children
        {
            get
            {
                return new ReadOnlyCollection<TableOfContentsElement>(m_children);
            }
        }

        /// <summary>
        /// Gets whether or not this table of contents element is valid to navigate to.
        /// </summary>
        /// <remarks>
        /// This value is only guaranteed valid at the time immediately after the calling of
        /// <c>LoadTableOfContents</c>.  Changes made to the current activity's data model or
        /// navigations performed after this call may change whether or not this element is 
        /// actually valid to navigate to, but this will not be reflected in this value unless
        /// or until <c>LoadTableOfContents</c> is called again.
        /// </remarks>
        public override bool IsValidChoiceNavigationDestination
        {
            get
            {
                return m_isValidToNavigateTo;
            }
        }

        /// <summary>
        /// Gets the resource type from the specified navigator
        /// </summary>
        /// <param name="parent">The navigator that holds the resourceType attribute</param>
        /// <returns>The resource type</returns>
        /// <remarks>Added to get around the FxCop warning CA2214, which complains about 
        /// calling a virtual method in a constructor.</remarks>
        private static ResourceType GetResourceType(XPathNavigator parent)
        {
            XPathNavigator nav = parent.SelectSingleNode("@resourceType");

            if(nav == null)
            {
                return ResourceType.None;
            }
            return (ResourceType)Enum.Parse(typeof(ResourceType), nav.Value);
        }

        /// <summary>
        /// Gets the type of the resource associated with this table of contents element.
        /// </summary>
        public override ResourceType ResourceType
        {
            get
            {
                return GetResourceType(m_rawStaticXml);
            }
        }

        /// <summary>
        /// Gets a unique identifier for the activity associated with this table of contents element.
        /// </summary>
        /// <remarks>
        /// Using this identifier within <c>NavigateTo</c> instead of the activity's string identifier
        /// will result in improved performance.
        /// </remarks>
        public override long ActivityId
        {
            get
            {
                return m_activityId;
            }
        }

        #endregion

        /// <summary>
        /// Gets the data model associated with this activity.
        /// </summary>
        internal DataModel.LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }

        /// <summary>
        /// Gets the string identifier of this activity.
        /// </summary>
        internal string Key
        {
            get
            {
                return m_rawStaticXml.GetAttribute("id", String.Empty);
            }
        }

        /// <summary>
        /// Gets the parameters to be passed to the resource associated with this activity at launch time.
        /// </summary>
        internal string Parameters
        {
            get
            {
                return m_rawStaticXml.GetAttribute("parameters", String.Empty);
            }
        }

        /// <summary>
        /// Gets the string identifier for the resource associated with this activity.
        /// </summary>
        internal string ResourceKey
        {
            get
            {
                return m_rawStaticXml.GetAttribute("resourceId", String.Empty);
            }
        }

        /// <summary>
        /// Gets the default file from the resource associated with this activity.
        /// </summary>
        internal string DefaultResourceFile
        {
            get
            {
                return m_rawStaticXml.GetAttribute("defaultResourceFile", String.Empty);
            }
        }

        internal string ResourceXmlBase
        {
            get
            {
                return m_rawStaticXml.GetAttribute("xmlBase", String.Empty);
            }
        }

        /// <summary>
        /// Gets whether the UI should hide the "previous" button.
        /// </summary>
        internal bool HidePreviousUI
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_rawStaticXml, "hidePreviousUI", false);
            }
        }

        /// <summary>
        /// Gets whether the UI should hide the "continue" (e.g. "next") button.
        /// </summary>
        internal bool HideContinueUI
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_rawStaticXml, "hideContinueUI", false);
            }
        }

        /// <summary>
        /// Gets whether the UI should hide the "exit" button.
        /// </summary>
        internal bool HideExitUI
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_rawStaticXml, "hideExitUI", false);
            }
        }

        /// <summary>
        /// Gets whether the UI should hide the "abandon" button.
        /// </summary>
        internal bool HideAbandonUI
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_rawStaticXml, "hideAbandonUI", false);
            }
        }

        /// <summary>
        /// Gets or sets the parent activity of this activity
        /// </summary>
        internal Activity Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the next activity of this activity
        /// </summary>
        internal Activity Next
        {
            get
            {
                return m_next;
            }
            set
            {
                m_next = value;
            }
        }

        /// <summary>
        /// Gets or sets the next activity of this activity
        /// </summary>
        internal Activity Previous
        {
            get
            {
                return m_previous;
            }
            set
            {
                m_previous = value;
            }
        }

        /// <summary>
        /// Internal activity id, used for saving data to the database.
        /// </summary>
        internal long InternalActivityId
        {
            get
            {
                return m_internalActivityId;
            }
            set
            {
                m_internalActivityId = value;
                m_dataModel.InternalActivityId = value;
            }
        }

        /// <summary>
        /// Gets an activities actual current position within the parent, possibly modified by random placement.
        /// </summary>
        internal int Position
        {
            get
            {
                if(m_randomPlacement >= 0)
                {
                    return m_randomPlacement;
                }
                return DataModelUtilities.GetAttribute<int>(m_rawStaticXml, "origPlacement", 0);
            }
        }

        /// <summary>
        /// Gets whether or not this activity is displayed when the structure of the package is displayed or rendered.
        /// </summary>
        public override bool IsVisible
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_rawStaticXml, "isvisible", true);
            }
        }

        private bool CheckForVisibleChildren(TableOfContentsElement element)
        {
            bool hasVisibleChildren = false;
            foreach (TableOfContentsElement child in element.Children)
            {
                if (child.Children.Count == 0)
                {
                    hasVisibleChildren = child.IsVisible;
                }
                else
                {
                    hasVisibleChildren = CheckForVisibleChildren(child);
                }
                if (hasVisibleChildren)
                {
                    break;
                }
            }
            return hasVisibleChildren;
        }

        /// <summary>
        /// Gets whether or not this table of contents element has at least one descendant child, any level deep, with a value of
        /// <c>true</c> for <Mth>IsVisible</Mth>.
        /// </summary>
        /// <remarks>
        /// This value is used to determine how to display the UI of nodes - whether the node should be displayed as having available
        /// children or not.
        /// </remarks>
        public override bool HasVisibleChildren
        {
            get 
            {
                return CheckForVisibleChildren(this);
            }
        }

        /// <summary>
        /// Gets whether or not this activity is a leaf activity.
        /// </summary>
        internal bool IsLeaf
        {
            get
            {
                return (m_children.Count == 0);
            }
        }

        /// <summary>
        /// Gets whether or not this activity is the last activity in the tree.
        /// </summary>
        internal bool IsLastActivityInTree
        {
            get
            {
                if(!IsLeaf)
                {
                    return false;
                }
                for(Activity act = this ; act != null ; act = act.Parent)
                {
                    if(act.Next != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the next activity in the tree, in preorder traversal order.  
        /// If there is no next activity, null is returned.
        /// </summary>
        internal Activity NextInPreorderTraversal
        {
            get
            {
                if(m_children.Count > 0)
                {
                    return (Activity)m_children[0];
                }
                for(Activity a = this ; a != null ; a = a.Parent)
                {
                    if(a.Next != null)
                    {
                        return a.Next;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the previous activity in the tree, in preorder traversal order.  
        /// If there is no previous activity, null is returned.
        /// </summary>
        internal Activity PreviousInPreorderTraversal
        {
            get
            {
                if(m_previous == null)
                {
                    return m_parent;
                }
                Activity a = m_previous;
                while(true)
                {
                    if(a.m_children.Count == 0)
                    {
                        return a;
                    }
                    a = (Activity)a.m_children[a.m_children.Count - 1];
                }
            }
        }

        /// <summary>
        /// Sets whether or not this activity is valid to navigate to.
        /// </summary>
        /// <param name="isValidToNavigateTo">Whether or not this activity is valid to navigate to.</param>
        /// <remarks>
        /// We can't just make an internal set for this property because that conflicts with
        /// the abstract get accessor.
        /// </remarks>
        internal void SetValidToNavigateTo(bool isValidToNavigateTo)
        {
            m_isValidToNavigateTo = isValidToNavigateTo;
        }

        /// <summary>
        /// The prerequisites for the activity.  This value is only valid for SCORM 1.2
        /// </summary>
        private string m_prerequisites;

        /// <summary>
        /// Gets the prerequisites for the activity.  This value is only valid for SCORM 1.2
        /// </summary>
        internal string Prerequisites
        {
            get
            {
                if(m_prerequisites == null)
                {
                    XmlNamespaceManager ns = new XmlNamespaceManager(m_rawStaticXml.NameTable);
                    ns.AddNamespace("adlcp", "http://www.adlnet.org/xsd/adlcp_rootv1p2");
                    XPathNavigator nav = m_rawStaticXml.SelectSingleNode("adlcp:prerequisites", ns);
                    if(nav == null)
                    {
                        m_prerequisites = String.Empty;
                    }
                    else
                    {
                        m_prerequisites = nav.Value;
                    }
                }
                return m_prerequisites;
            }
        }

        /// <summary>
        /// Gets the static sequencing information
        /// </summary>
        internal SequencingNodeReader Sequencing
        {
            get
            {
                if(m_sequencing == null)
                {
                    XmlNamespaceManager ns = new XmlNamespaceManager(m_rawStaticXml.NameTable);
                    ns.AddNamespace("imsss", "http://www.imsglobal.org/xsd/imsss");
                    XPathNavigator nav = m_rawStaticXml.SelectSingleNode("imsss:sequencing", ns);
                    if(nav == null)
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlElement elem = doc.CreateElement("imsss", "sequencing", "http://www.imsglobal.org/xsd/imsss");
                        doc.AppendChild(elem);
                        nav = elem.CreateNavigator();
                    }
                    m_sequencing = new SequencingNodeReader(nav, new ManifestReaderSettings(false, false),
                        new PackageValidatorSettings(ValidationBehavior.Enforce, ValidationBehavior.None, ValidationBehavior.None, ValidationBehavior.None), false, null);
                }
                return m_sequencing;
            }
        }

        /// <summary>
        /// Gets the primary objective associated with this activity.
        /// </summary>
        internal Objective PrimaryObjective
        {
            get
            {
                return m_dataModel.PrimaryObjective;
            }
        }

        /// <summary>
        /// Gets or sets the random placement of this activity within its parent
        /// </summary>
        internal int RandomPlacement
        {
            get
            {
                return m_randomPlacement;
            }
            set
            {
                m_randomPlacement = value;
            }
        }

        /// <summary>
        /// Adds a child activity to this activity.
        /// </summary>
        /// <param name="child">The activity to add as a child.</param>
        internal void AddChild(Activity child)
        {
            m_children.Add(child);
        }

        /// <summary>
        /// Removes a child activity at the specified index position.
        /// </summary>
        /// <param name="index">The location to remove the child activity from.</param>
        internal void RemoveChild(int index)
        {
            m_children.RemoveAt(index);
        }

        /// <summary>
        /// Compares two activites for their position within their parent.
        /// </summary>
        /// <param name="elem1">First activity to compare.</param>
        /// <param name="elem2">Second activity to compare.</param>
        /// <returns>Less than 0 if elem1 is less than elem2, 0 if they are equal, greater than 0 
        /// if elem1 is greater than elem2.</returns>
        private int CompareActivities(TableOfContentsElement elem1, TableOfContentsElement elem2)
        {
            Activity act1 = (Activity)elem1;
            Activity act2 = (Activity)elem2;

            return act1.Position - act2.Position;
        }

        /// <summary>
        /// Sorts children and makes sure their <Prp>Next</Prp>/<Prp>Previous</Prp> pointers are correct.
        /// </summary>
        internal void SortChildren()
        {
            if(m_children.Count != 0)
            {
                m_children.Sort(CompareActivities);
                ((Activity)m_children[0]).Previous = null;
                for(int i = 1 ; i < m_children.Count ; ++i)
                {
                    ((Activity)m_children[i-1]).Next = (Activity)m_children[i];
                    ((Activity)m_children[i]).Previous = (Activity)m_children[i-1];
                }
                ((Activity)m_children[m_children.Count - 1]).Next = null;
            }
        }
    }
}
