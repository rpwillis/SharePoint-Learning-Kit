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
    /// <summary>
    /// Represents a reader that can access packages stored in SharePoint and are permanently cached. 
    /// </summary>
    public class PermanentCacheSharePointPackageReader : FileSystemBasedSharePointPackageReader
    {
#region constructors
        /// <summary>
        /// Creates a package reader to read the specified package from SharePoint. The package 
        /// must be valid e-learning content.The package 
        /// is read using the current user's credentials.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In addition to the exceptions listed below, this method may throw exceptions caused by the 
        /// identity not having access to the <paramref name="cacheSettings"/> CachePath location.
        /// </para>
        /// <para>
        /// The contents of the package are not read in the constructor.  The contents of the package are read
        /// only once when they are first needed.  If the referenced SharePoint file does not contain a 
        /// valid e-learning package, accessing other methods and properties on this object will result 
        /// in an <c>InvalidPackageException</c>.
        /// </para>
        /// </remarks>
        /// <param name="cacheDirectory"></param>
        /// <param name="packageLocation">The location of the package to be read. Any changes to this SharePointFileLocation
        /// object after the PermanentCacheSharePointPackageReader is created are not reflected in the behavior of this object.</param>
        public PermanentCacheSharePointPackageReader(DirectoryInfo cacheDirectory, SharePointFileLocation packageLocation) : base(packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("cacheDirectory", cacheDirectory);
            Utilities.ValidateParameterNonNull("packageLocation", packageLocation);
           
            Initialize(cacheDirectory, ImpersonationBehavior.UseOriginalIdentity);
        }
#endregion constructors

#region properties
#endregion properties

    }
}
