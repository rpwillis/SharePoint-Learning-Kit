using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Properties;

namespace Microsoft.SharePointLearningKit.Administration
{
    /// <summary>A job to send reminder emails.</summary>
    public class ReminderEmailsJob : SPJobDefinition
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="CacheDataToFile"/>.</summary>
        public CacheDataToFile() : base()
        {
        }

        /// <summary>Initializes a new instance of <see cref="CacheDataToFile"/>.</summary>
        public CacheDataToFile(SPWebApplication application) : base(AppResources.ReminderEmailsJobTitle, application, null, SPJobLockType.None)
        {
        }
#endregion constructors

#region properties
        /// <summary>Indicates if it is an interactive job.</summary>
        public bool Interactive { get; set; }
#endregion properties

#region public methods
        /// <summary>See <see cref="SPJobDefinition.Execute"/>.</summary>
        public override void Execute(Guid targetInstanceId)
        {
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }
}

