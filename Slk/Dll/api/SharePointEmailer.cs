using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePointLearningKit.WebControls;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Uses SPUtility to send emails.</summary>
    class SharePointEmailer : IEmailSender
    {
        List<Email> cachedEmails = new List<Email>();
        SPWeb web;
        ISlkStore store;

#region constructors
        /// <summary>Initializes a new instance of <see cref="SharePointEmailer"/>.</summary>
        /// <param name="store">The ISlkStore to log errors to.</param>
        /// <param name="web">The <see cref="SPWeb"/> to use.</param>
        public SharePointEmailer(ISlkStore store, SPWeb web)
        {
            this.store = store;
            this.web = web;
        }
#endregion constructors

#region properties
        /// <summary>See <see cref="IEmailSender.CacheEmails"/>.</summary>
        public bool CacheEmails { get; set; }
#endregion properties

#region public methods
        /// <summary>See <see cref="IEmailSender.SendEmail"/>.</summary>
        public void SendEmail(string emailAddress, string subject, string body)
        {
            if (CacheEmails)
            {
                cachedEmails.Add(new Email(emailAddress, subject, body));
            }
            else
            {
                SendEmailNow(emailAddress, subject, body);
            }
        }

        /// <summary>See <see cref="IEmailSender.SendCachedEmails"/>.</summary>
        public void SendCachedEmails()
        {
            foreach (Email email in cachedEmails)
            {
                SendEmailNow(email.Address, email.Subject, email.Body);
            }
        }

        /// <summary>See <see cref="IEmailSender.ClearCachedEmails"/>.</summary>
        public void ClearCachedEmails()
        {
            cachedEmails = new List<Email>();
        }
#endregion public methods

#region private methods
        private void SendEmailNow(string emailAddress, string subject, string body)
        {
            try
            {
                SPUtility.SendEmail(web, false, false, emailAddress, subject, body);
            }
            catch (Exception e)
            {
                SlkError.WriteException(store, e);
                throw;
            }
        }
#endregion private methods

#region Email
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
#endregion Email
    }
}

