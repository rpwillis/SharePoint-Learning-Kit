using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A class for saving assignments.</summary>
    public sealed class AssignmentSaver : IDisposable
    {
        List<DropBoxUpdate> cachedDropBoxUpdates = new List<DropBoxUpdate>();
        ISlkStore store;
        AssignmentEmailer emailSender;
        AssignmentProperties properties;
        DropBoxManager dropBoxManager;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentSaver"/>.</summary>
        /// <param name="properties">The <see cref="AssignmentProperties"/> to save.</param>
        public AssignmentSaver(AssignmentProperties properties)
        {
            this.properties = properties;
            emailSender = new AssignmentEmailer(properties, properties.Store.Settings.EmailSettings, properties.SPSiteGuid, properties.SPWebGuid);
            emailSender.CacheEmails = true;
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentSaver"/>.</summary>
        /// <param name="store">The <see cref="ISlkStore"/> to use.</param>
        /// <param name="properties">The <see cref="AssignmentProperties"/> to save.</param>
        public AssignmentSaver(ISlkStore store, AssignmentProperties properties) : this(properties)
        {
            this.store = store;
            CurrentJob = store.CreateCurrentJob();
        }
#endregion constructors

#region properties
        /// <summary>The current job.</summary>
        public ICurrentJob CurrentJob { get; private set; }
#endregion properties

#region public methods
        /// <summary>Completes the batch for saving results.</summary>
        public void Save()
        {
            CurrentJob.Complete();
            UpdateCachedDropBox();
        }

        /// <summary>Error action on result saving.</summary>
        public void Cancel()
        {
            emailSender.ClearCachedEmails();
            CurrentJob.Cancel();
        }

        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            if (CurrentJob != null)
            {
                CurrentJob.Dispose();
            }

            if (emailSender != null)
            {
                emailSender.Dispose();
            }
        }

        /// <summary>Updates the drop box permissions.</summary>
        /// <param name="state">The state the assignment is moving to.</param>
        /// <param name="user">The SLK user.</param>
        public void UpdateDropBoxPermissions(LearnerAssignmentState state, SlkUser user)
        {
            if (properties.IsNonELearning)
            {
                if (store != null)
                {
                    cachedDropBoxUpdates.Add(new DropBoxUpdate(state, user.SPUser));
                }
                else
                {
                    UpdateDropBoxPermissionsNow(state, user.SPUser);
                }
            }
        }

        /// <summary>Sends an email when an assignment is collected.</summary>
        /// <param name="learner">The learner being collected.</param>
        public void SendCollectEmail(SlkUser learner)
        {
            if (properties.EmailChanges)
            {
                emailSender.SendCollectEmail(learner);
            }
        }

        /// <summary>Sends an email when an assignment is reactivated.</summary>
        /// <param name="learner">The learner being reactivated.</param>
        public void SendReactivateEmail(SlkUser learner)
        {
            if (properties.EmailChanges)
            {
                emailSender.SendReactivateEmail(learner);
            }
        }

        /// <summary>Sends an email when an assignment is returned.</summary>
        /// <param name="learner">The learner being returned.</param>
        public void SendReturnEmail(SlkUser learner)
        {
            if (properties.EmailChanges)
            {
                emailSender.SendReturnEmail(learner);
            }
        }

#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        /// <summary>Updates the drop box permissions.</summary>
        /// <param name="state">The state the assignment is moving to.</param>
        /// <param name="user">The user.</param>
        void UpdateDropBoxPermissionsNow(LearnerAssignmentState state, SPUser user)
        {
            // If user is null, it's a deleted user so no need to update.
            if (user != null)
            {
                bool createdManager = false;
                if (dropBoxManager == null)
                {
                    dropBoxManager = new DropBoxManager(properties);
                    createdManager = true;
                }

                switch (state)
                {
                    case LearnerAssignmentState.Active:
                        // Reactivated
                        dropBoxManager.ApplyReactivateAssignmentPermission(user);
                        break;

                    case LearnerAssignmentState.Completed:
                        // Collected
                        dropBoxManager.ApplyCollectAssignmentPermissions(user);
                        break;

                    case LearnerAssignmentState.Final:
                        // Return
                        dropBoxManager.ApplyReturnAssignmentPermission(user);
                        break;
                }

                if (createdManager)
                {
                    dropBoxManager = null;
                }
            }
        }

        void UpdateCachedDropBox()
        {
            StringBuilder errors = new StringBuilder();
            dropBoxManager = new DropBoxManager(properties);
            foreach (DropBoxUpdate update in cachedDropBoxUpdates)
            {
                try
                {
                    UpdateDropBoxPermissionsNow(update.State, update.User);
                }
                catch (Exception e)
                {
                    properties.Store.LogException(e);
                    errors.AppendFormat(AppResources.ErrorSavingDropBoxPermissions, update.State, update.User.LoginName);
                }
            }

            if (errors.Length > 0)
            {
                throw new SafeToDisplayException(errors.ToString());
            }

            dropBoxManager = null;
        }

#endregion private methods

#region static members
#endregion static members

#region DropBoxUpdate
        class DropBoxUpdate
        {
            public LearnerAssignmentState State { get; private set; }
            public SPUser User { get; private set; }

            public DropBoxUpdate(LearnerAssignmentState state, SPUser user)
            {
                State = state;
                User = user;
            }
        }
#endregion DropBoxUpdate
    }
}

