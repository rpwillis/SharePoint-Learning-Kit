using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The type of email.</summary>
    public enum EmailType
    {
        /// <summary>A new assignment.</summary>
        New,
        /// <summary>The assignment is canceled.</summary>
        Cancel,
        /// <summary>On sumitting an assignment.</summary>
        Submit,
        /// <summary>On reactivating assignment.</summary>
        Reactivate,
        /// <summary>On collecting assignment.</summary>
        Collect,
        /// <summary>A reminder for an assignment.</summary>
        Reminder
    }
    /// <summary>The Email settings</summary>
    public class EmailSettings
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="EmailSettings"/>.</summary>
        /// <param name="reader">The XmlReader containing the setting details.</param>
        public EmailSettings(XmlReader reader)
        {
            if (reader.IsEmptyElement == false)
            {
                reader.Read();

                while (reader.Name != "EmailSettings")
                {
                    switch (reader.Name)
                    {
                        case "NewAssignment":
                            NewAssignment = new EmailDetails(reader);
                            break;
                        case "CancelAssignment":
                            CancelAssignment = new EmailDetails(reader);
                            break;
                        case "SubmitAssignment":
                            SubmitAssignment = new EmailDetails(reader);
                            break;
                        case "ReactivateAssignment":
                            ReactivateAssignment = new EmailDetails(reader);
                            break;
                        case "CollectAssignment":
                            CollectAssignment = new EmailDetails(reader);
                            break;
                        case "AssignmentReminder":
                            AssignmentReminder = new EmailDetails(reader);
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
            }

            // Do not Move off end element
        }
#endregion constructors

#region properties
        /// <summary>Email details for a new assignment.</summary>
        public EmailDetails NewAssignment { get; private set; }

        /// <summary>Email details for a cancelled assignment.</summary>
        public EmailDetails CancelAssignment { get; private set; }

        /// <summary>Email details for a submitted assignment.</summary>
        public EmailDetails SubmitAssignment { get; private set; }

        /// <summary>Email details for a collected assignment.</summary>
        public EmailDetails CollectAssignment { get; private set; }

        /// <summary>Email details for a reactivated assignment.</summary>
        public EmailDetails ReactivateAssignment { get; private set; }

        /// <summary>Email details for a assignment reminder.</summary>
        public EmailDetails AssignmentReminder { get; private set; }

#endregion properties

#region public methods
        /// <summary>The details for a particular type.</summary>
        /// <param name="type">The type of email</param>
        /// <returns></returns>
        public EmailDetails this[EmailType type]
        {
            get
            {
                switch (type)
                {
                    case EmailType.New:
                        return NewAssignment;
                    case EmailType.Cancel:
                        return CancelAssignment;
                    case EmailType.Submit:
                        return SubmitAssignment;
                    case EmailType.Reactivate:
                        return ReactivateAssignment;
                    case EmailType.Collect:
                        return CollectAssignment;
                    case EmailType.Reminder:
                        return AssignmentReminder;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }

    /// <summary>The Email details</summary>
    public class EmailDetails
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="EmailDetails"/>.</summary>
        /// <param name="reader">The XmlReader containing the setting details.</param>
        public EmailDetails(XmlReader reader)
        {
            Subject = reader.GetAttribute("Subject");

            reader.Read();
            reader.MoveToContent(); // Move to Body tag
            Body = reader.ReadInnerXml();
            reader.MoveToContent(); // Move to end tag
            reader.Read();
            reader.MoveToContent(); // Move to next tag
        }
#endregion constructors

#region properties
        /// <summary>The email's Subject.</summary>
        public string Subject { get; private set; }

        /// <summary>The email's body.</summary>
        public string Body { get; private set; }

#endregion properties

#region public methods
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }

}

