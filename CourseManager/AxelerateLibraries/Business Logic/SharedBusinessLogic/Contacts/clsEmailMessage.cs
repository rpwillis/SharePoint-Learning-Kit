using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.Contacts;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;


namespace Axelerate.BusinessLogic.SharedBusinessLogic.Contacts
{
    /// <summary>
    /// Represents an automatic Email message that will be sent automatically by the application
    /// </summary>
    [Serializable(), SecurityToken("clsEmailMessage", "clsEmailMessages", "MIPCustom")]
    public class clsEmailMessage : GUIDNameBusinessTemplate<clsEmailMessage>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsEmailMessage), "EmailMessages", "_emg", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Email Message's subject
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Subject = "";

        /// <summary>
        /// Email Message's body
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Body = "";

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or Sets the Email Message's Subject
        /// </summary>
        public string Subject
        {
            get { return m_Subject; }
            set
            {
                m_Subject = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Email Message's Body
        /// </summary>
        public string Body
        {
            get { return m_Body; }
            set
            {
                m_Body = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Sends this email message to the desired recipient.  This version of the SendMail message is used
        /// to send an email message that has email request actions associated to it.  Each email request action will be 
        /// added at the end of the body of the message.  The recipients of the message will be able to follow the actions
        /// by clicking on the automatically generated links.
        /// </summary>
        /// <param name="pObjectGUID">GUID of the action's object where the action request will be executed.  The object type for 
        /// this object is specified in the EMail Action request </param>
        /// <param name="pConfirmationGUID">Confirmation GUID that allows the action object to verify that the request is valid</param>
        /// <param name="pRecipient">Comma separated email addresses of the recipients for thsi EMail </param>
        public void SendMail(string pObjectGUID, string pConfirmationGUID, string pRecipient)
        {

            if (pRecipient != "")
            {

                try
                {
                    MailMessage Mail = new MailMessage();
                    try
                    {
                        string Mail_From = clsConfigurationProfile.Current.getPropertyValue("Mail_From");
                        Mail.From = new MailAddress(Mail_From);
                        Mail.To.Add(pRecipient);
                        Mail.Subject = Subject;
                        Mail.Body = AddRequestActionLinks(pObjectGUID, pConfirmationGUID);
                        Mail.IsBodyHtml = true;
                        Mail.Priority = MailPriority.Normal;
                        SendMailMessage(Mail);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch(Exception ex)
                { 
                }
            }
        }

        /// <summary>
        /// Sends this email message to the desired recipient.  This version of the SendMail message is used
        /// to send an email message that has no email request actions associated to it. 
        /// </summary>
        /// <param name="pRecipient">Comma separated email addresses of the recipients for thsi EMail </param>
        public void SendMail(string pRecipient)
        {
            if (pRecipient != "")
            {
                try
                {
                    MailMessage Mail = new MailMessage();
                    try
                    {

                        Mail.From = new MailAddress(clsConfigurationProfile.Current.getPropertyValue("Mail_From"));
                        Mail.To.Add(pRecipient);
                        Mail.Subject = Subject;
                        Mail.IsBodyHtml = true;
                        Mail.Body = Body;
                        Mail.Priority = MailPriority.Normal;
                        SendMailMessage(Mail);
                    }
                    catch (Exception ex)
                    {
                        clsLog.Trace(Resources.ErrorMessages.errSendingEmail + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    clsLog.Trace(Resources.ErrorMessages.errPreparingEmail + ex.Message);
                }
            }
        }


        /// <summary>
        /// Returns the SmtpClient associated with the portal
        /// </summary>
        /// <returns>SmtpClient object</returns>
        private SmtpClient GetSmtp()
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = clsConfigurationProfile.Current.getPropertyValue("Smtp_Host");
            smtp.Port = int.Parse(clsConfigurationProfile.Current.getPropertyValue("Smtp_Port"));
            String Credential_UserName = clsConfigurationProfile.Current.getPropertyValue("Smtp_Credentials_UserName");
            String Credential_Password = clsConfigurationProfile.Current.getPropertyValue("Smtp_Credentials_Password");
            smtp.Credentials = new System.Net.NetworkCredential(Credential_UserName, Credential_Password);
            smtp.EnableSsl = bool.Parse(clsConfigurationProfile.Current.getPropertyValue("Smtp_EnableSsl"));
            return smtp;
        }

        /// <summary>
        /// Adds the action request links to the end of the Email's body. 
        /// </summary>
        /// <param name="pObjectGUID">GUID of the action's object where the action request will be executed.  The object type for 
        /// this object is specified in the EMail Action request</param>
        /// <param name="pConfirmationGUID">Confirmation GUID that allows the action object to verify that the request is valid</param>
        /// <returns>An string that concatenates the request links with the message body</returns>
        private string AddRequestActionLinks(string pObjectGUID,string pConfirmationGUID)
        {
            string Enter = "<br>";
            string URL = clsConfigurationProfile.Current.getPropertyValue("ActionRequestURL");
            string EmailBody = Body + Enter + Enter;

            BLCriteria Criteria = new BLCriteria(typeof(clsEmailActionRequest));
            Criteria.AddBinaryExpression("MasterGUID_ear", "MasterGUID", "=", GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            clsEmailActionRequests EmailActionRequests = clsEmailActionRequests.GetCollection(Criteria);
            foreach (clsEmailActionRequest EmailActionRequest in EmailActionRequests)
            {
                string Parameters = "?" + "ObjectGUID" + "=" + pObjectGUID + "&" +
                    "ActionGUID" + "=" + EmailActionRequest.GUID + "&" + "ConfirmationGUID" + "=" + pConfirmationGUID;
                string URLParameters = URL + Parameters;
                   string Link = "<a href=" + URLParameters + ">" + EmailActionRequest.Message + "</a>";
                EmailBody = EmailBody + Link + Enter + Enter;
            }
            return EmailBody;
        }

        #region "Exchange Code"
        class SendMessageWebDAV
        {
            public static void SendMail(MailMessage Mail)
            {
                System.Net.HttpWebRequest PUTRequest;
                System.Net.WebResponse PUTResponse;
                System.Net.HttpWebRequest MOVERequest;
                System.Net.WebResponse MOVEResponse;
                System.Net.CredentialCache MyCredentialCache;
                string strMailboxURI = "";
                string strSubURI = "";
                string strTempURI = "";
                string strServer = clsConfigurationProfile.Current.getPropertyValue("Exchange_ServerName");
                string strDomain = clsConfigurationProfile.Current.getPropertyValue("Exchange_Domain");
                string strAlias = clsConfigurationProfile.Current.getPropertyValue("Exchange_SenderAlias");
                string strPassword = clsConfigurationProfile.Current.getPropertyValue("Exchange_SenderPassword");
                string strTo = "";
                foreach (MailAddress Recipient in Mail.To)
                {
                    strTo = strTo + Recipient.Address + ";";
                }
                
                string strSubject = Mail.Subject;
                string strBody = Mail.Body;
                byte[] bytes = null;
                System.IO.Stream PUTRequestStream = null;

                try
                {
                    // Build the mailbox URI.
                    strMailboxURI = "http://" + strServer + "/exchange/" + strAlias;

                    // Build the submission URI for the message.  If Secure
                    // Sockets Layer (SSL) is set up on the server, use
                    // "https://" instead of "http://".
                    strSubURI = "http://" + strServer + "/exchange/" + strAlias
                              + "/##DavMailSubmissionURI##/";

                    // Build the temporary URI for the message. If SSL is set
                    // up on the server, use "https://" instead of "http://".
                    strTempURI = "http://" + strServer + "/exchange/" + strAlias
                               + "/drafts/" + strSubject + ".eml";

                    // Construct the RFC 822 formatted body of the PUT request.
                    // Note: If the From: header is included here,
                    // the MOVE method request will return a
                    // 403 (Forbidden) status. The From address will
                    // be generated by the Exchange server.
                    strBody = "To: " + strTo + "\n" +
                    "Subject: " + strSubject + "\n" +
                    "Date: " + System.DateTime.Now +
                    "X-Mailer: test mailer" + "\n" +
                    "MIME-Version: 1.0" + "\n" +
                    "Content-Type: text/html;" + "\n" +
                    "Charset = \"iso-8859-1\"" + "\n" +
                    "Content-Transfer-Encoding: 7bit" + "\n" +
                    "\n" + strBody;

                    // Create a new CredentialCache object and fill it with the network
                    // credentials required to access the server.
                    MyCredentialCache = new System.Net.CredentialCache();
                    MyCredentialCache.Add(new System.Uri(strMailboxURI),
                      "NTLM",
                       new System.Net.NetworkCredential(strAlias, strPassword, strDomain)
                       );

                    // Create the HttpWebRequest object.

                    PUTRequest = (System.Net.HttpWebRequest) System.Net.HttpWebRequest.Create(strTempURI);

                    // Add the network credentials to the request.
                    PUTRequest.Credentials = MyCredentialCache;

                    // Specify the PUT method.
                    PUTRequest.Method = "PUT";

                    // Encode the body using UTF-8.
                    bytes = Encoding.UTF8.GetBytes((string)strBody);

                    // Set the content header length.  This must be
                    // done before writing data to the request stream.
                    PUTRequest.ContentLength = bytes.Length;

                    // Get a reference to the request stream.
                    PUTRequestStream = PUTRequest.GetRequestStream();

                    // Write the message body to the request stream.
                    PUTRequestStream.Write(bytes, 0, bytes.Length);

                    // Close the Stream object to release the connection
                    // for further use.
                    PUTRequestStream.Close();

                    // Set the Content-Type header to the RFC 822 message format.
                    PUTRequest.ContentType = "message/rfc822";

                    // PUT the message in the Drafts folder of the
                    // sender's mailbox.
                    PUTResponse = (System.Net.HttpWebResponse) PUTRequest.GetResponse();

                    // Create the HttpWebRequest object.
                    MOVERequest = (System.Net.HttpWebRequest) System.Net.HttpWebRequest.Create(strTempURI);

                    // Add the network credentials to the request.
                    MOVERequest.Credentials = MyCredentialCache;

                    // Specify the MOVE method.
                    MOVERequest.Method = "MOVE";

                    // Set the Destination header to the
                    // mail submission URI.
                    MOVERequest.Headers.Add("Destination", strSubURI);

                    // Send the message by moving it from the Drafts folder of the
                    // sender's mailbox to the mail submission URI.
                    MOVEResponse = (System.Net.HttpWebResponse)MOVERequest.GetResponse();

                    // Clean up.
                    PUTResponse.Close();
                    MOVEResponse.Close();

                }
                catch (Exception ex)
                {
                    // Catch any exceptions. Any error codes from the PUT
                    // or MOVE method requests on the server will be caught
                    // here, also.
                    clsLog.WriteLog("1", ex.Message);
                }
            }
        }


        #endregion


        private void SMTP_SendMailMessage(MailMessage Mail)
        {
            SmtpClient smtp = GetSmtp();
            smtp.Send(Mail);
        }

        private void Exchange_SendMailMessage(MailMessage Mail)
        {
            SendMessageWebDAV.SendMail(Mail);
        }

        private void SendMailMessage(MailMessage Mail)
        {
            if (clsConfigurationProfile.Current.getPropertyValue("Mail_Client") == "Exchange")
            {
                Exchange_SendMailMessage(Mail);
            }
            else
            {
                SmtpClient smtp = GetSmtp();
                smtp.Send(Mail);
            }
        }

    }


        #endregion






}