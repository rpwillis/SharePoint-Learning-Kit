/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
#region VerifierHelpers
    // a group of helper functions, mostly for SCORM 1.2
    internal static class VerifierHelpers
    {
        /// <summary>
        /// Validates a time (second,10,0) format used by SCORM 2004
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public static void ValidateTimeStampV1p3(string value)
        {
            string[] ValidTimeStampFormats = {"yyyy",
                                              "yyyy-MM",
                                              "yyyy-MM-dd",
                                              "yyyy-MM-ddTHH",
                                              "yyyy-MM-ddTHH:mm",
                                              "yyyy-MM-ddTHH:mm:ss",
                                              "yyyy-MM-ddTHH:mm:ss.f",
                                              "yyyy-MM-ddTHH:mm:ss.fzz",
                                              "yyyy-MM-ddTHH:mm:ss.fzzz",
                                              "yyyy-MM-ddTHH:mm:ss.fZ",
                                              "yyyy-MM-ddTHH:mm:ss.ff",
                                              "yyyy-MM-ddTHH:mm:ss.ffzz",
                                              "yyyy-MM-ddTHH:mm:ss.ffzzz",
                                              "yyyy-MM-ddTHH:mm:ss.ffZ"};

            // let DateTime.ParseExact throw an appropriate exception if the string
            // is not valid
            DateTime d = DateTime.ParseExact(value, ValidTimeStampFormats,
                                             CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            // check that the years are in SCORM valid range
            if(d.Year < 1970 || d.Year > 2038)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates a CMITime format used by SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public static void ValidateCMITime(string value)
        {
            string[] ValidTimeStampFormats = {"HH:mm:ss",
                                              "HH:mm:ss.f",
                                              "HH:mm:ss.ff"};

            // let DateTime.ParseExact throw an appropriate exception if the string
            // is not valid
            DateTime.ParseExact(value, ValidTimeStampFormats,
                                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        /// <summary>
        /// Validates a field defined as CMIFeedback.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <remarks>
        /// Since the actual valid contents of the string vary based on the 
        /// Interaction.InteractionType, it is not reliably possible to validate this 
        /// field for anything beyond length without getting into some questions that 
        /// we really don't want to answer (e.g. what should we do with these values 
        /// if the type has not yet been set, what should be done with them if the 
        /// type changes).
        /// </remarks>
        public static void ValidateCMIFeedback(string value)
        {
            if(value != null && value.Length > 255)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates a field defined as CMIIdentifier.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public static void ValidateCMIIdentifier(string value)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;;
            if(value != null)
            {
                if(value.Length > 255 || value.Length < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                foreach(char c in value)
                {
                    // only alphanumeric chars, dashes, and unserscores are valid
                    if(!Char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != ':')
                    {
                        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p2Value, "Id"));
                    }
                }
            }
        }
    }
#endregion VerifierHelpers
}
