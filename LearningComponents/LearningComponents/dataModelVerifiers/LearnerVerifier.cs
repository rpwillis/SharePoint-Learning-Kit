/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
