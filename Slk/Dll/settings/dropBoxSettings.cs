using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The location of the drop box.</summary>
    [Flags]
    public enum DropBoxLocation
    {
        /// <summary>The drop box is in the site assigned to.</summary>
        InSite = 1,
        /// <summary>The drop box is in a subsite of the site assigned to.</summary>
        SubSite = 2,
        /// <summary>The drop box is in a fixed specified site.</summary>
        FixedExisting = 4,
        /// <summary>The drop box is in an existing subsite of the site assigned to. i.e. do not create if not present</summary>
        ExistingSubSite = 6
    }

    /// <summary>The Drop Box settings</summary>
    public class DropBoxSettings
    {
#region constructors
        internal DropBoxSettings()
        {
        }

        /// <summary>Initializes a new instance of <see cref="DropBoxSettings"/>.</summary>
        /// <param name="reader">The XmlReader containing the setting details.</param>
        public DropBoxSettings(XmlReader reader)
        {
            // Default the location
            Location = DropBoxLocation.SubSite;

            string location = reader.GetAttribute("Location");

            if (string.IsNullOrEmpty(location) == false)
            {
                Location = (DropBoxLocation)Enum.Parse(typeof(DropBoxLocation), location, true);
            }

            Url = reader.GetAttribute("Url");

            UseOfficeWebApps = SlkSettings.BooleanAttribute(reader, "UseOfficeWebApps");
            OpenOfficeInIpadApp = SlkSettings.BooleanAttribute(reader, "OpenOfficeInIpadApp");
            OpenSubmittedInSameWindow = SlkSettings.BooleanAttribute(reader, "OpenSubmittedInSameWindow");
        }
#endregion constructors

#region properties
        /// <summary>The location of the drop box.</summary>
        public DropBoxLocation Location { get; private set; }

        /// <summary>Email details for a cancelled assignment.</summary>
        public string Url { get; private set; }

        /// <summary>Whether to use Office Web Apps for editing or not.</summary>
        public bool UseOfficeWebApps { get; private set; }

        /// <summary>Whether to open office documents in iPad apps if using an iPad.</summary>
        public bool OpenOfficeInIpadApp { get; private set; }

        /// <summary>Whether to open submitted files in same window.</summary>
        public bool OpenSubmittedInSameWindow { get; private set; }
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

