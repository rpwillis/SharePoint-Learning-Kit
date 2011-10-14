using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.IO;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.LearningComponents.Storage;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A file in an assignment.</summary>
    public struct AssignmentFile
    {
        string name;
        string url;

        /// <summary>The file's Url.</summary>
        public string Url
        {
            get { return url ;}
        }
        /// <summary>The file's Name.</summary>
        public string Name
        {
            get { return name ;}
        }
        /// <summary>Initializes a new instance of <see cref="AssignmentFile"/>.</summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="url">The file's url.</param>
        public AssignmentFile(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
}
