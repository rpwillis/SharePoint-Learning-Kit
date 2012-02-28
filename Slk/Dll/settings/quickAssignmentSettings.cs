using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The Quick Assignment settings</summary>
    public class QuickAssignmentSettings
    {
#region constructors
        internal QuickAssignmentSettings()
        {
            DefaultLibraries = new string[0];
        }

        /// <summary>Initializes a new instance of <see cref="QuickAssignmentSettings"/>.</summary>
        /// <param name="reader">The XmlReader containing the setting details.</param>
        public QuickAssignmentSettings(XmlReader reader)
        {
            string defaultLibrary = reader.GetAttribute("DefaultLibrary");

            if (string.IsNullOrEmpty(defaultLibrary))
            {
                DefaultLibraries = new string[0];
            }
            else
            {
                DefaultLibraries = defaultLibrary.Trim().Split(',');
            }
        }
#endregion constructors

#region properties
        /// <summary>Email details for a cancelled assignment.</summary>
        public string[] DefaultLibraries { get; private set; }
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

