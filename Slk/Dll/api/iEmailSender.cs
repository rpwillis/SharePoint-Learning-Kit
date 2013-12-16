using System;
using System.Globalization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>An interface for sending emails.</summary>
    public interface IEmailSender
    {
        /// <summary>Indicates whether to cache emails to send later.</summary>
        /// <value></value>
        bool CacheEmails { get; set; }

        /// <summary>Sends an email address.</summary>
        /// <param name="emailAddress">The address to send to.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        void SendEmail(string emailAddress, string subject, string body);

        /// <summary>Sends all cached emails.</summary>
        void SendCachedEmails();

        /// <summary>Clears cached emails without sending them.</summary>
        void ClearCachedEmails();
    }
}

