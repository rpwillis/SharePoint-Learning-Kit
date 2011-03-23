/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.LearningComponents;
using System.Globalization;

namespace Microsoft.LearningComponents.DataModel
{
    #region NavigationRequestVerifier

    internal abstract class NavigationRequestVerifier
    {
        public abstract void ValidateCommand(NavigationCommand? value);
        public abstract void ValidateExitMode(ExitMode? value);
        public abstract void ValidateDestination(string value);
    }
    internal class NavigationRequestVerifierV1p3 : NavigationRequestVerifier
    {
        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case NavigationCommand.Abandon:
            case NavigationCommand.AbandonAll:
            case NavigationCommand.ChoiceStart:
            case NavigationCommand.Choose:
            case NavigationCommand.Continue:
            case NavigationCommand.ExitAll:
            case NavigationCommand.None:
            case NavigationCommand.Previous:
            case NavigationCommand.ResumeAll:
            case NavigationCommand.Start:
            case NavigationCommand.SuspendAll:
            case NavigationCommand.UnqualifiedExit:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// SCORM 2004 doesn't specifically set an SPM for this, however it must be a
        /// valid activity ID, and that implies a SPM 4000 URI. String length is validated according to MLC limits.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateDestination(string value)
        {
            if(value != null)
            {
                Utilities.Assert(BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength >= 4000, "DMV0170");
                if(value.Length > BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }
    }
    internal class NavigationRequestVerifierV1p2 : NavigationRequestVerifier
    {
        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Command"));
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateDestination(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Destination"));
        }
    }
    internal class NavigationRequestVerifierLrm : NavigationRequestVerifier
    {
        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateCommand(NavigationCommand? value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Command"));
        }

        /// <summary>
        /// Any valid enum value is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        public override void ValidateExitMode(ExitMode? value)
        {
            switch(value)
            {
            // these are all valid values
            case null:
            case ExitMode.Logout:
            case ExitMode.Normal:
            case ExitMode.Suspended:
            case ExitMode.TimeOut:
            case ExitMode.Undetermined:
                break;
            default:
                throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// This is invalid for SCORM 1.2
        /// </summary>
        /// <param name="value">The value to validate</param>
        public override void ValidateDestination(string value)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.OnlyValidForSCORMV1p3, "Destination"));
        }
    }

    #endregion
}
