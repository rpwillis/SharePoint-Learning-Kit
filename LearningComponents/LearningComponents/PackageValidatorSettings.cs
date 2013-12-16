/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.LearningComponents.Manifest;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.LearningComponents
{

    /// <summary>
    /// Validation behaviors used by <Typ>PackageValidatorSettings</Typ>.
    /// </summary>
    public enum ValidationBehavior
    {
        /// <summary>
        /// Validation violations are ignored.
        /// </summary>
        None = 0,
        /// <summary>
        /// Validation violations are logged as warnings.
        /// </summary>
        LogWarning,
        /// <summary>
        /// Validation violations are logged as errors.
        /// </summary>
        LogError,
        /// <summary>
        /// Validation violations throw exceptions.
        /// </summary>
        Enforce
    }

    /// <summary>
    /// Validation behaviors used during package validation.
    /// </summary>
    public class PackageValidatorSettings
    {
        private ValidationBehavior m_scormRequirementValidation;
        private ValidationBehavior m_scormRecommendationValidation;
        private ValidationBehavior m_mlcRequirementValidation;
        private ValidationBehavior m_lrmRequirementValidation;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scormRequirementValidation">How to validate SCORM requirement violations.</param>
        /// <param name="scormRecommendationValidation">How to validate SCORM recommendation violations.</param>
        /// <param name="mlcRequirementValidation">How to validate MLC requirement violations.</param>
        /// <param name="lrmRequirementValidation">How to validate LRM requirement violations.</param>
        public PackageValidatorSettings(ValidationBehavior scormRequirementValidation,
            ValidationBehavior scormRecommendationValidation,
            ValidationBehavior mlcRequirementValidation,
            ValidationBehavior lrmRequirementValidation)
        {
            m_mlcRequirementValidation = mlcRequirementValidation;
            m_scormRecommendationValidation = scormRecommendationValidation;
            m_scormRequirementValidation = scormRequirementValidation;
            m_lrmRequirementValidation = lrmRequirementValidation;
        }

        /// <summary>
        /// How to validate SCORM requirement violations.
        /// </summary>
        public ValidationBehavior ScormRequirementValidation
        {
            get
            {
                return m_scormRequirementValidation;
            }
            set
            {
                m_scormRequirementValidation = value;
            }
        }

        /// <summary>
        /// How to validate SCORM recommendation violations.
        /// </summary>
        public ValidationBehavior ScormRecommendationValidation
        {
            get
            {
                return m_scormRecommendationValidation;
            }
            set
            {
                m_scormRecommendationValidation = value;
            }
        }

        /// <summary>
        /// How to validate MLC requirement violations.
        /// </summary>
        public ValidationBehavior MlcRequirementValidation
        {
            get
            {
                return m_mlcRequirementValidation;
            }
            set
            {
                m_mlcRequirementValidation = value;
            }
        }

        /// <summary>
        /// How to validate Lrm requirement violations.
        /// </summary>
        public ValidationBehavior LrmRequirementValidation
        {
            get
            {
                return m_lrmRequirementValidation;
            }
            set
            {
                m_lrmRequirementValidation = value;
            }
        }


        /// <summary>
        /// Returns true if the behavior is LogError or LogWarning.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static bool RequiresLog(ValidationBehavior behavior)
        {
            if (behavior == ValidationBehavior.LogError || behavior == ValidationBehavior.LogWarning)
                return true;
            else return false;
        }

        /// <summary>
        /// Returns true if any of the validation requirements is LogError or LogWarning.
        /// </summary>
        /// <returns></returns>
        internal bool RequiresLog()
        {
            if (RequiresLog(m_mlcRequirementValidation)) return true;
            if (RequiresLog(m_scormRecommendationValidation)) return true;
            if (RequiresLog(m_scormRequirementValidation)) return true;
            if (RequiresLog(m_lrmRequirementValidation)) return true;
            return false;
        }
    }
}
