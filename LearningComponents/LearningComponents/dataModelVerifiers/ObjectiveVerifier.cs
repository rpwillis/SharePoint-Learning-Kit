/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
