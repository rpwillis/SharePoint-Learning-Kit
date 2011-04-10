using System;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Details of a package.</summary>
    public class PackageDetails
    {
        /// <summary>Where the returned <c>PackageItemIdentifier</c> is stored.</summary>
        public PackageItemIdentifier PackageId { get; set; }

        /// <summary>Where the returned warnings are stored.  This XML consists of
        ///     a root "&lt;Warnings&gt;" element containing one "&lt;Warning&gt;" element per
        ///     warning, each of which contains the text of the warning as the content of the element
        ///     plus the following attributes: the "Code" attribute contains the warning's
        ///     <c>ValidationResultCode</c>, and the "Type" attribute contains the warning's
        ///     type, either "Error" or "Warning".  Warnings is set to <c>null</c>
        ///     if there are no warnings.</summary>
        public LearningStoreXml Warnings { get; set; }
    }
}
