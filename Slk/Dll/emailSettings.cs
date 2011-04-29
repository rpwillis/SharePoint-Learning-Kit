using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.SharePointLearningKit
{
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

