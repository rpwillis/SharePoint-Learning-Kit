/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
