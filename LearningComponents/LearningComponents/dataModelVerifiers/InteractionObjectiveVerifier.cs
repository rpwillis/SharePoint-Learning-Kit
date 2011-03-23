/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
