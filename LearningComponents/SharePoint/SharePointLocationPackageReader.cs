using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.Xml.XPath;
using System.Security.Principal;
using Microsoft.SharePoint;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security;
using System.Security.Policy;
using System.Web;
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>A base class for SharePoint package readers.</summary>
    public abstract class SharePointLocationPackageReader : PackageReader
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="SharePointLocationPackageReader"/>.</summary>
        /// <param name="packageLocation">The location of the package.</param>
        public SharePointLocationPackageReader(SharePointFileLocation packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("packageLocation", packageLocation);
            Location = new SharePointFileLocation(packageLocation);
        }
#endregion constructors

#region properties
        /// <summary>See <see cref="PackageReader.UniqueLocation"/>.</summary>
        public override string UniqueLocation
        {
            get {  return Location.ToString() ;}
        }

        /// <summary>The location of the package.</summary>
        public SharePointFileLocation Location { get; private set; }
#endregion properties

    }
}
