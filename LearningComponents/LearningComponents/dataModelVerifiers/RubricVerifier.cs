/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
