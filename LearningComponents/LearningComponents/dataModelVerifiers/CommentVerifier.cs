/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
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
}
