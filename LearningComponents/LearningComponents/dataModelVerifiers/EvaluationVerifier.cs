/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
