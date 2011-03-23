/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{

#region InteractionVerifier
    internal abstract class InteractionVerifier
    {
#region abstract methods
        public abstract void ValidateId(string value);
        public abstract void ValidateLearnerResponse(object value);
#endregion abstract methods

#region virtual methods
        public virtual string ValidateDescription(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Description"));
        }

        public virtual void ValidateTimeStamp(string value)
        {
            VerifierHelpers.ValidateCMITime(value);
        }
#endregion virtual methods

#region common methods
        /// <summary>
        /// All normal enum values are valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public void ValidateInteractionType(InteractionType? value)
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
        /// Any value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public void ValidateWeighting(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid enum value is valid for value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public void ValidateResultState(InteractionResultState value)
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
        public void ValidateNumericResult(float? value)
        {
            if(value != null && (Single.IsNaN(value.Value) || Single.IsInfinity(value.Value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        public void ValidateLatency(TimeSpan value)
        {
        }

#endregion common methods
    }
#endregion InteractionVerifier

#region InteractionVerifierV1p3
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
        /// Validates the value as a time(second, 10, 0)
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateTimeStamp(string value)
        {
            VerifierHelpers.ValidateTimeStampV1p3(value);
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
        /// Just validate for length.  SCORM 2004 defines an SPM of 250. String length is validated according to MLC limits.
        /// </summary>
        /// <remarks>This is normally the text of the question. If this is over 250 characters, although strictly speaking it is invalid
        /// there is definitely value in not rejecting it. So just use the first 250 characters.</remarks>
        /// <param name="value">The value to validate.</param>
        public override string ValidateDescription(string value)
        {
            int maxLength = BaseSchemaInternal.InteractionItem.MaxDescriptionLength;
            Utilities.Assert(maxLength >= 250, "DMV0140");
            if (value != null && value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }
            else
            {
                return value;
            }
        }
    }
#endregion InteractionVerifierV1p3

#region InteractionVerifierV1p2
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

    }
#endregion InteractionVerifierV1p2

#region InteractionVerifierLrm
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
    }
#endregion InteractionVerifierLrm
}
