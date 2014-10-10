using System;
using System.Globalization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// A class which resolves a macro name (used within a "MacroName" attribute of a
    /// "&lt;Condition&gt;" element in an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings") to a value.
    /// </summary>
    public class MacroResolver
    {
        ISlkStore store;
        Guid[] webIds;

#region constructors
        /// <summary>Initializes a new instance of <see cref="MacroResolver"/>.</summary>
        /// <param name="store">The current Slk store.</param>
        /// <param name="webIds">A list of SPWeb ids to use.</param>
        public MacroResolver(ISlkStore store, Guid[] webIds)
        {
            this.store = store;
            this.webIds = webIds;
        }
#endregion constructors

#region properties
#endregion properties

#region public methods
        /// <summary>Resolves the macro.</summary>
        /// <param name="macroName">The name of the macro.</param>
        /// <returns>The value of the macro, or <c>null</c> if the macro is not defined.</returns>
        public object Resolve(string macroName)
        {
            switch (macroName)
            {
                case "SPWebScope":
                    // return the GUID of the SPWeb that query results will be limited to (i.e.
                    // filtered by), or null for no filter
                    return webIds;

                case "CurrentUserKey":
                    // return the LearningStore user key string value of the current user
                    return store.CurrentUserKey;

                case "Now":
                    return DateTime.Now.ToUniversalTime();

                case "StartOfToday":
                    // return midnight of today
                    return DateTime.Today.ToUniversalTime();

                case "StartOfTomorrow":
                    // return midnight of tomorrow
                    return DateTime.Today.AddDays(1).ToUniversalTime();

                case "StartOfThisWeek":
                    // return midnight of the preceding Sunday** (or "Today" if "Today" is Sunday**)
                    // ** Actually, it's only Sunday for regional setting for which Sunday is the first
                    // day of the week.  For example, using Icelandic regional settings, the first day of
                    // the week is Monday, and that's what's used below.
                    return StartOfWeek(DateTime.Today).ToUniversalTime();

                case "StartOfNextWeek":
                    // return midnight of the following Sunday**
                    return StartOfWeek(DateTime.Today).AddDays(7).ToUniversalTime();

                case "StartOfWeekAfterNext":
                    // return midnight of the Sunday** after the following Sunday**
                    return StartOfWeek(DateTime.Today).AddDays(14).ToUniversalTime();

                default:
                    return null;

            }
        }

        /// <summary>
        /// Returns midnight on the day that begins the week containing a given date/time, using the
        /// current culture settings.
        /// </summary>
        ///
        /// <param name="dateTime">The given date/time.</param>
        ///
        public static DateTime StartOfWeek(DateTime dateTime)
        {
            // set <cultureInfo> to information about the current user's culture
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            // this method imagines that today is the day of <dateTime>
            DateTime today = dateTime.Date;
            DayOfWeek currentDayOfWeek = today.DayOfWeek;
            DayOfWeek firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            int delta = (int) firstDayOfWeek - (int) currentDayOfWeek;
            if (delta <= 0)
                return today.AddDays(delta);
            else
                return today.AddDays(delta - 7);
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

