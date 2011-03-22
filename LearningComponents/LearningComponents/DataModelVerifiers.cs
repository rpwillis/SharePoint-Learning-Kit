/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents.DataModel
{
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
            Resources.Culture = LocalizationManager.GetCurrentCulture();
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

    #region DataModelVerifier

    internal abstract class DataModelVerifier
    {
        public abstract void ValidateCompletionStatus(CompletionStatus value);
        public abstract void ValidateEntry(EntryMode value);
        public abstract void ValidateLessonStatus(LessonStatus? value);
        public abstract void ValidateLocation(string value);
        public abstract void ValidateProgressMeasure(float? value);
        public abstract void ValidateSuccessStatus(SuccessStatus value);
        public abstract void ValidateSuspendData(string value);
        public abstract void ValidateTotalTime(TimeSpan value);
        public abstract void ValidateSessionTime(TimeSpan value);
        public abstract void ValidateEvaluationPoints(float? value);
    }
    internal class DataModelVerifierV1p3 : DataModelVerifier
    {
        /// <summary>
        /// Any valid float is valid.
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateEvaluationPoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            switch(value)
            {
            case CompletionStatus.Completed:
            case CompletionStatus.Incomplete:
            case CompletionStatus.NotAttempted:
            case CompletionStatus.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateEntry(EntryMode value)
        {
            switch(value)
            {
            case EntryMode.AbInitio:
            case EntryMode.AllOtherConditions:
            case EntryMode.Resume:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 2004.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLessonStatus(LessonStatus? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p2, "LessonStatus"));
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 1000.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLocation(string value)
        {
            Utilities.Assert(BaseSchemaInternal.ActivityAttemptItem.MaxLocationLength >= 1000, "DMV0010");
            if(value != null && value.Length > BaseSchemaInternal.ActivityAttemptItem.MaxLocationLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Valid values are 0.0 to 1.0
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateProgressMeasure(float? value)
        {
            if(value != null && (value < 0.0 || value > 1.0 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            switch(value)
            {
            case SuccessStatus.Failed:
            case SuccessStatus.Passed:
            case SuccessStatus.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 4000.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuspendData(string value)
        {
            Utilities.Assert(BaseSchemaInternal.ActivityAttemptItem.MaxSuspendDataLength >= 4000, "DMV0020");
            if(value != null && value.Length > BaseSchemaInternal.ActivityAttemptItem.MaxSuspendDataLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateTotalTime(TimeSpan value)
        {
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSessionTime(TimeSpan value)
        {
        }
    }
    internal class DataModelVerifierV1p2 : DataModelVerifier
    {
        /// <summary>
        /// Any valid float is valid.
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateEvaluationPoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "CompletionStatus"));
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateEntry(EntryMode value)
        {
            switch(value)
            {
            case EntryMode.AbInitio:
            case EntryMode.AllOtherConditions:
            case EntryMode.Resume:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLessonStatus(LessonStatus? value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch(value.Value)
            {
            case LessonStatus.Browsed:
            case LessonStatus.Completed:
            case LessonStatus.Failed:
            case LessonStatus.Incomplete:
            case LessonStatus.NotAttempted:
            case LessonStatus.Passed:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  Defined as CMIString255.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLocation(string value)
        {
            Utilities.Assert(BaseSchemaInternal.ActivityAttemptItem.MaxLocationLength >= 255, "DMV0030");
            if(value != null && value.Length > 255)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateProgressMeasure(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "ProgressMeasure"));
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "SuccessStatus"));
        }

        /// <summary>
        /// Just validate for length.  Defined as a CMIString4096.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuspendData(string value)
        {
            Utilities.Assert(BaseSchemaInternal.ActivityAttemptItem.MaxSuspendDataLength >= 4096, "DMV0040");
            if(value != null && value.Length > 4096)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateTotalTime(TimeSpan value)
        {
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSessionTime(TimeSpan value)
        {
        }
    }
    internal class DataModelVerifierLrm : DataModelVerifier
    {
        /// <summary>
        /// Any valid float is valid.
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateEvaluationPoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "CompletionStatus"));
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateEntry(EntryMode value)
        {
            switch(value)
            {
            case EntryMode.AbInitio:
            case EntryMode.AllOtherConditions:
            case EntryMode.Resume:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLessonStatus(LessonStatus? value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch(value.Value)
            {
            case LessonStatus.Browsed:
            case LessonStatus.Completed:
            case LessonStatus.Failed:
            case LessonStatus.Incomplete:
            case LessonStatus.NotAttempted:
            case LessonStatus.Passed:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  Defined as CMIString255.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLocation(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.ActivityAttemptItem.MaxLocationLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateProgressMeasure(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "ProgressMeasure"));
        }

        /// <summary>
        /// This is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "SuccessStatus"));
        }

        /// <summary>
        /// Just validate for length.  Defined as a CMIString4096.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuspendData(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.ActivityAttemptItem.MaxSuspendDataLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateTotalTime(TimeSpan value)
        {
        }

        /// <summary>
        /// All values are valid
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSessionTime(TimeSpan value)
        {
        }
    }

    #endregion

    #region ObjectiveVerifier

    internal abstract class ObjectiveVerifier
    {
        public abstract void ValidateId(string value);
        public abstract void ValidateSuccessStatus(SuccessStatus value);
        public abstract void ValidateCompletionStatus(CompletionStatus value);
        public abstract void ValidateStatus(LessonStatus? value);
        public abstract void ValidateProgressMeasure(float? value);
        public abstract void ValidateDescription(string value);
    }
    internal class ObjectiveVerifierV1p3 : ObjectiveVerifier
    {
        /// <summary>
        /// Validates that the ID is a valid URI and is within the defined length for MLC.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            Utilities.Assert(BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength >= 4000, "DMV0050");
            if(value.Length > BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }

        /// <summary>
        /// All enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            switch(value)
            {
            case SuccessStatus.Failed:
            case SuccessStatus.Passed:
            case SuccessStatus.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            switch(value)
            {
            case CompletionStatus.Completed:
            case CompletionStatus.Incomplete:
            case CompletionStatus.NotAttempted:
            case CompletionStatus.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 2004.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateStatus(LessonStatus? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p2, "Status"));
        }

        /// <summary>
        /// Valid values are 0.0 to 1.0
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateProgressMeasure(float? value)
        {
            if(value != null && (value < 0.0 || value > 1.0 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. This validates according to MLC string limit.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDescription(string value)
        {
            Utilities.Assert(BaseSchemaInternal.AttemptObjectiveItem.MaxDescriptionLength >= 250, "DMV0060");
            if(value != null && value.Length > BaseSchemaInternal.AttemptObjectiveItem.MaxDescriptionLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class ObjectiveVerifierV1p2 : ObjectiveVerifier
    {
        /// <summary>
        /// Validates value as a CMIIdentifier
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            Utilities.Assert(BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength >= 255, "DMV0070");
            VerifierHelpers.ValidateCMIIdentifier(value);
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "SuccessStatus"));
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "CompletionStatus"));
        }

        /// <summary>
        /// All enum values are valid
        /// </summary>
        /// <param name="value"></param>
        public override void ValidateStatus(LessonStatus? value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch(value.Value)
            {
            case LessonStatus.Browsed:
            case LessonStatus.Completed:
            case LessonStatus.Failed:
            case LessonStatus.Incomplete:
            case LessonStatus.NotAttempted:
            case LessonStatus.Passed:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateProgressMeasure(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "ProgressMeasure"));
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDescription(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Description"));
        }
    }
    internal class ObjectiveVerifierLrm : ObjectiveVerifier
    {
        /// <summary>
        /// Validates value as a CMIIdentifier
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            if(value.Length > BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateSuccessStatus(SuccessStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "SuccessStatus"));
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCompletionStatus(CompletionStatus value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "CompletionStatus"));
        }

        /// <summary>
        /// All enum values are valid
        /// </summary>
        /// <param name="value"></param>
        public override void ValidateStatus(LessonStatus? value)
        {
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch(value.Value)
            {
            case LessonStatus.Browsed:
            case LessonStatus.Completed:
            case LessonStatus.Failed:
            case LessonStatus.Incomplete:
            case LessonStatus.NotAttempted:
            case LessonStatus.Passed:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateProgressMeasure(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "ProgressMeasure"));
        }

        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDescription(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Description"));
        }
    }

    #endregion

    #region ScoreVerifier

    internal abstract class ScoreVerifier
    {
        public abstract void ValidateScaled(float? value);
        public abstract void ValidateRaw(float? value);
        public abstract void ValidateMinimum(float? value);
        public abstract void ValidateMaximum(float? value);
    }
    internal class ScoreVerifierV1p3 : ScoreVerifier
    {
        /// <summary>
        /// Valid values are -1.0 to 1.0
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateScaled(float? value)
        {
            if(value != null && (value < -1.0 || value > 1.0 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateRaw(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMinimum(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMaximum(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class ScoreVerifierV1p2 : ScoreVerifier
    {
        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateScaled(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Scaled"));
        }

        /// <summary>
        /// Any value is valid from 0 to 100
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateRaw(float? value)
        {
            if(value != null && (value < 0 || value > 100 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid from 0 to 100
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMinimum(float? value)
        {
            if(value != null && (value < 0 || value > 100 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid from 0 to 100
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMaximum(float? value)
        {
            if(value != null && (value < 0 || value > 100 || Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class ScoreVerifierLrm : ScoreVerifier
    {
        /// <summary>
        /// This value is not valid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateScaled(float? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Scaled"));
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateRaw(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMinimum(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any value is valid
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateMaximum(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }

    #endregion

    #region CommentVerifier

    internal abstract class CommentVerifier
    {
        public abstract void ValidateComment(string value);
        public abstract void ValidateLocation(string value);
        public abstract void ValidateTimeStamp(string value);
    }
    internal class CommentVerifierV1p3 : CommentVerifier
    {
        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 4000. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateComment(string value)
        {
            Utilities.Assert(BaseSchemaInternal.CommentFromLearnerItem.MaxCommentLength >= 4000, "DMV0080");
            if(value != null && value.Length > BaseSchemaInternal.CommentFromLearnerItem.MaxCommentLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLocation(string value)
        {
            Utilities.Assert(BaseSchemaInternal.CommentFromLearnerItem.MaxLocationLength >= 250, "DMV0090");
            if(value != null && value.Length > BaseSchemaInternal.CommentFromLearnerItem.MaxLocationLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates the value as a time(second, 10, 0)
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateTimeStamp(string value)
        {
            if(value != null)
            {
                VerifierHelpers.ValidateTimeStampV1p3(value);
            }
        }
    }
    internal class CommentVerifierV1p2 : CommentVerifier
    {
        /// <summary>
        /// Validate as CMIString4096
        /// </summary>
        /// <param name="value">Value to validate.</param>
        public override void ValidateComment(string value)
        {
            Utilities.Assert(BaseSchemaInternal.CommentFromLearnerItem.MaxCommentLength >= 4096, "DMV0100");
            if(value != null && value.Length > 4096) // Comment is defined as CMIString4096
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateLocation(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Location"));
        }

        /// <summary>
        /// Invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateTimeStamp(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "TimeStamp"));
        }
    }
    internal class CommentVerifierLrm : CommentVerifier
    {
        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 4000. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateComment(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.CommentFromLearnerItem.MaxCommentLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateLocation(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Location"));
        }

        /// <summary>
        /// Invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateTimeStamp(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "TimeStamp"));
        }
    }

    #endregion

    #region LearnerVerifier

    internal abstract class LearnerVerifier
    {
        public abstract void ValidateAudioLevel(float value);
        public abstract void ValidateLanguage(string value);
        public abstract void ValidateDeliverySpeed(float value);
        public abstract void ValidateAudioCaptioning(AudioCaptioning value);
    }
    internal class LearnerVerifierV1p3 : LearnerVerifier
    {
        /// <summary>
        /// Valid values are >= 0.0
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateAudioLevel(float value)
        {
            if(value < 0.0 || Single.IsNaN(value) || Single.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLanguage(string value)
        {
            Utilities.Assert(BaseSchemaInternal.UserItem.MaxLanguageLength >= 250, "DMV0110");
            if(value != null && value.Length > BaseSchemaInternal.UserItem.MaxLanguageLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Valid values are >= 0.0
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateDeliverySpeed(float value)
        {
            if(value < 0.0 || Single.IsNaN(value) || Single.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateAudioCaptioning(AudioCaptioning value)
        {
            switch(value)
            {
            case AudioCaptioning.NoChange:
            case AudioCaptioning.Off:
            case AudioCaptioning.On:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class LearnerVerifierV1p2 : LearnerVerifier
    {
        /// <summary>
        /// Validate that the value is an integer from -32768 to 100.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateAudioLevel(float value)
        {
            if(value != (float)(Int16)value || // not an integer
                value < -32768f ||
                value > 100f)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Value is any string CMIString255
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLanguage(string value)
        {
            Utilities.Assert(BaseSchemaInternal.UserItem.MaxLanguageLength >= 255, "DMV0120");
            if(value != null && value.Length > 255)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validate that the value is an integer from -100 to 100.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDeliverySpeed(float value)
        {
            if(value != (float)(Int16)value || // not an integer
                value < -100f ||
                value > 100f)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateAudioCaptioning(AudioCaptioning value)
        {
            switch(value)
            {
            case AudioCaptioning.NoChange:
            case AudioCaptioning.Off:
            case AudioCaptioning.On:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class LearnerVerifierLrm : LearnerVerifier
    {
        /// <summary>
        /// Validate that the value is an integer from -32768 to 100.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateAudioLevel(float value)
        {
            if(value != (float)(Int16)value || // not an integer
                value < -32768f ||
                value > 100f)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLanguage(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.UserItem.MaxLanguageLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validate that the value is an integer from -100 to 100.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDeliverySpeed(float value)
        {
            if(value != (float)(Int16)value || // not an integer
                value < -100f ||
                value > 100f)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateAudioCaptioning(AudioCaptioning value)
        {
            switch(value)
            {
            case AudioCaptioning.NoChange:
            case AudioCaptioning.Off:
            case AudioCaptioning.On:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }

    #endregion

    #region InteractionVerifier

    internal abstract class InteractionVerifier
    {
        public abstract void ValidateId(string value);
        public abstract void ValidateInteractionType(InteractionType? value);
        public abstract void ValidateTimeStamp(string value);
        public abstract void ValidateWeighting(float? value);
        public abstract void ValidateLearnerResponse(object value);
        public abstract void ValidateResultState(InteractionResultState value);
        public abstract void ValidateNumericResult(float? value);
        public abstract void ValidateLatency(TimeSpan value);
        public abstract void ValidateDescription(string value);
    }
    internal class InteractionVerifierV1p3 : InteractionVerifier
    {
        /// <summary>
        /// Validates that the ID is a valid URI. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            Utilities.Assert(BaseSchemaInternal.InteractionItem.MaxInteractionIdFromCmiLength >= 4000, "DMV0130");
            if(value.Length > BaseSchemaInternal.InteractionItem.MaxInteractionIdFromCmiLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateInteractionType(InteractionType? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case InteractionType.Attachment:
            case InteractionType.Essay:
            case InteractionType.FillIn:
            case InteractionType.Likert:
            case InteractionType.LongFillIn:
            case InteractionType.Matching:
            case InteractionType.MultipleChoice:
            case InteractionType.Numeric:
            case InteractionType.Other:
            case InteractionType.Performance:
            case InteractionType.Sequencing:
            case InteractionType.TrueFalse:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates the value as a time(second, 10, 0)
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateTimeStamp(string value)
        {
            VerifierHelpers.ValidateTimeStampV1p3(value);
        }

        /// <summary>
        /// Any value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateWeighting(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates learner response
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLearnerResponse(object value)
        {
            string svalue = value as string;
            if(value is float)
            {
                if(Single.IsNaN((float)value) || Single.IsInfinity((float)value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(svalue != null)
            {
                if(svalue.Length > BaseSchemaInternal.InteractionItem.MaxLearnerResponseStringLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(!(value == null || value is bool))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "LearnerResponse"));
            }
        }

        /// <summary>
        /// Any valid enum value is valid for value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateResultState(InteractionResultState value)
        {
            switch(value)
            {
            // these are all valid values
            case InteractionResultState.Correct:
            case InteractionResultState.Incorrect:
            case InteractionResultState.Neutral:
            case InteractionResultState.None:
            case InteractionResultState.Numeric:
            case InteractionResultState.Unanticipated:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any numeric value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateNumericResult(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid TimeSpan is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLatency(TimeSpan value)
        {
        }

        /// <summary>
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDescription(string value)
        {
            Utilities.Assert(BaseSchemaInternal.InteractionItem.MaxDescriptionLength >= 250, "DMV0140");
            if(value != null && value.Length > BaseSchemaInternal.InteractionItem.MaxDescriptionLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class InteractionVerifierV1p2 : InteractionVerifier
    {
        /// <summary>
        /// Validates value as a CMIIdentifier
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            Utilities.Assert(BaseSchemaInternal.InteractionItem.MaxInteractionIdFromCmiLength >= 255, "DMV0150");
            VerifierHelpers.ValidateCMIIdentifier(value);
        }

        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateInteractionType(InteractionType? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case InteractionType.Attachment:
            case InteractionType.Essay:
            case InteractionType.FillIn:
            case InteractionType.Likert:
            case InteractionType.LongFillIn:
            case InteractionType.Matching:
            case InteractionType.MultipleChoice:
            case InteractionType.Numeric:
            case InteractionType.Other:
            case InteractionType.Performance:
            case InteractionType.Sequencing:
            case InteractionType.TrueFalse:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates the value as a CMITime
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateTimeStamp(string value)
        {
            VerifierHelpers.ValidateCMITime(value);
        }

        /// <summary>
        /// Any value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateWeighting(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates learner response
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLearnerResponse(object value)
        {
            string svalue = value as string;
            if(value is float)
            {
                if(Single.IsNaN((float)value) || Single.IsInfinity((float)value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(svalue != null)
            {
                Utilities.Assert(BaseSchemaInternal.InteractionItem.MaxLearnerResponseStringLength >= 255, "DMV0160");
                if(svalue.Length > 255)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(!(value == null || value is bool))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p2Value, "LearnerResponse"));
            }
        }

        /// <summary>
        /// Any valid enum value is valid for value.State.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateResultState(InteractionResultState value)
        {
            switch(value)
            {
            // these are all valid values
            case InteractionResultState.Correct:
            case InteractionResultState.Incorrect:
            case InteractionResultState.Neutral:
            case InteractionResultState.None:
            case InteractionResultState.Numeric:
            case InteractionResultState.Unanticipated:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any numeric value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateNumericResult(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid TimeSpan is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLatency(TimeSpan value)
        {
        }

        public override void ValidateDescription(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Description"));
        }
    }
    internal class InteractionVerifierLrm : InteractionVerifier
    {
        /// <summary>
        /// Validates that the ID is a valid URI. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            if(value.Length > BaseSchemaInternal.InteractionItem.MaxInteractionIdFromCmiLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }


        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateInteractionType(InteractionType? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case InteractionType.Attachment:
            case InteractionType.Essay:
            case InteractionType.FillIn:
            case InteractionType.Likert:
            case InteractionType.LongFillIn:
            case InteractionType.Matching:
            case InteractionType.MultipleChoice:
            case InteractionType.Numeric:
            case InteractionType.Other:
            case InteractionType.Performance:
            case InteractionType.Sequencing:
            case InteractionType.TrueFalse:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates the value as a CMITime
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateTimeStamp(string value)
        {
            VerifierHelpers.ValidateCMITime(value);
        }

        /// <summary>
        /// Any value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateWeighting(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Validates learner response
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLearnerResponse(object value)
        {
            string svalue = value as string;
            if(value is float)
            {
                if(Single.IsNaN((float)value) || Single.IsInfinity((float)value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(svalue != null)
            {
                if(svalue.Length > BaseSchemaInternal.InteractionItem.MaxLearnerResponseStringLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            else if(!(value == null || value is bool))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p2Value, "LearnerResponse"));
            }
        }

        /// <summary>
        /// Any valid enum value is valid for value.State.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateResultState(InteractionResultState value)
        {
            switch(value)
            {
            // these are all valid values
            case InteractionResultState.Correct:
            case InteractionResultState.Incorrect:
            case InteractionResultState.Neutral:
            case InteractionResultState.None:
            case InteractionResultState.Numeric:
            case InteractionResultState.Unanticipated:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any numeric value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateNumericResult(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid TimeSpan is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateLatency(TimeSpan value)
        {
        }

        public override void ValidateDescription(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Description"));
        }
    }

    #endregion

    #region NavigationRequestVerifier

    internal abstract class NavigationRequestVerifier
    {
        public abstract void ValidateCommand(NavigationCommand? value);
        public abstract void ValidateExitMode(ExitMode? value);
        public abstract void ValidateDestination(string value);
    }
    internal class NavigationRequestVerifierV1p3 : NavigationRequestVerifier
    {
        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case NavigationCommand.Abandon:
            case NavigationCommand.AbandonAll:
            case NavigationCommand.ChoiceStart:
            case NavigationCommand.Choose:
            case NavigationCommand.Continue:
            case NavigationCommand.ExitAll:
            case NavigationCommand.None:
            case NavigationCommand.Previous:
            case NavigationCommand.ResumeAll:
            case NavigationCommand.Start:
            case NavigationCommand.SuspendAll:
            case NavigationCommand.UnqualifiedExit:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// SCORM 2004 doesn't specifically set an SPM for this, however it must be a
        /// valid activity ID, and that implies a SPM 4000 URI. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDestination(string value)
        {
            if(value != null)
            {
                Utilities.Assert(BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength >= 4000, "DMV0170");
                if(value.Length > BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }
    }
    internal class NavigationRequestVerifierV1p2 : NavigationRequestVerifier
    {
        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Command"));
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateDestination(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Destination"));
        }
    }
    internal class NavigationRequestVerifierLrm : NavigationRequestVerifier
    {
        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Command"));
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateDestination(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Destination"));
        }
    }

    #endregion

    #region InteractionObjectiveVerifier

    internal abstract class InteractionObjectiveVerifier
    {
        public abstract void ValidateId(string value);
    }
    internal class InteractionObjectiveVerifierV1p3 : InteractionObjectiveVerifier
    {
        /// <summary>
        /// Validates that the ID is a valid URI and is within the defined length for SCORM 2004.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            Utilities.Assert(BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength >= 4000, "DMV0180");
            if(value.Length > BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }
    }
    internal class InteractionObjectiveVerifierV1p2 : InteractionObjectiveVerifier
    {
        /// <summary>
        /// Validates value as a CMIIdentifier
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            Utilities.Assert(BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength >= 255, "DMV0190");
            VerifierHelpers.ValidateCMIIdentifier(value);
        }
    }
    internal class InteractionObjectiveVerifierLrm : InteractionObjectiveVerifier
    {
        /// <summary>
        /// Validates that the ID is a valid URI and is within the defined length for SCORM 2004.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateId(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            if(value.Length > BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if(!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidScormV1p3Value, "Id"));
            }
        }
    }

    #endregion

    #region CorrectResponseVerifier

    internal abstract class CorrectResponseVerifier
    {
        public abstract void ValidatePattern(string value);
    }
    internal class CorrectResponseVerifierV1p3 : CorrectResponseVerifier
    {
        /// <summary>
        /// Validate the value against the max database length.
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
        public override void ValidatePattern(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.CorrectResponseItem.MaxResponsePatternLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class CorrectResponseVerifierV1p2 : CorrectResponseVerifier
    {
        /// <summary>
        /// Validates value as a CMIFeedback.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePattern(string value)
        {
            Utilities.Assert(BaseSchemaInternal.CorrectResponseItem.MaxResponsePatternLength >= 255, "DMV0200");
            VerifierHelpers.ValidateCMIFeedback(value);
        }
    }
    internal class CorrectResponseVerifierLrm : CorrectResponseVerifier
    {
        /// <summary>
        /// Validate the value against the max database length.
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
        public override void ValidatePattern(string value)
        {
            if(value != null && value.Length > BaseSchemaInternal.CorrectResponseItem.MaxResponsePatternLength)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }

    #endregion

    #region EvaluationVerifier

    internal abstract class EvaluationVerifier
    {
        public abstract void ValidatePoints(float? value);
    }
    internal class EvaluationVerifierV1p3 : EvaluationVerifier
    {
        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class EvaluationVerifierV1p2 : EvaluationVerifier
    {
        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class EvaluationVerifierLrm : EvaluationVerifier
    {
        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }

    #endregion

    #region RubricVerifier

    internal abstract class RubricVerifier
    {
        public abstract void ValidateIsSatisfied(bool? value);
        public abstract void ValidatePoints(float? value);
    }
    internal class RubricVerifierV1p3 : RubricVerifier
    {
        /// <summary>
        /// Any value is valid here.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateIsSatisfied(bool? value)
        {
        }

        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class RubricVerifierV1p2 : RubricVerifier
    {
        /// <summary>
        /// Any value is valid here.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateIsSatisfied(bool? value)
        {
        }

        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
    internal class RubricVerifierLrm : RubricVerifier
    {
        /// <summary>
        /// Any value is valid here.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateIsSatisfied(bool? value)
        {
        }

        /// <summary>
        /// Any value is valid except NaN and infinity.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidatePoints(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }

    #endregion
}
