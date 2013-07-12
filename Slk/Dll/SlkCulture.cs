using System;
using System.Globalization;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Handles localization of SLK.</summary>
    public class SlkCulture
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="SlkCulture"/>.</summary>
        public SlkCulture(CultureInfo culture)
        {
            Culture = culture;
            Resources = new AppResourcesLocal();
            Resources.Culture = Culture;
        }

        /// <summary>Initializes a new instance of <see cref="SlkCulture"/>.</summary>
        public SlkCulture() : this((SPWeb)null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SlkCulture"/>.</summary>
        /// <param name="web">The web to localize.</param>
        public SlkCulture(SPWeb web)
        {
            Resources = new AppResourcesLocal();
            Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            if (web == null && HttpContext.Current != null)
            {
                web = SPControl.GetContextWeb(HttpContext.Current);
            }

            if (web != null)
            {
#if SP2010
                if (web.IsMultilingual)
                {
                    // Just use the current UI culture as set above
                }
                else
                {
                    // Not a multi-lingual site so return the web's locale
                    Culture = web.Locale;
                }
#else
                Culture = web.Locale;
#endif
            }

            Resources.Culture = Culture;
            AppResources.Culture = Culture;
        }
#endregion constructors

#region properties
        /// <summary>The culture to use.</summary>
        /// <value></value>
        public CultureInfo Culture { get; private set; }

        internal AppResourcesLocal Resources { get; private set; }
#endregion properties

#region public methods
        /// <summary>Formats a string according to the SLK culture.</summary>
        public string Format(string format, params Object[] args)
        {
            return string.Format(Culture, format, args);
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
        /// <summary>Get the culture to use.</summary>
        /// <returns>A <see cref="CultureInfo"/>.</returns>
        public static CultureInfo GetCulture()
        {
            SlkCulture culture = new SlkCulture();
            return culture.Culture;
        }
#endregion static members
    }
}

