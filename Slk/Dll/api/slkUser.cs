using System;
using System.Globalization;
using Microsoft.SharePoint;
using System.Diagnostics;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents a SharePoint Learning Kit learner or instructor.
    /// </summary>
    [DebuggerDisplay("SlkUser {UserId.GetKey()}: {Name}")]
    public class SlkUser : IComparable<SlkUser>
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>UserId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        UserItemIdentifier m_userId;

        /// <summary>
        /// Holds the value of the <c>SPUser</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SPUser m_spUser;

        /// <summary>
        /// Holds the value of the <c>Name</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_name;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.
        /// </summary>
        public UserItemIdentifier UserId
        {
            [DebuggerStepThrough]
            get
            {
                return m_userId;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_userId = value;
            }
        }

        /// <summary>
        /// Gets the SharePoint <c>SPUser</c> object that represents this user.
        /// </summary>
        public SPUser SPUser
        {
            [DebuggerStepThrough]
            get { return m_spUser; }
            internal set { m_spUser = value ;}
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return m_name;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>
        /// Initializes an instance of this class, given a <c>UserItemIdentifier</c>.
        /// </summary>
        ///
        /// <param name="userId">The SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.
        ///     </param>
        ///
        /// <remarks>
        /// When this constructor is used, the <c>SPUser</c> and <c>Name</c> properties are
        /// <c>null</c>.
        /// </remarks>
        ///
        public SlkUser(UserItemIdentifier userId)
        {
            if(userId == null)
            {
                throw new ArgumentNullException("userId");
            }
                
            m_userId = userId;
        }

        // If this constructor is used the calling code is responsible for setting the userId.
        internal SlkUser(SPUser spUser)
        {
            m_spUser = spUser;
            m_name = spUser.Name;
        }

        internal SlkUser(UserItemIdentifier userId, string name) : this (userId)
        {
            m_name = name;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // IComparable<SlkUser> Implementation
        //

        int IComparable<SlkUser>.CompareTo(SlkUser other)
        {
            return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        internal static string Key(SPUser user)
        {
             if (String.IsNullOrEmpty(user.Sid))
             {
                 return user.LoginName;
             }
             else
             {
                 return user.Sid;
             }
        }
    }
}

