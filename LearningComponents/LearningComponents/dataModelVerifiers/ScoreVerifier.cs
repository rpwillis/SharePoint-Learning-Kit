/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
