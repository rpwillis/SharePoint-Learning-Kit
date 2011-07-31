/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
