/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkApi.cs
//
// Implements SlkStore and related types.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Library;
using Microsoft.SharePoint.Navigation;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Microsoft.SharePointLearningKit
{

/// <summary>
/// Represents a connection to a SharePoint Learning Kit store.  Doesn't hold any information tied
/// to the identity of the current user, so an <c>AnonymousSlkStore</c> can be cached on a Web site
/// and shared by multiple users.
/// </summary>
///
internal class AnonymousSlkStore
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>SPSiteGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spSiteGuid;

    /// <summary>
    /// Holds the value of the <c>Mapping</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkSPSiteMapping m_mapping;

    /// <summary>
    /// Holds the value of the <c>Settings</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkSettings m_settings;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this <c>AnonymousSlkStore</c>.
    /// </summary>
    public Guid SPSiteGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spSiteGuid;
        }
    }

    /// <summary>
    /// Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this
    /// <c>AnonymousSlkStore</c>.
    /// </summary>
    public SlkSPSiteMapping Mapping
    {
        [DebuggerStepThrough]
        get
        {
            return m_mapping;
        }
    }

    /// <summary>
    /// Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this
    /// <c>AnonymousSlkStore</c>.
    /// </summary>
    public SlkSettings Settings
    {
        [DebuggerStepThrough]
        get
        {
            return m_settings;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    /// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
    ///     </param>
    ///
    /// <param name="mapping">The value to use to initialize the <c>Mapping</c> property.
    ///     </param>
    ///
    /// <param name="settings">The value to use to initialize the <c>Settings</c> property.
    ///     </param>
    ///
    public AnonymousSlkStore(Guid spSiteGuid, SlkSPSiteMapping mapping, SlkSettings settings)
    {
        m_spSiteGuid = spSiteGuid;
        m_mapping = mapping;
        m_settings = settings;
    }
}


/// <summary>
/// Represents an item in a SharePoint Learning Kit user web list, which is a list of SharePoint
/// SPWebs that a given user has added to the list of Web sites in the E-Learning Actions page.
/// Each user has zero or more associated <c>SlkUserWebListItem</c> objects stored in the
/// SharePoint Learning Kit database.
/// </summary>
///
[DebuggerDisplay("SlkUserWebListItem {m_spWebGuid}, {m_lastAccessTime}")]
public class SlkUserWebListItem
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>SPSiteGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spSiteGuid;

    /// <summary>
    /// Holds the value of the <c>SPWebGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spWebGuid;

    /// <summary>
    /// Holds the value of the <c>LastAccessTime</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime m_lastAccessTime;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this user web list item.
    /// </summary>
    public Guid SPSiteGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spSiteGuid;
        }
    }

    /// <summary>
    /// Gets the <c>Guid</c> of the <c>SPWeb</c> associated with this user web list item.
    /// </summary>
    public Guid SPWebGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spWebGuid;
        }
    }

    /// <summary>
    /// Gets the <c>DateTime</c> that the Web site specified by this user web list item was
    /// accessed using SharePoint Learning Kit.  Unlike the related value stored in the SharePoint
    /// Learning Kit database, this value is a local date/time, not a UTC value.
    /// </summary>
    public DateTime LastAccessTime
    {
        [DebuggerStepThrough]
        get
        {
            return m_lastAccessTime;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    /// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
    ///     </param>
    ///
    /// <param name="spWebGuid">The value to use to initialize the <c>SPWebGuid</c> property.
    ///     </param>
    ///
    /// <param name="lastAccessTime">The value to use to initialize the <c>LastAccessTime</c>
    ///     property.</param>
    ///
    internal SlkUserWebListItem(Guid spSiteGuid, Guid spWebGuid, DateTime lastAccessTime)
    {
        m_spSiteGuid = spSiteGuid;
        m_spWebGuid = spWebGuid;
        m_lastAccessTime = lastAccessTime;
    }
}

/// <summary>
/// Represents a collection of SharePoint Learning Kit learners and/or instructors contained
/// within a SharePoint <c>SPGroup</c>.
/// </summary>
[DebuggerDisplay("SlkGroup: {Name}")]
public class SlkGroup : IComparable<SlkGroup>
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>SPGroup</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPGroup m_spGroup;

    /// <summary>
    /// Holds the value of the <c>DomainGroup</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPUser m_domainGroup;

    /// <summary>
    /// Holds the value of the <c>Users</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    List<SlkUser> m_users;

    /// <summary>
    /// Holds the value of the <c>UserKeys</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    List<string> m_userKeys;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the SharePoint <c>SPGroup</c> that is represented by this <c>SlkGroup</c>, or
    /// <c>null</c> if this <c>SlkGroup</c> represents a domain group.
    /// </summary>
    public SPGroup SPGroup
    {
        [DebuggerStepThrough]
        get
        {
            return m_spGroup;
        }
    }

    /// <summary>
    /// Gets the SharePoint <c>SPUser</c> of the domain group that is represented by this
    /// <c>SlkGroup</c>, or <c>null</c> if this <c>SlkGroup</c> represents a SharePoint group
    /// instead of a domain group.
    /// </summary>
    public SPUser DomainGroup
    {
        [DebuggerStepThrough]
        get
        {
            return m_domainGroup;
        }
    }

    /// <summary>
    /// Gets the members of this group, represented as a collection of <c>SlkUser</c> objects.
    /// </summary>
    public ReadOnlyCollection<SlkUser> Users
    {
        [DebuggerStepThrough]
        get
        {
            return new ReadOnlyCollection<SlkUser>(m_users);
        }
    }

    /// <summary>
    /// Gets or sets the members of this group, represented as a collection of SID strings.
    /// </summary>
    internal List<string> UserKeys
    {
        [DebuggerStepThrough]
        get
        {
            return m_userKeys;
        }
        [DebuggerStepThrough]
        set
        {
            m_userKeys = value;
        }
    }

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name
    {
        [DebuggerStepThrough]
        get
        {
            if (m_spGroup != null)
                return m_spGroup.Name;
            else
                return m_domainGroup.Name;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    /// <param name="spGroup">The SharePoint <c>SPGroup</c> represented by this <c>SlkGroup</c>,
    ///     if this <c>SlkGroup</c> represents a SharePoint group; <c>null</c> otherwise.</param>
    /// 
    /// <param name="domainGroup">The SharePoint <c>SPUser</c> represented by this <c>SlkGroup</c>
    ///     if this <c>SlkGroup</c> represents a domain group; <c>null</c> otherwise.</param>
    ///
    internal SlkGroup(SPGroup spGroup, SPUser domainGroup)
    {
        m_spGroup = spGroup;
        m_domainGroup = domainGroup;
        m_users = new List<SlkUser>();
    }

    /// <summary>
    /// Adds a user to <c>Users</c>.
    /// </summary>
    ///
    /// <param name="slkUser">The user to add.</param>
    ///
    internal void AddUser(SlkUser slkUser)
    {
        m_users.Add(slkUser);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // IComparable<SlkGroup> Implementation
    //

    /// <summary>
    /// Compares this <r>SlkGroup</r> to another.
    /// </summary>
    ///
    /// <param name="other">The other <r>SlkGroup</r>.</param>
    ///
    /// <returns>
    /// A value less than, equal to, or greater than zero, depending on whether this
    /// <r>SlkGroup</r> is less than, equal to, or greather than <pr>other</pr>.
    /// </returns>
    ///
    int IComparable<SlkGroup>.CompareTo(SlkGroup other)
    {
        return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}

/// <summary>
/// Represents a collection of <c>SlkUser</c> objects.  The key for the collection is the
/// <c>UserItemIdentifier</c> of each <c>SlkUser</c>.
/// </summary>
///
public class SlkUserCollection : KeyedCollection<UserItemIdentifier, SlkUser>
{
    /// <summary>
    /// Identifies <c>SlkUser.UserId</c> as the key for items in this collection.
    /// </summary>
    protected override UserItemIdentifier GetKeyForItem(SlkUser item)
    {
        return item.UserId;
    }
}

/// <summary>
/// Contains lists of instructors, learners, and learner groups on a given <c>SPWeb</c>.
/// </summary>
///
/// <remarks>
/// A user is considered an instructor on a given SharePoint <c>SPWeb</c> if they have the
/// instructor permission on that group, i.e. the permission defined by
/// <c>SlkSPSiteMapping.InstructorPermission</c>.  Similarly, a user is considered a learner
/// if they have the learner permission, <c>SlkSPSiteMapping.LearnerPermission</c>.
/// </remarks>
///
public class SlkMemberships
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>Instructors</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkUser[] m_instructors;

    /// <summary>
    /// Holds the value of the <c>Learners</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkUser[] m_learners;

    /// <summary>
    /// Holds the value of the <c>LearnerGroups</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkGroup[] m_learnerGroups;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the collection of instructors on the SharePoint <c>SPWeb</c> associated with this
    /// object.  A user is considered an instructor on a given <c>SPWeb</c> if they have the
    /// instructor permission on that <c>SPWeb</c>, i.e. the permission defined by
    /// <c>SlkSPSiteMapping.InstructorPermission</c>.
    /// </summary>
    public ReadOnlyCollection<SlkUser> Instructors
    {
        [DebuggerStepThrough]
        get
        {
            return new ReadOnlyCollection<SlkUser>(m_instructors);
        }
    }

    /// <summary>
    /// Gets the collection of learners on the SharePoint <c>SPWeb</c> associated with this object.
    /// A user is considered a learner on a given <c>SPWeb</c> if they have the learner permission
    /// on that <c>SPWeb</c>, i.e. the permission defined by
    /// <c>SlkSPSiteMapping.LearnerPermission</c>.
    /// </summary>
    public ReadOnlyCollection<SlkUser> Learners
    {
        [DebuggerStepThrough]
        get
        {
            return new ReadOnlyCollection<SlkUser>(m_learners);
        }
    }

    /// <summary>
    /// Gets the collection of learner groups on the SharePoint <c>SPWeb</c> associated with this
    /// object.  A SharePoint <c>SPGroup</c> or domain group is considered a learner group on a
    /// given <c>SPWeb</c> if it has the learner permission on that <c>SPWeb</c>, i.e. the
    /// permission defined by <c>SlkSPSiteMapping.LearnerPermission</c>.
    /// </summary>
    public ReadOnlyCollection<SlkGroup> LearnerGroups
    {
        [DebuggerStepThrough]
        get
        {           
            return new ReadOnlyCollection<SlkGroup>(m_learnerGroups);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Constructor
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    /// <param name="instructors">The collection of instructors/>.
    ///     (See the <c>Instructors</c> property.)</param>
    ///
    /// <param name="learners">The collection of learners />.
    ///     (See the <c>Learners</c> property.)</param>
    ///
    /// <param name="learnerGroups">The collection of learner groups />.
    ///     (See the <c>LearnerGroups</c> property.)</param>
    ///
    public SlkMemberships(SlkUser[] instructors, SlkUser[] learners,
        SlkGroup[] learnerGroups)
    {
        //Copies the passed arrays to the respective members.
        //Creates an empty array when the passed array is null.
        if (instructors != null)
        {
            m_instructors = new SlkUser[instructors.Length];
            instructors.CopyTo(m_instructors, 0);
        }
        else
        {
            m_instructors = new SlkUser[0];
        }

        if (learners != null)
        {
            m_learners = new SlkUser[learners.Length];
            learners.CopyTo(m_learners, 0);
        }
        else
        {
            m_learners = new SlkUser[0];
        }

        if (learnerGroups != null)
        {
            m_learnerGroups = new SlkGroup[learnerGroups.Length];
            learnerGroups.CopyTo(m_learnerGroups, 0);
        }
        else
        {
            m_learnerGroups = new SlkGroup[0];
        }        
    }
}

/// <summary>
/// Contains information about a SharePoint Learning Kit package.
/// </summary>
/// 
public class PackageInformation
{
    /// <summary>
    /// Holds the value of the <c>SPFile</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPFile m_spFile;

    /// <summary>
    /// Holds the value of the <c>ManifestReader</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ManifestReader m_manifestReader;

    /// <summary>
    /// Holds the value of the <c>Title</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_title;

    /// <summary>
    /// Holds the value of the <c>Description</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_description;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// The <c>SPFile</c> of the package.
    /// </summary>
    public SPFile SPFile
    {
        [DebuggerStepThrough]
        get
        {
            return m_spFile;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_spFile = value;
        }
    }

    /// <summary>
    /// The manifest reader for the package
    /// </summary>
    public ManifestReader ManifestReader
    {
        [DebuggerStepThrough]
        get
        {
            return m_manifestReader;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_manifestReader = value;
        }
    }

    /// <summary>
    /// Title of the package
    /// </summary>
    public string Title
    {
        [DebuggerStepThrough]
        get
        {
            return m_title;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_title = value;
        }
    }

    /// <summary>
    /// Description for the package
    /// </summary>
    public string Description
    {
        [DebuggerStepThrough]
        get
        {
            return m_description;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_description = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    internal PackageInformation()
    {
    }
}


/// <summary>
/// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
/// about an assignment related to one of the learners of the assignment) that's used by an
/// instructor during grading.
/// </summary>
///
public class GradingProperties
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>LearnerAssignmentId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentItemIdentifier m_learnerAssignmentId;

    /// <summary>
    /// Holds the value of the <c>LearnerAssignmentGuidId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_learnerAssignmentGuidId;

    /// <summary>
    /// Holds the value of the <c>LearnerId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    UserItemIdentifier m_learnerId;

    /// <summary>
    /// Holds the value of the <c>LearnerName</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_learnerName; // from: LearnerName

    /// <summary>
    /// Holds the value of the <c>Status</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentState? m_status; // from: LearnerAssignmentState

    /// <summary>
    /// Holds the value of the <c>CompletionStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    CompletionStatus m_completionStatus;

    /// <summary>
    /// Holds the value of the <c>SuccessStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SuccessStatus m_successStatus;

    /// <summary>
    /// Holds the value of the <c>GradedPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_gradedPoints;

    /// <summary>
    /// Holds the value of the <c>FinalPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_finalPoints; // from: FinalPoints

    /// <summary>
    /// Holds the value of the <c>IgnoreFinalPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool m_ignoreFinalPoints;

    /// <summary>
    /// Holds the value of the <c>InstructorComments</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_instructorComments; // from: InstructorComments

    /// <summary>
    /// Holds the value of the <c>AttemptId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    AttemptItemIdentifier m_attemptId;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the identifier of the learner assignment represented by this object.
    /// </summary>
    public LearnerAssignmentItemIdentifier LearnerAssignmentId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentId;
        }
    }

    /// <summary>
    /// Gets the GUID identifier of the learner assignment represented by this object.
    /// </summary>
    public Guid LearnerAssignmentGuidId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentGuidId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerAssignmentGuidId = value;
        }
    }

    /// <summary>
    /// Gets the <c>UserItemIdentifier</c> of the user that this learner assignment is assigned to.
    /// </summary>
    public UserItemIdentifier LearnerId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerId = value;
        }
    }

    /// <summary>
    /// Gets the name of the user that this learner assignment is assigned to.
    /// </summary>
    public string LearnerName
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerName;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerName = value;
        }
    }

    /// <summary>
    /// Gets or sets the <c>LearnerAssignmentState</c> of this learner assignment.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// Changing the value of <c>Status</c>, and then calling
    /// <c>SlkStore.SetGradingProperties</c>, will transition this learner assignment to another
    /// <c>LearnerAssignmentState</c> value.  Only the following state transitions are supported
    /// by <c>SlkStore.SetGradingProperties</c>:
    /// </para>
    /// <list type="bullet">
    ///     <item><description><c>NotStarted</c> to <c>Completed</c>.
    ///         </description>
    ///     </item>
    ///     <item><description><c>Active</c> to <c>Completed</c>.
    ///         </description>
    ///     </item>
    ///     <item><description><c>Completed</c> to <c>Final</c>.
    ///         </description>
    ///     </item>
    ///     <item><description><c>Final</c> to <c>Active</c>.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para>
    /// When setting <c>Status</c>, use the value <c>null</c> to indicate that you don't want to
    /// change the status of the assignment.  If you use the current value, the status also won't
    /// be changed, but you run the risk of another user changing the status between your calls
    /// to <c>SlkStore.GetGradingProperties</c> and <c>SlkStore.SetGradingProperties</c>.
    /// </para>
    /// </remarks>
    ///
    public Nullable<LearnerAssignmentState> Status
    {
        [DebuggerStepThrough]
        get
        {
            return m_status;
        }
        [DebuggerStepThrough]
        set
        {
            m_status = value;
        }
    }

    /// <summary>
    /// Gets a <c>CompletionStatus</c> value indicating whether the SCORM 2005 package associated
    /// with this content considers the content to be completed by the learner.  This property is
    /// only used for assignments of SCORM 2004 packages; <c>CompletionStatus.Unknown</c> is
    /// returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public CompletionStatus CompletionStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_completionStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_completionStatus = value;
        }
    }

    /// <summary>
    /// Gets a <c>SuccessStatus</c> value indicating whether the SCORM package associated with
    /// this content considers the learner to have succeeded (as defined by the content).  This
    /// property is only used for assignments of SCORM 2004 packages; <c>SuccessStatus.Unknown</c>
    /// is returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public SuccessStatus SuccessStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_successStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_successStatus = value;
        }
    }

    /// <summary>
    /// Gets the number of points the learner received from automatic and manual grading of the
    /// learner assignment.  If the content type does not support grading, or if the grade is
    /// "blank", <c>GradedPoints</c> will be <c>null</c>.
    /// </summary>
    public Nullable<Single> GradedPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_gradedPoints;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_gradedPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets the number of points the learner received on this learner assignment.
    /// </summary>
    public Nullable<Single> FinalPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_finalPoints;
        }
        [DebuggerStepThrough]
        set
        {
            m_finalPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether <c>FinalPoints</c> will be ignore in subsequent
    /// calls to <c>SlkStore.SetGradingProperties</c>.  In that case, the current value of
    /// <c>FinalPoints</c> in the database is not changed to the value of
    /// <c>GradingProperties.FinalPoints</c>.
    /// </summary>
    public bool IgnoreFinalPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_ignoreFinalPoints;
        }
        [DebuggerStepThrough]
        set
        {
            m_ignoreFinalPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets comments from the instructor (if any) on this learner assignment;
    /// <c>String.Empty</c> if none. 
    /// </summary>
    public string InstructorComments
    {
        [DebuggerStepThrough]
        get
        {
            return m_instructorComments;
        }
        [DebuggerStepThrough]
        set
        {
            m_instructorComments = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Properties
    //

    /// <summary>
    /// Gets or sets the <c>AttemptItemIdentifier</c> of the attempt associated with this learner
    /// assignment, or <c>null</c> if none.
    /// </summary>
    internal AttemptItemIdentifier AttemptId
    {
        [DebuggerStepThrough]
        get
        {
            return m_attemptId;
        }
        [DebuggerStepThrough]
        set
        {
            m_attemptId = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by
    ///     this object.</param>
    ///
    public GradingProperties(LearnerAssignmentItemIdentifier learnerAssignmentId)
    {
        if(learnerAssignmentId == null)
            throw new ArgumentNullException("learnerAssignmentId");
            
        m_learnerAssignmentId = learnerAssignmentId;
    }
}

/// <summary>
/// Specifies a role that a user may have in SLK, with respect to a Web site or an assignment.
/// A user may have multiple roles.
/// </summary>
public enum SlkRole
{
    /// <summary>
    /// No role
    /// </summary>
    None = 0,
    
    /// <summary>
    /// The user is an instructor on the Web site or assignment.
    /// </summary>
    Instructor = 1,

    /// <summary>
    /// The user is a learner on the Web site or assignment.
    /// </summary>
    Learner = 2,

    /// <summary>
    /// The user is an observer on the Web site or assignment.
    /// </summary>
    Observer = 3,
}

}

