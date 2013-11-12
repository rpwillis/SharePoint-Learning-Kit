using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// A delegate which resolves a macro name (used within a "MacroName" attribute of a
    /// "&lt;Condition&gt;" element in an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings") to a value.
    /// </summary>
    ///
    /// <param name="macroName">The name of the macro.</param>
    ///
    /// <returns>
    /// The value of the macro, or <c>null</c> if the macro is not defined.
    /// </returns>
    ///
    public delegate object MacroResolver(string macroName);

    /// <summary>
    /// A delegate which resolves an SPWeb GUID and an SPSite GUID into an SPWeb name and URL.
    /// </summary>
    ///
    /// <param name="spWebGuid">The GUID of the SPWeb.</param>
    ///
    /// <param name="spSiteGuid">The GUID of the SPSite containing the SPWeb.</param>
    ///
    /// <param name="webName">Where to store the name of the SPWeb, or <c>null</c> if the SPWeb
    ///     cannot be found.</param>
    ///
    /// <param name="webUrl">Where to store the URL of the SPWeb, or <c>null</c> if the SPWeb
    ///     cannot be found.</param>
    ///
    public delegate void SPWebResolver(Guid spWebGuid, Guid spSiteGuid, out string webName,
        out string webUrl);

}
