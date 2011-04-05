using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Microsoft.SharePointLearningKit.Localization
{
    /// <summary>
    /// Main contract for providing localization information to SLK resources
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Gets the current culture from the provider prespective
        /// </summary>
        CultureInfo CurrentCulture{ get; }

    }
}
