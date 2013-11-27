using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Uses SPUtility to send emails.</summary>
    sealed class AssignmentEmailer : IDisposable
    {
        List<Email> cachedEmails = new List<Email>();
        AssignmentProperties assignment;
        IEmailSender emailSender;
        SPSite site;
        SPWeb web;
        EmailSettings settings;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentEmailer"/>.</summary>
        /// <param name="assignment">The assignment the emails are for.</param>
        /// <param name="settings">The email settings to use.</param>
        /// <param name="siteId">The id of the site.</param>
        /// <param name="webId">The id of the web.</param>
        /// <param name="emailSender">The <see cref="IEmailSender"/> to use.</param>
        public AssignmentEmailer(AssignmentProperties assignment, EmailSettings settings, Guid siteId, Guid webId, IEmailSender emailSender) : this(assignment, settings)
        {
            site = new SPSite(siteId);
            web = site.OpenWeb(webId);
            InitializeEmailSender(emailSender);
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentEmailer"/>.</summary>
        /// <param name="assignment">The assignment the emails are for.</param>
        /// <param name="settings">The email settings to use.</param>
        /// <param name="siteId">The id of the site.</param>
        /// <param name="webId">The id of the web.</param>
        public AssignmentEmailer(AssignmentProperties assignment, EmailSettings settings, Guid siteId, Guid webId) : this(assignment, settings, siteId, webId, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentEmailer"/>.</summary>
        /// <param name="assignment">The assignment the emails are for.</param>
        /// <param name="settings">The email settings to use.</param>
        /// <param name="web">The SPWeb for the assignment.</param>
        public AssignmentEmailer(AssignmentProperties assignment, EmailSettings settings, SPWeb web) : this(assignment, settings, web, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentEmailer"/>.</summary>
        /// <param name="assignment">The assignment the emails are for.</param>
        /// <param name="settings">The email settings to use.</param>
        /// <param name="web">The SPWeb for the assignment.</param>
        /// <param name="emailSender">The <see cref="IEmailSender"/> to use.</param>
        public AssignmentEmailer(AssignmentProperties assignment, EmailSettings settings, SPWeb web, IEmailSender emailSender) : this(assignment, settings)
        {
            this.web = web;
            InitializeEmailSender(emailSender);
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentEmailer"/>.</summary>
        /// <param name="assignment">The assignment the emails are for.</param>
        /// <param name="settings">The email settings to use.</param>
        public AssignmentEmailer(AssignmentProperties assignment, EmailSettings settings)
        {
            this.settings = settings;
            this.assignment = assignment;
        }
#endregion constructors

#region properties
        /// <summary>See <see cref="IEmailSender.CacheEmails"/>.</summary>
        public bool CacheEmails
        {
            get { return emailSender.CacheEmails ;}
            set { emailSender.CacheEmails = value ;}
        }
#endregion properties

#region public methods
        /// <summary>Sends and email when a learner submits an assignment.</summary>
        /// <param name="name">The name of the learner.</param>
        public void SendSubmitEmail(string name)
        {
            SendEmail(assignment.Instructors, SubmitSubjectText(name), SubmitBodyText(name));
        }

        /// <summary>Sends an email when an assignment is reactivated.</summary>
        /// <param name="learner">The learner being reactivated.</param>
        public void SendReactivateEmail(SlkUser learner)
        {
            SendEmail(learner, ReactivateSubjectText(), ReactivateBodyText());
        }

        /// <summary>Sends an email when an assignment is returned.</summary>
        /// <param name="learner">The learner being returned.</param>
        public void SendReturnEmail(SlkUser learner)
        {
            SendEmail(learner, ReturnSubjectText(), ReturnBodyText());
        }

        /// <summary>Sends an email when an assignment is collected.</summary>
        /// <param name="learner">The learner being collected.</param>
        public void SendCollectEmail(SlkUser learner)
        {
            SendEmail(learner, CollectSubjectText(), CollectBodyText());
        }

        /// <summary>Sends an email to a group of users.</summary>
        /// <param name="users">The users to send to.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="body">The email body.</param>
        public void SendEmail(IEnumerable<SlkUser> users, string subject, string body)
        {
            foreach (SlkUser user in users)
            {
                SendEmail(user, subject, body);
            }
        }

        /// <summary>Sends an email to a group of users.</summary>
        /// <param name="user">The user to send to.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="body">The email body.</param>
        public void SendEmail(SlkUser user, string subject, string body)
        {
            if (user.SPUser != null && string.IsNullOrEmpty(user.SPUser.Email) == false)
            {
                emailSender.SendEmail(user.SPUser.Email, subject, UserEmailText(body, user));
            }
        }

        /// <summary>Clears cached emails without sending them.</summary>
        public void ClearCachedEmails()
        {
            emailSender.ClearCachedEmails();
        }

        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            emailSender.SendCachedEmails();

            if (site != null)
            {
                web.Dispose();
                site.Dispose();
            }
        }

        /// <summary>Sends reminder emails.</summary>
        /// <param name="users">The users to send to.</param>
        public void SendReminderEmail(IEnumerable<SlkUser> users)
        {
            SendEmail(users, ReminderSubjectText(), ReminderBodyText());
        }

        /// <summary>Sends cancel emails.</summary>
        /// <param name="users">The users to send to.</param>
        public void SendCancelEmail(IEnumerable<SlkUser> users)
        {
            SendEmail(users, CancelSubjectText(), CancelBodyText());
        }

        /// <summary>Sends new assignment emails.</summary>
        /// <param name="users">The users to send to.</param>
        public void SendNewEmail(IEnumerable<SlkUser> users)
        {
            SendEmail(users, NewSubjectText(), NewBodyText());
        }
#endregion public methods

#region private methods
        private void InitializeEmailSender(IEmailSender emailSender)
        {
            if (emailSender == null)
            {
                this.emailSender = new SharePointEmailer(web);
            }
            else
            {
                this.emailSender = emailSender;
            }
        }

        private string UserEmailText(string baseText, SlkUser user)
        {
            string text = baseText;

            if (text.Contains("%url%"))
            {
                string url = "{0}{1}Lobby.aspx?LearnerAssignmentId={2}";
                url = string.Format(CultureInfo.InvariantCulture, url, web.Url, Constants.SlkUrlPath, user.AssignmentUserGuidId);
                text = text.Replace("%url%", url);
            }

            if (text.Contains("%gradingUrl%"))
            {
                string url = "{0}{1}grading.aspx?AssignmentId={2}";
                url = string.Format(CultureInfo.InvariantCulture, url, web.Url, Constants.SlkUrlPath, assignment.Id.GetKey());
                text = text.Replace("%gradingUrl%", url);
            }

            return text;
        }

        EmailDetails EmailDetails(EmailType type)
        {
            if (settings != null)
            {
                return settings[type];
            }
            else
            {
                return null;
            }
        }

#endregion private methods

#region subject and body text
        string SubmitSubjectText(string name)
        {
            string subject = null;
            if (settings != null && settings.SubmitAssignment != null)
            {
                subject = settings.SubmitAssignment.Subject;
            }
            else
            {
                subject = AppResources.SubmitAssignmentEmailDefaultSubject;
            }

            return EmailText(subject, name);
        }

        string BodyText(EmailType type, string defaultText)
        {
            string body = null;

            EmailDetails details = EmailDetails(type);

            if (details != null)
            {
                body = details.Body;
            }
            else
            {
                body = defaultText;
            }

            return EmailText(body);
        }

        string SubjectText(EmailType type, string defaultText)
        {
            string subject = null;
            EmailDetails details = EmailDetails(type);

            if (details != null)
            {
                subject = details.Subject;
            }
            else
            {
                subject = defaultText;
            }

            return EmailText(subject);
        }

        string ReturnSubjectText()
        {
            return SubjectText(EmailType.Return, AppResources.ReturnAssignmentEmailDefaultSubject);
        }

        string ReturnBodyText()
        {
            return BodyText(EmailType.Return, AppResources.ReturnAssignmentEmailDefaultBody);
        }

        string ReactivateSubjectText()
        {
            return SubjectText(EmailType.Reactivate, AppResources.ReactivateAssignmentEmailDefaultSubject);
        }

        string ReactivateBodyText()
        {
            return BodyText(EmailType.Reactivate, AppResources.ReactivateAssignmentEmailDefaultBody);
        }

        string CollectSubjectText()
        {
            return SubjectText(EmailType.Collect, AppResources.CollectAssignmentEmailDefaultSubject);
        }

        string CollectBodyText()
        {
            return BodyText(EmailType.Collect, AppResources.CollectAssignmentEmailDefaultBody);
        }

        string ReminderSubjectText()
        {
            return SubjectText(EmailType.Reminder, AppResources.AssignmentReminderEmailDefaultSubject);
        }

        string ReminderBodyText()
        {
            string body = BodyText(EmailType.Reminder, AppResources.AssignmentReminderEmailDefaultBody);
            if (assignment.DueDate != null)
            {
                body = body.Replace("%due%", assignment.DueDate.Value.ToString("f", web.Locale));
            }

            return body;
        }


        string CancelSubjectText()
        {
            return SubjectText(EmailType.Cancel, AppResources.CancelAssignmentEmailDefaultSubject);
        }

        string CancelBodyText()
        {
            return BodyText(EmailType.Cancel, AppResources.CancelAssignmentEmailDefaultBody);
        }

        string SubmitBodyText(string name)
        {
            string body = null;
            if (settings != null && settings.SubmitAssignment != null)
            {
                body = settings.SubmitAssignment.Body;
            }
            else
            {
                body = AppResources.SubmitAssignmentEmailDefaultBody;
            }

            return EmailText(body, name);
        }

        string NewSubjectText()
        {
            string subject = null;
            if (settings != null && settings.NewAssignment != null)
            {
                subject = settings.NewAssignment.Subject;
            }
            else
            {
                subject = AppResources.NewAssignmentEmailDefaultSubject;
            }

            return EmailText(subject);
        }

        string NewBodyText()
        {
            string body = null;
            if (settings != null && settings.NewAssignment != null)
            {
                body = settings.NewAssignment.Body;
            }
            else
            {
                body = AppResources.NewAssignmentEmailDefaultBody;
            }

            return EmailText(body);
        }

        string EmailText(string baseText)
        {
            return EmailText(baseText, null);
        }

        string EmailText(string baseText, string name)
        {
            string toReturn = baseText.Replace("%title%", assignment.Title);
            toReturn = toReturn.Replace("%description%", assignment.Description);

            if (string.IsNullOrEmpty(name) == false)
            {
                toReturn = toReturn.Replace("%name%", name);
            }

            return toReturn;
        }

#endregion subject and body text

#region Email class
        class Email
        {
            public string Address { get; private set; }
            public string Subject { get; private set; }
            public string Body { get; private set; }

            public Email(string address, string subject, string body)
            {
                Address = address;
                Subject = subject;
                Body = body;
            }
        }
#endregion Email class
    }
}

