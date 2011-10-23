/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Collections.ObjectModel;
using Microsoft.LearningComponents;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;

// General comments applied to all (or most) of these classes:
// * Re: Properties that get or set XPathNavigators. The classes will maintain an XPathNavigator.
//      It will return a copy of that when a 'get' accessor is accessed.
//      When a 'set' accessor is called, the class will make a copy of the provided object.
// 
// * Classes will hold private variables that correspond to property / method values. In the 
//      read/write classes, these values will be the ones that are 'set' by accessors. When
//      the file is written out, the original XmlDocument will be modified and the new values 
//      added (rather than the whole file re-written). This allows information that we don't know
//      about to be persisted in the file.
//
namespace Microsoft.LearningComponents.Manifest
{
    #region Manifest enums
    /// <summary>
    /// Settings used by the Manifest Reader classes to determine how to handle erroneous data in the manifest.
    /// </summary>
    public class ManifestReaderSettings
    {
        private bool m_fixScormRequirementViolations;
        private bool m_fixMlcRequirementViolations;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fixScormRequirementViolations">
        /// If <c>true</c>, when a SCORM requirement is violated when reading an element or attribute value from the 
        /// manifest, substitute a default value.  If <c>false</c>, throw an exception.
        /// </param>
        /// <param name="fixMlcRequirementViolations">
        /// If <c>true</c>, if a string value's length is greater than the maximum length allowed by the database, 
        /// truncate the string to that maximum.  If <c>false</c>, throw an exception.
        /// </param>
        public ManifestReaderSettings(bool fixScormRequirementViolations, bool fixMlcRequirementViolations)
        {
            m_fixScormRequirementViolations = fixScormRequirementViolations;
            m_fixMlcRequirementViolations = fixMlcRequirementViolations;
        }

        /// <summary>
        /// If <c>true</c>, when a SCORM requirement is violated when reading an element or attribute value from the 
        /// manifest, substitute a default value.  If <c>false</c>, throw an exception.
        /// </summary>
        public bool FixScormRequirementViolations
        {
            get
            {
                return m_fixScormRequirementViolations;
            }
            set
            {
                m_fixScormRequirementViolations = value;
            }
        }

        /// <summary>
        /// If <c>true</c>, if a string value's length is greater than the maximum length allowed by the database, 
        /// truncate the string to that maximum.  If <c>false</c>, throw an exception.
        /// </summary>
        public bool FixMlcRequirementViolations
        {
            get
            {
                return m_fixMlcRequirementViolations;
            }
            set
            {
                m_fixMlcRequirementViolations = value;
            }
        }
    }

    #endregion

    #region ManifestReader classes

    internal class Helper
    {
        // member variables holding values used in all node readers
        internal PackageValidatorSettings ValidatorSettings;
        internal ManifestReaderSettings ReaderSettings;
        internal ValidationResults Log;
        internal XPathNavigator Node;
        internal bool LogReplacement;

        /// <summary>
        /// Initializes this helper to point to a manifest node in the SCORM 2004 or SCORM 1.2 namespace.
        /// </summary>
        private void InitManifestNode(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log)
        {
            LogReplacement = logReplacement;
            if (node == null)
            {
                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.ManifestNodeMissing));
            }
            if (node.LocalName == Strings.Manifest && 
                (node.NamespaceURI == Strings.ImscpNamespaceV1p3 || node.NamespaceURI == Strings.ImscpNamespaceV1p2))
            {
                Node = node.Clone();
                ValidatorSettings = validatorSettings;
                ReaderSettings = readerSettings;
                Log = log;
                if (String.Compare(node.NamespaceURI, Strings.ImscpNamespaceV1p2, StringComparison.Ordinal) == 0)
                {
                    m_manifestVersion = ScormVersion.v1p2;
                }
                else if (String.Compare(node.NamespaceURI, Strings.ImscpNamespaceV1p3, StringComparison.Ordinal) == 0)
                {
                    m_manifestVersion = ScormVersion.v1p3;
                }
                else
                {
                    throw new LearningComponentsInternalException("MR003", node.LocalName + "Node");
                }
            }
            else
            {
                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.ManifestNodeMissing));
            }
        }

        /// <summary>
        /// Initializes this helper to point to a node in the SCORM 2004 or SCORM 1.2 IMSCP namespace.
        /// </summary>
        private void InitImscpNode(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log, string localName)
        {
            LogReplacement = logReplacement;
            if (node == null)
            {
                throw new LearningComponentsInternalException("MR004", localName + "Node");
            }
            // Make sure the node is in one of the valid IMSCP namespace versions.
            if (String.Compare(node.NamespaceURI, Strings.ImscpNamespaceV1p2, StringComparison.Ordinal) == 0)
            {
                m_manifestVersion = ScormVersion.v1p2;
            }
            else if (String.Compare(node.NamespaceURI, Strings.ImscpNamespaceV1p3, StringComparison.Ordinal) == 0)
            {
                m_manifestVersion = ScormVersion.v1p3;
            }
            else
            {
                throw new LearningComponentsInternalException("MR005", localName + "Node");
            }
            if (node.LocalName == localName)
            {
                Node = node.Clone();
                ValidatorSettings = validatorSettings;
                ReaderSettings = readerSettings;
                Log = log;
            }
            else
            {
                throw new LearningComponentsInternalException("MR006", localName + "," + node.NamespaceURI + "," + node.LocalName + "," + node.NamespaceURI);
            }
        }

        /// <summary>
        /// Initializes this helper to point to a node in the SCORM 2004 IMSSS namespace.
        /// </summary>
        private void InitImsssNode(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log, string localName)
        {
            LogReplacement = logReplacement;
            if (node == null)
            {
                throw new LearningComponentsInternalException("MR007", localName + "Node");
            }
            // Make sure the node is in the IMSSS namespace.
            if (String.Compare(node.NamespaceURI, Strings.ImsssNamespace, StringComparison.Ordinal) == 0)
            {
                m_manifestVersion = ScormVersion.v1p3;
            }
            else
            {
                throw new LearningComponentsInternalException("MR008", localName + "Node");
            }
            if (node.LocalName == localName)
            {
                Node = node.Clone();
                ValidatorSettings = validatorSettings;
                ReaderSettings = readerSettings;
                Log = log;
            }
            else
            {
                throw new LearningComponentsInternalException("MR009", localName + "," + node.NamespaceURI + "," + node.LocalName + "," + node.NamespaceURI);
            }
        }

        /// <summary>
        /// Initializes this helper to point to a node in the SCORM 2004 IMSSS namespace.
        /// </summary>
        private void InitImsssNode(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log)
        {
            LogReplacement = logReplacement;
            if (node == null)
            {
                throw new LearningComponentsInternalException("MR030");
            }
            // Make sure the node is in the IMSSS namespace.
            if (String.Compare(node.NamespaceURI, Strings.ImsssNamespace, StringComparison.Ordinal) == 0)
            {
                m_manifestVersion = ScormVersion.v1p3;
            }
            else
            {
                throw new LearningComponentsInternalException("MR031");
            }
            Node = node.Clone();
            ValidatorSettings = validatorSettings;
            ReaderSettings = readerSettings;
            Log = log;
        }

        /// <summary>
        /// Create method. Throws non-internal exceptions.
        /// </summary>
        /// <param name="node">The node to parse.</param>
        /// <param name="readerSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null. The parameter name thrown is not "node",
        /// however.  Instead, it is the string value of <paramref name="localName"/> + "Node".  E.g. the <Typ>ManifestReader</Typ>
        /// class constructor would call this, and the throw would have the parameter name "manifestNode". </exception>
        /// <exception cref="InvalidPackageException"><paramref name="node"/> does not point to the expected node.</exception>
        internal static Helper CreateManifestHelper(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings, 
            bool logReplacement, ValidationResults log)
        {
            Helper helper = new Helper();
            helper.InitManifestNode(node, readerSettings, validatorSettings, logReplacement, log);
            return helper;
        }

        /// <summary>
        /// Create method for nodes in the Imscp namespace. Throws internal exceptions only.
        /// </summary>
        /// <param name="node">The node to parse.</param>
        /// <param name="readerSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <param name="localName">The <c>localName</c> that should be on the node contained in <paramref name="node"/>.</param>
        internal static Helper InternalCreateImscp(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log, string localName)
        {
            Helper helper = new Helper();
            helper.InitImscpNode(node, readerSettings, validatorSettings, logReplacement, log, localName);
            return helper;
        }

        /// <summary>
        /// Create method for nodes in the Imsss namespace. Throws internal exceptions only.
        /// </summary>
        /// <param name="node">The node to parse.</param>
        /// <param name="readerSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <param name="localName">The <c>localName</c> that should be on the node contained in <paramref name="node"/>.</param>
        internal static Helper InternalCreateImsss(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log, string localName)
        {
            Helper helper = new Helper();
            helper.InitImsssNode(node, readerSettings, validatorSettings, logReplacement, log, localName);
            return helper;
        }

        /// <summary>
        /// Create method for nodes in the Imsss namespace, but don't validate against the localName. 
        /// Throws internal exceptions only. Does not check that the node's name is in the correct name/namespace.
        /// </summary>
        internal static Helper InternalNoNameCheckCreateImsss(XPathNavigator node, ManifestReaderSettings readerSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log)
        {
            Helper helper = new Helper();
            helper.InitImsssNode(node, readerSettings, validatorSettings, logReplacement, log);
            return helper;
        }

        /// <summary>
        /// Replace a string containing \r characters with \r\n.
        /// </summary>
        /// <remarks>
        /// Will not replace \r\n with \r\n\n.
        /// </remarks>
        /// <param name="value">Original string.</param>
        /// <returns>Fixed string.</returns>
        internal static string ReplaceCarriageReturns(string value)
        {
            return value.Replace("\r", "\r\n").Replace("\r\n\n", "\r\n");
        }

        /// <summary>
        /// Class that returns SPM's for various values.
        /// </summary>
        internal class SPM
        {
            internal const int DataFromLmsV1p2 = 255; // <dataFromLMS> - 4000 is from the SCORM 2004 Addendum Version 1.2, secction 2.6.
            internal const int DataFromLmsV1p3 = 4000; // <dataFromLMS> - 4000 is from the SCORM 2004 Addendum Version 1.2, secction 2.6.
            internal const int Href = 2000; // <resource>/href
            internal const int IdentfierRef = 2000; // <item>/identifierref
            internal const int Parameters = 1000; // <item>/parameters
            internal const int Prerequisites = 200; // <adlcp:prerequisites
            internal const int Structure = 200; // <organization>/structure
            internal const int Title = 200; // <title>
            internal const int XmlBase = 2000;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private Helper()
        {
        }

        /// <summary>
        /// Class that returns string constants.
        /// </summary>
        internal class Strings
        {
            internal const string Action = "action"; // <imsss:rollupAction>/action
            internal const string Adlcp = "adlcp";
            internal const string Adlnav = "adlnav";
            internal const string Adlseq = "adlseq";
            internal const string AdlcpNamespaceV1p2 = "http://www.adlnet.org/xsd/adlcp_rootv1p2";
            internal const string AdlcpNamespaceV1p3 = "http://www.adlnet.org/xsd/adlcp_v1p3";
            internal const string AdlnavNamespace = "http://www.adlnet.org/xsd/adlnav_v1p3";
            internal const string AdlseqNamespace = "http://www.adlnet.org/xsd/adlseq_v1p3";
            internal const string AttemptAbsoluteDurationLimit = "attemptAbsoluteDurationLimit"; // <imsss:limitConditions>/attemptAbsoluteDurationLimit
            internal const string AttemptLimit = "attemptLimit"; // <imsss:limitConditions>/attemptLimit
            internal const string AuxiliaryResources = "auxiliaryResources"; // <imsss:auxiliaryResources>
            internal const string Base = "base"; // <xml:base>
            internal const string ChildActivitySet = "childActivitySet"; // <imsss:rollupRule>/childActivitySet
            internal const string Choice = "choice"; // <imsss:controlMode>/choice
            internal const string ChoiceExit = "choiceExit"; // <imsss:controlMode>/choiceExit
            internal const string CompletionSetByContent = "completionSetByContent"; // <imsss:deliveryControls>/completionSetByContent
            internal const string CompletionThreshold = "completionThreshold"; // <adlcp:completionThreshold>
            internal const string Condition = "condition"; // <imsss:rollupCondition>/condition
            internal const string ConditionCombination = "conditionCombination"; // <imsss:rollupConditions>/conditionCombination
            internal const string ConstrainChoice = "constrainChoice"; // <adlseq:constrainedChoiceConsiderations>/constrainChoice
            internal const string ConstrainedChoiceConsiderations = "constrainedChoiceConsiderations"; // <adlseq:constrainedChoiceConsiderations>
            internal const string ControlMode = "controlMode"; // <imsss:sequencing>/controlMode
            internal const string DataFromLmsV1p2 = "datafromlms"; // <adlcp:datafromlms> (SCORM 1.2)
            internal const string DataFromLmsV1p3 = "dataFromLMS"; // <adlcp:dataFromLMS>
            internal const string Default = "default"; // <imscp:organizations>/default
            internal const string DeliveryControls = "deliveryControls"; // <imsss:deliveryControls>
            internal const string Dependency = "dependency"; // <imscp:dependency>
            internal const string Description = "description"; // <lom:description>
            internal const string ExitConditionRule = "exitConditionRule"; // <imsss:exitConditionRule>
            internal const string File = "file"; // <imscp:file>
            internal const string Flow = "flow"; // <imsss:controlMode>/flow
            internal const string ForwardOnly = "forwardOnly"; // <imsss:controlMode>/forwardOnly
            internal const string General = "general"; // <lom:general>
            internal const string HideLmsUi = "hideLMSUI"; // <adlnav:hideLMSUI>
            internal const string Href = "href"; // <imscp:file>/href
            internal const string Id = "ID"; // e.g. <imsss:sequencing>/ID
            internal const string IdRef = "IDRef"; // e.g. <imsss:sequencing>/IDRef
            internal const string Identifier = "identifier"; // <imscp:resource>/identifier
            internal const string IdentifierRef = "identifierref"; // e.g. <imscp:dependency>/identiferref
            internal const string Imscp = "imscp";
            internal const string Imsss = "imsss";
            internal const string ImscpNamespaceV1p2 = "http://www.imsproject.org/xsd/imscp_rootv1p1p2";
            internal const string ImscpNamespaceV1p3 = "http://www.imsglobal.org/xsd/imscp_v1p1";
            internal const string ImsssNamespace = "http://www.imsglobal.org/xsd/imsss";
            internal const string IsVisible = "isvisible"; // <imscp:item>/isvisible
            internal const string Item = "item"; // <imscp:item>
            internal const string LangString = "langstring"; // <lom:langstring> (SCORM 1.2)
            internal const string Language = "language"; // language attribute of the <lom:string>
            internal const string LimitConditions = "limitConditions"; // <imsss:limitConditions>
            internal const string Lom = "lom"; // <lom:lom>
            internal const string LomNamespaceV1p2 = "http://www.imsglobal.org/xsd/imsmd_rootv1p2p1";
            internal const string LomNamespaceV1p3 = "http://ltsc.ieee.org/xsd/LOM";
            internal const string String = "string"; // <lom:string>
            internal const string Manifest = "manifest"; // <imscp:manifest>
            internal const string MapInfo = "mapInfo"; // <imsss:mapInfo>
            internal const string MasteryScore = "masteryscore"; // <adlcp:masteryscore> (SCORM 1.2)
            internal const string MaxTimeAllowed = "maxtimeallowed"; // <adlcp:maxtimeallowed> (SCORM 1.2)
            internal const string MeasureSatisfactionIfActive = "measureSatisfactionIfActive"; // <adlseq:rollupConsiderations>/measureSatisfactionIfActive
            internal const string MeasureThreshold = "measureThreshold"; // <imsss:ruleCondition>/measureThreshold
            internal const string Metadata = "metadata"; // <imscp:metadata>
            internal const string MinimumCount = "minimumCount"; // <imsss:rollupRule>/minimumCount
            internal const string MinimumPercent = "minimumPercent"; // <imsss:rollupRule>/minimumPercent
            internal const string MinNormalizedMeasure = "minNormalizedMeasure"; // <imsss:objective>/<imsss:minNormalizedMeasure>
            internal const string MlcNamespace = "urn:schemas-microsoft-com:learning-components-manifest";
            internal const string NavigationInterface = "navigationInterface"; // <adlnav:navigationInterface>
            internal const string Objective = "objective"; // <imsss:objective>
            internal const string Objectives = "objectives"; // <imsss:objectives>
            internal const string ObjectiveId = "objectiveID"; // <imsss:objective>/objectiveID
            internal const string ObjectiveMeasureWeight = "objectiveMeasureWeight"; // <imsss:rollupRules>/objectiveMeasureWeight
            internal const string ObjectiveSetByContent = "objectiveSetByContent"; // <imsss:deliveryControls>/objectiveSetByContent
            internal const string ObjectivesGlobalToSystem = "objectivesGlobalToSystem"; // adlseq:objectivesGlobalToSystem
            internal const string Operator = "operator"; // <imsss:rollupCondition>/operator
            internal const string Organization = "organization"; // <imscp:organization>
            internal const string Organizations = "organizations"; // <imscp:organizations>
            internal const string Parameters = "parameters"; // <imscp:item>/parameters
            internal const string PostConditionRule = "postConditionRule"; // <imsss:postConditionRule>
            internal const string PreConditionRule = "preConditionRule"; // <imsss:preConditionRule>
            internal const string Prerequisites = "prerequisites"; // <adlcp:prerequisites> (SCORM 1.2)
            internal const string Presentation = "presentation"; // <adlnav:presentation>
            internal const string PreventActivation = "preventActivation"; // <adlseq:constrainedChoiceConsiderations>/preventActivation
            internal const string PrimaryObjective = "primaryObjective"; // <imsss:primaryObjective>
            internal const string RandomizationControls = "randomizationControls"; // <imsss:randomizationControls>
            internal const string RandomizationTiming = "randomizationTiming"; // <imsss:randomizationControls>/randomizationTiming
            internal const string ReadNormalizedMeasure = "readNormalizedMeasure"; // <imsss:mapInfo>/readNormalizedMeasure
            internal const string ReadSatisfiedStatus = "readSatisfiedStatus"; // <imsss:mapInfo>/readSatisfiedStatus
            internal const string ReferencedObjective = "referencedObjective"; // <imsss:ruleCondition>/referencedObjective
            internal const string ReorderChildren = "reorderChildren"; // <imsss:randomizationControls>/reorderChildren
            internal const string RequiredForCompleted = "requiredForCompleted"; // <adlseq:rollupConsiderations>/requiredForCompleted
            internal const string RequiredForIncomplete = "requiredForIncomplete"; // <adlseq:rollupConsiderations>/requiredForIncomplete
            internal const string RequiredForNotSatisfied = "requiredForNotSatisfied"; // <adlseq:rollupConsiderations>/requiredForNotSatisfied
            internal const string RequiredForSatisfied = "requiredForSatisfied"; // <adlseq:rollupConsiderations>/requiredForSatisfied
            internal const string Resource = "resource"; // <imscp:resource>
            internal const string Resources = "resources"; // <imscp:resources>
            internal const string RollupAction = "rollupAction"; // <imsss:rollupAction>
            internal const string RollupCondition = "rollupCondition"; // <imsss:rollupCondition>
            internal const string RollupConditions = "rollupConditions"; // <imsss:rollupConditions>
            internal const string RollupConsiderations = "rollupConsiderations"; // <adlseq:rollupConsiderations>
            internal const string RollupObjectiveSatisfied = "rollupObjectiveSatisfied"; // <imsss:rollupRules>/rollupObjectiveSatisfied
            internal const string RollupProgressCompletion = "rollupProgressCompletion"; // <imsss:rollupRules>/rollupProgressCompletion
            internal const string RollupRule = "rollupRule"; // <imsss:rollupRule>
            internal const string RollupRules = "rollupRules"; // <imsss:rollupRules>
            internal const string RuleAction = "ruleAction"; // <imsss:ruleAction>
            internal const string RuleCondition = "ruleCondition"; // <imsss:ruleCondition>
            internal const string RuleConditions = "ruleConditions"; // <imsss:ruleConditions>
            internal const string SatisfiedByMeasure = "satisfiedByMeasure"; // <imsss:objective>/satisfiedByMeasure
            internal const string ScormTypeV1p2 = "scormtype"; // adlcp:scormtype (SCORM 1.2)
            internal const string ScormTypeV1p3 = "scormType"; // adlcp:scormType
            internal const string SelectCount = "selectCount"; // <imsss:randomizationControls>/selectCount 
            internal const string SelectionTiming = "selectionTiming"; // <imsss:randomizationControls>/selectionTiming
            internal const string Sequencing = "sequencing"; // <imsss:sequencing>
            internal const string SequencingCollection = "sequencingCollection"; // <imsss:sequencingCollection>
            internal const string SequencingRules = "sequencingRules"; // <imsss:sequencingRules>
            internal const string Structure = "structure"; // <imscp:organizations>/structure
            internal const string TargetObjectiveID = "targetObjectiveID"; // <imsss:mapInfo>/targetObjectiveID
            internal const string TimeLimitActionV1p2 = "timelimitaction"; // <adlcp:timelimitaction> (SCORM 1.2)
            internal const string TimeLimitActionV1p3 = "timeLimitAction"; // <adlcp:timeLimitAction>
            internal const string Title = "title"; // <imscp:title> or <lom:title>
            internal const string Tracked = "tracked"; // <imsss:deliveryControls>/tracked
            internal const string Type = "type"; // <imscp:resource>/type or <adlcp:prerequisites>/type (SCORM 1.2)
            internal const string UseCurrentAttemptObjectiveInfo = "useCurrentAttemptObjectiveInfo"; // <imsss:controlMode>/useCurrentAttemptObjectiveInfo
            internal const string UseCurrentAttemptProgressInfo = "useCurrentAttemptProgressInfo"; // <imsss:controlMode>/useCurrentAttemptProgressInfo
            internal const string Version = "version"; // <imsss:manifest>/version
            internal const string Webcontent = "webcontent";
            internal const string WriteNormalizedMeasure = "writeNormalizedMeasure"; // <imsss:mapInfo>/writeNormalizedMeasure
            internal const string WriteSatisfiedStatus = "writeSatisfiedStatus"; // <imsss:mapInfo>/writeSatisfiedStatus
            internal const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
            internal const string XrloType = "xrloType"; // <imsss:resource>/mls:xrloType
        }

        /// <summary>
        /// Identifies if the manifest is SCORM 2004 (e.g. v1p3) or SCORM 1.2 (e.g. v1p2).
        /// </summary>
        internal enum ScormVersion
        {
            v1p2,
            v1p3
        }

        /// <summary>
        /// Identifies if the manifest is SCORM 2004 (e.g. v1p3) or SCORM 1.2 (e.g. v1p2).
        /// </summary>
        /// <remarks>Defaults to <c>ScormVersion.v1p3</c></remarks>
        private ScormVersion m_manifestVersion = ScormVersion.v1p3;

        /// <summary>
        /// Identifies if the manifest is SCORM 2004 (e.g. v1p3) or SCORM 1.2 (e.g. v1p2).
        /// </summary>
        internal ScormVersion ManifestVersion
        {
            get
            {
                return m_manifestVersion;
            }
        }

        /// <summary>
        /// Gets the correct IMSCP namepace string, depending on the SCORM version of the current manifest.
        /// </summary>
        internal string ImscpCurrentNamespace
        {
            get
            {
                return (ManifestVersion == ScormVersion.v1p2) ? Helper.Strings.ImscpNamespaceV1p2 : Helper.Strings.ImscpNamespaceV1p3;
            }
        }

        /// <summary>
        /// Gets the correct ADLCP namepace string, depending on the SCORM version of the current manifest.
        /// </summary>
        internal string AdlcpCurrentNamespace
        {
            get
            {
                return (ManifestVersion == ScormVersion.v1p2) ? Helper.Strings.AdlcpNamespaceV1p2 : Helper.Strings.AdlcpNamespaceV1p3;
            }
        }

        private static char[] queryOrAnchorChars = new char[] { '?', '#' };
        /// <summary>
        /// Combines the path with an absolute or relative path, and converts backward slashes to forward slashes.
        /// </summary>
        internal static Uri CombinePaths(Uri oldPath, string path)
        {
            if (path.Contains("\\"))
            {
                string newPath = path.Replace('\\', '/');
                path = newPath;
            }
            // Uri.IsWellFormedUriString doesn't work well with fragment identifiers or badly formed query string parameters
            // so strip them off before checking.
            string strippedPath;
            int stripIndex = path.IndexOfAny(queryOrAnchorChars);
            if (stripIndex > -1)
            {
                strippedPath = path.Substring(0, stripIndex);
            }
            else
            {
                strippedPath = path;
            }

            // Although it goes against http://www.ietf.org/rfc/rfc2396.txt, we do allow
            // spaces in URI's.
            if (strippedPath.Contains(" "))
            {
                strippedPath = strippedPath.Replace(" ", "%20");
            }

            if (Uri.IsWellFormedUriString(strippedPath, UriKind.Absolute))
            {
                // create a new Uri based on the whole path (not just strippedPath)
                return new Uri(path);
            }
            else if (Uri.IsWellFormedUriString(strippedPath, UriKind.Relative))
            {
                if (oldPath == null)
                {
                    // create a new Uri based on the whole path (not just strippedPath)
                    return new Uri(path, UriKind.Relative);
                }
                else if (oldPath.IsAbsoluteUri)
                {
                    // create a new Uri based on the whole path (not just strippedPath)
                    return new Uri(oldPath, path);
                }
                else
                {
                    // To use Path.Combine to create a new path string, strip off any query string and anchor parameters
                    // first.  Then append the ones obtained from the "path" parameter back onto the end.
                    string queryOrAnchor = String.Empty;
                    string pathNoQueryOrAnchor = path;
                    string oldNoQueryOrAnchor = oldPath.OriginalString;
                    int i;
                    i = oldNoQueryOrAnchor.IndexOfAny(queryOrAnchorChars);
                    if (i > -1)
                    {
                        oldNoQueryOrAnchor = oldNoQueryOrAnchor.Substring(0, i);
                    }
                    i = path.IndexOfAny(queryOrAnchorChars);
                    if (i > -1)
                    {
                        pathNoQueryOrAnchor = path.Substring(0, i);
                        queryOrAnchor = path.Substring(i);
                    }
                    string newPath = Path.Combine(oldPath.OriginalString, pathNoQueryOrAnchor);
                    return new Uri(newPath + queryOrAnchor, UriKind.Relative);
                }
            }
            else
            {
                // If the path violates href syntax rules, it's unlikely that changing it to anything or disallowing it
                // is going to be better than just using it "as-is".  Return a Uri object into which the path is
                // stuffed with no validation.
                return new Uri(path, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Combines the xml:base attribute on the root node of <paramref name="node"/> with the <paramref name="xmlBase"/>.
        /// </summary>
        /// <param name="xmlBase">xml:base to combine with the attribute.  Null if none.</param>
        /// <returns>Combined xml:base.  Null if there is no xml:base attribute and <paramref name="xmlBase"/> is null.
        /// If there is no xml:base attribute but <paramref name="xmlBase"/> is non-null, returns
        /// <paramref name="xmlBase"/>.</returns>
        /// <param name="node">Node containing the xml:base attribute</param>
        internal Uri CombineXmlBaseAttribute(Uri xmlBase, XPathNavigator node)
        {
            Uri retUri = xmlBase; // default return value
            XPathNavigator attribute = node.Clone();
            if (attribute.MoveToAttribute(Helper.Strings.Base, Helper.Strings.XmlNamespace))
            {
                if (attribute.Value.Length > SPM.XmlBase)
                {
                    LogSPMViolation(attribute.Value, attribute.Name, node, SPM.XmlBase);
                }
                string newXmlBase = attribute.Value.Replace('\\', '/');
                // make sure it does end with a slash
                if (!newXmlBase.EndsWith("/", StringComparison.Ordinal))
                {
                    // default value in this case is to append the "/"
                    if (newXmlBase.Length > 0)
                    {
                        newXmlBase += "/";
                    }
                    LogBadAttribute(attribute.Value, attribute.Name, node.Name, ValidatorResources.EmptyString, ValidatorResources.BadXmlBase);
                }
                // make sure it does not begin with a slash
                if (newXmlBase.StartsWith("/", StringComparison.Ordinal))
                {
                    LogBadAttribute(attribute.Value, attribute.Name, node.Name, ValidatorResources.EmptyString, ValidatorResources.BadXmlBase);
                }
                else
                {
                    try
                    {
                        retUri = CombinePaths(xmlBase, attribute.Value);
                    }
                    catch (LearningComponentsInternalException)
                    {
                        LogBadAttribute(attribute.Value, attribute.Name, node.Name, ValidatorResources.EmptyString, ValidatorResources.BadUri);
                    }
                }
            }
            return retUri;
        }

        /// <summary>
        /// Retrieve specified string attribute.
        /// </summary>
        /// <remarks>
        /// This is to cache string attributes for which the type is xs:string.
        /// </remarks>
        /// <param name="node">Node on which to find the attribute.</param>
        /// <param name="value">Ref value to return the attribute value.</param>
        /// <param name="attributeName">Name of attribute to obtain.</param>
        /// <param name="defaultValue">Default value to set if attribute is missing.</param>
        /// <param name="spm">SPM for the string attribute. 0 or less means "no effect".</param>
        /// <param name="mlcMaxLength">Max length for MLC validation. 0 or less means "no effect".</param>
        /// <returns></returns>
        /// <exception cref="InvalidPackageException">If <paramref name="mlcMaxLength"/> is violated and
        /// <Prp>ManifestReaderSettings.FixMlcRequirementViolation</Prp> is <c>false</c>.</exception>
        internal string CacheStringAttribute(
            XPathNavigator node,
            ref string value,
            string attributeName,
            string defaultValue,
            int spm,
            int mlcMaxLength)
        {
            XPathNavigator attribute = node.Clone();
            if (attribute.MoveToAttribute(attributeName, String.Empty))
            {
                value = attribute.Value;
            }
            else
            {
                value = defaultValue;
            }
            // need to check that value is not null, as it is set to null by ResourceNodeReader.EntryPoint.
            if (value != null)
            {
                if (spm > 0 && value.Length > spm)
                {
                    LogSPMViolation(value, attributeName, node, spm);
                }
                if (mlcMaxLength > 0 && value.Length > mlcMaxLength)
                {
                    LogMlcViolation(value, attributeName, mlcMaxLength, node);
                    if (ReaderSettings.FixMlcRequirementViolations)
                    {
                        value = value.Substring(0, mlcMaxLength);
                    }
                    else
                    {
                        throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcViolation,
                                value,
                                attributeName,
                                GetLogInfo_NodeText(node)));
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Stores the specified string element.
        /// </summary>
        /// <remarks>
        /// This is to cache string elements for which the type is xs:string.
        /// </remarks>
        /// <param name="value">Ref value to return the attribute value.</param>
        /// <param name="elementName">Name of element.</param>
        /// <param name="elementValue">Value of element.</param>
        /// <param name="spm">SPM for the string. 0 or less means "no effect".</param>
        /// <param name="mlcMaxLength">Max length for MLC validation. 0 or less means "no effect".</param>
        /// <returns></returns>
        /// <exception cref="InvalidPackageException">If <paramref name="mlcMaxLength"/> is violated and
        /// <Prp>ManifestReaderSettings.FixMlcRequirementViolation</Prp> is <c>false</c>.</exception>
        internal string CacheStringElement(
            ref string value,
            string elementName,
            string elementValue,
            int spm,
            int mlcMaxLength)
        {
            value = elementValue;
            if (spm > 0 && value.Length > spm)
            {
                LogSPMNodeViolation(value, elementName, spm);
            }
            if (mlcMaxLength > 0 && value.Length > mlcMaxLength)
            {
                LogMlcNodeViolation(value, elementName, mlcMaxLength);
                if (ReaderSettings.FixMlcRequirementViolations)
                {
                    value = value.Substring(0, mlcMaxLength);
                }
                else
                {
                    throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcNodeViolation,
                            value,
                            elementName));
                }
            }
            return value;
        }

        /// <summary>
        /// Validates if an id string is a valid id..
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true if valid, false if not, or if null or String.Empty.</returns>
        internal static bool ValidateId(string id)
        {
            if (String.IsNullOrEmpty(id)) return false;
            try
            {
                XmlConvert.VerifyNCName(id);
            }
            catch (XmlException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieve specified string identifier attribute.
        /// </summary>
        /// <remarks>
        /// This is to cache string attributes for which the type is xs:ID or xs:IDREF.
        /// </remarks>
        /// <param name="node">Node on which to find the attribute.</param>
        /// <param name="value">Ref value to return the attribute value.</param>
        /// <param name="attributeName">Name of attribute to obtain.</param>
        /// <param name="defaultValue">Default value to set if attribute is missing.</param>
        /// <param name="spm">SPM for the string attribute. 0 or less means "no effect".</param>
        /// <param name="mlcMaxLength">Max length for MLC validation. 0 or less means "no effect".</param>
        /// <returns></returns>
        internal string CacheRequiredStringIdentifierAttribute(
            XPathNavigator node,
            ref string value,
            string attributeName,
            string defaultValue,
            int spm,
            int mlcMaxLength)
        {
            // if the value already is cached, just return it
            if (value != null) return value;
            // otherwise go get it
            string attributeValue = null;
            attributeValue = CacheStringAttribute(node, ref attributeValue, attributeName, String.Empty, spm, mlcMaxLength);
            // validate the id
            if (String.IsNullOrEmpty(attributeValue))
            {
                LogRequiredAttributeMissing(attributeName, node.Name, GetLogInfo_NodeText(node));
                value = defaultValue;
            }
            else if (ValidateId(attributeValue))
            {
                value = attributeValue;
            }
            else
            {
                LogBadAttribute(attributeValue, attributeName, node.Name, defaultValue, ValidatorResources.InvalidIdentifier);
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Same as CacheStringIdentifierAttribute, but doesn't throw if the id is absent.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="spm">SPM for the string attribute. 0 or less means "no effect".</param>
        /// <param name="mlcMaxLength">Max length for MLC validation. 0 or less means "no effect".</param>
        /// <returns><c>String.Empty</c> if the attribute does not exist.</returns>
        internal string CacheOptionalStringIdentifierAttribute(
            XPathNavigator node,
            ref string value,
            string attributeName,
            string defaultValue,
            int spm,
            int mlcMaxLength)
        {
            // if the value already is cached, just return it
            if (value != null) return value;
            // otherwise go get it
            string attributeValue = null;
            attributeValue = CacheStringAttribute(node, ref attributeValue, attributeName, defaultValue, spm, mlcMaxLength);
            // validate the id if it exists
            if (!String.IsNullOrEmpty(attributeValue))
            {
                if (ValidateId(attributeValue))
                {
                    value = attributeValue;
                }
                else
                {
                    LogBadAttribute(attributeValue, attributeName, node.Name, defaultValue, ValidatorResources.InvalidIdentifier);
                    value = defaultValue;
                }
            }
            else
            {
                value = String.Empty;
            }
            return value;
        }

        /// <summary>
        /// Retrieves the specified attribute on a node if it hasn't already been retrieved.  If the <paramref name="value"/>
        /// is non-null, the value has been retrieved previously and is returned immediately.
        /// </summary>
        /// <param name="node">Points to the node on which to find the attribute.  If this value is null, the default value
        /// is returned.</param>
        /// <param name="value">A reference to a Nullable&lt;T&gt; which is set depending on the value of the attribue.  If
        /// this value is already non-null, this method simply returns that value.</param>
        /// <param name="attributeName">The name of the attribute to check.</param>
        /// <param name="defaultValue">The default value to use if the attribute is missing or illegal.</param>
        /// <param name="attributeNamespace">Namespace of the attribute.</param>
        /// <returns>As a convenience, the return value is the same value that is set on the <paramref name="value"/>.</returns>
        /// <exception cref="InvalidPackageException">If <c>PackageValidatorSettings.ScormRequirementValidation=ValidationBehavior.Enforce</c>
        /// and the attribute value is neither "true" nor "false".</exception>
        /// <remarks>It is neither a warning nor an error if the attribute is missing entirely from the node.  In that case,
        /// the <paramref name="defaultValue"/> is used.
        /// <para>
        /// The <c>PackageValidatorSettings.ScormRequirementValidation</c> determines whether the default value should be used if the attribute value is illegal.
        /// In <c>ValidationBehavior.LogWarning</c> mode, the default value is used and a warning is issued to the log.
        /// In <c>ValidationBehavior.LogError</c> mode, the default value is used and an error is issued to the log.
        /// In <c>ValidationBehavior.Enforce</c> mode, an error is issued to the log and an InvalidPackageException is thrown.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Globalization", "CA1308")] // ToLowerInvariant() is not used in a comparison; no need to use ToUpperInvariant() instead.
        internal T? CacheAttribute<T>(
            XPathNavigator node,
            ref Nullable<T> value,
            string attributeName,
            Nullable<T> defaultValue,
            string attributeNamespace) where T: struct
        {
            // If node is null, return the default value immediately.
            if (null == node)
            {
                value = defaultValue;
                return value;
            }
            // If the value is null retrieve the attribute value from the node. Otherwise just return the value.Value.
            if (null == value)
            {
                XPathNavigator attribute = node.Clone();
                if (!attribute.MoveToAttribute(attributeName, attributeNamespace))
                {
                    // if the attribute doesn't exist, use the default value
                    value = defaultValue;
                }
                else
                {
                    try
                    {
                        // set the value to the attribute value
                        if(typeof(T) == typeof(TimeSpan))
                        {
                            // this method with this type is only called in the context of SCORM 2004
                            // packages, so this is a valid assumption
                            value = (T)(object)Utilities.StringToTimeSpanScormV1p3(attribute.Value);
                        }
                        else
                        {
                            value = (T)attribute.ValueAs(typeof(T), null);
                        }
                    }
                    catch (FormatException)
                    {
                        // The attribute value was invalid, so set the value to the defaultValue
                        LogBadAttribute(
                                    attribute.Value,
                                    attributeName,
                                    node.Name, // full name including prefix
                                    defaultValue.ToString().ToLowerInvariant(),
                                    Helper.GetLogInfo_NodeText(node));
                        value = defaultValue;
                    }
                    catch (OverflowException)
                    {
                        // The attribute value was invalid, so set the value to the defaultValue
                        LogBadAttribute(
                                    attribute.Value,
                                    attributeName,
                                    node.Name, // full name including prefix
                                    defaultValue.ToString().ToLowerInvariant(),
                                    Helper.GetLogInfo_NodeText(node));
                        value = defaultValue;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Retrieves the specified attribute on a node if it hasn't already been retrieved.  If the <paramref name="value"/>
        /// is non-null, the value has been retrieved previously and is returned immediately.
        /// </summary>
        /// <param name="node">Points to the node on which to find the attribute.  If this value is null, the default value
        /// is returned.</param>
        /// <param name="value">A reference to a Nullable&lt;T&gt; which is set depending on the value of the attribue.  If
        /// this value is already non-null, this method simply returns that value.</param>
        /// <param name="attributeName">The name of the attribute to check.</param>
        /// <param name="defaultValue">The default value to use if the attribute is missing or illegal.</param>
        /// <returns>As a convenience, the return value is the same value that is set on the <paramref name="value"/>.</returns>
        /// <exception cref="InvalidPackageException">If If <c>PackageValidatorSettings.ScormRequirementValidation=ValidationBehavior.Enforce</c>
        /// and the attribute value is neither "true" nor "false".</exception>
        /// <remarks>It is neither a warning nor an error if the attribute is missing entirely from the node.  In that case,
        /// the <paramref name="defaultValue"/> is used.
        /// <para>
        /// The <c>PackageValidatorSettings.ScormRequirementValidation</c> determines whether the default value should be used if the attribute value is illegal.
        /// In <c>ValidationBehavior.LogWarning</c> mode, the default value is used and a warning is issued to the log.
        /// In <c>ValidationBehavior.LogError</c> mode, the default value is used and an error is issued to the log.
        /// In <c>ValidationBehavior.Enforce</c> mode, an error is issued to the log and an InvalidPackageException is thrown.
        /// </para>
        /// </remarks>
        internal T CacheAttribute<T>(
            XPathNavigator node,
            ref Nullable<T> value,
            string attributeName,
            T defaultValue) where T : struct
        {
            return CacheAttribute<T>(node, ref value, attributeName, defaultValue, String.Empty).Value;
        }

        /// <summary>
        /// Cache token attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeNamespace"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308")] // ToLowerInvariant() is not used in a comparison; no need to use ToUpperInvariant() instead.
        internal T CacheTokenAttribute<T>(
            XPathNavigator node,
            ref Nullable<T> value,
            string attributeName,
            string attributeNamespace,
            T defaultValue) where T : struct
        {
            if (null == value)
            {
                // if node is null, the default is appropriate.  Otherwise, try to find the attribute.
                if (node == null)
                {
                    value = defaultValue;
                }
                else
                {
                    XPathNavigator attribute = node.Clone();
                    if (attribute.MoveToAttribute(attributeName, attributeNamespace))
                    {
                        // the attribute exists, so see if it is a valid token value.
                        try
                        {
                            // set the value to the attribute value, but with the first character upper-case to match the enum
                            string strValue = attribute.Value.Substring(0, 1).ToUpperInvariant() + attribute.Value.Substring(1);
                            value = (T)Enum.Parse(typeof(T), strValue);
                        }
                        catch (ArgumentException)
                        {
                            // strDefault is the enum value with the first character lower-case
                            string strDefault = defaultValue.ToString();
                            strDefault = strDefault.Substring(0, 1).ToLowerInvariant() + strDefault.Substring(1);

                            LogBadAttribute(attribute.Value, attributeName, node.Name, strDefault, Helper.GetLogInfo_NodeText(node));
                            value = defaultValue;
                        }
                    }
                    else
                    {
                        // by default, set the value to defaultValue
                        value = defaultValue;
                    }
                }
            }
            return value.Value;
        }

        internal T CacheTokenAttribute<T>(
            XPathNavigator node,
            ref Nullable<T> value,
            string attributeName,
            T defaultValue) where T : struct
        {
            return CacheTokenAttribute<T>(node, ref value, attributeName, String.Empty, defaultValue);
        }

        /// <summary>
        /// Retrieves the specified xs:anyURI attribute on a node if it hasn't already been retrieved.  If the <paramref name="value"/>
        /// is non-null, the value has been retrieved previously and is returned immediately.
        /// </summary>
        /// <param name="node">Points to the node on which to find the xs:anyURI attribute.</param>
        /// <param name="value">A reference to a string? which is set to the xs:anyURI value of the attribue or <c>String.Empty</c>.  If
        /// this value is already non-null, this method simply returns that value.</param>
        /// <param name="attributeName">The name of the attribute to check.</param>
        /// <param name="spm">SPM for the string attribute. 0 or less means "no effect".</param>
        /// <param name="mlcMaxLength">Max length for MLC validation. 0 or less means "no effect".</param>
        /// <returns>As a convenience, the return value is the same value that is set on the <paramref name="value"/>.</returns>
        /// <exception cref="InvalidPackageException">If <c>PackageValidatorSettings.ScormRequirementValidation=ValidationBehavior.Enforce</c>
        /// and the attribute value is invalid.</exception>
        /// <remarks>It is neither a warning nor an error if the attribute is missing entirely from the node.  In that case,
        /// <c>String.Empty</c> is returned.
        /// <para>
        /// The default value is <c>String.Empty</c>.
        /// The <c>PackageValidatorSettings.ScormRequirementValidation</c> determines whether the default value should be used if the attribute value is illegal.
        /// In <c>ValidationBehavior.LogWarning</c> mode, the default value is used and a warning is issued to the log.
        /// In <c>ValidationBehavior.LogError</c> mode, the default value is used and an error is issued to the log.
        /// In <c>ValidationBehavior.Enforce</c> mode, an error is issued to the log and an InvalidPackageException is thrown.
        /// </para>
        /// </remarks>
        internal string CacheAnyUriAttribute(
            XPathNavigator node,
            ref string value,
            string attributeName,
            int spm,
            int mlcMaxLength)
        {
            // If the value is null retrieve the attribute value from the node. Otherwise just return the value.Value.
            if (null == value)
            {
                string str = node.GetAttribute(attributeName, String.Empty);
                if (spm > 0 && str.Length > spm)
                {
                    LogSPMViolation(str, attributeName, node, spm);
                }
                if (mlcMaxLength > 0 && str.Length > mlcMaxLength)
                {
                    LogMlcViolation(str, attributeName, mlcMaxLength, node);
                    if (ReaderSettings.FixMlcRequirementViolations)
                    {
                        str = str.Substring(0, mlcMaxLength);
                    }
                    else
                    {
                        throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcViolation,
                                str,
                                attributeName,
                                GetLogInfo_NodeText(node)));
                    }
                }
                // If the attribute is missing, return String.Empty.  If it is a well-formed URI, return it.
                if (String.IsNullOrEmpty(str) || Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute))
                {
                    value = str;
                }
                else
                {
                    // If the attribute value is not a valid URI:
                    // In Relaxed mode, put a warning in the error log and return String.Empty.
                    // In Strict mode, put an error in the error log and throw InvalidPackageException.
                    LogBadAttribute(str, attributeName, node.Name, ValidatorResources.EmptyString, Helper.GetLogInfo_NodeText(node));
                    value = String.Empty;
                }
            }
            return value;
        }

        internal class ChildNode
        {
            internal bool m_cached;
            internal XPathNavigator m_node;
        }

        /// <summary>
        /// Sets an XPathNavigator pointing to the first named child node, and returns it.
        /// </summary>
        /// <returns>An XPathNavigator pointing to the requested child, or <c>null</c> if no such child exists.</returns>
        /// <param name="childNode">Reference to a cache to store the return value.  If this is already set, this
        /// method simply returns.</param>
        /// <param name="node">Points to node containing the desired child node.</param>
        /// <param name="childNodeName">Name of the child node.</param>
        /// <param name="namespaceUri">Namespace of the child node.</param>
        /// <param name="checkDuplicates">True to check if there are duplicate nodes and to log error/warning if so.</param>
        internal XPathNavigator CacheFirstChild(ref ChildNode childNode, XPathNavigator node, string childNodeName, string namespaceUri, bool checkDuplicates)
        {
            if (!childNode.m_cached)
            {
                childNode.m_node = GetFirstChild(node, childNodeName, namespaceUri, checkDuplicates);
                childNode.m_cached = true;
            }
            return childNode.m_node;
        }

        /// <summary>
        /// Sets an XPathNavigator pointing to the first named child node, and returns it.
        /// </summary>
        /// <returns>An XPathNavigator pointing to the requested child, or <c>null</c> if no such child exists.</returns>
        /// <param name="childNode">Reference to a cache to store the return value.  If this is already set, this
        /// method simply returns.</param>
        /// <param name="node">Points to node containing the desired child node.</param>
        /// <param name="childNodeName">Name of the child node.</param>
        /// <param name="namespaceUri">Namespace of the child node.</param>
        internal XPathNavigator CacheFirstChild(ref ChildNode childNode, XPathNavigator node, string childNodeName, string namespaceUri)
        {
            return CacheFirstChild(ref childNode, node, childNodeName, namespaceUri, true);
        }

        /// <summary>
        /// Gets an XPathNavigator pointing to the first named child node, and returns it.
        /// </summary>
        /// <returns>An XPathNavigator pointing to the requested child, or <c>null</c> if no such child exists.</returns>
        /// <param name="node">Points to node containing the desired child node.</param>
        /// <param name="childNodeName">Name of the child node.</param>
        /// <param name="namespaceUri">Namespace of the child node.</param>
        /// <param name="checkDuplicates">True to check if there are duplicate nodes and to log error/warning if so.</param>
        internal XPathNavigator GetFirstChild(XPathNavigator node, string childNodeName, string namespaceUri, bool checkDuplicates)
        {
            XPathNavigator childNode = null;
            XPathNodeIterator itr = node.SelectChildren(childNodeName, namespaceUri);
            if (itr.MoveNext())
            {
                // more than one is a warning or error
                if (checkDuplicates && itr.Count > 1)
                {
                    LogInvalidDuplicateNode(childNodeName, namespaceUri == Strings.MlcNamespace ? true : false,
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.GenericDuplicate,
                                Helper.GetNodeText(node)));
                }
                childNode = itr.Current;
            }
            return childNode;
        }

        /// <summary>
        /// Gets an XPathNavigator pointing to the first named child node, and returns it.
        /// </summary>
        /// <returns>An XPathNavigator pointing to the requested child, or <c>null</c> if no such child exists.</returns>
        /// <param name="node">Points to node containing the desired child node.</param>
        /// <param name="childNodeName">Name of the child node.</param>
        /// <param name="namespaceUri">Namespace of the child node.</param>
        internal XPathNavigator GetFirstChild(XPathNavigator node, string childNodeName, string namespaceUri)
        {
            return GetFirstChild(node, childNodeName, namespaceUri, true);
        }


        /// <summary>
        /// Creates an <Typ>XPathExpression</Typ> useful for an XPath query in the manifest, using the
        /// common namespaces needed.
        /// </summary>
        /// <param name="nav">The <Typ>XPathNavigator</Typ> on which the XPath query will be called.</param>
        /// <param name="xPath">The XPath query string, including the common namespace prefixes.  E.g "imsss:mapInfo" not "mapInfo".</param>
        /// <returns>XPath expression to use in e.g. a Select() on an XPath navigator.</returns>
        internal static XPathExpression CreateExpressionV1p3(XPathNavigator nav, string xPath)
        {
            XPathExpression expr = nav.Compile(xPath);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace(Helper.Strings.Imscp, Helper.Strings.ImscpNamespaceV1p3);
            manager.AddNamespace(Helper.Strings.Imsss, Helper.Strings.ImsssNamespace);
            manager.AddNamespace(Helper.Strings.Adlcp, Helper.Strings.AdlcpNamespaceV1p3);
            manager.AddNamespace(Helper.Strings.Adlseq, Helper.Strings.AdlseqNamespace);
            manager.AddNamespace(Helper.Strings.Adlnav, Helper.Strings.AdlnavNamespace);
            expr.SetContext(manager);
            return expr;
        }

        /// <summary>
        /// Creates an <Typ>XPathExpression</Typ> useful for an XPath query in the manifest, using the
        /// common namespaces needed.
        /// </summary>
        /// <param name="nav">The <Typ>XPathNavigator</Typ> on which the XPath query will be called.</param>
        /// <param name="xPath">The XPath query string, including the common namespace prefixes.  E.g "imsss:mapInfo" not "mapInfo".</param>
        /// <returns>XPath expression to use in e.g. a Select() on an XPath navigator.</returns>
        internal static XPathExpression CreateExpressionV1p2(XPathNavigator nav, string xPath)
        {
            XPathExpression expr = nav.Compile(xPath);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace(Helper.Strings.Imscp, Helper.Strings.ImscpNamespaceV1p2);
            manager.AddNamespace(Helper.Strings.Adlcp, Helper.Strings.AdlcpNamespaceV1p2);
            expr.SetContext(manager);
            return expr;
        }

        /// <summary>
        /// Creates an <Typ>XPathExpression</Typ> useful for an XPath query in the manifest, using the
        /// common namespaces needed.
        /// </summary>
        /// <param name="nav">The <Typ>XPathNavigator</Typ> on which the XPath query will be called.</param>
        /// <param name="xPath">The XPath query string, including the common namespace prefixes.  E.g "imsss:mapInfo" not "mapInfo".</param>
        /// <returns>XPath expression to use in e.g. a Select() on an XPath navigator.</returns>
        internal XPathExpression CreateExpression(XPathNavigator nav, string xPath)
        {
            return (ManifestVersion == ScormVersion.v1p2) ?
                CreateExpressionV1p2(nav, xPath) :
                CreateExpressionV1p3(nav, xPath);
        }

        /// <summary>
        /// Returns the text of the node from the beginning &lt; to the end &lt; 
        /// of the node (e.g. strips out the contents and end node of the OuterXml.)
        /// </summary>
        /// <param name="nav"></param>
        /// <exception cref="LearningComponentsInternalException">If <paramref name="nav"/> is null.</exception>
        internal static string GetNodeText(XPathNavigator nav)
        {
            if (nav == null) throw new LearningComponentsInternalException("MR011");
            return nav.OuterXml.Substring(0, nav.OuterXml.IndexOf('>')+1);
        }

        /// <summary>
        /// Returns the text to give as the "reason" in an error/warning message, that includes the 
        /// ValidatorResources.NodeXml plus the text of the node from the beginning &lt; to the end &lt; 
        /// of the node (e.g. strips out the contents and end node of the OuterXml.)
        /// </summary>
        internal static string GetLogInfo_NodeText(XPathNavigator nav)
        {
            return String.Format(CultureInfo.CurrentCulture, ValidatorResources.NodeXml, GetNodeText(nav));
        }

        /// <summary>
        /// Adds a warning to the log, if one was provided in the constructor.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <remarks>Actually this method just calls <c>ValidationResults.LogWarning()</c>.</remarks>
        private void LogWarning(string message)
        {
            ValidationResults.LogWarning(Log, message);
        }

        /// <summary>
        /// Adds an error to the log, if one was provided in the constructor.  Also throws a <Typ>InvalidPackageException</Typ>.
        /// </summary>
        private void LogErrorAndThrow(string message)
        {
            ValidationResults.LogError(Log, true, message);
        }

        /// <summary>
        /// Adds an error to the log, if one was provided in the constructor.
        /// </summary>
        private void LogError(string message)
        {
            ValidationResults.LogError(Log, false, message);
        }

        internal void LogSPMViolation(string attributeValue, string attributeName, XPathNavigator node, int spm)
        {
            // logs only contain up to the first 100 characters of the value. The message that it has been
            // truncated is only shown when LogReplacement is true.
            string log;
            if (attributeValue.Length > MaxValueLengthForLog)
            {
                attributeValue = attributeValue.Substring(0, MaxValueLengthForLog);
            }
            log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.SpmViolation,
                    attributeValue,
                    attributeName,
                    spm,
                    String.Format(CultureInfo.CurrentCulture, ValidatorResources.NodeName, node.Name));

            if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.Enforce)
            {
                LogErrorAndThrow(log);
            }
        }

        internal void LogSPMNodeViolation(string nodeValue, string nodeName, int spm)
        {
            // logs only contain up to the first 100 characters of the value.
            string log;
            if (nodeValue.Length > MaxValueLengthForLog)
            {
                nodeValue = nodeValue.Substring(0, MaxValueLengthForLog);
            }
            log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.SpmNodeViolation,
                    nodeValue,
                    nodeName,
                    spm);

            if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else if (ValidatorSettings.ScormRecommendationValidation == ValidationBehavior.Enforce)
            {
                LogErrorAndThrow(log);
            }
        }

        private const int MaxValueLengthForLog = 100;
        internal void LogMlcViolation(string attributeValue, string attributeName, int truncatedLength, XPathNavigator node)
        {
            // logs only contain up to the first 100 characters of the value. The message that it has been
            // truncated is only shown when LogReplacement is true.
            string log;
            if(attributeValue.Length > MaxValueLengthForLog)
            {
                attributeValue = attributeValue.Substring(0, MaxValueLengthForLog);
            }
            if (LogReplacement && ValidatorSettings.MlcRequirementValidation != ValidationBehavior.Enforce)
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcViolationTruncated,
                        attributeValue,
                        attributeName,
                        truncatedLength.ToString(CultureInfo.CurrentCulture),
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.NodeName, node.Name));
            }
            else
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcViolation,
                        attributeValue,
                        attributeName,
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.NodeName, node.Name));
            }

            if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.Enforce)
            {
                LogErrorAndThrow(log);
            }
        }

        internal void LogMlcNodeViolation(string nodeValue, string nodeName, int truncatedLength)
        {
            // logs only contain up to the first 100 characters of the value. The message that it has been
            // truncated is only shown when LogReplacement is true.
            string log;
            if (nodeValue.Length > MaxValueLengthForLog)
            {
                nodeValue = nodeValue.Substring(0, MaxValueLengthForLog);
            }
            if (LogReplacement && ValidatorSettings.MlcRequirementValidation != ValidationBehavior.Enforce)
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcNodeViolationTruncated,
                        nodeValue,
                        nodeName,
                        truncatedLength.ToString(CultureInfo.CurrentCulture));
            }
            else
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.MlcNodeViolation,
                        nodeValue,
                        nodeName);
            }

            if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else if (ValidatorSettings.MlcRequirementValidation == ValidationBehavior.Enforce)
            {
                LogErrorAndThrow(log);
            }
        }

        internal void LogBadAttribute(string attributeValue, string attributeName, string nodeName, string defaultValue, string reason)
        {
            string log;
            if (LogReplacement && ValidatorSettings.ScormRequirementValidation != ValidationBehavior.Enforce)
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.BadAttributeValueReplacement,
                        attributeValue,
                        attributeName,
                        nodeName,
                        defaultValue,
                        reason);
            }
            else
            {
                log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.BadAttributeValue,
                        attributeValue,
                        attributeName,
                        nodeName,
                        reason);
            }

            if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.None)
            {
                // do nothing
            }
            else if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else
            {
                LogErrorAndThrow(log);
            }
        }

        internal void LogIllegalAttributeInResourcePackage(string attributeValue, string attributeName, string nodeName)
        {
            string log;
            log = String.Format(CultureInfo.CurrentCulture, ValidatorResources.IllegalAttributeInResourcePackage,
                        attributeValue,
                        attributeName,
                        nodeName);

            if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.None)
            {
                // do nothing
            }
            else if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.LogWarning)
            {
                LogWarning(log);
            }
            else if (ValidatorSettings.ScormRequirementValidation == ValidationBehavior.LogError)
            {
                LogError(log);
            }
            else
            {
                LogErrorAndThrow(log);
            }
        }

        internal void LogEmptyReferencedObjective(XPathNavigator node)
        {
            string str = String.Format(CultureInfo.CurrentCulture, ValidatorResources.EmptyReferencedObjective, Helper.GetNodeText(node));
            switch (ValidatorSettings.ScormRequirementValidation)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(str);
                    break;
                case ValidationBehavior.LogError:
                    LogError(str);
                    break;
                default:
                    LogErrorAndThrow(str);
                    break;
            }
        }

        /// <summary>
        /// Logs or throws an invalid duplicate node error according to the validator settings.
        /// </summary>
        /// <param name="nodeName">Name of duplicated node.</param>
        /// <param name="mlcNamespace">true if the node is in the mlcNamespace (use ValidatorSettings.MlcRequirementValidation
        /// instead of ValidatorSettings.ScormRequirementValidation).</param>
        /// <param name="reason">Extra text to include in error message.</param>
        internal void LogInvalidDuplicateNode(string nodeName, bool mlcNamespace, string reason)
        {
            ValidationBehavior behavior;
            if (mlcNamespace)
            {
                behavior = ValidatorSettings.MlcRequirementValidation;
            }
            else
            {
                behavior = ValidatorSettings.ScormRequirementValidation;
            }

            switch (behavior)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidDuplicateNodesIgnored, nodeName, reason));
                    break;
                case ValidationBehavior.LogError:
                    LogError(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidDuplicateNodes, nodeName, reason));
                    break;
                default:
                    LogErrorAndThrow(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidDuplicateNodes, nodeName, reason));
                    break;
            }
        }

        internal void LogRequiredNodeMissing(string nodeName, string reason)
        {
            switch (ValidatorSettings.ScormRequirementValidation)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissingDefault, nodeName, reason));
                    break;
                case ValidationBehavior.LogError:
                    LogError(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissing, nodeName, reason));
                    break;
                default:
                    LogErrorAndThrow(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissing, nodeName, reason));
                    break;
            }
        }

        internal static void LogMissingManifestNode(ValidationResults log)
        {
            ValidationResults.LogError(log, true, 
                String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissing, Helper.Strings.Manifest, ValidatorResources.ManifestNodeMissing));
        }

        internal void LogRequiredAttributeMissing(string attributeName, string nodeName, string reason)
        {
            switch (ValidatorSettings.ScormRequirementValidation)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredAttributeMissing, attributeName, nodeName, reason));
                    break;
                case ValidationBehavior.LogError:
                    LogError(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredAttributeMissing, attributeName, nodeName, reason));
                    break;
                default:
                    LogErrorAndThrow(
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredAttributeMissing, attributeName, nodeName, reason));
                    break;
            }
        }

        internal void LogInvalidNodeValue(string nodeValue, string nodeName, string defaultValue, string reason)
        {
            switch (ValidatorSettings.ScormRequirementValidation)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(String.Format(CultureInfo.CurrentCulture, ValidatorResources.BadValueReplacement, nodeValue, nodeName, defaultValue, reason));
                    break;
                case ValidationBehavior.LogError:
                    LogError(String.Format(CultureInfo.CurrentCulture, ValidatorResources.BadValue, nodeValue, nodeName, reason));
                    break;
                default:
                    LogErrorAndThrow(String.Format(CultureInfo.CurrentCulture, ValidatorResources.BadValue, nodeValue, nodeName, reason));
                    break;
            }
        }

        internal void LogInvalidNode(string nodeName, string reason)
        {
            switch (ValidatorSettings.ScormRequirementValidation)
            {
                case ValidationBehavior.None:
                    // do nothing
                    break;
                case ValidationBehavior.LogWarning:
                    LogWarning(String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidNode, nodeName, reason));
                    break;
                case ValidationBehavior.LogError:
                    LogError(String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidNode, nodeName, reason));
                    break;
                default:
                    LogErrorAndThrow(String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidNode, nodeName, reason));
                    break;
            }
        }

        /// <summary>
        /// Handles parsing, validation, and evaluation of the &lt;adlcp:prerequisites&gt; script value.
        /// </summary>
        internal class PrerequisitesParser
        {
            public class PrerequisitesParserException : Exception
            {
                public PrerequisitesParserException() : base() { }
                public PrerequisitesParserException(string message) : base(message) { }
                public PrerequisitesParserException(string message, Exception innerException) : base(message, innerException) { }
                protected PrerequisitesParserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
            }

            /// <summary>
            /// A delegate method that accepts an &lt;item&gt; identifier attribute value and returns
            /// the <Typ>LessonStatus</Typ> value of the cmi.core.lesson_status for that item.
            /// </summary>
            /// <remarks>
            /// An implementation of this delegate should handle the case where
            /// a <Typ>LessonStatus</Typ> is requested for an identifier that does not exist.  One solution
            /// would to be to return <c>LessonStatus.NotAttempted</c> in that case.  Another solution
            /// would be to throw an exception.
            /// </remarks>
            /// <param name="identifier">The identifier of the lesson item.</param>
            /// <returns>The <Typ>LessonStatus</Typ> value of the cmi.core.lesson_status for the requested item.</returns>
            internal delegate LessonStatus GetLessonStatusDelegate(string identifier);

            /// <summary>
            /// This regex matches identifiers, where identifiers start with a letter or underscore and are
            /// followed by any characters other than those reserved to be "tokens."  "Tokens" are the
            /// characters &amp;, |, ~, =, &lt;&gt;, {, }, (, ), *, whitespace, a number that begins
            /// a word, and the comma character.
            /// </summary>
            /// <remarks>
            /// The technical SCORM definition of an identifier is more restrictive than what it matched
            /// by this expression.  However, since the identifier must also match an item identifier
            /// in the manifest, it isn't worth the overhead to ensure the legality of the characters
            /// used in the identifiers.
            /// <para>
            /// 
            /// </para>
            /// </remarks>
            private static Regex s_regexPrerequisites = new Regex(
                "\\s*(?<id>\\b[a-zA-Z_][^\\s&~|=<(){}*,]*)|(?<token>[&|~=(){}*,]|<>|\\b\\d+\\b)|(?<lessonStatus>\"passed\"|\"completed\"|\"browsed\"|\"failed\"|\"not attempted\"|\"incomplete\")\\s*");

            /// <summary>
            /// Verify that the characters in the string are whitespace from the startIndex to the endIndex.
            /// </summary>
            /// <returns>true if the analyzed characters are all whitespace.</returns>
            private static bool VerifyWhitespace(string str, int startIndex, int endIndex)
            {
                for (int charIndex = startIndex; charIndex < endIndex; charIndex++)
                {
                    if (!Char.IsWhiteSpace(str[charIndex])) return false;
                }
                return true;
            }

            /// <summary>
            /// Evaluates the script expression of the prerequisite according to the <Typ>LessonStatus</Typ> values
            /// of the lesson items referenced in the script, according to the "AICC CMI001 Guidelines for 
            /// Interoperability" document referenced in the "Version 1.2 SCORM Content Aggregation Model".
            /// </summary>
            /// <remarks>
            /// If the script expression is null or String.Empty, this method always returns <c>true</c>.
            /// </remarks>
            /// <param name="script">The original string value of the script contained in the &lt;adlcp:prerequisites&gt; 
            /// element.</param>
            /// <param name="getLessonStatus">The delegate method that returns the individual lesson
            /// item status for the items referenced in the prerequisite script. May not be null.
            /// </param>
            /// <returns><c>true</c> if the prerequisite script evaluates to <c>true</c>.  <c>false</c>
            /// if it does not.</returns>
            /// <exception cref="InvalidPackageException">The <paramref name="script"/> is syntactically
            /// invalid, or the <paramref name="getLessonStatus"/> delegate threw the exception.</exception>
            internal static bool Evaluate(string script, GetLessonStatusDelegate getLessonStatus)
            {
                if (getLessonStatus == null) throw new LearningComponentsInternalException("PP002");
                if (String.IsNullOrEmpty(script)) return true;
                // Break the script up into a collection of identifiers and tokens.
                MatchCollection matches = s_regexPrerequisites.Matches(script);

                // If there are no matches but the string isn't empty, it is an error.
                if (matches.Count == 0) throw new PrerequisitesParserException();
                
                // Make sure that all non-whitespace characters are accounted for in the matches.
                // Otherwise, this is an syntactically invalid script.
                // For instance, the string, p="bad", will omit the first quote.
                int scriptIndex = 0;
                for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    // Verify any characters from the previous script index to the new match index
                    // are whitespace only.
                    if (scriptIndex < matches[matchIndex].Index)
                    {
                        if (!VerifyWhitespace(script, scriptIndex, matches[matchIndex].Index))
                        {
                            throw new PrerequisitesParserException();
                        }
                    }
                    scriptIndex = matches[matchIndex].Index + matches[matchIndex].Length;
                }
                // Check the characters after the final match.
                if (scriptIndex < script.Length)
                {
                    if (!VerifyWhitespace(script, scriptIndex, script.Length))
                    {
                        throw new PrerequisitesParserException();
                    }
                }

                int index = 0;
                try
                {
                    LessonStatus value = ParseAndOrExpression(matches, ref index, getLessonStatus);
                    while (index < matches.Count)
                    {
                        value = ParseAndOrExpression(matches, ref index, getLessonStatus);
                    }
                    if (value == LessonStatus.Completed || value == LessonStatus.Passed)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Any of the parsing calls can throw this if matches[index] has index out of range.
                    // This will only happen if the script is written badly.
                    throw new PrerequisitesParserException();
                }
            }

            /// <summary>
            /// Parse a prerequisites "and/or expression" consisting of a series of one
            /// or more "equality expressions".
            /// </summary>
            /// <param name="matches">The collection of tokens.</param>
            /// <param name="index">The current parsing index into the collection.</param>
            /// <param name="getLessonStatus">The delegate method that returns the individual lesson
            ///     item status for the items referenced in the prerequisite script.</param>
            /// <returns>
            /// The evaluation of the operation.
            /// </returns>
            private static LessonStatus ParseAndOrExpression(MatchCollection matches, ref int index, 
                GetLessonStatusDelegate getLessonStatus)
            {
                // parse an "equality expression"
                LessonStatus leftValue = ParseEqualityExpression(matches, ref index, getLessonStatus);

                while (index < matches.Count)
                {
                    // if there are more tokens to parse, we should be on an "&", "|", or ")".
                    string token = GetToken(matches[index]);
                    if (String.Compare(token, "|", StringComparison.Ordinal) == 0)
                    {
                        // skip the "|"
                        index++;

                        // this is an "|" operation, which is "completed" if either side is "completed" or "passed".
                        //
                        // note that "|" operations have lower order precedence to "&" operations, so parse the right
                        // value as another and/or operation.

                        LessonStatus rightValue = ParseAndOrExpression(matches, ref index, getLessonStatus);
                        if (leftValue == LessonStatus.Completed || leftValue == LessonStatus.Passed
                            || rightValue == LessonStatus.Completed || rightValue == LessonStatus.Passed)
                        {
                            leftValue = LessonStatus.Completed;
                        }
                        else
                        {
                            leftValue = LessonStatus.Incomplete;
                        }
                    }
                    else if (String.Compare(token, "&", StringComparison.Ordinal) == 0)
                    {
                        // skip the "&"
                        index++;

                        // this is an "&" operation, which is "completed" if both sides are "completed" or "passed".
                        // Parse the right value as an equality expression.
                        //
                        // note that "&" operations have order precedence over "|" operations.

                        LessonStatus rightValue = ParseEqualityExpression(matches, ref index, getLessonStatus);
                        if ((leftValue == LessonStatus.Completed || leftValue == LessonStatus.Passed)
                            && (rightValue == LessonStatus.Completed || rightValue == LessonStatus.Passed))
                        {
                            leftValue = LessonStatus.Completed;
                        }
                        else
                        {
                            leftValue = LessonStatus.Incomplete;
                        }
                    }
                    else if (String.Compare(token, ")", StringComparison.Ordinal) == 0)
                    {
                        // do not skip the ")" since this should only be found if this was
                        // called from ParsePrimaryExpression(), which will consume the ")"
                        break;
                    }
                    else
                    {
                        throw new PrerequisitesParserException();
                    }
                }
                return leftValue;
            }

            private static LessonStatus ParseEqualityExpression(MatchCollection matches, ref int index, GetLessonStatusDelegate getLessonStatus)
            {
                // parse a "unary expression"; use the result as the initial value
                // of <totalValue>, which tracks the current value of the "equality
                // expression" as we parse it
                LessonStatus totalValue = ParseUnaryExpression(matches, ref index, getLessonStatus);

                // loop once for each operator/expression pair
                while (index < matches.Count)
                {
                    // check for "=" or "<>" operator
                    bool isEquals;
                    if (String.Compare(GetToken(matches[index]), "=", StringComparison.Ordinal) == 0)
                    {
                        isEquals = true;
                    }
                    else if (String.Compare(GetToken(matches[index]), "<>", StringComparison.Ordinal) == 0)
                    {
                        isEquals = false;
                    }
                    else
                    {
                        break;
                    }

                    // skip the "=" or "<>" operator
                    index++;

                    // parse another "unary expression"
                    LessonStatus value = ParseUnaryExpression(matches, ref index, getLessonStatus);

                    // update <totalValue>
                    if (isEquals)
                    {
                        return totalValue == value ? LessonStatus.Completed : LessonStatus.Incomplete;
                    }
                    else
                    {
                        return totalValue != value ? LessonStatus.Completed : LessonStatus.Incomplete;
                    }
                }

                // this is not an equality expression, it is simply a single identifier.
                return totalValue;
            }

            private static LessonStatus ParseUnaryExpression(MatchCollection matches, ref int index, GetLessonStatusDelegate getLessonStatus)
            {
                // parse "~" operators
                bool isNot = false;
                while (true)
                {
                    // check for "~" operator
                    if (String.Compare(GetToken(matches[index]), "~", StringComparison.Ordinal) == 0)
                    {
                        isNot = !isNot;
                        index++;
                    }
                    else
                        break;
                }

                // parse the "primary expression"
                LessonStatus value = ParsePrimaryExpression(matches, ref index, getLessonStatus);

                // apply the "~" operator(s), if any
                // "~" means that the student may enter the SCO as long as it has not been completed
                // (e.g. the status must be "incomplete", "failed", or "not attempted")
                if (isNot)
                {
                    switch(value)
                    {
                        case LessonStatus.Browsed:
                            // doesn't change
                            break;
                        case LessonStatus.NotAttempted:
                            value = LessonStatus.Completed;
                            break;
                        case LessonStatus.Completed:
                            value = LessonStatus.Incomplete;
                            break;
                        case LessonStatus.Incomplete:
                            value = LessonStatus.Completed;
                            break;
                        case LessonStatus.Passed:
                            value = LessonStatus.Failed;
                            break;
                        case LessonStatus.Failed:
                            value = LessonStatus.Passed;
                            break;
                    }
                }

                // done
                return value;
            }

            private static LessonStatus ParsePrimaryExpression(MatchCollection matches, ref int index, GetLessonStatusDelegate getLessonStatus)
            {
                // look for an identifier
                if (!String.IsNullOrEmpty(GetIdentifier(matches[index])))
                {
                    // return the value of the identifier and skip to the next match
                    return getLessonStatus(GetIdentifier(matches[index++]));
                }

                // look for a lesson status
                if (!String.IsNullOrEmpty(GetLessonStatus(matches[index])))
                {
                    // get the lesson status and skip to the next match
                    string lessonStatus = GetLessonStatus(matches[index++]);
                    switch(lessonStatus)
                    {
                        case "\"passed\"":
                            return LessonStatus.Passed;
                        case "\"completed\"":
                            return LessonStatus.Completed;
                        case "\"browsed\"":
                            return LessonStatus.Browsed;
                        case "\"failed\"":
                            return LessonStatus.Failed;
                        case "\"incomplete\"":
                            return LessonStatus.Incomplete;
                        case "\"not attempted\"":
                            return LessonStatus.NotAttempted;
                        default:
                            throw new LearningComponentsInternalException("PP001");
                    }
                }

                // look for a parenthesized expression
                if (String.Compare(GetToken(matches[index]), "(", StringComparison.Ordinal) == 0)
                {
                    // skip the <LParen>
                    index++;

                    // parse an "and/or expression", which consumes up to the <RParen>
                    LessonStatus value = ParseAndOrExpression(matches, ref index, getLessonStatus);

                    // consume the <RParen>.  If this is not a <RParen> it is an error
                    if (index >= matches.Count
                        || String.Compare(GetToken(matches[index++]), ")", StringComparison.Ordinal) != 0)
                    {
                        throw new PrerequisitesParserException();
                    }

                    // return the results of the ParseAndOrExpression()
                    return value;
                }

                // this is either a "Set" or an invalid expression.
                return ParseSet(matches, ref index, getLessonStatus);
            }

            private static LessonStatus ParseSet(MatchCollection matches, ref int index, GetLessonStatusDelegate getLessonStatus)
            {
                // a "Set" must begin with a digit
                int minimum;
                try
                {
                    minimum = Convert.ToInt32(GetToken(matches[index]), CultureInfo.InvariantCulture);
                }
                catch (ArgumentException)
                {
                    // this isn't a valid "Set"
                    throw new PrerequisitesParserException();
                }
                catch (OverflowException)
                {
                    // this isn't a valid "Set"
                    throw new PrerequisitesParserException();
                }
                catch (FormatException)
                {
                    // this isn't a valid "Set"
                    throw new PrerequisitesParserException();
                }

                // skip the digit
                index++;

                // we should now be at a "*" token
                if (String.Compare(GetToken(matches[index]), "*", StringComparison.Ordinal) == 0)
                {
                    // skip the "*"
                    index++;

                    // we should now be at a "{" token
                    if (String.Compare(GetToken(matches[index]), "{", StringComparison.Ordinal) == 0)
                    {
                        // skip the "{"
                        index++;

                        // we should now be at a comma separated list of identifiers.
                        // Count the number of completed identifiers
                        int numberComplete = 0;
                        while (!String.IsNullOrEmpty(GetIdentifier(matches[index])))
                        {
                            LessonStatus status = ParsePrimaryExpression(matches, ref index, getLessonStatus);
                            if (status == LessonStatus.Passed || status == LessonStatus.Completed)
                            {
                                numberComplete++;
                            }

                            // we should now be either at a <Comma> or a <RTBrace>
                            string token = GetToken(matches[index++]);
                            if (String.Compare(token, "}", StringComparison.Ordinal) == 0)
                            {
                                // we are at the end of the set.  If the numberComplete is equal or
                                // greater than the minimum, return "completed" else return "incomplete".
                                return (numberComplete >= minimum) ? LessonStatus.Completed : LessonStatus.Incomplete;
                            }
                            else if (String.Compare(token, ",", StringComparison.Ordinal) != 0)
                            {
                                throw new PrerequisitesParserException();
                            }
                        }
                    }
                }

                // this isn't a valid "Set"
                throw new PrerequisitesParserException();
            }

            private static string GetToken(Match match)
            {
                return match.Groups[2].Value;
            }

            private static string GetIdentifier(Match match)
            {
                return match.Groups[1].Value;
            }

            private static string GetLessonStatus(Match match)
            {
                return match.Groups[3].Value;
            }

        }
    }

    /// <summary>
    /// This is a reader of information contained in the manifest in a SCORM package.
    /// The package may be 1.2 or 2004 (1.3) version. The package may not be an LRM.
    /// </summary>
    /// <remarks>
    /// Most of the methods on this class follow the terminology of SCORM 2004. Unless otherwise noted,
    /// they are valid for both 1.2 and 2004 content. 
    /// 
    /// The reading of information in the manifest is lazy. When an object is created, the data for the 
    /// object to be read is not parsed. When a property value is requested, that property is read from the 
    /// XPathNavigator.
    /// 
    /// Exceptions are only thrown if there's no log and reasonable behavior is not possible.
    /// 
    /// There is an option to add errors and warnings to a log provided by the application. 
    /// If there is a serious error (and exceptions should be
    /// thrown), then the exception is thrown.
    /// 
    /// When a list is accessed, the list is created but the elements in the list are only created when they are needed.
    /// 
    /// If <c>PackageValidatorSettings.ScormRequirementValidation=ValidationBehavior.LogWarning</c> or 
    /// <c>PackageValidatorSettings.ScormRequirementValidation=ValidationBehavior.LogError</c> is set, then exceptions
    /// are avoided whenever a reasonable default 
    /// is possible. For instance, if an element required by the SCORM spec does not exist in the 
    /// manifest, String.Empty or some other possible valid value is returned.
    /// 
    /// </remarks>
    public class ManifestReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;

        // private member variables to hold attributes, nodes, etc.
        private PackageReader m_packageReader;
        private Uri m_xmlBase;
        private Uri m_resourcesXmlBase;
        private bool m_gotResourcesXmlBase;
        private string m_identifier;
        private IDictionary<string, SequencingNodeReader> m_sequencing;
        private IDictionary<string, ResourceNodeReader> m_resources;
        private Helper.ChildNode m_resourcesNode = new Helper.ChildNode();
        private PackageType? m_packageType;
        private Helper.ChildNode m_metadata = new Helper.ChildNode();
        private ReadOnlyCollection<OrganizationNodeReader> m_organizations;
        private bool m_defaultOrganizationValid; // true when m_defaultOrganization is valid
        private OrganizationNodeReader m_defaultOrganization;
        private Helper.ChildNode m_organizationsNode = new Helper.ChildNode();
        private string m_version;
        private PackageFormat m_packageFormat;
        private bool m_gotPackageFormat;
        private Dictionary<string, bool> m_activityIdentifiers;


        /// <summary>
        /// Create a ManifestReader from an <Typ>XPathNavigator</Typ>, using the supplied settings.
        /// </summary>
        /// <param name="packageReader">The <Typ>PackageReader</Typ> to use when <Typ>MetadataNodeReader</Typ>
        /// requires files outside of the manifest.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="packageValidatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">The log in which to write error and warning log entries.  Cannot be null if
        /// a <c>ValidationBehavior.LogWarning</c> or <c>ValidationBehavior.LogError</c> is chosen
        /// for one of the settings in <paramref name="validatorSettings"/></param>
        /// <param name="manifestNode">The <Typ>XPathNavigator</Typ>, pointing to the &lt;manifest&gt; node.</param>
        internal ManifestReader(PackageReader packageReader, ManifestReaderSettings manifestSettings, PackageValidatorSettings packageValidatorSettings,
            bool logReplacement, ValidationResults log, XPathNavigator manifestNode)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            if (manifestSettings == null) throw new LearningComponentsInternalException("MR012");
            if (packageValidatorSettings == null) throw new LearningComponentsInternalException("MR013");
            if (log == null && packageValidatorSettings.RequiresLog()) throw new LearningComponentsInternalException("MR014");
            if (packageReader == null) throw new ArgumentNullException("packageReader");
            m_packageReader = packageReader;
            m_helper = Helper.CreateManifestHelper(manifestNode, manifestSettings, packageValidatorSettings, logReplacement, log);
            m_xmlBase = m_helper.CombineXmlBaseAttribute(null, manifestNode);
        }

        /// <summary>
        /// Create a ManifestReader from a stream containing imsmanifest.xml.
        /// </summary>
        /// <param name="packageReader">The <Typ>PackageReader</Typ> to use when <Typ>MetadataNodeReader</Typ>
        /// requires files outside of the manifest.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">If provided, errors and warnings encountered when reading from the 
        /// manifest are added to the log. See the ValidationResults class for 
        /// information about errors and warnings.</param>
        /// <returns>
        /// <Typ>ManifestReader</Typ>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="InvalidPackageException">There is no valid manifest..</exception>
        /// <exception cref="XmlException">An error was encountered in the XML data.</exception>
        internal static ManifestReader Create(PackageReader packageReader, ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            Stream stream = packageReader.GetFileStream("imsmanifest.xml");
            return new ManifestReader(packageReader, manifestSettings, validatorSettings, logReplacement, log, ManifestStreamToXPathNavigator(stream, log));
        }
        internal static ManifestReader Create(PackageReader packageReader, ManifestReaderSettings manifestSettings,
            bool logReplacement, ValidationResults log)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            Stream stream = packageReader.GetFileStream("imsmanifest.xml");

            PackageValidatorSettings validatorSettings;
            if (manifestSettings.FixScormRequirementViolations)
            {
                // These are the validator settings used by the unit tests.
                validatorSettings = new PackageValidatorSettings(ValidationBehavior.LogWarning, ValidationBehavior.None, ValidationBehavior.Enforce, ValidationBehavior.None);
            }
            else
            {
                validatorSettings = new PackageValidatorSettings(ValidationBehavior.Enforce, ValidationBehavior.None, ValidationBehavior.Enforce, ValidationBehavior.None);
            }
            return new ManifestReader(packageReader, manifestSettings, validatorSettings, logReplacement, log, ManifestStreamToXPathNavigator(stream, log));
        }

        /// <summary>
        /// Finds the &lt;manifest&gt; node in the provided stream, and returns an XPathNavigator pointing to it.
        /// This will work for both SCORM 1.2 and 2004 manifests.
        /// </summary>
        internal static XPathNavigator ManifestStreamToXPathNavigator(Stream stream, ValidationResults log)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            if (stream == null) throw new ArgumentNullException("stream");
            XPathDocument doc = new XPathDocument(stream);
            XPathNavigator nav = doc.CreateNavigator();
            // move to the first manifest node. First try SCORM 2004.
            XPathExpression expr = Helper.CreateExpressionV1p3(nav, Helper.Strings.Imscp + ":" + Helper.Strings.Manifest);
            XPathNavigator manifest = nav.SelectSingleNode(expr);
            if (manifest == null)
            {
                // Didn't find a SCORM 2004 manifest node.  Try SCORM 1.2.
                expr = Helper.CreateExpressionV1p2(nav, Helper.Strings.Imscp + ":" + Helper.Strings.Manifest);
                manifest = nav.SelectSingleNode(expr);
                if (manifest == null)
                {
                    Helper.LogMissingManifestNode(log);
                }
            }
            return manifest;
        }

        /// <summary>
        /// Create a ManifestReader object that provides access to information within the manifest. 
        /// </summary>
        /// <param name="packageReader">The <Typ>PackageReader</Typ> to use that contains the manifest.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <remarks>The caller should not dispose the <Typ>PackageReader</Typ> during the lifetime of this object.</remarks>
        /// <exception cref="InvalidPackageException">There is no valid manifest in the <paramref name="packageReader"/>.</exception>
        public ManifestReader(PackageReader packageReader, ManifestReaderSettings manifestSettings)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            if (manifestSettings == null) throw new ArgumentNullException("manifestSettings");
            if (packageReader == null) throw new ArgumentNullException("packageReader");
            m_packageReader = packageReader;
            PackageValidatorSettings validatorSettings = new PackageValidatorSettings(ValidationBehavior.None, ValidationBehavior.None,
                ValidationBehavior.None, ValidationBehavior.None);
            // if FixScormRequirementViolations is false, ScormRequirementValidation should be "Enforce" because exceptions must be thrown
            // because a default value cannot be used.
            if (manifestSettings.FixScormRequirementViolations == false) validatorSettings.ScormRequirementValidation = ValidationBehavior.Enforce;
            // if fixMlcRequirementViolations is false, MlcRequirementValidation should be "Enforce" because exceptions must be thrown
            // because string values cannot be truncated to fit into the database
            if (manifestSettings.FixMlcRequirementViolations == false) validatorSettings.MlcRequirementValidation = ValidationBehavior.Enforce;
            XPathNavigator manifestNode = packageReader.CreateManifestNavigator();
            m_helper = Helper.CreateManifestHelper(manifestNode, manifestSettings, validatorSettings, false, null);
            m_xmlBase = m_helper.CombineXmlBaseAttribute(null, manifestNode);
        }

        /// <summary>
        /// Return the log of messages relating to this manifest.
        /// </summary>
        internal ValidationResults Log
        {
            get
            {
                return m_helper.Log;
            }
        }

        /// <summary>
        /// Return whether to write the message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.
        /// </summary>
        internal bool LogReplacement
        {
            get
            {
                return m_helper.LogReplacement;
            }
        }

        /// <summary>
        /// Add an activity and its child activities identifiers to the dictionary.
        /// Used by the ActivityIdentifiers property.
        /// </summary>
        private void AddActivityIdentifiers(ActivityNodeReader activity, Dictionary<string, bool> activityIdentifiers)
        {
            if (!activityIdentifiers.ContainsKey(activity.Id)) activityIdentifiers.Add(activity.Id, true);
            // recursively add child activity identifiers
            foreach (ActivityNodeReader child in activity.ChildActivities)
            {
                AddActivityIdentifiers(child, activityIdentifiers);
            }
        }

        /// <summary>
        /// Used by ActivityNodeReader.Prerequisites
        /// </summary>
        internal Dictionary<string, bool> ActivityIdentifiers
        {
            get
            {
                if (m_activityIdentifiers == null)
                {
                    // create a list of activity identifiers for the prerequisites node (only needed if
                    // the manifest is SCORM 1.2)  Use a dictionary with a dummy value for efficient searching.
                    m_activityIdentifiers = new Dictionary<string, bool>(100);
                    if (PackageFormat == PackageFormat.V1p2)
                    {
                        foreach (OrganizationNodeReader organization in Organizations)
                        {
                            foreach (ActivityNodeReader activity in organization.Activities)
                            {
                                AddActivityIdentifiers(activity, m_activityIdentifiers);
                            }
                        }
                    }
                }
                return m_activityIdentifiers;
            }
        }

        /// <summary>
        /// Returns the id in the manifest that uniquely identifies the package.
        /// </summary>
        /// <exception cref="InvalidPackageException">This exception is thrown if the 
        /// attribute does not exist in the package and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.
        /// </exception>
        /// <remarks>
        /// Although this is a SCORM required attribute, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set,
        /// a default value of <c>MANIFEST</c> will be returned if the attribute is missing.
        /// </remarks>
        public string Id
        {
            get
            {
                return m_helper.CacheRequiredStringIdentifierAttribute(m_helper.Node, ref m_identifier, Helper.Strings.Identifier, "MANIFEST", 0, 0);
            }
        }

        /// <summary>
        /// The <Typ>PackageFormat</Typ> of the package, which identifies whether this is a SCORM 2004, SCORM 1.2, or
        /// LRM package.
        /// </summary>
        /// <exception cref="InvalidPackageException">This exception is thrown if the package format
        /// cannot be determined. A default is not assumed.</exception>
        public PackageFormat PackageFormat
        {
            get
            {
                // If there is a <LearningResource Version="1.0" xmlns="urn:schemas-microsoft-com:learning-resource">
                // node in the manifest, it is an LRM.
                if (!m_gotPackageFormat)
                {
                    Helper.ChildNode lrNode = new Helper.ChildNode();
                    m_helper.CacheFirstChild(ref lrNode, m_helper.Node, ManifestConverter.Strings.LearningResource,
                        ManifestConverter.Strings.XmlnsLearningResource, false);
                    if (lrNode.m_node == null)
                    {
                        if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                        {
                            m_packageFormat = PackageFormat.V1p2;
                        }
                        else
                        {
                            m_packageFormat = PackageFormat.V1p3;
                        }
                    }
                    else
                    {
                        m_packageFormat = PackageFormat.Lrm;
                    }
                    m_gotPackageFormat = true;
                }
                return m_packageFormat;
            }
        }

        /// <summary>
        /// Corresponds to the version attribute of the &lt;manifest&gt; node, which is an optional attribute used to
        /// distinguish between manifests with the same identifier.
        /// </summary>
        /// <remarks>Returns String.Empty if it doesn't exist.  The SPM is 20. A length longer than that is a violation
        /// of SCORM recommendations.</remarks>
        public string Version
        {
            get
            {
                return m_helper.CacheStringAttribute(m_helper.Node, ref m_version, Helper.Strings.Version, String.Empty, 20, 0);
            }
        }

        /// <summary>
        /// Returns the type of SCORM package. Currently, it is either a resource package or a content organization
        /// package.
        /// </summary>
        /// <remarks>
        /// If the manifest does not have any organization nodes, it is assumed to be a ResourcePackage.
        /// </remarks>
        public PackageType PackageType
        {
            get
            {
                if (null == m_packageType)
                {
                    // use MoveNext to check if there is an organization node. Don't use count, because
                    // count iterates through all of them, whereas MoveNext only creates the first (valid) one.
                    if (Organizations.GetEnumerator().MoveNext())
                    {
                        m_packageType = PackageType.ContentAggregation;
                    }
                    else
                    {
                        m_packageType = PackageType.Resource;
                    }
                }
                return m_packageType.Value;
            }
        }

        /// <summary>
        /// Return the xml:base attribute of the manifest, if it exists. 
        /// </summary>
        /// <remarks>Returns null if the xml:base attribute of the manifest does not exist.</remarks>
        public Uri XmlBase
        {
            get
            {
                return m_xmlBase;
            }
        }

        /// <summary>
        /// The xml:base attribute, resolved with the manifest's xml:base attribute, of the resources node, if it exists. Otherwise
        /// returns null.
        /// </summary>
        /// <remarks>
        /// <para>If the attribute is provided in the manifest but is not valid and 
        /// <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> was set 
        /// for this object, an empty Uri is returned.
        /// </para>
        /// <para>
        /// Example: if the &lt;manifest&gt; has xml:base="http://foo/" and the &lt;resources&gt; has xml:base="bar/",
        /// this returns "http://foo/bar/".  If the &lt;resources&gt; has xml:base="http://bar/", this returns "http://bar/"
        /// regardless of the &lt;manifest&gt; xml:base value.  If the &lt;resources&gt; has no xml:base, this returns
        /// the &lt;manifest&gt; xml:base, in this case, "http://foo/".
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The provided attribute is not valid, or
        /// there are more than one &lt;resources&gt; node,
        /// and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> was set.</exception>
        public Uri ResourcesXmlBase
        {
            get
            {
                if (!m_gotResourcesXmlBase)
                {
                    // get <resources>'s xml:base attribute.
                    // the following throws in strict mode if there are multiple <resources>
                    m_helper.CacheFirstChild(ref m_resourcesNode, m_helper.Node, Helper.Strings.Resources, m_helper.ImscpCurrentNamespace);
                    if (m_resourcesNode.m_node != null)
                    {
                        m_resourcesXmlBase = m_helper.CombineXmlBaseAttribute(m_xmlBase, m_resourcesNode.m_node);
                    }
                    else
                    {
                        m_resourcesXmlBase = m_xmlBase;
                    }
                    m_gotResourcesXmlBase = true;
                }
                return m_resourcesXmlBase;
            }
        }

        /// <summary>
        /// Package level metadata. Since there is no guarantee on the format of this node, 
        /// simply return a copy of the node. The metadata node is required in 1.3, optional in 1.2.
        /// </summary>
        /// <exception cref="InvalidPackageException">If the node is not provided in a SCORM 2004 
        /// package, and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> was set.</exception>
        /// <remarks>This node is required in SCORM 2004.
        /// </remarks>
        private XPathNavigator CreateMetadataNavigator()
        {
            if (!m_metadata.m_cached)
            {
                // the following throws in strict mode if there are multiple <metadata>
                m_helper.CacheFirstChild(ref m_metadata, m_helper.Node, Helper.Strings.Metadata, m_helper.ImscpCurrentNamespace);
            }
            // In SCORM 2004, metadata is required.  If m_metadata.m_node is still null, throw in strict mode, warning in relaxed
            if (m_metadata.m_node == null && m_helper.ManifestVersion == Helper.ScormVersion.v1p3)
            {
                m_helper.LogRequiredNodeMissing(Helper.Strings.Metadata, ValidatorResources.ManifestMetaDataMissing);
            }
            return m_metadata.m_node;
        }

        /// <summary>
        /// Return a reader of the package level metadata.
        /// </summary>
        /// <exception cref="InvalidPackageException">If the &lt;metadata&gt; node is not provided in a SCORM 2004 
        /// package, or if the &lt;location&gt; element of the metadata refers to a file in the package that can not
        /// be found, and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> was set.</exception>
        /// <remarks>The &lt;metadata&gt; node is required in SCORM 2004.  It is optional in SCORM 1.2.
        /// </remarks>
        public MetadataNodeReader Metadata
        {
            get
            {
                return MetadataNodeReader.Create(m_helper, m_packageReader, CreateMetadataNavigator(), m_xmlBase);
            }
        }

        /// <summary>
        /// Return a read-only list of organizations (<Typ>OrganizationNodeReader</Typ>) in the package.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The order of organziation nodes in this collection is the same as the order in the manifest.
        /// </para>
        /// <para>
        /// Although SCORM mandates that no two organization nodes may have the same identifier in a manifest,
        /// there is not check for that when building this collection.  It is up to the application to handle
        /// if multiple organizations have identical identifier attributes.
        /// </para>
        /// <para>
        /// This collection is not populated until methods or properties on it are called.  If during population,
        /// an organization node is encountered lacking an identifier, a warning will be added to the log if
        /// <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set, or an error will be added 
        /// to the log and an <Typ>InvalidPackageException</Typ> thrown if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// is set.
        /// </para>
        /// <para>
        /// The absence of &lt;organization&gt; nodes implies this is a Resource package, instead of a
        /// Content Aggregation package.
        /// </para>
        /// </remarks>
		/// <example>
        /// Loop through the available organizations in the manifest.
        /// <code language="C#">
		///     foreach (OrganizationNodeReader organization in manifest.Organizations)
		///     {
		///         // use the OrganizationNodeReader
		///     }
		/// </code></example>
        public ReadOnlyCollection<OrganizationNodeReader> Organizations
        {
            get
            {
                if (m_organizations == null)
                {
                    // get the organizations node
                    m_helper.CacheFirstChild(ref m_organizationsNode, m_helper.Node, Helper.Strings.Organizations, m_helper.ImscpCurrentNamespace);
                    if (m_organizationsNode.m_node == null)
                    {
                        // create an empty collection if there is no organization nodes.
                        m_organizations = new ReadOnlyCollection<OrganizationNodeReader>(
                            new ReadOnlyMlcCollection<OrganizationNodeReader>());
                    }
                    else
                    {
                        XPathExpression expr = m_helper.CreateExpression(m_organizationsNode.m_node,
                            Helper.Strings.Imscp + ":" + Helper.Strings.Organization);
                        ReadOnlyCollection<OrganizationNodeReader> organizations = new ReadOnlyCollection<OrganizationNodeReader>(
                            new ReadOnlyMlcCollection<OrganizationNodeReader>(m_organizationsNode.m_node, expr,
                                delegate(XPathNavigator node)
                                {
                                    // if the <organization>/identifier is missing or invalid, skip it
                                    string identifier = node.GetAttribute(Helper.Strings.Identifier, String.Empty);
                                    if (identifier == String.Empty)
                                    {
                                        m_helper.LogRequiredAttributeMissing(Helper.Strings.Identifier, node.Name, ValidatorResources.OrganizationIdentifierMissing);
                                        return null;
                                    }
                                    else if (Helper.ValidateId(identifier))
                                    {
                                        return new OrganizationNodeReader(m_packageReader, node, this);
                                    }
                                    else
                                    {
                                        m_helper.LogBadAttribute(identifier, Helper.Strings.Identifier, node.Name, ValidatorResources.ElementRemoved,
                                            ValidatorResources.InvalidIdentifier);
                                        return null;
                                    }
                                }));
                        m_organizations = organizations;
                    }
                }
                return m_organizations;
            }
        }

        /// <summary>
        /// The default organization that will be used, unless another is specified when attempting the package.
        /// This will return null content that does not include any organizations.
        /// <para>
        /// If there is an error in the default organization identifier, the first organization will be returned
        /// when <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The default organization for SCORM 1.2 content is the first organization node found in the manifest.
        /// </remarks>
        /// <exception cref="InvalidPackageException"><c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set, 
        /// and the &lt;organizations&gt; default attribute is invalid if this is a Content Aggregation package, or the default
        /// attribute exists if this is a Resource package, or the identifier for the default organization doesn't exist
        /// in the manifest.</exception>
        public OrganizationNodeReader DefaultOrganization
        {
            get
            {
                if (!m_defaultOrganizationValid)
                {
                    // get the organizations node
                    m_helper.CacheFirstChild(ref m_organizationsNode, m_helper.Node, Helper.Strings.Organizations, m_helper.ImscpCurrentNamespace);
                    // if the organizations node doesn't exist, return null.
                    // if the default attribute doesn't exist, return the first organization.
                    if (m_organizationsNode.m_node != null)
                    {
                        XPathNavigator organizationsNode = m_organizationsNode.m_node;
                        string defaultId = organizationsNode.GetAttribute(Helper.Strings.Default, String.Empty);
                        // defaultId is illegal in Resource packages
                        if (!String.IsNullOrEmpty(defaultId) && PackageType == PackageType.Resource)
                        {
                            m_helper.LogIllegalAttributeInResourcePackage(defaultId, Helper.Strings.Default, Helper.Strings.Organizations);
                        }
                        IEnumerator<OrganizationNodeReader> organization = Organizations.GetEnumerator();
                        if (organization.MoveNext())
                        {
                            // set to the first one by default, in case no others match, but usually the first is the default
                            m_defaultOrganization = organization.Current;
                            if (Helper.ValidateId(defaultId))
                            {
                                if (m_defaultOrganization.Id != defaultId)
                                {
                                    bool found = false;
                                    while (organization.MoveNext())
                                    {
                                        // search for defaultId
                                        if (organization.Current.Id == defaultId)
                                        {
                                            m_defaultOrganization = organization.Current;
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        m_helper.LogBadAttribute(defaultId, Helper.Strings.Default, Helper.Strings.Organizations, 
                                            m_defaultOrganization.Id,
                                            ValidatorResources.ReferenceNotFound);
                                    }
                                }
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(defaultId))
                                {
                                    m_helper.LogRequiredAttributeMissing(Helper.Strings.Default, Helper.Strings.Organizations, Helper.GetLogInfo_NodeText(organizationsNode));
                                }
                                else
                                {
                                    m_helper.LogBadAttribute(defaultId, Helper.Strings.Default, Helper.Strings.Organizations, m_defaultOrganization.Id,
                                        ValidatorResources.InvalidIdentifier);
                                }
                            }
                        }
                    }
                    m_defaultOrganizationValid = true;
                }
                return m_defaultOrganization;
            }
        }

        /// <summary>
        /// Return a list of resources contained in the package.  The dictionary has the resource id as the key, 
        /// the <Typ>ResourceNodeReader</Typ> as the value. 
        /// </summary>
        /// <remarks>
        /// In a SCORM package, the ids of each element must be unique. If the manifest being read
        /// violates this requirement and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set, the collection
        /// of resources will throw an <Typ>InvalidPackageException</Typ> when the error is encountered. 
        /// If <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set, only the first resource of a duplicated ID will be contained in the list.
        /// <para>
        /// SCORM states there must be resources in a valid manifest.  However, this method simply returns an empty dictionary
        /// in this case.  It is up to the application to determine what to do if this happens.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">There are multiple &lt;resources&gt; nodes and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// is set.</exception>
		/// <example>Loop through the resources in the manifest.
        /// <code language="C#">
		///     foreach(ResourceNodeReader resource in manifest.Resources.Values)
		///     {
		///         // use the resource
		///     }
		/// </code></example>
        public IDictionary<string, ResourceNodeReader> Resources
        {
            get
            {
                if (m_resources == null)
                {
                    // find the <resources> node
                    m_helper.CacheFirstChild(ref m_resourcesNode, m_helper.Node, Helper.Strings.Resources,
                        m_helper.ImscpCurrentNamespace);
                    if (null == m_resourcesNode.m_node)
                    {
                        m_resources = new ReadOnlyMlcDictionary<ResourceNodeReader>();
                    }
                    else
                    {
                        XPathExpression expr = m_helper.CreateExpression(m_resourcesNode.m_node,
                            Helper.Strings.Imscp + ":" + Helper.Strings.Resource);
                        IDictionary<string, ResourceNodeReader> resources = new ReadOnlyMlcDictionary<ResourceNodeReader>(m_helper, m_resourcesNode.m_node,
                            expr, "@" + Helper.Strings.Identifier, delegate(XPathNavigator nav)
                            {
                                XPathNavigator identifier = nav.Clone();
                                if (identifier.MoveToAttribute(Helper.Strings.Identifier, String.Empty))
                                {
                                    // verify that the id is valid
                                    if (Helper.ValidateId(identifier.Value))
                                    {
                                        return new ResourceNodeReader(m_packageReader, nav, this, ResourcesXmlBase);
                                    }
                                    else
                                    {
                                        // not valid. Log bad attribute value and return null.
                                        m_helper.LogBadAttribute(identifier.Value, identifier.Name, nav.Name, ValidatorResources.ElementRemoved,
                                            ValidatorResources.InvalidIdentifier);
                                        return null;
                                    }
                                }
                                return null; // this will never be called, because the identifier is guaranteed to be there
                            },
                            delegate(XPathNavigator nav)
                            {
                                // the identifier is missing from the resource node
                                m_helper.LogRequiredAttributeMissing(Helper.Strings.Identifier, nav.Name, ValidatorResources.ResourceIdentifierMissing);
                                return null;
                            });
                        m_resources = resources;
                    }
                    // Handle the case when there are submanifests
                    AppendSubmanifestResourceNodes();
                }
                return m_resources;
            }
        }

        /// <summary>
        /// Returns a list of the decendant submanifest nodes (all manifest nodes beneath this one - not just the immediate children).
        /// </summary>
        /// <remarks>
        /// Called by ActivityNodeReader.ParseIdentifierref.
        /// </remarks>
        internal XPathNodeIterator DecendantSubmanifests
        {
            get
            {
                return m_helper.Node.SelectDescendants(Helper.Strings.Manifest, m_helper.ImscpCurrentNamespace, false);
            }
        }

        /// <summary>
        /// Append any resource nodes on submanifests to the current list in m_resources.
        /// </summary>
        private void AppendSubmanifestResourceNodes()
        {
            XPathNodeIterator submanifests = m_helper.Node.SelectChildren(Helper.Strings.Manifest, m_helper.ImscpCurrentNamespace);
            if (submanifests.Count > 0)
            {
                Dictionary<string, ResourceNodeReader> resources = new Dictionary<string, ResourceNodeReader>(m_resources);
                while (submanifests.MoveNext())
                {
                    ManifestReader submanifest = new ManifestReader(m_packageReader, m_helper.ReaderSettings,
                        m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log, submanifests.Current);
                    foreach (ResourceNodeReader resource in submanifest.Resources.Values)
                    {
                        if (!resources.ContainsKey(resource.Id))
                        {
                            resources.Add(resource.Id, resource);
                        }
                    }
                }
                m_resources = new ReadOnlyDictionary<string, ResourceNodeReader>(resources);
            }
        }

        /// <summary>
        /// Return an XPathNavigator to provide XPath query access to the navigator.
        /// This allows access to non-Scorm compliant elements and attributes in a manifest.
        /// </summary>
        public XPathNavigator CreateNavigator()
        {
            return m_helper.Node.Clone();
        }

        /// <summary>
        /// Get the ManifestReaderSettings for this manifest.
        /// </summary>
        internal ManifestReaderSettings ManifestSettings
        {
            get
            {
                return m_helper.ReaderSettings;
            }
        }

        /// <summary>
        /// Get the PackageValidatorSettings for this manifest.
        /// </summary>
        internal PackageValidatorSettings ValidatorSettings
        {
            get
            {
                return m_helper.ValidatorSettings;
            }
        }

        /// <summary>
        /// Return the sequencing collection information in the manifest. This is only valid
        /// in SCORM 2004 content, so in SCORM 1.2 content, this collection will always be empty.
        /// The key is the &lt;sequencing&gt;/ID and the value is a <Typ>SequencingNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// This collection only returns &lt;sequencing&gt; nodes from the &lt;imsss:sequencingCollection&gt;. 
        /// These all contain ID attributes that are referenced by the IDRef attribute of &lt;imsss:sequencing&gt; 
        /// nodes inside an &lt;item&gt; node.
        /// <para>
        /// Because the nodes are parsed as they are accessed, and not before, errors or warnings can occur
        /// when the collection's <c>Count</c> property or <c>MoveNext</c> method is accessed.  E.g. if a
        /// &lt;sequencing&gt; node contains no valid ID attribute, in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> a warning
        /// is put into the log and that &lt;sequencing&gt; node is skipped.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>,
        /// an error is put into the log and an <Typ>InvalidPackageException</Typ> is thrown.
        /// </para>
        /// <para>
        /// &lt;imsss:sequencingCollection&gt; is only allowed on Content Aggregation packages, and not
        /// on Resources packages.  If <Prp>PackageType</Prp> is <c>PackageType.Resource</c> and there is
        /// a &lt;imsss:sequencingCollection&gt; node in the manifest, then if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is
        /// set, the <Prp>SequencingCollection</Prp> is returned anyway.  However, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// is set, an exception is thrown.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">If <Prp>PackageType</Prp> is <c>PackageType.Resource</c> and there is
        /// a &lt;imsss:sequencingCollection&gt; node in the manifest, and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is
        /// set</exception>
		/// <example>Loop through sequencing nodes in the sequencingCollection.<code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         // use the SequencingNodeReader
		///     }
		/// </code></example>
        public IDictionary<string, SequencingNodeReader> SequencingCollection
        {
            // Note that this collection should only contain complete <sequencing> nodes with valid
            // ID's.
            get
            {
                if (null == m_sequencing)
                {
                    // always return an emtpy collection in SCORM 1.2.
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                    {
                        m_sequencing = new ReadOnlyMlcDictionary<SequencingNodeReader>();
                    }
                    else
                    {
                        // find the <sequencingCollection> node
                        XPathNavigator sequencingCollection =
                            m_helper.GetFirstChild(m_helper.Node, Helper.Strings.SequencingCollection,
                                Helper.Strings.ImsssNamespace);
                        if (null == sequencingCollection)
                        {
                            m_sequencing = new ReadOnlyMlcDictionary<SequencingNodeReader>();
                        }
                        else
                        {
                            // check if this is a resource package, since they can't have sequencing collections
                            if (PackageType == PackageType.Resource)
                            {
                                // strict throws if there is a sequencingCollection node.  Relaxed just puts
                                // a warning in the log.
                                m_helper.LogInvalidNode(sequencingCollection.Name, ValidatorResources.SequencingCollectionInResource);
                            }

                            XPathExpression expr = m_helper.CreateExpression(sequencingCollection,
                                Helper.Strings.Imsss + ":" + Helper.Strings.Sequencing);
                            IDictionary<string, SequencingNodeReader> sequencing = new ReadOnlyMlcDictionary<SequencingNodeReader>(m_helper, sequencingCollection,
                                expr, "@" + Helper.Strings.Id, delegate(XPathNavigator nav)
                                {
                                    // if there is no ID attribute on this sequencing node, log it and skip or throw.
                                    XPathNavigator id = nav.Clone();
                                    if (id.MoveToAttribute(Helper.Strings.Id, String.Empty))
                                    {
                                        // verify that the id is valid
                                        if (Helper.ValidateId(id.Value))
                                        {
                                            return new SequencingNodeReader(nav, this, null, ManifestSettings, ValidatorSettings, LogReplacement, Log);
                                        }
                                        else
                                        {
                                            // not valid. Log bad attribute value and return null.
                                            m_helper.LogBadAttribute(id.Value, id.Name, nav.Name, ValidatorResources.ElementRemoved, ValidatorResources.InvalidIdentifier);
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        // attribute is missing. Log it and return null.
                                        m_helper.LogRequiredAttributeMissing(Helper.Strings.Id, nav.Name, ValidatorResources.SequencingIdMissing);
                                        return null;
                                    }
                                }, null);
                            m_sequencing = sequencing;
                        }
                    }
                }
                return m_sequencing;
            }
        }
    }

    /// <summary>
    /// Parses SCORM &lt;imsss:sequencing&gt; nodes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The &lt;imsss:sequencing&gt; nodes are from a SCORM manifest or the SequencingDataCache
    /// field in the ActivityAttemptItem table.
    /// </para>
    /// <para>
    /// AuxiliaryResources are not available here because they are not recommended for use in 2004.
    /// </para>
    /// <para>
    /// &lt;imsss:sequencing&gt; nodes occur 0 or more times in &lt;item&gt; and &lt;imsss:sequencingCollection&gt; nodes.
    /// In &lt;imsss:sequencingCollection&gt;'s, the &lt;imsss:sequencing&gt; nodes must have an ID attribute, which is
    /// referenced by the IDRef attribute of a &lt;imsss:sequencing&gt; in an &lt;item&gt; node.
    /// </para>
    /// <para>
    /// When used to import a package, a <Typ>ManifestReader</Typ> is provided in the constructor, allowing the
    /// &lt;sequencing&gt;/IDRef to be resolved.  No IDRef values are imported into the database.  Only the node 
    /// corresponding to the reference is imported, in-place.
    /// </para>
    /// <para>
    /// By implication, this object can only be created for SCORM 2004 content, since the 
    /// sequencing nodes do not exist in previous SCORM versions.
    /// </para>
    /// <para>
    /// Holds lists of &lt;imsss:rollupRule&gt; &lt;imsss:preConditionRule&gt;, &lt;imsss:exitConditionRule&gt;, 
    /// and &lt;imsss:postConditionRule&gt; nodes, and a dictionary of &lt;imsss:objective&gt; nodes (including
    /// &lt;primaryObjective&gt; nodes, all in the same dictionary.
    /// </para>
    /// </remarks>
    public class SequencingNodeReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;

        // holds the sequencing element referenced by this element's IDRef attribute.
        private SequencingNodeReader m_referenced;
        
        // private variables that hold attribute values and collections of nodes
        private Helper.ChildNode m_controlModeNode = new Helper.ChildNode(); // <imsss:controlMode>
        private Helper.ChildNode m_limitConditionsNode = new Helper.ChildNode(); // <imsss:limitConditions>
        private Helper.ChildNode m_rollupRulesNode = new Helper.ChildNode(); // <imsss:rollupRules>
        private Helper.ChildNode m_randomizationControlsNode = new Helper.ChildNode(); // <imsss:randomizationControls>
        private Helper.ChildNode m_deliveryControlsNode = new Helper.ChildNode(); // <imsss:deliveryControls>
        private Helper.ChildNode m_constrainedChoiceConsiderationsNode = new Helper.ChildNode(); // <adlseq:constrainedChoiceConsiderations>
        private Helper.ChildNode m_rollupConsiderationsNode = new Helper.ChildNode(); // <adlseq:rollupConsiderations>
        private Helper.ChildNode m_sequencingRules = new Helper.ChildNode(); // <imsss:sequencingRules> - first node found only
        private bool? m_choice;
        private bool? m_choiceExit;
        private bool? m_flow;
        private bool? m_forwardOnly;
        private bool? m_useCurrentAttemptObjectiveInfo;
        private bool? m_useCurrentAttemptProgressInfo;
        private int? m_attemptLimit;
        private bool m_gotAttemptLimit;
        private bool? m_rollupObjectiveSatisfied;
        private bool? m_rollupProgressCompletion;
        private double? m_objectiveMeasureWeight;
        private RandomizationTiming? m_randomizationTiming;
        private int? m_selectionCount;
        private bool? m_reorderChildren;
        private RandomizationTiming? m_selectionTiming;
        private bool? m_tracked;
        private bool? m_completionSetByContent;
        private bool? m_objectiveSetByContent;
        private bool? m_preventActivation;
        private bool? m_constrainChoice;
        private RollupConsideration? m_requiredForSatisfied;
        private RollupConsideration? m_requiredForNotSatisfied;
        private RollupConsideration? m_requiredForCompleted;
        private RollupConsideration? m_requiredForIncomplete;
        private bool? m_measureSatisfactionIfActive;
        private TimeSpan? m_attemptAbsoluteDurationLimit;
        private ReadOnlyCollection<SequencingRollupRuleNodeReader> m_rollupRules;
        private ReadOnlyCollection<SequencingRuleNodeReader> m_preConditionRules;
        private ReadOnlyCollection<SequencingRuleNodeReader> m_exitConditionRules;
        private ReadOnlyCollection<SequencingRuleNodeReader> m_postConditionRules;
        private IDictionary<string, SequencingObjectiveNodeReader> m_objectives;
        private ActivityNodeReader m_activity; // the activity that owns this sequencing node
        private ManifestReader m_manifest; // manifest that owns this rule condition.

        // Returns true if the reference should be used.
        private bool UseReference(Helper.ChildNode node)
        {
            if (node.m_node == null && m_referenced != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a SequencingNodeReader for the requested node. 
        /// </summary>
        /// <remarks>This is used when the sequencing node may have an IDRef attribute that needs to be cross-referenced
        /// with the <Typ>SequencingNodeReader</Typ> collection held by the <Typ>ManifestReader</Typ>.  Instead of
        /// creating a new <Typ>SequencingNodeReader</Typ>, in the case where the IDRef attribute exists, the
        /// <Typ>SequencingNodeReader</Typ> in the <c>ManifestReader.SequencingCollection</c> that has the matching
        /// ID attribute is returned.
        /// <para>
        /// If this sequencing node has an IDRef attribute, but no sequencing node with a matching ID can be found,
        /// null is returned if parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, and an <Typ>InvalidPackageException</Typ>
        /// is thrown if parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>.
        /// </para>
        /// </remarks>
        /// <param name="sequencingNode">A &lt;sequencing&gt; node held inside an item or organization.  E.g. not
        /// one from a &lt;sequencingCollection&gt;.</param>
        /// <param name="manifest">The <Typ>ManifestReader</Typ> that contains the manifest that holds this 
        /// &lt;sequencing&gt; node.</param>
        /// <param name="activity">The <Typ>ActivityNodeReader</Typ> that contains this &lt;sequencing&lt; node.</param>
        internal static SequencingNodeReader Create(XPathNavigator sequencingNode, ManifestReader manifest, ActivityNodeReader activity)
        {
            if (manifest == null) throw new LearningComponentsInternalException("MR015");
            // calling the Helper constructor guarantees this is a <sequencing> node, but doesn't check if it needs to
            // be resolved.
            Helper helper = Helper.InternalCreateImsss(sequencingNode, manifest.ManifestSettings, manifest.ValidatorSettings, manifest.LogReplacement, manifest.Log, Helper.Strings.Sequencing);
            // if this <sequencing> node contains the IDRef attribute, use ManifestReader to go
            // grab the actual <sequencing> node.
            string idRef = null;
            helper.CacheOptionalStringIdentifierAttribute(sequencingNode, ref idRef, Helper.Strings.IdRef, String.Empty, 0, 0);

            SequencingNodeReader reference = null;
            if (!String.IsNullOrEmpty(idRef))
            {
                // there is a valid IDRef attribute, need to go get the SequencingNodeReader
                // from the manifest.
                if (manifest.SequencingCollection.ContainsKey(idRef))
                {
                    reference = manifest.SequencingCollection[idRef];
                }
                else
                {
                    // The ID referenced by IDRef can't be found. The following will throw in strict mode.
                    helper.LogBadAttribute(idRef, Helper.Strings.IdRef, sequencingNode.Name, ValidatorResources.ElementRemoved, ValidatorResources.ReferenceNotFound);
                    reference = null;
                }
            }
            SequencingNodeReader node = new SequencingNodeReader(sequencingNode, manifest, activity, manifest.ManifestSettings, manifest.ValidatorSettings, manifest.LogReplacement, manifest.Log);
            node.m_referenced = reference;
            return node;
        }

        /// <summary>
        /// Creates a SequencingNodeReader for the requested node. 
        /// </summary>
        /// <remarks>This is used when the sequencing node may have an IDRef attribute that needs to be cross-referenced
        /// with the <Typ>SequencingNodeReader</Typ> collection held by the <Typ>ManifestReader</Typ>.  Instead of
        /// creating a new <Typ>SequencingNodeReader</Typ>, in the case where the IDRef attribute exists, the
        /// <Typ>SequencingNodeReader</Typ> in the <c>ManifestReader.SequencingCollection</c> that has the matching
        /// ID attribute is returned.
        /// <para>
        /// If this sequencing node has an IDRef attribute, but no sequencing node with a matching ID can be found,
        /// null is returned if parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, and an <Typ>InvalidPackageException</Typ>
        /// is thrown if parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>.
        /// </para>
        /// </remarks>
        /// <param name="sequencingNode">A &lt;sequencing&gt; node held inside an item or organization.  E.g. not
        /// one from a &lt;sequencingCollection&gt;.</param>
        /// <param name="manifest">The <Typ>ManifestReader</Typ> that contains the manifest that holds this 
        /// &lt;sequencing&gt; node.</param>
        internal static SequencingNodeReader Create(XPathNavigator sequencingNode, ManifestReader manifest)
        {
            return Create(sequencingNode, manifest, null);
        }

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="sequencingNode">Points to an &lt;imsss:sequencing&gt; node and its contents.</param>
        /// <param name="manifest">The <Typ>ManifestReader</Typ> that contains this &lt;sequencing&lt; node.  Can be null.</param>
        /// <param name="activity">The <Typ>ActivityNodeReader</Typ> that contains this &lt;sequencing&lt; node.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="log">Log for warnings/errors.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <remarks>This constructor is designed to read the sequencing node XML
        /// stored in the SequencingDataCache field in the ActivityAttemptItem table of LearningStore. 
        /// This allows creating a SequencingNodeReader without requiring
        /// a ManifestReader. A ManifestReader is only required for &lt;sequence&gt; nodes that contain
        /// the IDRef attribute, since this references a &lt;sequence&gt; node under the 
        /// &lt;manifest&gt;/&lt;sequenceCollection%gt; node. When using this constructor, the &lt;sequence&gt;
        /// node is guaranteed not to contain the IDRef attribute.
        /// <para>
        /// This constructor verifies the first node in <paramref name="sequencingNode"/> is a &lt;sequencing&gt; node
        /// in the http://www.imsglobal.org/xsd/imsss namespace.
        /// </para>
        /// </remarks>
        internal SequencingNodeReader(XPathNavigator sequencingNode, ManifestReader manifest, ActivityNodeReader activity,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            m_activity = activity;
            m_manifest = manifest;
            m_helper = Helper.InternalCreateImsss(sequencingNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.Sequencing);
        }

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="sequencingNode">Points to an &lt;imsss:sequencing&gt; node and its contents.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Log for warnings/errors.</param>
        /// <remarks>This constructor is designed to read the sequencing node XML
        /// stored in the SequencingDataCache field in the ActivityAttemptItem table of LearningStore. 
        /// This allows creating a SequencingNodeReader without requiring
        /// a ManifestReader. A ManifestReader is only required for &lt;sequence&gt; nodes that contain
        /// the IDRef attribute, since this references a &lt;sequence&gt; node under the 
        /// &lt;manifest&gt;/&lt;sequenceCollection%gt; node. When using this constructor, the &lt;sequence&gt;
        /// node is guaranteed not to contain the IDRef attribute.
        /// <para>
        /// This constructor verifies the first node in <paramref name="sequencingNode"/> is a &lt;sequencing&gt; node
        /// in the http://www.imsglobal.org/xsd/imsss namespace.
        /// </para>
        /// </remarks>
        internal SequencingNodeReader(XPathNavigator sequencingNode,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            m_helper = Helper.InternalCreateImsss(sequencingNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.Sequencing);
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/choice attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool Choice
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                    ref m_choice, Helper.Strings.Choice, !UseReference(m_controlModeNode) ? true : m_referenced.Choice);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/choiceExit attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool ChoiceExit
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                   m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                   ref m_choiceExit, Helper.Strings.ChoiceExit, !UseReference(m_controlModeNode) ? true : m_referenced.ChoiceExit);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/flow attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool Flow
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                   m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                   ref m_flow, Helper.Strings.Flow, !UseReference(m_controlModeNode) ? false : m_referenced.Flow);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/forwardOnly attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool ForwardOnly
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                    ref m_forwardOnly, Helper.Strings.ForwardOnly, !UseReference(m_controlModeNode) ? false : m_referenced.ForwardOnly);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/useCurrentAttemptObjectiveInfo attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool UseCurrentAttemptObjectiveInfo
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                    ref m_useCurrentAttemptObjectiveInfo, Helper.Strings.UseCurrentAttemptObjectiveInfo,
                    !UseReference(m_controlModeNode) ? true : m_referenced.UseCurrentAttemptObjectiveInfo);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:controlMode&gt;/useCurrentAttemptProgressInfo attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool UseCurrentAttemptProgressInfo
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_controlModeNode, m_helper.Node, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace),
                    ref m_useCurrentAttemptProgressInfo, Helper.Strings.UseCurrentAttemptProgressInfo,
                    !UseReference(m_controlModeNode) ? true : m_referenced.UseCurrentAttemptProgressInfo);
            }
        }

        private bool HasSequencingRules
        {
            get
            {
                return m_helper.CacheFirstChild(ref m_sequencingRules, m_helper.Node,
                    Helper.Strings.SequencingRules, Helper.Strings.ImsssNamespace, false) != null;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingRuleNodeReader&gt;</c> containing the &lt;imsss:preConditionRules&gt; nodes,
        /// of type <Typ>SequencingRuleNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that if this sequencing node has a valid IDRef
        /// attribute, if there is no &lt;imsss:sequencingRules&gt; node in this sequencing node, the values inside the
        /// &lt;imsss:sequencingRules&gt; node in the sequencing node referenced by IDRef is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node.</exception>
		/// <example>Loop through the preConditionRules for all sequencing nodes in the sequencingCollection.<code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingRuleNodeReader rule in sequencing.PreConditionRules)
		///         {
		///             // Use the SequencingRuleNodeReader
		///         }
		///     }
		/// </code></example>
        [SuppressMessage("Microsoft.Naming", "CA1702")] // "PreCondition" is the correct casing. "Precondition" is not.
        public ReadOnlyCollection<SequencingRuleNodeReader> PreConditionRules
        {
            get
            {
                if (HasSequencingRules || m_referenced == null)
                {
                    if (null == m_preConditionRules)
                    {
                        m_preConditionRules = CreateConditionRulesCollection(Helper.Strings.PreConditionRule);
                    }
                }
                else if (m_referenced != null)
                {
                    return m_referenced.PreConditionRules;
                }
                return m_preConditionRules;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingRuleNodeReader&gt;</c> containing the &lt;imsss:postConditionRules&gt; nodes,
        /// of type <Typ>SequencingRuleNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that if this sequencing node has a valid IDRef
        /// attribute, if there is no &lt;imsss:sequencingRules&gt; node in this sequencing node, the values inside the
        /// &lt;imsss:sequencingRules&gt; node in the sequencing node referenced by IDRef is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node.</exception>
        /// <example>Loop through the postConditionRules for all sequencing nodes in the sequencingCollection.<code language="C#">
        ///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
        ///     {
        ///         foreach (SequencingRuleNodeReader rule in sequencing.PostConditionRules)
        ///         {
        ///             // Use the SequencingRuleNodeReader
        ///         }
        ///     }
        /// </code></example>
        public ReadOnlyCollection<SequencingRuleNodeReader> PostConditionRules
        {
            get
            {
                if (HasSequencingRules || m_referenced == null)
                {
                    if (null == m_postConditionRules)
                    {
                        m_postConditionRules = CreateConditionRulesCollection(Helper.Strings.PostConditionRule);
                    }
                }
                else if (m_referenced != null)
                {
                    return m_referenced.PostConditionRules;
                }
                return m_postConditionRules;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingRuleNodeReader&gt;</c> containing the &lt;imsss:exitConditionRules&gt; nodes,
        /// of type <Typ>SequencingRuleNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that if this sequencing node has a valid IDRef
        /// attribute, if there is no &lt;imsss:sequencingRules&gt; node in this sequencing node, the values inside the
        /// &lt;imsss:sequencingRules&gt; node in the sequencing node referenced by IDRef is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node.</exception>
        /// <example>Loop through the exitConditionRules for all sequencing nodes in the sequencingCollection.<code language="C#">
        ///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
        ///     {
        ///         foreach (SequencingRuleNodeReader rule in sequencing.ExitConditionRules)
        ///         {
        ///             // Use the SequencingRuleNodeReader
        ///         }
        ///     }
        /// </code></example>
        public ReadOnlyCollection<SequencingRuleNodeReader> ExitConditionRules
        {
            get
            {
                if (HasSequencingRules || m_referenced == null)
                {
                    if (null == m_exitConditionRules)
                    {
                        m_exitConditionRules = CreateConditionRulesCollection(Helper.Strings.ExitConditionRule);
                    }
                }
                else if (m_referenced != null)
                {
                    return m_referenced.ExitConditionRules;
                }
                return m_exitConditionRules;
            }
        }
        
        /// <summary>
        /// Create a collection of sequencing rules. 
        /// </summary>
        /// <param name="nodeName">Name of the node in the imsss namespace, e.g. "preConditionRule".  
        /// Differentiates between exit, pre and post condition rules.</param>
        /// <returns></returns>
        private ReadOnlyCollection<SequencingRuleNodeReader> CreateConditionRulesCollection(string nodeName)
        {
            XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imsss + ":" + Helper.Strings.SequencingRules
                + "/" + Helper.Strings.Imsss + ":" + nodeName);
            ReadOnlyCollection<SequencingRuleNodeReader> coll = new ReadOnlyCollection<SequencingRuleNodeReader>(
                new ReadOnlyMlcCollection<SequencingRuleNodeReader>(m_helper.Node, expr,
                    delegate(XPathNavigator node /* the navigator pointing to the condition rule node */)
                    {
                        return new SequencingRuleNodeReader(node, m_manifest, m_activity, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                    }));
            return coll;
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:limitConditions&gt;/attemptLimit attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "null".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid positive integer, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid positive integer, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public int? AttemptLimit
        {
            get
            {
                if (!m_gotAttemptLimit)
                {
                    m_helper.CacheAttribute<int>(
                        m_helper.CacheFirstChild(ref m_limitConditionsNode, m_helper.Node, Helper.Strings.LimitConditions, Helper.Strings.ImsssNamespace),
                        ref m_attemptLimit, Helper.Strings.AttemptLimit, 
                        !UseReference(m_limitConditionsNode) ? null : m_referenced.AttemptLimit, String.Empty);
                    if (m_attemptLimit.HasValue && m_attemptLimit < 0)
                    {
                        m_helper.LogBadAttribute(m_attemptLimit.ToString(), Helper.Strings.AttemptLimit, m_limitConditionsNode.m_node.Name, "",
                            ValidatorResources.IllegalNegativeInteger);
                        m_attemptLimit = null;
                    }
                    m_gotAttemptLimit = true;
                }
                return m_attemptLimit;
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:limitConditions&gt;/attemptAbsoluteDurationLimit attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>TimeSpan.Zero</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:duration, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:duration, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public TimeSpan AttemptAbsoluteDurationLimit
        {
            get
            {
                return m_helper.CacheAttribute<TimeSpan>(
                        m_helper.CacheFirstChild(ref m_limitConditionsNode, m_helper.Node, Helper.Strings.LimitConditions, Helper.Strings.ImsssNamespace),
                        ref m_attemptAbsoluteDurationLimit, Helper.Strings.AttemptAbsoluteDurationLimit,
                        !UseReference(m_limitConditionsNode) ? TimeSpan.Zero : m_referenced.AttemptAbsoluteDurationLimit);
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingRollupRuleNodeReader&gt;</c> containing the
        /// &lt;imsss:rollupRules&gt;/&lt;imsss:rollupRule&gt; nodes, of type <Typ>SequencingRollupRuleNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that if this sequencing node has a valid IDRef
        /// attribute, if there is no &lt;imsss:rollupRules&gt; node in this sequencing node, the values inside the
        /// &lt;imsss:rollupRules&gt; node in the sequencing node referenced by IDRef is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node, or if there
        /// are multiple &lt;imsss:rollupRules&gt; nodes.</exception>
		/// <example>Loop through all rollup rules in the sequencingCollection.<code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingRollupRuleNodeReader rollupRule in sequencing.RollupRules)
		///         {
		///             // Use the SequencingRollupRuleNodeReader
		///         }
		///     }
		/// </code></example>
        public ReadOnlyCollection<SequencingRollupRuleNodeReader> RollupRules
        {
            get
            {
                if (null == m_rollupRules)
                {
                    m_rollupRules = CreateRollupRulesCollection();
                }
                return m_rollupRules;
            }
        }

        private ReadOnlyCollection<SequencingRollupRuleNodeReader> CreateRollupRulesCollection()
        {
            // if we haven't already tried to get the rollupRules node, try to get it.  If there isn't one, then check the
            // referenced sequencing node (if it exists)
            m_helper.CacheFirstChild(ref m_rollupRulesNode, m_helper.Node, Helper.Strings.RollupRules, Helper.Strings.ImsssNamespace);
            if (m_rollupRulesNode.m_node == null)
            {
                // there is no rollupRules node so get the referenced one if possible
                if (m_referenced != null)
                {
                    return m_referenced.CreateRollupRulesCollection();
                }
                else
                {
                    // there isn't a referenced one either, so return an empty one
                    return new ReadOnlyCollection<SequencingRollupRuleNodeReader>(new ReadOnlyMlcCollection<SequencingRollupRuleNodeReader>());
                }
            }
            else
            {
                XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imsss + ":" + Helper.Strings.RollupRule);
                ReadOnlyCollection<SequencingRollupRuleNodeReader> coll = new ReadOnlyCollection<SequencingRollupRuleNodeReader>(
                    new ReadOnlyMlcCollection<SequencingRollupRuleNodeReader>(
                        m_rollupRulesNode.m_node,
                        expr,
                        delegate(XPathNavigator node /* the navigator pointing to the condition rule node */)
                        {
                            return new SequencingRollupRuleNodeReader(node, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                        }));
                return coll;
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:rollupRules&gt;/rollupObjectiveSatisfied attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool RollupObjectiveSatisfied
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_rollupRulesNode, m_helper.Node, Helper.Strings.RollupRules, Helper.Strings.ImsssNamespace),
                    ref m_rollupObjectiveSatisfied, Helper.Strings.RollupObjectiveSatisfied,
                    !UseReference(m_rollupRulesNode) ? true : m_referenced.RollupObjectiveSatisfied);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:rollupRules&gt;/rollupProgressCompletion attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool RollupProgressCompletion
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_rollupRulesNode, m_helper.Node, Helper.Strings.RollupRules, Helper.Strings.ImsssNamespace),
                    ref m_rollupProgressCompletion, Helper.Strings.RollupProgressCompletion,
                    !UseReference(m_rollupRulesNode) ? true : m_referenced.RollupProgressCompletion);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:rollupRules&gt;/objectiveMeasureWeight attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "1".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:decimal, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:decimal, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public double ObjectiveMeasureWeight
        {
            get
            {
                if (m_objectiveMeasureWeight == null)
                {
                    double? objectiveMeasureWeight = null;
                    m_helper.CacheAttribute<double>(
                        m_helper.CacheFirstChild(ref m_rollupRulesNode, m_helper.Node, Helper.Strings.RollupRules, Helper.Strings.ImsssNamespace),
                        ref objectiveMeasureWeight, Helper.Strings.ObjectiveMeasureWeight,
                        !UseReference(m_rollupRulesNode) ? 1 : m_referenced.ObjectiveMeasureWeight);
                    // range check: objectiveMeasureWeight must be between 0 and 1.
                    if (objectiveMeasureWeight < 0 || objectiveMeasureWeight > 1)
                    {
                        m_helper.LogBadAttribute(objectiveMeasureWeight.ToString(), Helper.Strings.ObjectiveMeasureWeight,
                            m_helper.Node.Name, "0", ValidatorResources.IllegalPercentage);
                        objectiveMeasureWeight = 1;
                    }
                    m_objectiveMeasureWeight = objectiveMeasureWeight;
                }
                return m_objectiveMeasureWeight.Value;
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:rollupConsiderations&gt;/requiredForSatisfied attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RollupConsideration.Always</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RollupConsideration RequiredForSatisfied
        {
            get
            {
                return m_helper.CacheTokenAttribute<RollupConsideration>(
                    m_helper.CacheFirstChild(ref m_rollupConsiderationsNode, m_helper.Node, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_requiredForSatisfied, Helper.Strings.RequiredForSatisfied,
                    !UseReference(m_rollupConsiderationsNode) ? RollupConsideration.Always : m_referenced.RequiredForSatisfied);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:rollupConsiderations&gt;/requiredForNotSatisfied attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RollupConsideration.Always</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RollupConsideration RequiredForNotSatisfied
        {
            get
            {
                return m_helper.CacheTokenAttribute<RollupConsideration>(
                    m_helper.CacheFirstChild(ref m_rollupConsiderationsNode, m_helper.Node, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_requiredForNotSatisfied, Helper.Strings.RequiredForNotSatisfied,
                    !UseReference(m_rollupConsiderationsNode) ? RollupConsideration.Always : m_referenced.RequiredForNotSatisfied);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:rollupConsiderations&gt;/requiredForCompleted attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RollupConsideration.Always</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RollupConsideration RequiredForCompleted
        {
            get
            {
                return m_helper.CacheTokenAttribute<RollupConsideration>(
                    m_helper.CacheFirstChild(ref m_rollupConsiderationsNode, m_helper.Node, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_requiredForCompleted, Helper.Strings.RequiredForCompleted,
                    !UseReference(m_rollupConsiderationsNode) ? RollupConsideration.Always : m_referenced.RequiredForCompleted);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:rollupConsiderations&gt;/requiredForIncomplete attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RollupConsideration.Always</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RollupConsideration RequiredForIncomplete
        {
            get
            {
                return m_helper.CacheTokenAttribute<RollupConsideration>(
                    m_helper.CacheFirstChild(ref m_rollupConsiderationsNode, m_helper.Node, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_requiredForIncomplete, Helper.Strings.RequiredForIncomplete,
                    !UseReference(m_rollupConsiderationsNode) ? RollupConsideration.Always : m_referenced.RequiredForIncomplete);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:rollupConsiderations&gt;/measureSatisfactionIfActive attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool MeasureSatisfactionIfActive
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_rollupConsiderationsNode, m_helper.Node, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_measureSatisfactionIfActive, Helper.Strings.MeasureSatisfactionIfActive,
                    !UseReference(m_rollupConsiderationsNode) ? true : m_referenced.MeasureSatisfactionIfActive);
            }
        }

        /// <summary>
        /// Returns the <c>IDictionary&lt;string, SequencingObjectiveNodeReader&gt;</c> containing the
        /// &lt;imsss:primaryObjective&gt; and &lt;imsss:objective&gt; nodes under the &lt;imsss:objectives&gt; node,
        /// indexed by the <c>string</c> parameter, which is the objectiveID attribute of the objective node.
        /// The value of the dictionary is <Typ>SequencingObjectiveNodeReader</Typ>.
        /// </summary>
        /// <remarks>
        /// The <Typ>IDictionary</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed, e.g. <c>IDictionary.ContainsKey</c>, <c>Keys</c>, or accessing
        /// the enumerator.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// will be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// Calling any property or method the first time results in an n-order operation as all nodes are scanned and
        /// objects created.
        /// </para>
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>IDictionary</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that if this sequencing node has a valid IDRef
        /// attribute, if there is no &lt;imsss:objectives&gt; node in this sequencing node, the values inside the
        /// &lt;imsss:objectives&gt; node in the sequencing node referenced by IDRef is returned.
        /// </para>
        /// <para>
        /// In the case of primary objectives, SCORM permits the objectiveID attribute to be absent if there are
        /// no &lt;mapInfo&gt; nodes contained within the primary objective.  When this happens, the objectiveID
        /// will be <c>String.Empty</c>, which will be the index into this <Typ>IDictionary</Typ>.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node, or if there
        /// are multiple &lt;imsss:objectives&gt; nodes.</exception>
		/// <example>Loop through all objectives in the sequencingCollection.<code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingObjectiveNodeReader objective in sequencing.Objectives.Values)
		///         {
		///             // Use the SequencingObjectiveNodeReader
		///         }
		///     }
		/// </code></example>
        public IDictionary<string, SequencingObjectiveNodeReader> Objectives
        {
            get
            {
                if (null == m_objectives)
                {
                    // first, get the first <imsss:objectives> node
                    XPathNavigator objectivesNode = 
                        m_helper.GetFirstChild(m_helper.Node, Helper.Strings.Objectives, Helper.Strings.ImsssNamespace);
                    if (null == objectivesNode)
                    {
                        // no <imsss:objectives> nodes, return the referenced objectives if possible, or an empty dictionary.
                        if (m_referenced != null)
                        {
                            m_objectives = m_referenced.Objectives;
                        }
                        else
                        {
                            m_objectives = new ReadOnlyMlcDictionary<SequencingObjectiveNodeReader>();
                        }
                    }
                    else
                    {
                        // then, get the <imsss:objective> and <imsss:primaryObjective> node(s)
                        XPathExpression valueExpr = m_helper.CreateExpression(m_helper.Node, 
                            Helper.Strings.Imsss + ":" + Helper.Strings.Objective + "|"
                            + Helper.Strings.Imsss + ":" + Helper.Strings.PrimaryObjective);
                        IDictionary<string, SequencingObjectiveNodeReader> objectives =
                            new ReadOnlyMlcDictionary<SequencingObjectiveNodeReader>(
                                m_helper,
                                objectivesNode,
                                valueExpr,
                                "@" + Helper.Strings.ObjectiveId,
                                delegate(XPathNavigator node)
                                {
                                    // if there is no objectiveID return null so this one isn't 
                                    // cached in the dictionary, except in the case for a primary
                                    // objective with no mapInfo nodes.
                                    SequencingObjectiveNodeReader objective = new SequencingObjectiveNodeReader(node, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                                    if (objective.Id == String.Empty)
                                    {
                                        if (objective.IsPrimaryObjective && objective.Mappings.Count == 0)
                                        {
                                            return objective;
                                        }
                                        else
                                        {
                                            m_helper.LogRequiredAttributeMissing(Helper.Strings.ObjectiveId, node.Name, Helper.GetLogInfo_NodeText(node));
                                            return null;
                                        }
                                    }
                                    else return objective;
                                },
                                delegate(XPathNavigator node)
                                {
                                    // if there is no objectiveID return null so this one isn't 
                                    // cached in the dictionary, except in the case for a primary
                                    // objective with no mapInfo nodes.
                                    SequencingObjectiveNodeReader objective = new SequencingObjectiveNodeReader(node, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                                    if (objective.IsPrimaryObjective && objective.Mappings.Count == 0)
                                    {
                                        return objective.Id;
                                    }
                                    else
                                    {
                                        m_helper.LogRequiredAttributeMissing(Helper.Strings.ObjectiveId, node.Name, Helper.GetLogInfo_NodeText(node));
                                        return null;
                                    }
                                }
                                );
                        // if the validator settings are set to throw or log warnings/errors, we have to check all of the objectives
                        // to make sure 1) there is only a single primary objective and 2) a primary objective exists if any objectives
                        // exist.  Iterating through the objectives defeats the "lazy loading" nature of the reader, but it accomplishes
                        // the goal, and rarely would there be so many objective nodes as to make this a performance issue in any case.
                        if (m_helper.ValidatorSettings.ScormRequirementValidation != ValidationBehavior.None)
                        {
                            bool foundPrimary = false;
                            foreach (SequencingObjectiveNodeReader obj in objectives.Values)
                            {
                                if (obj.IsPrimaryObjective)
                                {
                                    if (foundPrimary)
                                    {
                                        m_helper.LogInvalidDuplicateNode(Helper.Strings.PrimaryObjective, false, ValidatorResources.PrimaryObjectiveMissingOrDuplicate);
                                    }
                                    foundPrimary = true;
                                }
                            }
                            if (!foundPrimary)
                            {
                                m_helper.LogRequiredNodeMissing(Helper.Strings.PrimaryObjective, ValidatorResources.PrimaryObjectiveMissingOrDuplicate);
                            }
                        }
                        m_objectives = objectives;
                    }
                }
                return m_objectives;
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:randomizationControls&gt;/randomizationTiming attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RandomizationTiming.Never</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RandomizationTiming RandomizationTiming
        {
            get
            {
                return m_helper.CacheTokenAttribute<RandomizationTiming>(
                    m_helper.CacheFirstChild(ref m_randomizationControlsNode, m_helper.Node, Helper.Strings.RandomizationControls, Helper.Strings.ImsssNamespace),
                    ref m_randomizationTiming, Helper.Strings.RandomizationTiming,
                    !UseReference(m_randomizationControlsNode) ? RandomizationTiming.Never : m_referenced.RandomizationTiming);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:randomizationControls&gt;/selectCount attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "0".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:nonNegativeInteger, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is a valid xs:nonNegativeInteger, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public int RandomizationSelectCount
        {
            get
            {
                if (m_selectionCount == null)
                {
                    m_helper.CacheAttribute<int>(
                        m_helper.CacheFirstChild(ref m_randomizationControlsNode, m_helper.Node, Helper.Strings.RandomizationControls, Helper.Strings.ImsssNamespace),
                        ref m_selectionCount, Helper.Strings.SelectCount,
                        !UseReference(m_randomizationControlsNode) ? 0 : m_referenced.RandomizationSelectCount);
                    if (m_selectionCount < 0)
                    {
                        m_helper.LogBadAttribute(m_selectionCount.ToString(), Helper.Strings.SelectCount, m_randomizationControlsNode.m_node.Name, "0",
                            ValidatorResources.IllegalNegativeInteger);
                        m_selectionCount = 0;
                    }
                }
                return m_selectionCount.Value;
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:randomizationControls&gt;/reorderChildren attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool ReorderChildren
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_randomizationControlsNode, m_helper.Node, Helper.Strings.RandomizationControls, Helper.Strings.ImsssNamespace),
                    ref m_reorderChildren, Helper.Strings.ReorderChildren,
                    !UseReference(m_randomizationControlsNode) ? false : m_referenced.ReorderChildren);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:randomizationControls&gt;/selectionTiming attribute.
        /// </summary>
        /// <remarks>
        /// The default value is <c>RandomizationTiming.Never</c>.  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public RandomizationTiming SelectionTiming
        {
            get
            {
                return m_helper.CacheTokenAttribute<RandomizationTiming>(
                    m_helper.CacheFirstChild(ref m_randomizationControlsNode, m_helper.Node, Helper.Strings.RandomizationControls, Helper.Strings.ImsssNamespace),
                    ref m_selectionTiming, Helper.Strings.SelectionTiming,
                    !UseReference(m_randomizationControlsNode) ? RandomizationTiming.Never : m_referenced.SelectionTiming);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:deliveryControls&gt;/tracked attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "true".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool Tracked
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_deliveryControlsNode, m_helper.Node, Helper.Strings.DeliveryControls, Helper.Strings.ImsssNamespace),
                    ref m_tracked, Helper.Strings.Tracked,
                    !UseReference(m_deliveryControlsNode) ? true : m_referenced.Tracked);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:deliveryControls&gt;/completionSetByContent attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool CompletionSetByContent
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_deliveryControlsNode, m_helper.Node, Helper.Strings.DeliveryControls, Helper.Strings.ImsssNamespace),
                    ref m_completionSetByContent, Helper.Strings.CompletionSetByContent,
                    !UseReference(m_deliveryControlsNode) ? false : m_referenced.CompletionSetByContent);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;imsss:deliveryControls&gt;/objectiveSetByContent attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool ObjectiveSetByContent
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_deliveryControlsNode, m_helper.Node, Helper.Strings.DeliveryControls, Helper.Strings.ImsssNamespace),
                    ref m_objectiveSetByContent, Helper.Strings.ObjectiveSetByContent,
                    !UseReference(m_deliveryControlsNode) ? false : m_referenced.ObjectiveSetByContent);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:constrainedChoiceConsiderations&gt;/preventActivation attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool PreventActivation
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_constrainedChoiceConsiderationsNode, m_helper.Node, Helper.Strings.ConstrainedChoiceConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_preventActivation, Helper.Strings.PreventActivation,
                    !UseReference(m_constrainedChoiceConsiderationsNode) ? false : m_referenced.PreventActivation);
            }
        }

        /// <summary>
        /// Returns the value of the &lt;adlseq:constrainedChoiceConsiderations&gt;/constrainChoice attribute.
        /// </summary>
        /// <remarks>
        /// The default value is "false".  See below for when the default value is returned.
        /// <para>
        /// The return value is determined as follows:
        /// </para>
        /// <para>
        /// If this node has the requested attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// If this node does not have the requested attribute, then if the parent &lt;sequencing&gt; element has an "idref"
        /// attribute referring to a &lt;sequencing&gt; node in the manifest's &lt;sequencingCollection&gt;, and that
        /// &lt;sequencing&gt; node contains the requested node and attribute:
        /// <ul>
        /// <li>
        /// If the attribute value is valid, the attribute value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = true</c>, the default value is returned.
        /// </li>
        /// <li>
        /// Otherwise, if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations = false</c>, an exception is thrown.
        /// </li>
        /// </ul>
        /// </para>
        /// <para>
        /// Otherwise, the default value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is invalid.</exception>
        public bool ConstrainChoice
        {
            get
            {
                return m_helper.CacheAttribute<bool>(
                    m_helper.CacheFirstChild(ref m_constrainedChoiceConsiderationsNode, m_helper.Node, Helper.Strings.ConstrainedChoiceConsiderations, Helper.Strings.AdlseqNamespace),
                    ref m_constrainChoice, Helper.Strings.ConstrainChoice,
                    !UseReference(m_constrainedChoiceConsiderationsNode) ? false : m_referenced.ConstrainChoice);
            }
        }

        /// <summary>
        /// Private method used by CreateNavigator() to move data from the referenced node into this one.
        /// </summary>
        /// <param name="navTo">Should be positioned on the &lt;sequencing&gt; node.</param>
        /// <param name="navFrom">Should be positioned on the &lt;sequencing&gt; node.</param>
        /// <param name="childName"></param>
        /// <param name="childNamespace"></param>
        private static void CheckAndCopyReference(XPathNavigator navTo, XPathNavigator navFrom, string childName, string childNamespace)
        {
            if (navTo.MoveToFollowing(childName, childNamespace))
            {
                // found it, so move back to parent and return
                navTo.MoveToParent();
            }
            else
            {
                // it's not here. insert it from the reference, if it's there.
                if (navFrom.MoveToFollowing(childName, childNamespace))
                {
                    navTo.AppendChild(navFrom);
                    navFrom.MoveToParent();
                }
            }
        }

        /// <summary>
        /// Return the XPathNavigator for this resource.
        /// </summary>
        /// <remarks>
        /// This allows an application to get custom namespaced data (e.g. "foo:bar") from the XML.
        /// <para>
        /// Note that because SCORM allows a &lt;sequencing&gt; node that has an IDRef attribute and is not in 
        /// a &lt;sequencingCollection&gt; refer to a &lt;sequencing&gt; node that has an ID attribute (equal to the
        /// IDRef of the previously mentioned &lt;sequencing&gt;) and is in a &lt;sequencingCollection&gt;, if the
        /// &lt;sequencing&gt; node read by this <Typ>SequencingNodeReader</Typ> has a valid IDRef attribute,
        /// the XML returned in the <Typ>XPathNavigator</Typ> of this method contains the synthesis of the node
        /// read by this <Typ>SequencingNodeReader</Typ> and the node referenced by the IDRef attribute.
        /// </para>
        /// <para>
        /// Following SCORM's rules, the direct child nodes of the current node take precedence over the
        /// direct child nodes of the referenced node.
        /// </para>
        /// </remarks>
        /// <example><code>
        /** If there is the following:

            &lt;sequencingCollection&gt;
              &lt;sequencing ID="seq1"&gt;
                &lt;deliveryControls completionSetByContent="true"/&gt;
                &lt;limitConditions attemptLimit="2"/&gt;
              &lt;/sequencing&gt;
            &lt;/sequencingCollection&gt;

            and there is also a &lt;sequencing&gt; node elsewhere such as:

            &lt;sequencing IDRef="seq1"&gt;
              &lt;limitConditions attemptLimit="3"/&gt;
            &lt;/sequencing&gt;

            The synthesized XML from CreateNavigator would contain:

            &lt;sequencing IDRef="seq1"&gt;
              &lt;deliveryControls completionSetByContent="true"/&gt;
              &lt;limitConditions attemptLimit="3"/&gt;
            &lt;/sequencing&gt;

            where the &lt;limitConditions&gt; exists in the IDRef node, and takes precedence over the one with the ID.
         */
        /// </code></example>
        public XPathNavigator CreateNavigator()
        {
            // If there is no reference, just return the current XPathNavigator.
            if (m_referenced == null)
            {
                return m_helper.Node.Clone();
            }
            // Synthesize XML from the current <sequencing> node and the referenced one.
            // Create a new <sequencing> node with no attributes.
            // Stream the contents of this node into another XPathDocument by going through an
            // XmlWriter containing a Stream.
            using (MemoryStream stream = new MemoryStream())
            {
                using(XmlWriter writer = XmlWriter.Create(stream))
                {
                    m_helper.Node.WriteSubtree(writer);
                    writer.Flush();
                }
                stream.Position = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XPathNavigator navTo = doc.CreateNavigator();
                // remove the IDRef attribute
                navTo.MoveToRoot();
                navTo.MoveToFollowing(Helper.Strings.Sequencing, Helper.Strings.ImsssNamespace);
                if (navTo.MoveToAttribute(Helper.Strings.IdRef, String.Empty))
                {
                    navTo.DeleteSelf();
                }
                // fill in necessary top-level child nodes from the reference for each of the possible nodes:
                XPathNavigator navFrom = m_referenced.CreateNavigator();
                // <controlMode>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.ControlMode, Helper.Strings.ImsssNamespace);
                // <sequencingRules>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.SequencingRules, Helper.Strings.ImsssNamespace);
                // <limitConditions>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.LimitConditions, Helper.Strings.ImsssNamespace);
                // <auxiliaryResources>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.AuxiliaryResources, Helper.Strings.ImsssNamespace);
                // <rollupRules>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.RollupRules, Helper.Strings.ImsssNamespace);
                // <objectives>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.Objectives, Helper.Strings.ImsssNamespace);
                // <randomizationControls>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.RandomizationControls, Helper.Strings.ImsssNamespace);
                // <deliveryControls>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.DeliveryControls, Helper.Strings.ImsssNamespace);
                // <adlseq:constrainedChoiceConsiderations>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.ConstrainedChoiceConsiderations, Helper.Strings.AdlseqNamespace);
                // <adlseq:rollupConsiderations>
                CheckAndCopyReference(navTo, navFrom, Helper.Strings.RollupConsiderations, Helper.Strings.AdlseqNamespace);

                return navTo;
            }
        }
    }

    /// <summary>
    /// Rules related to rollup. Every sequencing node can have 0 or more of these.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:rollupRule&gt; node, the &lt;imsss:rollupConditions&gt; node, and the
    /// &lt;imsss:rollupAction&gt; node.  It contains the list of &lt;imsss:rollupCondition&gt; nodes.
    /// </remarks>
    public class SequencingRollupRuleNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        // private member variables corresponding to attributes and node values
        private RollupChildActivitySet? m_childActivitySet;
        private int? m_minimumCount;
        private double? m_minimumPercent;
        private SequencingConditionCombination? m_conditionCombination;
        private ReadOnlyCollection<SequencingRollupConditionNodeReader> m_Conditions;
        private RollupAction? m_action;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rollupRuleNode">The &lt;imsss:rollupRule&gt; node to parse.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rollupRuleNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="rollupRuleNode"/> does not point to a &lt;imsss:rollupRule2&gt; node.</exception>
        internal SequencingRollupRuleNodeReader(XPathNavigator rollupRuleNode,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {

            m_helper = Helper.InternalCreateImsss(rollupRuleNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.RollupRule);
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupRule&gt;/childActivitySet attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>RollupChildActivitySet.All</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public RollupChildActivitySet ChildActivitySet
        {
            get
            {
                return m_helper.CacheTokenAttribute(m_helper.Node, ref m_childActivitySet, Helper.Strings.ChildActivitySet, RollupChildActivitySet.All);
            }
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupRule&gt;/minimumCount attribute.
        /// </summary>
        /// <remarks>
        /// Default value is 0.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public int MinimumCount
        {
            get
            {
                if (null == m_minimumCount)
                {
                    int? minimumCount = null;
                    m_helper.CacheAttribute<int>(m_helper.Node, ref minimumCount, Helper.Strings.MinimumCount, 0);
                    // range check: minimumCount must be >= 0
                    if (minimumCount < 0)
                    {
                        m_helper.LogBadAttribute(minimumCount.ToString(), Helper.Strings.MinimumCount,
                            m_helper.Node.Name, "0", ValidatorResources.IllegalNegativeInteger);
                        minimumCount = 0;
                    }
                    m_minimumCount = minimumCount;
                }
                return m_minimumCount.Value;
            }
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupRule&gt;/minimumPercent attribute.
        /// </summary>
        /// <remarks>
        /// Default value is 0.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public double MinimumPercent
        {
            get
            {
                if (null == m_minimumPercent)
                {
                    double? minimumPercent = null;
                    m_helper.CacheAttribute<double>(m_helper.Node, ref minimumPercent, Helper.Strings.MinimumPercent, 0);
                    // range check: minimumPercent must be between 0 and 1.
                    if (minimumPercent < 0 || minimumPercent > 1)
                    {
                        m_helper.LogBadAttribute(minimumPercent.ToString(), Helper.Strings.MinimumPercent,
                            m_helper.Node.Name, "0", ValidatorResources.IllegalPercentage);
                        minimumPercent = 0;
                    }
                    m_minimumPercent = minimumPercent;
                }
                return m_minimumPercent.Value;
            }
        }

        // private methods to return an iterator on the rollupConditions node so the parsing only occurs once.
        private XPathNodeIterator m_rollupConditions;
        private XPathNodeIterator GetRollupConditions()
        {
            if (null == m_rollupConditions)
            {
                XPathNodeIterator rollupConditions = m_helper.Node.SelectChildren(Helper.Strings.RollupConditions, Helper.Strings.ImsssNamespace);
                if (rollupConditions.Count > 1)
                {
                    m_helper.LogInvalidDuplicateNode(Helper.Strings.RollupConditions, false, ValidatorResources.DuplicateRollupConditions);
                }
                m_rollupConditions = rollupConditions;
            }
            return m_rollupConditions.Clone();
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupRule&gt;/&lt;imsss:rollupConditions&gt;/conditionCombination attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>SequencingConditionCombination.Any</c>, which is applied if the conditionCombination attribute
        /// is missing.  If the entire &lt;imsss:rollupConditions&gt; node is missing, the value of
        /// <c>SequencingConditionCombination.Any</c> is also returned in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, and a warning issued
        /// to the log.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> this issues an error to the log and an exception is thrown.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> and there is more than one &lt;imsss:rollupConditions&gt;
        /// node, a warning is issued to the log and only the first node is parsed.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid, or there are more than one &lt;imsss:rollupConditions&gt; nodes, or the 
        /// node is missing.</exception>
        public SequencingConditionCombination ConditionCombination
        {
            get
            {
                if (null == m_conditionCombination)
                {
                    // The rollupConditions element is required in content aggregation. Its absence invalidates the entire
                    // rollupRule.  In relaxed parsing, if there are multiple rollupConditions elements, use the first
                    // one.  If not valid, return the default.  In strict parsing, throw.
                    XPathNodeIterator itr = GetRollupConditions();
                    SequencingConditionCombination? conditionCombination = null;
                    if (itr.MoveNext())
                    {
                        m_helper.CacheTokenAttribute<SequencingConditionCombination>(itr.Current, ref conditionCombination,
                            Helper.Strings.ConditionCombination, SequencingConditionCombination.Any);
                    }
                    else
                    {
                        m_helper.LogRequiredNodeMissing(Helper.Strings.RollupConditions, ValidatorResources.RollupConditionsMissing);
                        conditionCombination = SequencingConditionCombination.Any; // value when the node is missing
                    }
                    m_conditionCombination = conditionCombination;
                }
                return m_conditionCombination.Value;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;<Typ>SequencingRollupConditionNodeReader</Typ>&gt;</c> containing the &lt;imsss:rollupCondition&gt;
        /// nodes inside this &lt;imsss:rollupRule&gt;/&lt;imsss:rollupConditions&gt;.
        /// </summary>
        /// <remarks>
        /// The <c>ReadOnlyCollection&lt;SequencingRollupConditionNodeReader&gt;</c> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid &lt;imsss:rollupCondition&gt; nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid &lt;imsss:rollupCondition&gt; nodes are ignored.
        /// </para>
        /// <para>SCORM dictates there should always be exactly one &lt;imsss:rollupConditions&gt; node containing at
        /// least one &lt;imsss:rollupCondition&gt; node.  However, it is possible this ReadOnlyCollection will contain zero nodes,
        /// if none exist in the manifest.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid
        /// &lt;imsss:rollupCondition&gt; node, or the &lt;rollupConditions&gt; node does not exist.</exception>
		/// <example>
        /// Iterate through all rollup conditions in the manifest's sequencing collection node.
        /// <code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingRollupRuleNodeReader rollupRule in sequencing.RollupRules)
		///         {
		///             foreach (SequencingRollupConditionNodeReader rollupCondition in rollupRule.Conditions)
		///             {
		///                 // use the rollup condition reader
		///             }
		///         }
		///     }
		/// </code></example>
        public ReadOnlyCollection<SequencingRollupConditionNodeReader> Conditions
        {
            get
            {
                if (null == m_Conditions)
                {
                    XPathNodeIterator itr = GetRollupConditions();
                    if (itr.Count == 0)
                    {
                        // if there are no nodes, return an empty collection in relaxed mode, or throw in strict mode.
                        m_helper.LogRequiredNodeMissing(Helper.Strings.RollupConditions, ValidatorResources.RollupConditionsMissing);
                        m_Conditions = new ReadOnlyCollection<SequencingRollupConditionNodeReader>(
                            new ReadOnlyMlcCollection<SequencingRollupConditionNodeReader>());
                    }
                    else
                    {
                        itr.MoveNext();
                        m_Conditions = CreateConditionsCollection(itr.Current);
                    }
                }
                return m_Conditions;
            }
        }

        // method to create the ReadOnlyMLCCollection for the rollupCondition nodes.  Can throw InvalidPackageException.
        private ReadOnlyCollection<SequencingRollupConditionNodeReader> CreateConditionsCollection(XPathNavigator rollupConditionsNode)
        {
            XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imsss + ":" + Helper.Strings.RollupCondition);
            ReadOnlyCollection<SequencingRollupConditionNodeReader> coll = new ReadOnlyCollection<SequencingRollupConditionNodeReader>(
                new ReadOnlyMlcCollection<SequencingRollupConditionNodeReader>(rollupConditionsNode, expr,
                    delegate(XPathNavigator conditionNode /* the navigator pointing to the rollupCondition node */)
                    {
                        // The "condition" attribute is required.  If it is not present, this node is invalid.
                        if(conditionNode.GetAttribute(Helper.Strings.Condition, String.Empty) == String.Empty)
                        {
                            m_helper.LogRequiredAttributeMissing(Helper.Strings.Condition, conditionNode.Name, ValidatorResources.RollupConditionConditionMissing);
                            return null; // in the warning case, this node gets skipped. The error case throws InvalidPackageException.
                        }
                        return new SequencingRollupConditionNodeReader(conditionNode, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                    }));
            return coll;
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupRule&gt;/&lt;imsss:rollupAction&gt;/action attribute.
        /// </summary>
        /// <remarks>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> and the &lt;imsss:rollupAction&gt; node has
        /// an invalid action attribute, a default action of <c>RollupAction.Satisfied</c> is returned.  Note: this
        /// is not a SCORM default.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> and there is more than one &lt;imsss:rollupAction&gt;
        /// node, a warning is issued to the log and only the first node is parsed.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid, or there are more than one &lt;imsss:rollupAction&gt; nodes.</exception>
        public RollupAction Action
        {
            get
            {
                if (null == m_action)
                {
                    XPathNodeIterator itr = m_helper.Node.SelectChildren(Helper.Strings.RollupAction, Helper.Strings.ImsssNamespace);
                    if (itr.MoveNext())
                    {
                        // if there are more than one node, issue a warning or error as appropriate
                        if (itr.Count > 1)
                        {
                            m_helper.LogInvalidDuplicateNode(Helper.Strings.RollupAction, false, ValidatorResources.DuplicateRollupAction);
                        }
                        m_helper.CacheTokenAttribute<RollupAction>(itr.Current, ref m_action,
                            Helper.Strings.Action, RollupAction.Satisfied);
                    }
                    else
                    {
                        // if there are no nodes
                        m_helper.LogRequiredNodeMissing(Helper.Strings.RollupAction, ValidatorResources.RollupActionMissing);
                        m_action = RollupAction.Satisfied; // made-up default value
                    }
                }
                return m_action.Value;
            }
        }
    }

    /// <summary>
    /// A conditon for rollup. Every sequencing rule can have 0 or many of these.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:rollupCondition&gt; node.
    /// </remarks>
    public class SequencingRollupConditionNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        // private member variables corresponding to attributes on <imsss:rollupCondition>
        private SequencingConditionOperator? m_operator = null;
        private RollupCondition? m_condition = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rollupConditionNode">The &lt;imsss:rollupCondition&gt; node to parse.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rollupConditionNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="rollupConditionNode"/> does not point to a &lt;imsss:rollupCondition2&gt; node.</exception>
        internal SequencingRollupConditionNodeReader(XPathNavigator rollupConditionNode,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            m_helper = Helper.InternalCreateImsss(rollupConditionNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.RollupCondition);
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupCondition&gt;/operator attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>SequencingConditionOperator.NoOp</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public SequencingConditionOperator Operator
        {
            get
            {
                return m_helper.CacheTokenAttribute<SequencingConditionOperator>(
                    m_helper.Node, ref m_operator, Helper.Strings.Operator, SequencingConditionOperator.NoOp);
            }
        }

        /// <summary>
        /// Respresents the &lt;imsss:rollupCondition&gt;/condition attribute.
        /// </summary>
        /// <remarks>
        /// Non-SCORM default is <c>RollupCondition.Satisfied</c>.  This is returned when the attribute is not included, or if
        /// the attribute value is invalid and the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public RollupCondition Condition
        {
            get
            {
                return m_helper.CacheTokenAttribute<RollupCondition>(
                    m_helper.Node, ref m_condition, Helper.Strings.Condition, RollupCondition.Satisfied);
            }
        }
    }

    /// <summary>
    /// An objective related to the sequencing node. Every sequencing node can have 1 or 
    /// more of these. One of them must be the primary objective.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:primaryObjective&gt; and &lt;imsss:objective&gt; nodes.
    /// Contains the &lt;imsss:minNormalizedMeasure&gt; node and &lt;imsss:mapInfo&gt; nodes.
    /// </remarks>
    public class SequencingObjectiveNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        // private member variables corresponding to attributes on <primaryObjective> and <objective>
        private bool? m_satisfiedByMeasure;
        private string m_objectiveId;
        private double? m_minNormalizedMeasure;
        private ReadOnlyCollection<SequencingObjectiveMapNodeReader> m_mapInfo;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectiveNode">A navigator to an &lt;objective&gt; or &lt;primaryObjective&gt; node.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objectiveNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="objectiveNode"/> does not point to a &lt;imsss:primaryObjective&gt; 
        /// or &lt;imsss:objective&gt; node.</exception>
        internal SequencingObjectiveNodeReader(XPathNavigator objectiveNode,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            // Doesn't call usual Helper constructor because the node can be either <objective> or <primaryObjective> and
            // the normal constructor doesn't support that.
            if (objectiveNode == null) throw new LearningComponentsInternalException("MR016", "objectiveNode");
            if ((objectiveNode.LocalName == Helper.Strings.Objective 
                || objectiveNode.LocalName == Helper.Strings.PrimaryObjective)
                && objectiveNode.NamespaceURI == Helper.Strings.ImsssNamespace)
            {
                m_helper = Helper.InternalNoNameCheckCreateImsss(objectiveNode, manifestSettings, validatorSettings, logReplacement, log);
            }
            else
            {
                throw new LearningComponentsInternalException("MR017", objectiveNode.Name + "," + objectiveNode.NamespaceURI);
            }
        }

        /// <summary>
        /// True if this node is a &lt;primaryObjective&gt; node.  Otherwise, false (it is an &lt;objective&gt; node).
        /// </summary>
        public bool IsPrimaryObjective
        {
            get
            {
                if (m_helper.Node.LocalName == Helper.Strings.Objective)
                    return false;
                else return true;
            }
        }

        /// <summary>
        /// Returns the value of the satisfiedByMeasure attribute.  Default value of <c>false</c> is returned if the
        /// attribute is omitted.  The <c>false</c> value will also be returned in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> if an
        /// invalid value is applied to the satisfiedByMeasure attribute (legal values are "true", "false", "0", and "1".)
        /// </summary>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public bool SatisfiedByMeasure
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_satisfiedByMeasure, Helper.Strings.SatisfiedByMeasure, false);
            }
        }

        /// <summary>
        /// Get the value of the objectiveID attribute.
        /// </summary>
        /// <remarks>
        /// If the attribute value is an illegal URI: 
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, will log a warning and return <c>String.Empty</c> 
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, will log an error and throw an <Typ>InvalidPackageException</Typ>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public string Id
        {
            get
            {
                // required in content aggregation if a mapInfo is contained. Otherwise, this objective is ignored.
                return m_helper.CacheAnyUriAttribute(m_helper.Node, ref m_objectiveId, Helper.Strings.ObjectiveId,
                    0, BaseSchemaInternal.ActivityObjectiveItem.MaxKeyLength);
            }
        }

        /// <summary>
        /// Get the value of the &lt;minimumNormalizedMeasure&gt; node.
        /// </summary>
        /// <remarks>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, if there are more than one &lt;minimumNormalizedMeasure&gt; nodes within
        /// the objective, a warning is added to the log and the first node is returned, or if the value is not a
        /// decimal value, a warning is added to the log and a value of "1.0" is returned..  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// in these cases, errors are added to the log and an <Typ>InvalidPackageException</Typ> is thrown.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public double MinimumNormalizedMeasure
        {
            get
            {
                // get a list of all <imsss:minNormalizedMeasure> nodes under the objective.
                // If there are no such nodes, return 1.0.
                // If there is more than one, it is a warning in "Relaxed" mode and an error in "Strict" mode.
                // Get the value of the first one.  If it is not a decimal, issue a warning in "Relaxed" mode
                // and set to 1.0.  Issue an error in "Strict" mode.
                if (null == m_minNormalizedMeasure)
                {
                    XPathNodeIterator itr = m_helper.Node.SelectChildren(Helper.Strings.MinNormalizedMeasure, Helper.Strings.ImsssNamespace);
                    if (itr.MoveNext())
                    {
                        // if there are more than one node, issue a warning or error as appropriate
                        if (itr.Count > 1)
                        {
                            // the following can also cause an error if there is no ObjectiveId.
                            string reason = String.Format(CultureInfo.CurrentCulture, ValidatorResources.DuplicateMinNormalizedMeasure,
                                m_helper.Node.Name, Id);

                            m_helper.LogInvalidDuplicateNode(Helper.Strings.MinNormalizedMeasure, false, reason);
                        }
                        double d;
                        try
                        {
                            d = itr.Current.ValueAsDouble;
                        }
                        catch (FormatException)
                        {
                            m_helper.LogInvalidNodeValue(itr.Current.Value, itr.Current.Name, ValidatorResources.ElementRemoved, ValidatorResources.ValueIncorrectType);
                            d = 1; // default value
                        }
                        // check that the value is not out of range [-1 to 1].  If it is, issue a warning or error as appropriate, and
                        // set to the default value.
                        if (d > 1 || d < -1)
                        {
                            // the following can also cause an error if there is no ObjectiveId.
                            string reason = String.Format(CultureInfo.CurrentCulture, ValidatorResources.MinNormalizedMeasureOutOfRange,
                                m_helper.Node.Name, Id);

                            m_helper.LogInvalidNodeValue(itr.Current.Value, itr.Current.Name, "1.0", reason);
                            d = 1;
                        }
                        m_minNormalizedMeasure = d;
                    }
                    else
                    {
                        // if there are no nodes, use the default value without warning or error
                        m_minNormalizedMeasure = 1; // default value
                    }
                }
                return m_minNormalizedMeasure.Value;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingObjectiveMapNodeReader&gt;</c> containing the &lt;imsss:mapInfo&gt;
        /// nodes inside this &lt;imsss:objective&gt; or &lt;imsss:primaryObjective&gt;.
        /// </summary>
        /// <remarks>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, an <Typ>InvalidPackageException</Typ> is thrown if there are illegal
        /// &lt;imsss:mapInfo&gt; nodes in the list.  For instance, if there are more than one &lt;imsss:mapInfo&gt; nodes
        /// containing an attribute value of "true" for the "readSatisfiedStatus" attribute, this is in illegal
        /// condition.
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, a warning is added to the log and only the first instance of a 
        /// &lt;imsss:mapInfo&gt; containing a value of "true" for a specific attribute is parsed.  Any illegal
        /// &lt;imsss:mapInfo&gt; nodes are omitted and ignored.
        /// </para>
        /// </remarks>
		/// <example>
        /// Iterate through all &lt;mapInfo&gt; nodes in a manifest's sequencing collection.
        /// <code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingObjectiveNodeReader objective in sequencing.Objectives.Values)
		///         {
		///             foreach (SequencingObjectiveMapNodeReader map in objective.Mappings)
		///             {
		///                 // Use the SequencingObjectiveMapNodeReader
		///             }
		///         }
		///     }
		/// </code></example>
        public ReadOnlyCollection<SequencingObjectiveMapNodeReader> Mappings
        {
            get
            {
                if (null == m_mapInfo)
                    m_mapInfo = CreateMappingsCollection(m_helper.Node);
                return m_mapInfo;
            }
        }

        // bool values used in the CreateMappingsCollection method to make sure nodes are valid (only one
        // <mapInfo> node can contain a "true" value for each of these attributes.
        private bool m_foundReadSatisfiedStatus;
        private bool m_foundReadNormalizedMeasure;
        // the "write" attributes can only have a single "true" value per targetObjectiveID, so need to keep a collection
        // for each of them, and methods to check them.
        private Collection<string> m_foundWriteSatisfiedStatus;
        private Collection<string> m_foundWriteNormalizedMeasure;
        private static bool CheckWriteAttribute(ref Collection<string> collection, string targetObjectiveId)
        {
            if (collection == null)
            {
                collection = new Collection<string>();
                return false;
            }
            else
            {
                return collection.Contains(targetObjectiveId);
            }
        }
        private static void SetWriteAttribute(ref Collection<string> collection, string targetObjectiveid)
        {
            if (collection == null)
            {
                collection = new Collection<string>();
            }
            if (!collection.Contains(targetObjectiveid))
            {
                collection.Add(targetObjectiveid);
            }
        }

        // method to create the ReadOnlyMLCCollection for the mapInfo nodes.
        private ReadOnlyCollection<SequencingObjectiveMapNodeReader> CreateMappingsCollection(XPathNavigator objectiveNode)
        {
            XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imsss + ":" + Helper.Strings.MapInfo);
            ReadOnlyCollection<SequencingObjectiveMapNodeReader> coll = new ReadOnlyCollection<SequencingObjectiveMapNodeReader>(
                new ReadOnlyMlcCollection<SequencingObjectiveMapNodeReader>(objectiveNode, expr,
                    delegate(XPathNavigator mapNode /* the navigator pointing to the sequencing rule node */)
                    {
                        // There can only be one mapInfo with a "true" value for each of the readSatisfiedStatus and
                        // readNormalizedMeasure, and the writeSatisfiedStatus and writeNormalizedMeasure for a targetObjectiveId.
                        // In relaxed mode, only the first mapInfo node with each of these set to true is added to the list.
                        // In strict mode, if more than one mapInfo node has one of these set to true an InvalidPackageException is thrown.
                        SequencingObjectiveMapNodeReader mapInfo = new SequencingObjectiveMapNodeReader(mapNode, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                        if ((mapInfo.ReadNormalizedMeasure && m_foundReadNormalizedMeasure)
                            || (mapInfo.ReadSatisfiedStatus && m_foundReadSatisfiedStatus)
                            || (mapInfo.WriteNormalizedMeasure && CheckWriteAttribute(ref m_foundWriteNormalizedMeasure, mapInfo.TargetObjectiveId))
                            || (mapInfo.WriteSatisfiedStatus && CheckWriteAttribute(ref m_foundWriteSatisfiedStatus, mapInfo.TargetObjectiveId)))
                        {
                            m_helper.LogInvalidNode(mapNode.Name, ValidatorResources.InvalidTrueValuesInMapInfo);
                            mapInfo = null;
                        }
                        // if we still have a valid mapInfo, set the bool values we'll need to check the next mapInfos.
                        else
                        {
                            m_foundReadNormalizedMeasure |= mapInfo.ReadNormalizedMeasure;
                            m_foundReadSatisfiedStatus |= mapInfo.ReadSatisfiedStatus;
                            if (mapInfo.WriteNormalizedMeasure)
                            {
                                SetWriteAttribute(ref m_foundWriteNormalizedMeasure, mapInfo.TargetObjectiveId);
                            }
                            if (mapInfo.WriteSatisfiedStatus)
                            {
                                SetWriteAttribute(ref m_foundWriteSatisfiedStatus, mapInfo.TargetObjectiveId);
                            }
                        }
                        return mapInfo;
                    }));
            return coll;
        }
    }

    /// <summary>
    /// Map an objective to a global objective. Every objective can be mapped to 
    /// 0 or more global objectives.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:mapInfo&gt; node.
    /// </remarks>
    public class SequencingObjectiveMapNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        // private member variables corresponding to attributes on <mapInfo>
        private string m_targetObjectiveId;
        private bool? m_readSatisfiedStatus;
        private bool? m_readNormalizedMeasure;
        private bool? m_writeSatisfiedStatus;
        private bool? m_writeNormalizedMeasure;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapInfoNode">The &lt;imsss:mapInfo&gt; node to parse.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="mapInfoNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="mapInfoNode"/> does not point to a &lt;imsss:mapInfo&gt; node.</exception>
        internal SequencingObjectiveMapNodeReader(XPathNavigator mapInfoNode,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            m_helper = Helper.InternalCreateImsss(mapInfoNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.MapInfo);
        }

        /// <summary>
        /// Represents the &lt;imsss:mapInfo&gt;/targetObjectiveId attribute.
        /// </summary>
        /// <remarks>The attribute value must be a valid URI, and may not be whitespace.  The default value
        /// is String.Empty.  SCORM mandates that this &lt;imsss:mapInfo&gt; node be ignored when targetObjectiveId is
        /// missing, which is signified by the String.Empty return value.</remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public string TargetObjectiveId
        {
            get
            {
                return m_helper.CacheAnyUriAttribute(m_helper.Node, ref m_targetObjectiveId, Helper.Strings.TargetObjectiveID, 0 ,0);
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:mapInfo&gt;/readSatisfiedStatus attribute.
        /// </summary>
        /// <remarks>Default value is true.</remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public bool ReadSatisfiedStatus
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_readSatisfiedStatus, Helper.Strings.ReadSatisfiedStatus, true);
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:mapInfo&gt;/readNormalizedMeasure attribute.
        /// </summary>
        /// <remarks>Default value is true.</remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public bool ReadNormalizedMeasure
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_readNormalizedMeasure, Helper.Strings.ReadNormalizedMeasure, true);
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:mapInfo&gt;/writeSatisfiedStatus attribute.
        /// </summary>
        /// <remarks>Default value is false.</remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public bool WriteSatisfiedStatus
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_writeSatisfiedStatus, Helper.Strings.WriteSatisfiedStatus, false);
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:mapInfo&gt;/writeNormalizedMeasure attribute.
        /// </summary>
        /// <remarks>Default value is false.</remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public bool WriteNormalizedMeasure
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_writeNormalizedMeasure, Helper.Strings.WriteNormalizedMeasure, false);
            }
        }
    }

    /// <summary>
    /// A single rule related to sequencing.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:preConditionRule&gt;, &lt;imsss:exitConditionRule&gt;, and &lt;imsss:postConditionRule&gt;
    /// nodes, subnodes &lt;imsss:ruleConditions&gt;, and &lt;imsss:ruleAction&gt; and holds a list of the &lt;imsss:ruleConditions&gt; under
    /// the &lt;imsss:ruleConditions&gt; node.
    /// <para>
    /// Note that whether this node actually corresponds to a pre, exit, or post condition is information not held in this
    /// node.  Rather, these nodes are contained as separate lists of pre, exit, and post condition nodes in the
    /// <Typ>SequencingNodeReader</Typ>.
    /// </para>
    /// </remarks>
    public class SequencingRuleNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        // private member variables corresponding to attributes on the <preConditionRule>, <exitConditionRule>, or
        // <postConditionRule>, and the subnodes <ruleConditions> and <ruleAction>, as well as a list of <ruleCondtion>
        // nodes beneath the <ruleConditions>
        private SequencingConditionCombination? m_combination;
        private XPathNodeIterator m_conditionsNode; // the <imsss:ruleConditions> nodes under this condition. Use GetRuleConditionsNode()
            // instead of accessing this directly.
        private ReadOnlyCollection<SequencingRuleConditionNodeReader> m_conditions;
        private SequencingRuleAction? m_action; // m_foundAction is true when this is has been parsed, since null is a legal value
        private bool m_foundAction; // is true when m_action is viable
        // private enum and variable to track the type of sequencing rule node this is: preConditionRule, exitConditionRule, or postConditionRule
        private enum ConditionType
        {
            Pre,
            Exit,
            Post
        };
        private ConditionType m_conditionType;
        private ActivityNodeReader m_activity; // the activity that owns this sequencing rule
        private ManifestReader m_manifest; // manifest that owns this rule condition.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sequencingRuleNode">The node to parse.  Must be of type &lt;preConditionRule&gt;,
        /// &lt;exitConditionRule&gt;, or &lt;postConditionRule&gt;.</param>
        /// <param name="manifest">The <Typ>ManifestReader</Typ> that contains this &lt;sequencing&lt; node.  Can be null.</param>
        /// <param name="activity">The <Typ>ActivityNodeReader</Typ> that contains this &lt;sequencing&lt; node.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="mapInfoNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="mapInfoNode"/> does not point to a &lt;imsss:mapInfo&gt; node.</exception>
        internal SequencingRuleNodeReader(XPathNavigator sequencingRuleNode, ManifestReader manifest, ActivityNodeReader activity,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings, bool logReplacement, ValidationResults log)
        {
            m_activity = activity;
            m_manifest = manifest;
            if (sequencingRuleNode == null) throw new LearningComponentsInternalException("MR018", "sequencingRuleNode");
            if ((sequencingRuleNode.LocalName == Helper.Strings.PostConditionRule
                || sequencingRuleNode.LocalName == Helper.Strings.PreConditionRule
                || sequencingRuleNode.LocalName == Helper.Strings.ExitConditionRule)
                && sequencingRuleNode.NamespaceURI == Helper.Strings.ImsssNamespace)
            {
                switch(sequencingRuleNode.LocalName)
                {
                    case Helper.Strings.PreConditionRule:
                        m_conditionType = ConditionType.Pre;
                        break;
                    case Helper.Strings.ExitConditionRule:
                        m_conditionType = ConditionType.Exit;
                        break;
                    default:
                    case Helper.Strings.PostConditionRule:
                        m_conditionType = ConditionType.Post;
                        break;
                }
                m_helper = Helper.InternalNoNameCheckCreateImsss(sequencingRuleNode, manifestSettings, validatorSettings, logReplacement, log);
            }
            else
            {
                throw new LearningComponentsInternalException("MR019", sequencingRuleNode.Name + "," + sequencingRuleNode.NamespaceURI);
            }
        }

        /// <summary>
        /// Helper function to get the &lt;imsss:ruleConditions&gt; node.  Caller should check the <c>Count</c> to make sure
        /// there is a node.  This method returns with the <Typ>XPathNodeIterator</Typ> already selected to the first node,
        /// so the caller should *not* call <c>MoveNext()</c>.
        /// </summary>
        /// <remarks>
        /// There should be only one &lt;imsss:ruleConditions&gt; node.  If there is more than one, in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>,
        /// a warning is issued to the log and the first one is returned.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an error is issued to the
        /// log and an <Typ>InvalidPackageException</Typ> is thrown.
        /// </remarks>
        /// <exception cref="InvalidPackageException">Thrown in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there are multiple
        /// &lt;imsss:ruleCondition&gt; nodes.</exception>
        private XPathNodeIterator GetRuleConditionsNode()
        {
            if (m_conditionsNode == null)
            {
                // get the list of <imsss:ruleConditions> nodes.  There should be only one.
                XPathNodeIterator conditionsNode = m_helper.Node.SelectChildren(Helper.Strings.RuleConditions, Helper.Strings.ImsssNamespace);
                conditionsNode.MoveNext();
                // if there is more than one <imsss:ruleConditions> this is a warning or error.
                if (conditionsNode.Count > 1)
                {
                    m_helper.LogInvalidDuplicateNode(Helper.Strings.RuleConditions, false,
                        String.Format(CultureInfo.CurrentCulture, ValidatorResources.DuplicateRuleConditions, m_helper.Node.Name));
                }
                m_conditionsNode = conditionsNode;
            }
            return m_conditionsNode.Clone();
        }

        /// <summary>
        /// Returns the &lt;imsss:ruleConditions&gt;/conditionCombination attribute value.
        /// </summary>
        /// <remarks>
        /// There should be only one &lt;imsss:ruleConditions&gt; node.  If there is more than one, in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>,
        /// a warning is issued to the log and the first one is returned.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an error is issued to the
        /// log and an <Typ>InvalidPackageException</Typ> is thrown.
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> if there is an invalid token for the conditionCombination attribute, the default
        /// value of <c>SequencingConditionCombination.All</c> is returned.
        /// </para>
        /// The default value of <c>SequencingConditionCombination.All</c> is returned in both <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>
        /// and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is no &lt;imsss:ruleConditions&gt; node.
        /// <para>
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Thrown in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there are multiple
        /// &lt;imsss:ruleConditions&gt; nodes or if the conditionCombination attribute is an invalid value.</exception>
        public SequencingConditionCombination Combination  // all or any combination of conditions
        {
            get
            {
                if (m_combination == null)
                {
                    XPathNodeIterator itr = GetRuleConditionsNode();
                    SequencingConditionCombination? combination = null;
                    // Get the conditionCombination attribute from the first <imsss:ruleConditions> node.  Since the conditionCombination
                    // is the only attribute there is no need to make a separate helper method to
                    // parse the list of ruleConditions nodes.
                    // it is not an error condition for this node to be missing. Just return the default in that case.
                    if (itr.Count == 0)
                    {
                        combination = SequencingConditionCombination.All;
                    }
                    else
                    {
                        m_helper.CacheTokenAttribute<SequencingConditionCombination>(itr.Current,
                            ref combination, Helper.Strings.ConditionCombination, SequencingConditionCombination.All);
                    }
                    m_combination = combination;
                }
                return m_combination.Value;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;SequencingRuleConditionNodeReader&gt;</c> containing the &lt;imsss:ruleCondition&gt;
        /// nodes inside the &lt;imsss:ruleConditions&gt;.
        /// </summary>
        /// <remarks>
        /// The <c>ReadOnlyCollection&lt;SequencingRuleConditionNodeReader&gt;</c> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid &lt;imsss:ruleCondition&gt; nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid &lt;imsss:ruleCondition&gt; nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no &lt;imsss:ruleCondition&gt; nodes, an empty <Typ>ReadOnlyCollection&lt;SequencingRuleConditionNodeReader&gt;</Typ>
        /// is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid
        /// &lt;imsss:ruleCondition&gt; node.</exception>
		/// <example>
        /// Iterate through all rule conditions for the post condition rules in a manifest's sequencing collection.
        /// <code language="C#">
		///     foreach(SequencingNodeReader sequencing in manifest.SequencingCollection.Values)
		///     {
		///         foreach (SequencingRuleNodeReader rule in sequencing.PostConditionRules)
		///         {
		///             foreach (SequencingRuleConditionNodeReader condition in rule.Conditions)
		///             {
		///                 // Use the SequencingRuleConditionNodeReader
		///             }
		///         }
		///     }
		/// </code></example>
        public ReadOnlyCollection<SequencingRuleConditionNodeReader> Conditions
        {
            get
            {
                if (null == m_conditions)
                {
                    XPathNodeIterator itr = GetRuleConditionsNode();
                    if (itr.Count == 0)
                    {
                        // if there aren't any, return an empty collection.
                        return new ReadOnlyCollection<SequencingRuleConditionNodeReader>(
                            new ReadOnlyMlcCollection<SequencingRuleConditionNodeReader>());
                    }
                    else
                    {
                        // the itr is already positioned on the first node.  Create the collection from that node.
                        m_conditions = CreateRuleConditionCollection(itr.Current);
                    }
                }
                return m_conditions;
            }
        }

        private ReadOnlyCollection<SequencingRuleConditionNodeReader> CreateRuleConditionCollection(XPathNavigator ruleNode)
        {
            XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imsss + ":" + Helper.Strings.RuleCondition);
            ReadOnlyCollection<SequencingRuleConditionNodeReader> coll = new ReadOnlyCollection<SequencingRuleConditionNodeReader>(
                new ReadOnlyMlcCollection<SequencingRuleConditionNodeReader>(ruleNode, expr,
                    delegate(XPathNavigator conditionNode /* the navigator pointing to the ruleCondition node */)
                    {
                        // The "condition" attribute is required.  If it is not present, this node is invalid.
                        if (conditionNode.GetAttribute(Helper.Strings.Condition, String.Empty) == String.Empty)
                        {
                            m_helper.LogRequiredAttributeMissing(Helper.Strings.Condition, conditionNode.Name, ValidatorResources.RuleConditionConditionMissing);
                            return null; // in the warning case, this node gets skipped. The error case throws InvalidPackageException.
                        }
                        return new SequencingRuleConditionNodeReader(conditionNode, m_manifest, m_activity, m_helper.ReaderSettings, m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log);
                    }));
            return coll;
        }

        /// <summary>
        /// Returns the &lt;imsss:ruleAction&gt;/action attribute value, or <c>SequencingRuleAction.NoAction</c> if there is none.
        /// </summary>
        /// <remarks>
        /// There should be only one &lt;imsss:ruleAction&gt; node.  If there is more than one, in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>,
        /// a warning is issued to the log and the first one is returned.  In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an error is issued to the
        /// log and an <Typ>InvalidPackageException</Typ> is thrown.
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> if there is an invalid token for the action attribute, <c>SequencingRuleAction.NoAction</c>
        /// is returned.
        /// </para>
        /// <para>
        /// Only a subset of <Typ>SequencingRuleAction</Typ> values are valid for each rule condition type.
        /// For &lt;imsss:preConditionRule&gt;, valid <Typ>SequencingRuleAction</Typ> values are: <c>Skip, Disabled,
        /// HiddenFromChoice, and StopForwardTraversal</c>.
        /// For &lt;imsss:exitConditionRule&gt;, the only valid <Typ>SequencingRuleAction</Typ> value is: <c>Exit</c>.
        /// For &lt;imsss:postConditionRule&gt;, valid <Typ>SequencingRuleAction</Typ> values are: <c>ExitParent,
        /// ExitAll, Retry, RetryAll, Continue, and Previous</c>.
        /// </para>
        /// <para>
        /// <c>SequencingRuleAction.NoAction</c> is returned in both <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>
        /// and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is no &lt;imsss:ruleAction&gt; node.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Thrown in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there are multiple
        /// &lt;imsss:ruleAction&gt; nodes or if the action attribute is an invalid value.</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")] // Much of the complexity is due to error checking and simple switch statements.  Not sure if it would truly be less complex if it was split into different methods...
        public SequencingRuleAction Action
        {
            get
            {
                if (!m_foundAction)
                {
                    // get the list of <imsss:ruleAction> nodes.  There should be only one.
                    XPathNodeIterator itr = m_helper.Node.SelectChildren(Helper.Strings.RuleAction, Helper.Strings.ImsssNamespace);
                    if (itr.MoveNext())
                    {
                        if (itr.Count > 1)
                        {
                            m_helper.LogInvalidDuplicateNode(Helper.Strings.RuleAction, false, ValidatorResources.DuplicateRuleAction);
                        }
                        XPathNavigator attribute = itr.Current.Clone();
                        if (!attribute.MoveToAttribute(Helper.Strings.Action, String.Empty))
                        {
                            // if the attribute doesn't exist, set to SequencingRuleAction.NoAction
                            m_action = SequencingRuleAction.NoAction;
                        }
                        else
                        {
                            try
                            {
                                // set the value to the attribute value, but with the first character upper-case to match the enum
                                string strValue = attribute.Value.Substring(0, 1).ToUpperInvariant() + attribute.Value.Substring(1);
                                m_action = (SequencingRuleAction)Enum.Parse(typeof(SequencingRuleAction), strValue);

                                switch(m_conditionType)
                                {
                                    case ConditionType.Pre:
                                        if (m_action != SequencingRuleAction.Skip
                                            && m_action != SequencingRuleAction.Disabled
                                            && m_action != SequencingRuleAction.HiddenFromChoice
                                            && m_action != SequencingRuleAction.StopForwardTraversal)
                                        {
                                            throw new ArgumentException(); // get caught by the catch below
                                        }
                                        break;
                                    case ConditionType.Exit:
                                        if (m_action != SequencingRuleAction.Exit)
                                        {
                                            throw new ArgumentException(); // get caught by the catch below
                                        }
                                        break;
                                    default:
                                    case ConditionType.Post:
                                        if (m_action != SequencingRuleAction.ExitParent
                                            && m_action != SequencingRuleAction.ExitAll
                                            && m_action != SequencingRuleAction.Retry
                                            && m_action != SequencingRuleAction.RetryAll
                                            && m_action != SequencingRuleAction.Continue
                                            && m_action != SequencingRuleAction.Previous)
                                        {
                                            throw new ArgumentException(); // get caught by the catch below
                                        }
                                        break;
                                }
                            }
                            catch (ArgumentException)
                            {
                                m_helper.LogBadAttribute(attribute.Value, attribute.Name, itr.Current.Name, ValidatorResources.ElementRemoved,
                                    ValidatorResources.ValueOutOfRangeToken);
                                m_action = SequencingRuleAction.NoAction;
                            }
                        }
                    }
                    else
                    {
                        // there are no <imsss:ruleAction> nodes so return SequencingRuleAction.NoAction
                        m_action = SequencingRuleAction.NoAction;
                    }
                    m_foundAction = true;
                }
                return m_action.Value;
            }
        }
    }

    /// <summary>
    /// A single rule condition associated with sequencing.
    /// </summary>
    /// <remarks>
    /// Corresponds to the &lt;imsss:ruleCondition&gt; node.
    /// </remarks>
    public class SequencingRuleConditionNodeReader
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        private string m_referencedObjective;
        private double? m_measureThreshold;
        private SequencingConditionOperator? m_operator;
        private SequencingRuleCondition? m_condition;
        private ActivityNodeReader m_activity; // activity that owns this rule condition.
        private ManifestReader m_manifest; // manifest that owns this rule condition.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ruleConditionNode">The &lt;imsss:ruleCondition&gt; node to parse.</param>
        /// <param name="manifest">The <Typ>ManifestReader</Typ> that contains this &lt;sequencing&lt; node.  Can be null.</param>
        /// <param name="activity">The <Typ>ActivityNodeReader</Typ> that contains this &lt;sequencing&lt; node.</param>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="validatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Where errors and warnings are logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rollupRuleNode"/> is null.</exception>
        /// <exception cref="InvalidPackageException"><paramref name="rollupRuleNode"/> does not point to a &lt;imsss:rollupRule2&gt; node.</exception>
        internal SequencingRuleConditionNodeReader(XPathNavigator ruleConditionNode, ManifestReader manifest, ActivityNodeReader activity,
            ManifestReaderSettings manifestSettings, PackageValidatorSettings validatorSettings,
            bool logReplacement, ValidationResults log)
        {
            m_activity = activity;
            m_manifest = manifest;
            m_helper = Helper.InternalCreateImsss(ruleConditionNode, manifestSettings, validatorSettings, logReplacement, log, Helper.Strings.RuleCondition);
        }

        /// <summary>
        /// Represents the &lt;imsss:ruleCondition&gt;/referencedObjective attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>String.Empty</c>, which is returned when the attribute is omitted, or
        /// when the attribute value is invalid or
        /// when the value is not an objective identifier in the surrounding activity.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set and the
        /// attribute value is invalid, such as a string containing only whitespace, or is not an objective
        /// identifier in the surrounding activity.</exception>
        public string ReferencedObjectiveId
        {
            get
            {
                if (null == m_referencedObjective)
                {
                    string referencedObjective = m_helper.Node.GetAttribute(Helper.Strings.ReferencedObjective, String.Empty);
                    // check that the attribute isn't only whitespace, except String.Empty which is the default value.
                    if (!String.IsNullOrEmpty(referencedObjective))
                    {
                        referencedObjective = referencedObjective.Trim();
                        if (referencedObjective.Length == 0)
                        {
                            m_helper.LogEmptyReferencedObjective(m_helper.Node);
                            referencedObjective = String.Empty;
                        }
                        // if there is an owner-activity, make sure this id is an objective identifier therein
                        else if (m_activity != null)
                        {
                            bool idIsValid = false;
                            foreach(SequencingObjectiveNodeReader objective in m_activity.Sequencing.Objectives.Values)
                            {
                                if (0 == String.Compare(referencedObjective, objective.Id, false, CultureInfo.CurrentCulture))
                                {
                                    idIsValid = true;
                                    break;
                                }
                            }
                            if (!idIsValid)
                            {
                                m_helper.LogBadAttribute(m_helper.Node.GetAttribute(Helper.Strings.ReferencedObjective, String.Empty),
                                    Helper.Strings.ReferencedObjective, m_helper.Node.Name, ValidatorResources.EmptyString,
                                    ValidatorResources.ReferenceNotFound);
                                referencedObjective = String.Empty;
                            }
                        }
                        // if there is an owner-manifest, make sure this id is an objective identifier therein
                        else if (m_manifest != null)
                        {
                            bool idIsValid = false;
                            foreach(SequencingNodeReader sequencing in m_manifest.SequencingCollection.Values)
                            {
                                foreach (SequencingObjectiveNodeReader objective in sequencing.Objectives.Values)
                                {
                                    if (0 == String.Compare(referencedObjective, objective.Id, false, CultureInfo.CurrentCulture))
                                    {
                                        idIsValid = true;
                                        break;
                                    }
                                }
                                if (idIsValid) break;
                            }
                            if (!idIsValid)
                            {
                                m_helper.LogBadAttribute(m_helper.Node.GetAttribute(Helper.Strings.ReferencedObjective, String.Empty),
                                    Helper.Strings.ReferencedObjective, m_helper.Node.Name, ValidatorResources.EmptyString,
                                    ValidatorResources.ReferenceNotFound);
                                referencedObjective = String.Empty;
                            }
                        }
                    }
                    m_referencedObjective = referencedObjective;
                }
                return m_referencedObjective;
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:ruleCondition&gt;/measureThreshold attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>0</c>, which is returned when the attribute is omitted, or
        /// when the attribute value is invalid and the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid, such as a non-decimal value outside the range of [-1 to 1].</exception>
        public double MeasureThreshold
        {
            get
            {
                if (m_measureThreshold == null)
                {
                    double? measureThreshold = null;
                    m_helper.CacheAttribute<double>(m_helper.Node, ref measureThreshold, Helper.Strings.MeasureThreshold, 0);
                    if (measureThreshold < -1 || measureThreshold > 1)
                    {
                        m_helper.LogBadAttribute(measureThreshold.ToString(), Helper.Strings.MeasureThreshold, m_helper.Node.Name, "0",
                            ValidatorResources.OutOfRangeNegOneToOne);
                        measureThreshold = 0;
                    }
                    m_measureThreshold = measureThreshold;
                }
                return m_measureThreshold.Value;
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:ruleCondition&gt;/operator attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>SequencingConditionOperator.NoOp</c>, which is returned when the attribute is omitted, or
        /// when the attribute value is invalid and the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid, e.g. when it is not one of the valid tokens: noOp, not.</exception>
        public SequencingConditionOperator Operator
        {
            get
            {
                return m_helper.CacheTokenAttribute<SequencingConditionOperator>(m_helper.Node, ref m_operator, Helper.Strings.Operator,
                    SequencingConditionOperator.NoOp);
            }
        }

        /// <summary>
        /// Represents the &lt;imsss:ruleCondition&gt;/condition attribute.
        /// </summary>
        /// <remarks>
        /// Default value is <c>SequencingRuleCondition.Always</c>, which is returned when the attribute is omitted, or
        /// when the attribute value is invalid and the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid, e.g. when it is not one of the valid tokens: satisfied, objectiveStatusKnown,
        /// objectiveMeasureKnown, objectiveMeasureGreaterThan, objectiveMeasureLessThan, completed, activityProgressKnown,
        /// attempted, attemptLimitExceeded, timeLimitExceeded, outsideAvailableTimeRange, always.</exception>
        public SequencingRuleCondition Condition   // e.g., satisfied, objectiveStatusKnown, etc.
        {
            get
            {
                return m_helper.CacheTokenAttribute<SequencingRuleCondition>(m_helper.Node, ref m_condition, Helper.Strings.Condition,
                    SequencingRuleCondition.Always);
            }
        }
    }

    internal class ReadOnlyMlcDictionary<TValue> : IDictionary<string, TValue> where TValue:class
    {
        private Dictionary<string, TValue> m_internalDictionary;

        /// <summary>
        /// Constructor to create empty dictionary.
        /// </summary>
        internal ReadOnlyMlcDictionary()
        {
            m_internalDictionary = new Dictionary<string, TValue>();
        }

        /// <summary>
        /// Read only Mlc dictionary.
        /// </summary>
        /// <param name="helper">Helper object.</param>
        /// <param name="collectionNode">The node that will be searched for items in this dictionary 
        /// collection.</param>
        /// <param name="childValueXPath">The xpath expression, relative to <paramref name="collectionNode"/> 
        /// of values in the dictionary.</param>
        /// <param name="childKeyXPath">The xpath expression, relative to <paramref name="childValueXPath"/>that 
        /// will return the key for the associated value in the collection.</param>
        /// <param name="createItemDelegate">The delegate which will be result of the xpath query for 
        /// an item in the collection and will create the actual instance of the value of the collection. </param>
        /// <param name="keyMissingDelegate">What to call if the node doesn't have the requested key.</param>
        /// <remarks>
        /// The dictionary gets built in the constructor.
        /// <para>
        /// For instance:
        /// collectionNode is the manifest node.
        /// childValueXPath = ".//resource"
        /// childKeyXpath = "@id"
        /// Then, 
        /// ContainsKey does xpath query: childValueXPath + "[" + childKeyXPath + "=\"{0}\"]" where {0} is parameter
        ///     to ContainsKey.
        /// Keys does xpath query: childValueXPath + "/" childKeyXPath
        /// ETC...
        /// </para>
        /// </remarks>
        internal ReadOnlyMlcDictionary(Helper helper, XPathNavigator collectionNode, XPathExpression childValueXPath, string childKeyXPath,
                            CreateItemFromXPathNavigatorDelegate<TValue> createItemDelegate,
                            KeyMissingDelegate keyMissingDelegate)
        {
            m_internalDictionary = new Dictionary<string, TValue>();
            XPathNodeIterator iter = collectionNode.Select(childValueXPath);
            TValue value;
            string key;
            foreach (XPathNavigator itemNode in iter)
            {
                key = null;
                XPathNavigator keyNav = itemNode.SelectSingleNode(childKeyXPath);
                if (keyNav == null)
                {
                    if (keyMissingDelegate != null) key = keyMissingDelegate(itemNode);
                }
                else
                {
                    key = keyNav.Value;
                }
                if(key != null)
                {
                    value = createItemDelegate(itemNode);
                    if (value != null)
                    {
                        // take care if the key is already in the dictionary
                        if (m_internalDictionary.ContainsKey(key))
                        {
                            helper.LogInvalidDuplicateNode(itemNode.Name, false, String.Format(CultureInfo.CurrentCulture, ValidatorResources.IdenticalKeyValues,
                                keyNav.Name, keyNav.Value));
                        }
                        else
                        {
                            m_internalDictionary.Add(key, value);
                        }
                    }
                }
            }
        }

        #region IDictionary<string,TValue> Members

        public void Add(string key, TValue value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(string key)
        {
            return m_internalDictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get
            {
                return m_internalDictionary.Keys;
            }
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out TValue value)
        {
            return m_internalDictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get
            {
                return m_internalDictionary.Values;
            }
        }

        public TValue this[string key]
        {
            get
            {
                return m_internalDictionary[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,TValue>> Members

        public void Add(KeyValuePair<string, TValue> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            if (m_internalDictionary.ContainsKey(item.Key))
            {
                if(m_internalDictionary[item.Key] == item.Value)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (arrayIndex + m_internalDictionary.Count >= array.Length)
            {
                throw new ArgumentException();
            }
            int i = 0;
            foreach(string key in m_internalDictionary.Keys)
            {
                array[i++] = new KeyValuePair<string,TValue>(key, m_internalDictionary[key]);
            }
        }

        public int Count
        {
            get
            {
                return m_internalDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,TValue>> Members

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, TValue>> Enumerate()
        {
            foreach(string itemKey in m_internalDictionary.Keys)
            {
                yield return new KeyValuePair<string, TValue>(itemKey, m_internalDictionary[itemKey]);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
        #endregion
    }

    internal delegate T CreateItemFromXPathNavigatorDelegate<T>(XPathNavigator node);
    internal delegate string KeyMissingDelegate(XPathNavigator node);

    internal class ReadOnlyMlcCollection<T> : ICollection<T>, IList<T> where T:class
    {
        XPathNavigator m_collectionNode;
        XPathExpression m_childXPath; // passed into constructor
        CreateItemFromXPathNavigatorDelegate<T> m_createItemDelegate;

        /// <summary>
        /// Constructor to create an empty collection.
        /// </summary>
        internal ReadOnlyMlcCollection()
        {
            m_count = 0;
        }

        internal ReadOnlyMlcCollection(XPathNavigator collectionNode, XPathExpression childXPath, CreateItemFromXPathNavigatorDelegate<T> createItemDelegate)
        {
            m_collectionNode = collectionNode;
            m_childXPath = childXPath;
            m_createItemDelegate = createItemDelegate;
        }

        #region ICollection<T> Members

        private int? m_count;
        [SuppressMessage("Microsoft.Performance", "CA1804")] // need to iterate using collectionItem even though it's never used
        public int Count
        {
            get
            {
                // we can't just return a count of all the nodes, because the delegate method can return null if
                // a node is invalid, whereas an iterator returns all nodes irregardless of their validity.
                // E.g. if there are two <mapInfo> nodes under an <objective> node, and both have their
                // readSatisfiedStatus attributes set to true, only the first is valid.
                if (null == m_count)
                {
                    int count = 0;
                    IEnumerable<T> collectionItems = Enumerate();
                    foreach (T collectionItem in collectionItems)
                    {
                        count++;
                    }
                    m_count = count;
                }
                return m_count.Value;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            IEnumerable<T> collectionItems = Enumerate();
            foreach (T collectionItem in collectionItems)
            {
                if (item == collectionItem)
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if (arrayIndex + Count >= array.Length)
            {
                throw new ArgumentException();
            }
            int count = 0;
            IEnumerable<T> collectionItems = Enumerate();
            foreach (T collectionItem in collectionItems)
            {
                array[count++] = collectionItem;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        private Collection<T> m_internalCollection;
        private IEnumerable<T> Enumerate()
        {
            if (m_internalCollection == null)
            {
                Collection<T> internalCollection = new Collection<T>();
                if (m_collectionNode != null)
                {
                    XPathNodeIterator iter = m_collectionNode.Select(m_childXPath);
                    foreach (XPathNavigator itemNode in iter)
                    {
                        internalCollection.Add(m_createItemDelegate(itemNode));
                    }
                }
                m_internalCollection = internalCollection;
            }
            foreach (T item in m_internalCollection)
            {
                if(item != null) yield return item;
            }
        }

        //protected abstract T CreateItem(XPathNavigator itemNode);

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            int i = 0;
            IEnumerable<T> collectionItems = Enumerate();
            foreach (T collectionItem in collectionItems)
            {
                if (collectionItem == item)
                {
                    return i;
                }
                i++;
            }
            return -1; // didn't find it
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    int count = 0;
                    IEnumerable<T> collectionItems = Enumerate();
                    foreach (T collectionItem in collectionItems)
                    {
                        if (count++ == index)
                        {
                            return collectionItem;
                        }
                    }
                }
                throw new ArgumentOutOfRangeException("index");
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }

    /// <summary>
    /// OrganizationNodeReader provides read-only access to an organization in a package.
    /// </summary>
    public class OrganizationNodeReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        private ManifestReader m_manifestReader;
        // private members to hold attribute values, lists, etc.
        private string m_identifier;
        private string m_title;
        private string m_structure;
        private string m_description;
        private string m_instructions;
        private Helper.ChildNode m_metadata = new Helper.ChildNode();
        private bool? m_objectivesGlobalToSystem;
        private ReadOnlyCollection<ActivityNodeReader> m_activities;
        private Helper.ChildNode m_sequencingNode = new Helper.ChildNode();
        private SequencingNodeReader m_sequencing;
        private bool m_gotPointsPossible;
        private float? m_pointsPossible;
        private PackageReader m_packageReader;

        /// <summary>
        /// Provides information about an organization.
        /// </summary>
        /// <param name="packageReader"></param>
        /// <param name="organizationNode">Points to this organization.
        /// </param>
        /// <param name="manifestReader">The <Typ>ManifestReader</Typ> that contains this organization.</param>
        internal OrganizationNodeReader(PackageReader packageReader, XPathNavigator organizationNode, ManifestReader manifestReader)
        {
            if (manifestReader == null || packageReader == null) throw new LearningComponentsInternalException("MR020");
            m_packageReader = packageReader;
            m_helper = Helper.InternalCreateImscp(organizationNode, manifestReader.ManifestSettings, manifestReader.ValidatorSettings,
                manifestReader.LogReplacement, manifestReader.Log, Helper.Strings.Organization);
            m_manifestReader = manifestReader;
        }

        /// <summary>
        /// Id unique within the package for the organization.
        /// </summary>
        /// <remarks>
        /// Returns <c>String.Empty</c> if the identifier attribute is missing and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set, 
        /// and issues a warning to the log.
        /// </remarks>
        /// <exception cref="InvalidPackageException">Identifier is missing and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.</exception>
        public string Id
        {
            get
            {
                return m_helper.CacheRequiredStringIdentifierAttribute(m_helper.Node, ref m_identifier, Helper.Strings.Identifier, String.Empty, 
                    0, BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength);
            }
        }

        /// <summary>
        /// Title (description) of the organization.
        /// </summary>
        /// <remarks>
        /// Returns <c>String.Empty</c> if the &lt;title&gt; node is missing and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set, 
        /// and issues a warning to the log.
        /// </remarks>
        /// <exception cref="InvalidPackageException">&lt;title&gt; node is missing and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.</exception>
        public string Title
        {
            get
            {
                if (m_title == null)
                {
                    XPathNavigator titleNode = 
                        m_helper.GetFirstChild(m_helper.Node, Helper.Strings.Title, m_helper.ImscpCurrentNamespace);
                    if (titleNode == null)
                    {
                        // Note that the Id property referenced here can also cause an error.
                        m_helper.LogRequiredNodeMissing(Helper.Strings.Title, String.Format(CultureInfo.CurrentCulture, ValidatorResources.OrganiztionTitleMissing, Id));
                        m_title = String.Empty;
                    }
                    else
                    {
                        m_helper.CacheStringElement(ref m_title, titleNode.Name, titleNode.Value, Helper.SPM.Title,
                            BaseSchemaInternal.ActivityPackageItem.MaxTitleLength);
                    }
                }
                return m_title;
            }
        }

        /// <summary>
        /// Metadata applied to the organization.
        /// </summary>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        private XPathNavigator CreateMetadataNavigator()
        {
            // the following throws in strict mode if there are multiple <metadata>
            return m_helper.CacheFirstChild(ref m_metadata, m_helper.Node, Helper.Strings.Metadata, m_helper.ImscpCurrentNamespace);
        }

        /// <summary>
        /// Return a reader of the organization level metadata.
        /// </summary>
        /// <exception cref="InvalidPackageException">If there are multiple &lt;metadata&gt; nodes.</exception>
        public MetadataNodeReader Metadata
        {
            get
            {
                return MetadataNodeReader.Create(m_helper, m_packageReader, CreateMetadataNavigator(), m_manifestReader.XmlBase);
            }
        }

        /// <summary>
        /// Helper method to get mlc metadata data from the organization/metadata node.
        /// </summary>
        /// <param name="nodeName">Name of the node under the metadata node that contains the data.</param>
        /// <param name="cache">Where to store the retrieved metadata if null.  If not null, this method just returns
        /// the cached value.</param>
        /// <param name="defaultValue">Default to use if requested data doesn't exist, or metadata node doesn't exist.</param>
        /// <param name="maxLength">Maximum length of the returned string.</param>
        /// <returns>The value obtained from the metadata.</returns>
        private string CacheMlcMetadata(string nodeName, ref string cache, string defaultValue, int maxLength)
        {
            if (cache == null)
            {
                cache = defaultValue;
                XPathNavigator metaData = CreateMetadataNavigator();
                if (metaData != null)
                {
                    XPathNavigator nav = m_helper.GetFirstChild(metaData, nodeName, Helper.Strings.MlcNamespace, true);
                    if (nav != null)
                    {
                        m_helper.CacheStringElement(ref cache, nodeName, nav.Value, 0, maxLength);
                    }
                }
            }
            return cache;
        }

        /// <summary>
        /// Description of the organization.
        /// </summary>
        /// <remarks>
        /// Returns <c>String.Empty</c> if it does not exist in the package.  Max length is 1024; uses 
        /// <c><Typ>ManifestReaderSettings</Typ>.FixMlcRequirementViolation</c> and <c>PackageValidatorSettings.MlcRequirementValidation</c> to
        /// determine what to do in case of error.
        /// </remarks>
        public string Description
        {
            get
            {
                return CacheMlcMetadata(ManifestConverter.Strings.description, ref m_description, String.Empty, 1024);
            }
        }

        /// <summary>
        /// Instructions to the learner executing the organization.
        /// </summary>
        /// <remarks>
        /// Returns <c>String.Empty</c> if it does not exist in the package.  Max length is 4096; uses
        /// <c><Typ>ManifestReaderSettings</Typ>.FixMlcRequirementViolation</c> and <c>PackageValidatorSettings.MlcRequirementValidation</c> to
        /// determine what to do in case of error.
        /// </remarks>
        public string Instructions
        {
            get
            {
                return CacheMlcMetadata(ManifestConverter.Strings.instructions, ref m_instructions, String.Empty, 4096);
            }
        }

        /// <summary>
        /// The nominal maximum number of points possible for the organization.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if it does not exist in the package or as the default.  Must be blank or a valid <c>float</c> value; uses
        /// <c><Typ>ManifestReaderSettings</Typ>.FixMlcRequirementViolation</c> and <c>PackageValidatorSettings.MlcRequirementValidation</c> to
        /// determine what to do in case of error (e.g. return default value or throw <Typ>InvalidPackageException</Typ>.
        /// <para>
        /// The value of PointsPossible is contained in the &lt;organization&gt; node's &lt;metadata&gt;.
        /// </para>
        /// <para>
        /// The PointsPossible value originates from LRM content.  When <Typ>ManifestConverter</Typ> is used to import LRM content, it
        /// creates an &lt;mlc:pointsPossible&gt; node under the organization's &lt;metadata&gt;.
        /// Note that this property must be a valid <c>float</c>.  However, <Typ>ManifestConverter</Typ>
        /// enforces a value from 0 - 10000 when converting from an <c>index.xml</c>.
        /// </para>
        /// </remarks>
        public float? PointsPossible
        {
            get
            {
                // default value of m_pointsPossible is null
                if (!m_gotPointsPossible)
                {
                    string str = null;
                    CacheMlcMetadata(ManifestConverter.Strings.pointsPossible, ref str, String.Empty, 0);
                    if (!String.IsNullOrEmpty(str))
                    {
                        try
                        {
                            double d = XmlConvert.ToDouble(str);
                            if (d < float.MinValue || d > float.MaxValue)
                            {
                                m_helper.LogInvalidNodeValue(str, ManifestConverter.Strings.pointsPossible, ValidatorResources.EmptyString,
                                    ValidatorResources.ValueOutOfRange);
                            }
                            else
                            {
                                m_pointsPossible = (float)d;
                            }
                        }
                        catch (OverflowException)
                        {
                            m_helper.LogInvalidNodeValue(str, ManifestConverter.Strings.pointsPossible, ValidatorResources.EmptyString,
                                ValidatorResources.ValueOutOfRange);
                        }
                        catch (FormatException)
                        {
                            m_helper.LogInvalidNodeValue(str, ManifestConverter.Strings.pointsPossible, ValidatorResources.EmptyString,
                                ValidatorResources.ValueIncorrectType);
                        }
                    }
                    m_gotPointsPossible = true;
                }
                return m_pointsPossible;
            }
        }

        /// <summary>
        /// The shape of the organization.  
        /// </summary>
        /// <remarks>Returns "hierarchical" (without the quotes) if there is no structure attribute.</remarks>
        public string Structure
        {
            get
            {
                return m_helper.CacheStringAttribute(m_helper.Node, ref m_structure, Helper.Strings.Structure, "hierarchical", Helper.SPM.Structure, 0);
            }
        }

        /// <summary>
        /// If true, the objective information (for instance, the learner's scores) should 
        /// update the learner's lifetime score for the objective.
        /// </summary>
        /// <remarks>
        /// Default value is true.  If the attribute is invalid (non-bool) and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set,
        /// return the default value and issue a warning to the log.
        /// </remarks>
        /// <exception cref="InvalidPackageException">Attribute is invalid and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.</exception>
        public bool ObjectivesGlobalToSystem
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_objectivesGlobalToSystem, Helper.Strings.ObjectivesGlobalToSystem, true,
                    Helper.Strings.AdlseqNamespace).Value;
            }
        }


        /// <summary>
        /// List of activities within the organization. This list returns all the top-level
        /// activities in the organization. Each of those Activity objects includes its child activities.
        /// </summary>
        /// <remarks>List all list elements, when this property is accessed, the list is populated.
        /// <para>
        /// The activity (e.g. &lt;item&gt;) nodes are in this collection in the same order that they are in the manifest file.
        /// </para>
        /// <para>
        /// &lt;item&gt; nodes that do not have a valid identifier attribute are not included in this list.
        /// </para>
        /// <para>
        /// Although SCORM mandates that no two item nodes may have the same identifier in a manifest,
        /// there is no check for that when building this collection.  It is up to the application to handle
        /// if multiple items have identical identifier attributes.
        /// </para>
        /// </remarks>
		/// <example>
        /// Iterate through all top-level activities in the manifest.
        /// <code language="C#">
		///     foreach (OrganizationNodeReader organization in manifest.Organizations)
		///     {
		///         foreach (ActivityNodeReader activity in organization.Activities)
		///         {
		///             // use the ActivityNodeReader
		///         }
		///     }
		/// </code></example>
        public ReadOnlyCollection<ActivityNodeReader> Activities
        {
            get
            {
                if (m_activities == null)
                {
                    XPathExpression expr = m_helper.CreateExpression(m_helper.Node,
                        Helper.Strings.Imscp + ":" + Helper.Strings.Item);
                    ReadOnlyCollection<ActivityNodeReader> activities = new ReadOnlyCollection<ActivityNodeReader>(
                        new ReadOnlyMlcCollection<ActivityNodeReader>(m_helper.Node, expr,
                            delegate(XPathNavigator node)
                            {
                                // if the <item>/identifier is missing or invalid, skip it
                                string identifier = node.GetAttribute(Helper.Strings.Identifier, String.Empty);
                                if (identifier == String.Empty)
                                {
                                    m_helper.LogRequiredAttributeMissing(Helper.Strings.Identifier, node.Name, ValidatorResources.ItemIdentifierMissing);
                                    return null;
                                }
                                else if (Helper.ValidateId(identifier))
                                {
                                    return new ActivityNodeReader(m_packageReader, node, m_manifestReader);
                                }
                                else
                                {
                                    m_helper.LogBadAttribute(identifier, Helper.Strings.Identifier, node.Name, ValidatorResources.ElementRemoved,
                                        ValidatorResources.InvalidIdentifier);
                                    return null;
                                }
                            }));
                    m_activities = activities;
                }
                return m_activities;
            }
        }

        /// <summary>
        /// Returns sequencing information which applies at the organization level.
        /// </summary>
        /// <remarks>
        /// If there is no sequencing information for this organization, the property returns null.
        /// This information is only valid for SCORM 2004 content. It will be null for earlier versions
        /// of SCORM.
        /// <para>
        /// If there are multiple &lt;sequencing&gt; nodes and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set, a warning is issued to the log.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">
        /// There are multiple &lt;sequencing&gt; nodes and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.
        /// </exception>
		/// <example>
        /// Iterate through all the sequencing nodes that are beneath the organization nodes in the manifest.
        /// <code language="C#">
		///     foreach (OrganizationNodeReader organization in manifest.Organizations)
		///     {
		///         SequencingNodeReader sequencing = organization.Sequencing;
		///         // use the SequencingNodeReader
		///     }
		/// </code></example>
        public SequencingNodeReader Sequencing
        {
            get
            {
                if(!m_sequencingNode.m_cached)
                {
                    // SCORM 1.2 doesn't support sequencing, so return null.
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                    {
                        // setting m_cached is enough. m_sequencing is initialized to null.
                        m_sequencingNode.m_cached = true;
                    }
                    else
                    {
                        m_helper.CacheFirstChild(ref m_sequencingNode, m_helper.Node, Helper.Strings.Sequencing, Helper.Strings.ImsssNamespace);
                        if (m_sequencingNode.m_node != null)
                        {
                            m_sequencing = SequencingNodeReader.Create(m_sequencingNode.m_node, m_manifestReader, null);
                        }
                    }
                }
                return m_sequencing;
            }
        }

        /// <summary>
        /// Return the XPathNavigator for this resource.
        /// </summary>
        /// <remarks>
        /// This allows an application to get custom namespaced data (e.g. "foo:bar") from the XML.
        /// </remarks>
        public XPathNavigator CreateNavigator()
        {
            return m_helper.Node.Clone();
        }
    }

    /// <summary>
    /// Activity class corresponds to the item node in the manifest.
    /// </summary>
    /// <remarks>
    /// Corresponds to an &lt;item&gt; node under the &lt;manifest&gt;/&lt;organizations&gt;/&lt;organization&gt; hierarchy.
    /// </remarks>
    public class ActivityNodeReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        private ManifestReader m_manifestReader;
        // private member variables to hold attribute and node values
        private string m_identifier;
        private string m_identifierref;
        private string m_title;
        private bool? m_isVisible;
        private string m_resourceParameters;
        private Helper.ChildNode m_metadata = new Helper.ChildNode();
        private bool m_lmsUiValuesCached;
        private bool m_hideContinueUi;
        private bool m_hidePreviousUi;
        private bool m_hideExitUi;
        private bool m_hideAbandonUi;
        private ResourceNodeReader m_resource; // valid once m_identifierref != null
        private OrganizationNodeReader m_referencedOrganization; // valid once m_identifierref != null
        private Helper.ChildNode m_sequencingNode = new Helper.ChildNode();
        private SequencingNodeReader m_sequencing;
        private ReadOnlyCollection<ActivityNodeReader> m_childActivities;
        private TimeLimitAction? m_timeLimitAction;
        private string m_dataFromLms;
        private bool m_gotCompletionThreshold; // if true, m_completionThreshold is valid
        private double? m_completionThreshold;
        private bool m_gotMaxTimeAllowed; // if true, m_maxTimeAllowed is valid
        private TimeSpan? m_maxTimeAllowed; // SCORM 1.2
        private bool m_gotMasteryScore; // if true, m_masteryScore is valid
        private long? m_masteryScore;
        private bool m_gotPrerequisites; // if true, m_prerequisites is valid
        private string m_prerequisites; // SCORM 1.2
        private PackageReader m_packageReader;

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="packageReader"></param>
        /// <param name="itemNode">The &lt;item&gt; node.</param>
        /// <param name="manifestReader">A reader of the manifest.</param>
        internal ActivityNodeReader(PackageReader packageReader, XPathNavigator itemNode, ManifestReader manifestReader)
        {
            if (manifestReader == null || packageReader == null) throw new LearningComponentsInternalException("MR021");
            m_packageReader = packageReader;
            m_helper = Helper.InternalCreateImscp(itemNode, manifestReader.ManifestSettings, manifestReader.ValidatorSettings, 
                manifestReader.LogReplacement, manifestReader.Log, Helper.Strings.Item);
            m_manifestReader = manifestReader;
        }

        /// <summary>
        /// If the identifierref of this activity points to a submanifest, this returns the first organization
        /// within that submanifest.
        /// </summary>
        private OrganizationNodeReader ReferencedOrganization
        {
            get
            {
                ParseIdentifierref();
                return m_referencedOrganization;
            }
        }

        /// <summary>
        /// An identifier for the activity that is unique within the package.
        /// </summary>
        /// <remarks>
        /// This element is required. If it does not exist or is invalid in the package, this property will 
        /// throw an InvalidPackageException if the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>.
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, it will return <c>String.Empty</c>.
        /// <para>
        /// Corresponds to the &lt;item&gt;/identifier attribute.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// and the attribute is missing or invalid.</exception>
        public string Id
        {
            get
            {
                // if this item references a submanifest, return the id of the organization.
                if (ReferencedOrganization != null)
                {
                    m_identifier = ReferencedOrganization.Id;
                }
                else
                {
                    m_helper.CacheRequiredStringIdentifierAttribute(m_helper.Node, ref m_identifier, Helper.Strings.Identifier, String.Empty,
                        0, BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength);
                }
                return m_identifier;
            }
        }

        /// <summary>
        /// Title (description) of the activity.  Represents the &lt;title&gt; node under the &lt;item&gt;.
        /// </summary>
        /// <remarks>This node is required for each &lt;item&gt;.  If it does not exist in the package, this property will 
        /// return <c>String.Empty</c> in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, and throw an <c>InvalidPackageException</c>
        /// in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The &lt;title&gt; node does not exist
        /// and <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set.</exception>
        public string Title
        {
            get
            {
                if (m_title == null)
                {
                    // if this item references a submanifest, return the title of the organization.
                    if (ReferencedOrganization != null)
                    {
                        m_title = ReferencedOrganization.Title;
                    }
                    else
                    {
                        XPathNavigator title = m_helper.GetFirstChild(m_helper.Node, Helper.Strings.Title, m_helper.ImscpCurrentNamespace);
                        if (title == null)
                        {
                            m_helper.LogRequiredNodeMissing(Helper.Strings.Title,
                                String.Format(CultureInfo.CurrentCulture, ValidatorResources.ItemTitleMissing,
                                Helper.GetNodeText(m_helper.Node)));
                            m_title = String.Empty;
                        }
                        else
                        {
                            m_helper.CacheStringElement(ref m_title, title.Name, title.Value, Helper.SPM.Title,
                                BaseSchemaInternal.ActivityPackageItem.MaxTitleLength);
                        }
                    }
                }
                return m_title;
            }
        }

        // This method gets called from the Resource property and from ReferencedOrganization.  It sets m_identifierref,
        // m_resource, and m_referencedOrganization.
        private void ParseIdentifierref()
        {
            // m_resource is valid after m_identifierref != null
            if (m_identifierref == null)
            {
                string identifierref = null;
                m_helper.CacheOptionalStringIdentifierAttribute(m_helper.Node, ref identifierref, Helper.Strings.IdentifierRef, String.Empty,
                    Helper.SPM.IdentfierRef, 0);
                if (!String.IsNullOrEmpty(identifierref))
                {
                    // if there are child nodes, it is an error condition in SCORM 2004, or if there is no corresponding
                    // manifest or resource to match the identifierref
                    XPathNodeIterator childActivities = m_helper.Node.SelectChildren(Helper.Strings.Item, m_helper.ImscpCurrentNamespace);
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p3 && childActivities.Count > 0)
                    {
                        m_helper.LogInvalidNode(m_helper.Node.Name, 
                            String.Format(CultureInfo.CurrentCulture, ValidatorResources.ResourceNotAllowedOnActivity,
                            Helper.GetNodeText(m_helper.Node)));
                    }
                    else if (m_manifestReader.Resources.ContainsKey(identifierref))
                    {
                        m_resource = m_manifestReader.Resources[identifierref];
                        // according to CAM, when an item references a resource it must adhere to certain rules.
                        // Validate these rules.  The following will throw InvalidPackageException or place log
                        // entries if needed.
                        m_resource.ValidateActivityResource();
                    }
                    else
                    {
                        // see if there is a submanifest that matches
                        XPathNodeIterator submanifests = m_manifestReader.DecendantSubmanifests;
                        while (submanifests.MoveNext())
                        {
                            if (String.CompareOrdinal(submanifests.Current.GetAttribute(Helper.Strings.Identifier, String.Empty),
                                identifierref) == 0)
                            {
                                ManifestReader manifest = new ManifestReader(m_packageReader, m_helper.ReaderSettings,
                                    m_helper.ValidatorSettings, m_helper.LogReplacement, m_helper.Log, submanifests.Current);
                                m_referencedOrganization = manifest.DefaultOrganization;
                                break;
                            }
                        }
                        // if the identifierref attribute exists but there is no corresponding resource or submanifest in the manifest,
                        // it is an error condition.
                        if (m_referencedOrganization == null)
                        {
                            m_helper.LogBadAttribute(identifierref, Helper.Strings.IdentifierRef, m_helper.Node.Name, ValidatorResources.ElementRemoved,
                                ValidatorResources.ResourceOrOrganizationNotFound);
                        }
                    }
                }
                m_identifierref = identifierref;
            }
        }

        /// <summary>
        /// Primary resource for the activity. For activities that do not have a resource, this 
        /// returns null.
        /// </summary>
        /// <remarks>
        /// If there is no resource associated with this activity, this property returns null. 
        /// 
        /// In SCORM 2004 packages, if the activity has a resource and also has child &lt;item&gt; nodes, this
        /// property will throw an InvalidPackageException in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>.
        /// <para>
        /// If parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> and the activity has an identifierref attribute,
        /// but no matching resource can be found, a warning is issued to the log and null is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and
        /// the identifierref attribute is not a valid id value, there are child &lt;item&gt; nodes,
        /// or there is no corresponding resource with a matching identifier.</exception>
		/// <example>
        /// Given an <Typ>ActivityNodeReader</Typ>, iterate through the resources for all the child activities.
        /// <code language="C#">
		///             foreach (ActivityNodeReader child in activity.ChildActivities)
		///             {
		///                 ResourceNodeReader resource = child.Resource;
		///                 if (resource != null)
		///                 {
		///                     // Use the ResourceNodeReader
		///                 }
		///             }
		/// </code></example>
        public ResourceNodeReader Resource
        {
            get
            {
                // Call the ParseIdentifierref() method to ensure m_resource is valid.
                ParseIdentifierref();
                return m_resource;
            }
        }

        /// <summary>
        /// Returns true if the activity should be visible.
        /// </summary>
        /// <remarks>
        /// The default value, if absent, is "true".
        /// <para>
        /// If the value of the attribute is non-boolean, and the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>,
        /// a warning is put into the log, if provided, and the default value is returned.  If the parsing mode is
        /// <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, an error is put into the log and an exception is thrown.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, and the attribute
        /// value is non-boolean.</exception>
        public bool IsVisible
        {
            get
            {
                return m_helper.CacheAttribute<bool>(m_helper.Node, ref m_isVisible, Helper.Strings.IsVisible, true);
            }
        }

        /// <summary>
        /// Parameters that should be passed to the resource at run time. If there are no parameters for this 
        /// resource, the property returns <c>String.Empty</c>.
        /// </summary>
        /// <remarks>
        /// The default value, if absent, is <c>String.Empty</c>.
        /// <para>
        /// SCORM dictates that this attribute is valid only for items that reference resources.  However,
        /// in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> this still simply returns <c>String.Empty</c> even when the item
        /// does not reference a resource.  If the application wishes to be that strict, it can check the
        /// <c>Resource</c> property when this returns a non-<c>String.Empty</c> value.  If the <c>Resource</c> property
        /// returns no resources, the application will know that this attribute value is not technically allowed.
        /// </para>
        /// </remarks>
        public string ResourceParameters
        {
            get
            {
                return m_helper.CacheStringAttribute(m_helper.Node, ref m_resourceParameters, Helper.Strings.Parameters, String.Empty,
                    Helper.SPM.Parameters, BaseSchemaInternal.ActivityPackageItem.MaxResourceParametersLength);
            }
        }

        /// <summary>
        /// Metadata applied to the activity. If there is no metadata for this activity, the property returns null.
        /// </summary>
        /// <remarks>
        /// Returns the &lt;metadata&gt; node child of the &lt;item&gt; node, or <c>null</c> if none exists.
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, if there is more than one &lt;metadata&gt; node, a warning is put into the log
        /// and the first node is returned.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        private XPathNavigator CreateMetadataNavigator()
        {
            return m_helper.CacheFirstChild(ref m_metadata, m_helper.Node, Helper.Strings.Metadata, m_helper.ImscpCurrentNamespace);
        }

        /// <summary>
        /// Return a reader of the activity level metadata.
        /// </summary>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        public MetadataNodeReader Metadata
        {
            get
            {
                return MetadataNodeReader.Create(m_helper, m_packageReader, CreateMetadataNavigator(), m_manifestReader.XmlBase);
            }
        }

        /// <summary>
        /// Return the prerequisites for this activity. That is, the conditions 
        /// that must be met in order for this activity to be activated. Returns <c>null</c>
        /// if there are no prerequisites.
        /// </summary>
        /// <remarks>
        /// This is only valid 
        /// for SCORM 1.2 content and will be null in other cases. For SCORM 2004 content, 
        /// use the Sequencing information.
        /// <para>
        /// This corresponds to the &lt;adlcp:prerequisites&gt; element.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>
        /// and the prerequisites script is syntactically invalid.</exception>
        public string Prerequisites
        {
            get
            {
                if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2 && !m_gotPrerequisites)
                {
                    XPathNavigator node = 
                        m_helper.GetFirstChild(m_helper.Node, Helper.Strings.Prerequisites, m_helper.AdlcpCurrentNamespace);
                    if (node != null)
                    {
                        // validate the type node is aicc_script, validate the script is valid
                        string type = null;
                        if (String.Compare(m_helper.CacheStringAttribute(node, ref type, Helper.Strings.Type, String.Empty, 0, 0),
                            "aicc_script", StringComparison.Ordinal) != 0)
                        {
                            m_helper.LogBadAttribute(type, Helper.Strings.Type, node.Name, "aicc_script",
                                ValidatorResources.PrerequisitesTypeWrong);
                        }
                        string script = null;
                        m_helper.CacheStringElement(ref script, node.Name, node.Value, Helper.SPM.Prerequisites, 0);

                        try
                        {
                            // Call Evaluate to verify the script, using a "placebo" delegate. This does not verify
                            // that identifiers in the script are valid identifiers.  That is done inside the
                            // ManifestValidator class.
                            Helper.PrerequisitesParser.Evaluate(script, delegate(string identifier)
                            {
                                if (!m_manifestReader.ActivityIdentifiers.ContainsKey(identifier))
                                {
                                    throw new Helper.PrerequisitesParser.PrerequisitesParserException();
                                }
                                return LessonStatus.Passed;
                            });
                        }
                        catch (Helper.PrerequisitesParser.PrerequisitesParserException)
                        {
                            // There is a problem with the script syntax.
                            m_helper.LogInvalidNodeValue(script, node.Name, String.Empty, ValidatorResources.PrerequisitesScriptInvalid);
                        }
                        m_prerequisites = script;
                    }
                    m_gotPrerequisites = true;
                }
                return m_prerequisites;
            }
        }

        /// <summary>
        /// Sequencing information for this node.
        /// </summary>
        /// <remarks>
        /// If there is no sequencing information for this activity, the property returns null.
        /// This information is only valid for SCORM 2004 content. It will be null for earlier versions
        /// of SCORM.
        /// </remarks>
		/// <example>
        /// Iterate through the sequencing nodes on the top-level activity nodes in the manifest.
        /// <code language="C#">
		///     foreach (OrganizationNodeReader organization in manifest.Organizations)
		///     {
		///         foreach (ActivityNodeReader activity in organization.Activities)
		///         {
		///             SequencingNodeReader sequencing = activity.Sequencing;
		///             if (sequencing != null)
		///             {
		///                 // use the SequencingNodeReader
		///             }
		///         }
		///     }
		/// </code></example>
        public SequencingNodeReader Sequencing
        {
            get
            {
                // find the <sequencing> child node. m_sequencing is valid after m_sequencingNode is non-null.
                if (!m_sequencingNode.m_cached)
                {
                    // if this item references a submanifest, return the sequencing of that.
                    if (ReferencedOrganization != null)
                    {
                        m_sequencing = ReferencedOrganization.Sequencing;
                    }
                    else
                    {
                        // SCORM 1.2 doesn't support sequencing, so return null.
                        if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                        {
                            // setting m_cached is enough. m_sequencing is initialized to null.
                            m_sequencingNode.m_cached = true;
                        }
                        else
                        {
                            m_helper.CacheFirstChild(ref m_sequencingNode, m_helper.Node, Helper.Strings.Sequencing, Helper.Strings.ImsssNamespace);
                            if (m_sequencingNode.m_node != null)
                            {
                                m_sequencing = SequencingNodeReader.Create(m_sequencingNode.m_node, m_manifestReader, this);
                            }
                        }
                    }
                }
                return m_sequencing;
            }
        }
        
        /// <summary>
        /// Return list of children of this activity.
        /// </summary>
        /// <remarks>
        /// If this is a SCORM 2004 package and there are child activities and a resource for this activity,
        /// this property will throw an <Typ>InvalidPackageException</Typ> if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set, or issue
        /// a warning to the log if <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c> is set.
        /// <para>
        /// Although SCORM mandates that no two item nodes may have the same identifier in a manifest,
        /// there is not check for that when building this collection.  It is up to the application to handle
        /// if multiple items have identical identifier attributes.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException"><c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> is set and there are child activities
        /// and resources on this node.</exception>
		/// <example>
        /// Given an <Typ>ActivityNodeReader</Typ>, iterate through all child activities.
        /// <code language="C#">
		///             foreach (ActivityNodeReader child in activity.ChildActivities)
		///             {
		///                 // use the ActivityNodeReader
		///             }
		/// </code></example>
        public ReadOnlyCollection<ActivityNodeReader> ChildActivities
        {
            get
            {
                if (m_childActivities == null)
                {
                    // if this item references a submanifest, return the activities of that.
                    if (ReferencedOrganization != null)
                    {
                        m_childActivities = ReferencedOrganization.Activities;
                    }
                    else
                    {
                        ReadOnlyCollection<ActivityNodeReader> childActivities;
                        // get a list of child <item> nodes. This doesn't use the normal common code to create a ReadOnlyMlcCollection
                        // because of the extra necessary check.
                        XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imscp + ":" + Helper.Strings.Item);
                        childActivities = new ReadOnlyCollection<ActivityNodeReader>(
                            new ReadOnlyMlcCollection<ActivityNodeReader>(m_helper.Node, expr,
                            delegate(XPathNavigator node)
                            {
                                // identifier attribute is required, or skip this node
                                ActivityNodeReader activity = new ActivityNodeReader(m_packageReader, node, m_manifestReader);
                                // the following property will throw if parsing mode is strict.
                                if (activity.Id == String.Empty) return null;
                                else return activity;
                            }));
                        if (childActivities.GetEnumerator().MoveNext())
                        {
                            // if there are child nodes, it is an error condition in SCORM 2004 if there is a
                            // resource
                            if (m_helper.ManifestVersion == Helper.ScormVersion.v1p3 && Resource != null)
                            {
                                m_helper.LogInvalidNode(m_helper.Node.Name,
                                    String.Format(CultureInfo.CurrentCulture, ValidatorResources.ResourceNotAllowedOnActivity,
                                    Helper.GetNodeText(m_helper.Node)));
                            }
                        }
                        m_childActivities = childActivities;
                    }
                }
                return m_childActivities;
            }
        }

        /// <summary>
        /// Does validation for SCORM 1.2 (that this item references a SCO) and further validation for SCORM 1.3
        /// (that this is a leaf item).
        /// </summary>
        /// <param name="nodeName"></param>
        private void ValidateItemReferencingScoResource(string nodeName)
        {
            // validate this is only on a leaf item node referencing a SCO resource.
            if ((Resource != null && Resource.ResourceType != ResourceType.Sco )
                || (m_manifestReader.PackageFormat == PackageFormat.V1p3 && ChildActivities.GetEnumerator().MoveNext()))
            {
                if (m_manifestReader.PackageFormat == PackageFormat.V1p3)
                {
                    m_helper.LogInvalidNode(nodeName,
                                String.Format(CultureInfo.CurrentCulture, ValidatorResources.ResourceNotAllowedOnActivity,
                                Helper.GetNodeText(m_helper.Node)));
                }
                else
                {
                    m_helper.LogInvalidNode(nodeName,
                                String.Format(CultureInfo.CurrentCulture, ValidatorResources.ItemMustRepresentSco,
                                Helper.GetNodeText(m_helper.Node)));
                }
            }
        }

        /// <summary>
        /// Action to perform when this activity exceeds the timelimit.  Corresponds to the
        /// &lt;adlcp:timeLimitAction&gt; node.
        /// </summary>
        /// <remarks>
        /// The default is "continue,no message".
        /// </remarks>
        public TimeLimitAction TimeLimitAction
        {
            get
            {
                if (m_timeLimitAction == null)
                {
                    // get the adlcp:timeLimitAction node and use its value to set the correct TimeLimitAction
                    XPathNavigator tla = 
                        m_helper.GetFirstChild(m_helper.Node, 
                        m_helper.ManifestVersion == Helper.ScormVersion.v1p2 ? Helper.Strings.TimeLimitActionV1p2 : Helper.Strings.TimeLimitActionV1p3,
                        m_helper.AdlcpCurrentNamespace);
                    if (tla == null)
                    {
                        m_timeLimitAction = TimeLimitAction.ContinueNoMessage;
                    }
                    else
                    {
                        ValidateItemReferencingScoResource(tla.Name);
                        switch (tla.Value)
                        {
                            case "exit,message":
                                m_timeLimitAction = TimeLimitAction.ExitWithMessage;
                                break;
                            case "exit,no message":
                                m_timeLimitAction = TimeLimitAction.ExitNoMessage;
                                break;
                            case "continue,message":
                                m_timeLimitAction = TimeLimitAction.ContinueWithMessage;
                                break;
                            case "continue,no message":
                                m_timeLimitAction = TimeLimitAction.ContinueNoMessage;
                                break;
                            default:
                                m_helper.LogInvalidNodeValue(tla.Value, tla.Name, "continue,no message", ValidatorResources.ValueOutOfRangeToken);
                                m_timeLimitAction = TimeLimitAction.ContinueNoMessage;
                                break;
                        }
                    }
                }
                return m_timeLimitAction.Value;
            }
        }

        /// <summary>
        /// Returns the &lt;adlcp:dataFromLMS&gt; value, or <c>String.Empty</c> if there is none.
        /// </summary>
        public string DataFromLms
        {
            get
            {
                if (m_dataFromLms == null)
                {
                    XPathNavigator nav = 
                        m_helper.GetFirstChild(m_helper.Node, 
                        m_helper.ManifestVersion == Helper.ScormVersion.v1p2 ? Helper.Strings.DataFromLmsV1p2 : Helper.Strings.DataFromLmsV1p3,
                        m_helper.AdlcpCurrentNamespace);
                    if (nav == null)
                    {
                        m_dataFromLms = String.Empty;
                    }
                    else
                    {
                        ValidateItemReferencingScoResource(nav.Name);
                        m_helper.CacheStringElement(ref m_dataFromLms, nav.Name, nav.Value, 
                            m_helper.ManifestVersion == Helper.ScormVersion.v1p2 ? Helper.SPM.DataFromLmsV1p2 : Helper.SPM.DataFromLmsV1p3,
                            BaseSchemaInternal.ActivityPackageItem.MaxLaunchDataLength);
                    }
                }
                return m_dataFromLms;
            }
        }

        /// <summary>
        /// Returns the &lt;adlcp:completionThreshold&gt; value, or null if none.
        /// </summary>
        /// <remarks>
        /// Default value is null.
        /// </remarks>
        /// <remarks>
        /// SCORM 1.2 content always returns null for this value.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the
        /// attribute value is invalid.</exception>
        public double? CompletionThreshold
        {
            get
            {
                if (!m_gotCompletionThreshold)
                {
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p3)
                    {
                        XPathNavigator nav =
                            m_helper.GetFirstChild(m_helper.Node, Helper.Strings.CompletionThreshold, m_helper.AdlcpCurrentNamespace);
                        if (nav != null)
                        {
                            ValidateItemReferencingScoResource(nav.Name);
                            try
                            {
                                m_completionThreshold = nav.ValueAsDouble;
                                // range check: m_completionThreshold must be between 0 and 1.
                                if (m_completionThreshold < 0 || m_completionThreshold > 1)
                                {
                                    m_helper.LogBadAttribute(m_completionThreshold.ToString(), Helper.Strings.CompletionThreshold,
                                        m_helper.Node.Name, "<null>", ValidatorResources.IllegalPercentage);
                                    m_completionThreshold = null;
                                }
                            }
                            catch (FormatException)
                            {
                                m_helper.LogInvalidNodeValue(nav.Value, nav.Name, ValidatorResources.ElementRemoved, ValidatorResources.ValueIncorrectType);
                            }
                        }
                    }
                    m_gotCompletionThreshold = true;
                }
                return m_completionThreshold;
            }
        }

        /// <summary>
        /// Maximum time allowed on this activity. This is only valid for SCORM 1.2 content. 
        /// </summary>
        /// <remarks>
        /// The default value, if absent, <c>null</c>.
        /// </remarks>
        public TimeSpan? MaximumTimeAllowed
        {
            get
            {
                if (!m_gotMaxTimeAllowed)
                {
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                    {
                        Helper.ChildNode node = new Helper.ChildNode();
                        m_helper.CacheFirstChild(ref node, m_helper.Node, Helper.Strings.MaxTimeAllowed, m_helper.AdlcpCurrentNamespace, true);
                        if (node.m_node != null)
                        {
                            // make sure this item represents a SCO
                            ValidateItemReferencingScoResource(node.m_node.Name);
                            try
                            {
                                m_maxTimeAllowed = Utilities.StringToTimeSpanScormV1p2(node.m_node.Value);
                            }
                            catch (FormatException)
                            {
                                m_helper.LogInvalidNodeValue(node.m_node.Value, node.m_node.LocalName, ValidatorResources.ElementRemoved,
                                    ValidatorResources.ValueIncorrectType);
                            }
                        }
                    }
                    m_gotMaxTimeAllowed = true;
                }
                return m_maxTimeAllowed;
            }
        }

        /// <summary>
        /// The passing score for this activity. This is only valid for SCORM 1.2 content.
        /// </summary>
        /// <remarks>
        /// Default value, if missing or out of range, is <c>null</c>.
        /// </remarks>
        public long? MasteryScore
        {
            get
            {
                if (!m_gotMasteryScore)
                {
                    if (m_helper.ManifestVersion == Helper.ScormVersion.v1p2)
                    {

                        Helper.ChildNode node = new Helper.ChildNode();
                        m_helper.CacheFirstChild(ref node, m_helper.Node, Helper.Strings.MasteryScore, m_helper.AdlcpCurrentNamespace, true);
                        if (node.m_node != null)
                        {
                            // make sure this item represents a SCO
                            ValidateItemReferencingScoResource(node.m_node.Name);
                            try
                            {
                                m_masteryScore = XmlConvert.ToInt64(node.m_node.Value);
                                if (m_masteryScore < 0 || m_masteryScore > 100)
                                {
                                    m_helper.LogInvalidNodeValue(node.m_node.Value, node.m_node.LocalName, ValidatorResources.ElementRemoved,
                                        ValidatorResources.ValueOutOfRange);
                                    m_masteryScore = null;
                                }
                            }
                            catch (FormatException)
                            {
                                m_helper.LogInvalidNodeValue(node.m_node.Value, node.m_node.LocalName, ValidatorResources.ElementRemoved,
                                    ValidatorResources.ValueIncorrectType);
                            }
                        }
                    }
                    m_gotMasteryScore = true;
                }
                return m_masteryScore;
            }
        }

        /// <summary>
        /// Looks for and caches all &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt;
        /// values.  If there are invalid tokens or duplicate nodes, they are ignored or exceptions are thrown in accordance with the
        /// parsing mode.
        /// </summary>
        private void CacheLmsUiValues()
        {
            // return if the values are already cached
            if (m_lmsUiValuesCached) return;

            // retrieve the presentation node
            XPathNavigator presentation = 
                m_helper.GetFirstChild(m_helper.Node, Helper.Strings.Presentation, Helper.Strings.AdlnavNamespace);
            if (presentation != null)
            {
                // The presentation node is only allowed on an element that references a leaf that references
                // a resource.
                if (ChildActivities.Count > 0 || Resource == null)
                {
                    m_helper.LogInvalidNode(presentation.Name, String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidPresentationNode));
                }
                // retrieve the navigationInterface node
                XPathNavigator navigationInterface = 
                    m_helper.GetFirstChild(presentation, Helper.Strings.NavigationInterface, Helper.Strings.AdlnavNamespace);
                if (navigationInterface != null)
                {
                    // retrieve all hideLMSUI nodes
                    XPathNodeIterator hideLmsUi = navigationInterface.SelectChildren(Helper.Strings.HideLmsUi, Helper.Strings.AdlnavNamespace);
                    // go through them all and cache the appropriate values
                    while (hideLmsUi.MoveNext())
                    {
                        switch(hideLmsUi.Current.Value)
                        {
                            case "continue":
                                m_hideContinueUi = true;
                                break;
                            case "previous":
                                m_hidePreviousUi = true;
                                break;
                            case "exit":
                                m_hideExitUi = true;
                                break;
                            case "abandon":
                                m_hideAbandonUi = true;
                                break;
                            default:
                                m_helper.LogInvalidNodeValue(hideLmsUi.Current.Value, hideLmsUi.Current.Name, ValidatorResources.ElementRemoved, ValidatorResources.ValueOutOfRangeToken);
                                break;
                        }
                    }
                }
            }
            // mark that this has been called and completed
            m_lmsUiValuesCached = true;
        }

        // The following four properties correspond to the <item>/<adlnav:presentation>/<adlnav:navigationInterface>/<adlnav:hideLMSUI> values.

        /// <summary>
        /// Represents the &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; value
        /// of "continue".  Default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>: If a 
        /// &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; token value is
        /// invalid, it is ignored.  If there are more than one &lt;adnav:presentation&gt; or &lt;adlnav:navigationInterface&gt;
        /// nodes, only the first is read and the others are ignored.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an <Typ>InvalidPackageException</Typ> is thrown in the above cases.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and a token value is invalid, or
        /// there are invalid duplicate nodes.</exception>
        public bool HideContinueUI
        {
            get
            {
                CacheLmsUiValues();
                return m_hideContinueUi;
            }
        }

        /// <summary>
        /// Represents the &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; value
        /// of "previous".  Default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>: If a 
        /// &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; token value is
        /// invalid, it is ignored.  If there are more than one &lt;adnav:presentation&gt; or &lt;adlnav:navigationInterface&gt;
        /// nodes, only the first is read and the others are ignored.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an <Typ>InvalidPackageException</Typ> is thrown in the above cases.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and a token value is invalid, or
        /// there are invalid duplicate nodes.</exception>
        public bool HidePreviousUI
        {
            get
            {
                CacheLmsUiValues();
                return m_hidePreviousUi;
            }
        }

        /// <summary>
        /// Represents the &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; value
        /// of "exit".  Default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>: If a 
        /// &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; token value is
        /// invalid, it is ignored.  If there are more than one &lt;adnav:presentation&gt; or &lt;adlnav:navigationInterface&gt;
        /// nodes, only the first is read and the others are ignored.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an <Typ>InvalidPackageException</Typ> is thrown in the above cases.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and a token value is invalid, or
        /// there are invalid duplicate nodes.</exception>
        public bool HideExitUI
        {
            get
            {
                CacheLmsUiValues();
                return m_hideExitUi;
            }
        }

        /// <summary>
        /// Represents the &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; value
        /// of "abandon".  Default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>: If a 
        /// &lt;item&gt;/&lt;adlnav:presentation&gt;/&lt;adlnav:navigationInterface&gt;/&lt;adlnav:hideLMSUI&gt; token value is
        /// invalid, it is ignored.  If there are more than one &lt;adnav:presentation&gt; or &lt;adlnav:navigationInterface&gt;
        /// nodes, only the first is read and the others are ignored.
        /// <para>
        /// If the parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> an <Typ>InvalidPackageException</Typ> is thrown in the above cases.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and a token value is invalid, or
        /// there are invalid duplicate nodes.</exception>
        public bool HideAbandonUI
        {
            get
            {
                CacheLmsUiValues();
                return m_hideAbandonUi;
            }
        }

        /// <summary>
        /// Return the XPathNavigator for this resource.
        /// </summary>
        /// <remarks>
        /// This allows an application to get custom namespaced data (e.g. "foo:bar") from the XML.
        /// </remarks>
        public XPathNavigator CreateNavigator()
        {
            return m_helper.Node.Clone();
        }
    }

    /// <summary>
    /// ResourceNodeReader provides read-only access to a resource defined in a package manifest.
    /// </summary>
    public class ResourceNodeReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        private ManifestReader m_manifestReader;
        private Uri m_xmlBase;
        // private variables that cache the attribute values
        private Helper.ChildNode m_metadata = new Helper.ChildNode();
        private string m_id;
        private ReadOnlyCollection<FileNodeReader> m_files;
        private ResourceType? m_resourceType;
        private ReadOnlyCollection<ResourceNodeReader> m_dependencies;
        private Uri m_entryPoint;
        private bool m_gotEntryPoint;
        private PackageReader m_packageReader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="packageReader"></param>
        /// <param name="resourceNode">A navigator pointing to a &lt;resource&gt; node.</param>
        /// <param name="manifestReader">A reader of the manifest.</param>
        /// <param name="xmlBase">The xml:base to use when constructing the href location for href values that are relative Uri's.
        /// Set to <c>null</c> if there is no xml:base.</param>
        internal ResourceNodeReader(PackageReader packageReader, XPathNavigator resourceNode, ManifestReader manifestReader, Uri xmlBase)
        {
            if (manifestReader == null || packageReader == null) throw new LearningComponentsInternalException("MR022");
            m_packageReader = packageReader;
            m_helper = Helper.InternalCreateImscp(resourceNode, manifestReader.ManifestSettings, manifestReader.ValidatorSettings, 
                manifestReader.LogReplacement, manifestReader.Log, Helper.Strings.Resource);
            m_manifestReader = manifestReader;
            m_xmlBase = m_helper.CombineXmlBaseAttribute(xmlBase, resourceNode);
        }

        /// <summary>
        /// Called from ActivityNodeReader when this resource belongs to an activity, this method validates against the CAM
        /// rules that a resource owned by an activity:
        /// 1. Has an href.
        /// 2. Has an adlcp:scormType of "sco".
        /// 3. Has a type of "webcontent".
        /// </summary>
        /// <remarks>
        /// Note that if the resource type is not "Sco" or "Asset" the validation will always pass.
        /// </remarks>
        internal void ValidateActivityResource()
        {
            if (ResourceType == ResourceType.Sco || ResourceType == ResourceType.Asset)
            {
                string type = null;
                m_helper.CacheStringAttribute(m_helper.Node, ref type, Helper.Strings.Type, String.Empty, 0, 0);
                if (m_helper.ValidatorSettings.ScormRequirementValidation != ValidationBehavior.None &&
                    (EntryPoint == null || (ResourceType != ResourceType.Sco && ResourceType != ResourceType.Asset) ||
                    (0 != String.Compare(type, Helper.Strings.Webcontent, StringComparison.Ordinal))))
                {
                    m_helper.LogInvalidNode(Helper.Strings.Resource, String.Format(CultureInfo.CurrentCulture,
                        ValidatorResources.ActivityResourceBadAttribute, Helper.GetNodeText(m_helper.Node)));
                }
            }
        }


        /// <summary>
        /// Represents the &lt;resource&gt;/identifer attribute.
        /// </summary>
        /// <remarks>
        /// If parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, and this attribute is missing, returns String.Empty and issues
        /// a warning to the log.
        /// </remarks>
        /// <exception cref="InvalidPackageException">If parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and this attribute is missing.</exception>
        public string Id
        {
            get
            {
                return m_helper.CacheRequiredStringIdentifierAttribute(m_helper.Node, ref m_id, Helper.Strings.Identifier, String.Empty, 0, 0);
            }
        }

        /// <summary>
        /// Represents the type of resource.
        /// </summary>
        /// <remarks>
        /// If there is an mlc:xrloType="lrm" attribute, this returns <c>ResourceType.Lrm</c>.  Otherwise,
        /// it returns either <c>ResourceType.Sco</c> or <c>ResourceType.Asset</c> depending on the
        /// adlcp:scormType attribute value.
        /// </remarks>
        public ResourceType ResourceType
        {
            get
            {
                if (m_resourceType == null)
                {
                    // If there is an mlc:xrloType="lrm" this is an LRM resource type.
                    m_helper.CacheTokenAttribute<ResourceType>(m_helper.Node, ref m_resourceType, Helper.Strings.XrloType,
                        Helper.Strings.MlcNamespace, ResourceType.None);
                    // Otherwise, use the standard SCORM types.
                    if (m_resourceType == ResourceType.None)
                    {
                        m_resourceType = null; // reset m_resourceType
                        // SCORM mandates an SPM for this value, but since we only allow "Sco" and "Asset", the SPM of 1000 is irrelevant.
                        m_helper.CacheTokenAttribute<ResourceType>(m_helper.Node, ref m_resourceType, 
                            m_helper.ManifestVersion == Helper.ScormVersion.v1p2 ? Helper.Strings.ScormTypeV1p2 : Helper.Strings.ScormTypeV1p3,
                            m_helper.AdlcpCurrentNamespace, ResourceType.Sco);
                    }
                }
                return m_resourceType.Value;
            }
        }

        /// <summary>
        /// The file that should be launched when this resource is activated. 
        /// </summary>
        /// <remarks>
        /// Represents the href attribute of the resource.  For SCORM 2004 content, if there is no href attribute, returns null.
        /// For SCORM 1.2 content, logs an error if there is no href attribute.
        /// </remarks>
        public Uri EntryPoint
        {
            get
            {
                if (!m_gotEntryPoint)
                {
                    string href = null;
                    m_helper.CacheStringAttribute(m_helper.Node, ref href, Helper.Strings.Href, null,
                        Helper.SPM.Href, BaseSchemaInternal.ActivityPackageItem.MaxPrimaryResourceFromManifestLength);
             
                    if (href == null && m_helper.ManifestVersion.Equals(Helper.ScormVersion.v1p2))
                    {
                        m_helper.LogRequiredAttributeMissing("href", "resource", ValidatorResources.ResourceHrefMissing);
                    }

                    
                    if (!String.IsNullOrEmpty(href))
                    {
                        try
                        {
                            m_entryPoint = Helper.CombinePaths(m_xmlBase, href);
                        }
                        catch(LearningComponentsInternalException)
                        {
                            m_helper.LogBadAttribute(href, Helper.Strings.Href, m_helper.Node.Name, ValidatorResources.EmptyString,
                                ValidatorResources.BadUri);
                        }
                    }
                    m_gotEntryPoint = true;
                }
                return m_entryPoint;
            }
        }

        /// <summary>
        /// Returns the &lt;metadata&gt; node child of the &lt;resource&gt; node, or <c>null</c> if none exists.
        /// </summary>
        /// <remarks>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, if there is more than one &lt;metadata&gt; node, a warning is put into the log
        /// and the first node is returned.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        private XPathNavigator CreateMetadataNavigator()
        {
            return m_helper.CacheFirstChild(ref m_metadata, m_helper.Node, Helper.Strings.Metadata, m_helper.ImscpCurrentNamespace);
        }

        /// <summary>
        /// Return a reader of the resource level metadata.
        /// </summary>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        public MetadataNodeReader Metadata
        {
            get
            {
                return MetadataNodeReader.Create(m_helper, m_packageReader, CreateMetadataNavigator(), m_xmlBase);
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;FileNodeReader&gt;</c> containing the &lt;file&gt; nodes within this &lt;resource&gt;.
        /// </summary>
        /// <remarks>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this method is not populated until
        /// properties or methods on it are accessed.  This means that in <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c>, <Typ>InvalidPackageException</Typ> 
        /// may be thrown if there are invalid nodes in the list (e.g. nodes containing illegal attribute values.)
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that dependency files are not included in this list.  Applications should access the
        /// <c>Dependency</c> property to build a full list of files for this resource.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node.</exception>
		/// <example>
        /// Given an <Typ>ActivityNodeReader</Typ>, iterate through all files in all resources in all child activities.
        /// <code language="C#">
		///             foreach (ActivityNodeReader child in activity.ChildActivities)
		///             {
		///                 ResourceNodeReader resource = child.Resource;
		///                 if (resource != null)
		///                 {
		///                     foreach (FileNodeReader file in resource.Files)
		///                     {
		///                         // use the FileNodeReader
		///                     }
		///                 }
		///             }
		/// </code></example>
        public ReadOnlyCollection<FileNodeReader> Files
        {
            get
            {
                if (null == m_files)
                {
                    XPathExpression expr = m_helper.CreateExpression(m_helper.Node, Helper.Strings.Imscp + ":" + Helper.Strings.File);
                    m_files = new ReadOnlyCollection<FileNodeReader>(
                        new ReadOnlyMlcCollection<FileNodeReader>(m_helper.Node, expr,
                            delegate(XPathNavigator fileNav)
                            {
                                FileNodeReader file = new FileNodeReader(m_packageReader, fileNav, m_manifestReader, this, m_xmlBase);
                                // if the file doesn't contain the Location, skip or throw. Accessing Location does this automatically.
                                if (file.Location == null) return null;
                                else return file;
                            }));
                }
                return m_files;
            }
        }

        /// <summary>
        /// Returns the <c>ReadOnlyCollection&lt;ResourceNodeReader&gt;</c> containing the resources this resource is dependent upon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To build this list, the identifierref attributes on the &lt;dependency&gt; nodes inside this &lt;resource&gt; node
        /// are cross-referenced with the identifier attributes on the &lt;resource&gt; nodes within this manifest.
        /// </para>
        /// <para>
        /// The <Typ>ReadOnlyCollection</Typ> returned from this property is populated upon calling this property.  E.g. non-trivial
        /// processing time is required.
        /// </para>
        /// <para>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, warnings are added to the log and invalid nodes are ignored.
        /// </para>
        /// <para>
        /// If there are no nodes of the specified type, an empty <Typ>ReadOnlyCollection</Typ> is returned.
        /// </para>
        /// <para>
        /// Note that this only includes dependencies that are one layer deep.  E.g. if resource A has resource B
        /// as a dependency, resource B is included in the list of dependencies for resource A.  However, if resource B 
        /// has a dependency on resource C, resource C is not included in the list of dependencies for resource A.
        /// An application can recursively call each <Typ>ResourceNodeReader</Typ> dependency list, but care must
        /// be taken for circular dependencies to avoid a code-deadlock.  E.g. if resource A has resource B as a dependency,
        /// but resource B has resource A as a dependency, there is a circular dependency.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> if there is an invalid node.</exception>
		/// <example>
        /// Given an <Typ>ActivityNodeReader</Typ>, iterate through all dependencies on all resources on all child activities.
        /// <code language="C#">
		///             foreach (ActivityNodeReader child in activity.ChildActivities)
		///             {
		///                 ResourceNodeReader resource = child.Resource;
		///                 if (resource != null)
		///                 {
		///                     foreach (ResourceNodeReader dependency in resource.Dependencies)
		///                     {
		///                         // Use the ResourceNodeReader if not null
		///                     }
		///                 }
		///             }
		/// </code></example>
        public ReadOnlyCollection<ResourceNodeReader> Dependencies
        {
            get
            {
                if (m_dependencies == null)
                {
                    // dependency nodes are created by the ManifestReader. Simply need to create a collection of them here.
                    Collection<ResourceNodeReader> dependencies = new Collection<ResourceNodeReader>();
                    XPathNodeIterator itr = m_helper.Node.SelectChildren(Helper.Strings.Dependency, m_helper.ImscpCurrentNamespace);
                    while (itr.MoveNext())
                    {
                        string idref = String.Empty;
                        m_helper.CacheStringAttribute(itr.Current, ref idref, Helper.Strings.IdentifierRef, String.Empty,
                            Helper.SPM.IdentfierRef, 0);
                        if (!String.IsNullOrEmpty(idref))
                        {
                            if (m_manifestReader.Resources.ContainsKey(idref))
                            {
                                dependencies.Add(m_manifestReader.Resources[idref]);
                            }
                            else
                            {
                                m_helper.LogBadAttribute(idref, Helper.Strings.IdentifierRef, itr.Current.Name, ValidatorResources.EmptyString,
                                    ValidatorResources.ReferenceNotFound);
                            }
                        }
                    }
                    m_dependencies = new ReadOnlyCollection<ResourceNodeReader>(dependencies);
                }
                return m_dependencies;
            }
        }

        /// <summary>
        /// Return the XPathNavigator for this resource.
        /// </summary>
        /// <remarks>
        /// This allows an application to get custom namespaced data (e.g. "foo:bar") from the XML.
        /// </remarks>
        public XPathNavigator CreateNavigator()
        {
            return m_helper.Node.Clone();
        }

        /// <summary>
        /// Returns the complete xml:base associated with this item.  "Complete" means that the xml:base is computed
        /// relative to the parent &lt;resources&gt; xml:base and its parent &lt;manifest&gt; xml:base.
        /// </summary>
        public Uri XmlBase
        {
            get
            {
                return m_xmlBase;
            }
        }
    }

    /// <summary>
    /// FileNodeReader provides a read-only view of files that are referenced in a manifest.
    /// </summary>
    /// <remarks>
    /// Parses the &lt;file&gt; node.
    /// </remarks>
    public class FileNodeReader : IXPathNavigable
    {
        // Instance of Helper class to hold common variables and constructor method.  This is used instead
        // of a base class to avoid a public base class (the base class would need to be public since this
        // class is public.
        private Helper m_helper;
        private Uri m_xmlBase; // holds the xml:base to use when generating the Location.
        private ResourceNodeReader m_resourceReader;
        // private member variables to hold attribute and node values
        private Helper.ChildNode m_metadata = new Helper.ChildNode();
        private Uri m_location;
        private bool m_gotLocation;
        private bool? m_isEntryPoint;
        private PackageReader m_packageReader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="packageReader"></param>
        /// <param name="fileNode">A navigator pointing to a &lt;file&gt; node.</param>
        /// <param name="manifestReader">A reader of the manifest.</param>
        /// <param name="resourceReader">A reader of the parent resource.  This is only used by the <c>IsEntryPoint</c>
        /// property.  If null, <c>IsEntryPoint</c> returns false.</param>
        /// <param name="xmlBase">The xml:base to use when constructing the href location for href values that are relative Uri's.
        /// Set to <c>null</c> if there is no xml:base.</param>
        internal FileNodeReader(PackageReader packageReader, XPathNavigator fileNode, ManifestReader manifestReader, ResourceNodeReader resourceReader, Uri xmlBase)
        {
            if (packageReader == null) throw new LearningComponentsInternalException("MR040");
            m_packageReader = packageReader;
            m_resourceReader = resourceReader;
            m_helper = Helper.InternalCreateImscp(fileNode, manifestReader.ManifestSettings, manifestReader.ValidatorSettings, 
                manifestReader.LogReplacement, manifestReader.Log, Helper.Strings.File);
            m_xmlBase = xmlBase;
        }

        /// <summary>
        /// The location of this file. If the path is relative, then the file is within the package. If the 
        /// path is an absolute URL, then the file is not included in the package.
        /// </summary>
        /// <remarks>
        /// If the &lt;file&gt;/href is relative, this value is built relative to the xml:base attributes on 
        /// the &lt;manifest&gt;, &lt;resources&gt;, and &lt;resource&gt; nodes.
        /// <para>
        /// If the href attribute does not exist on the &lt;file&gt; node, and parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>,
        /// a warning is logged and the value <c>null</c> is returned.
        /// </para>
        /// <para>
        /// The location is resolved with the xml:base attributes on the manifest, resources, and resource nodes.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">Parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and the href attribute
        /// is absent.</exception>
        public Uri Location
        {
            get
            {
                if (!m_gotLocation)
                {
                    string hrefValue = String.Empty;
                    m_helper.CacheStringAttribute(m_helper.Node, ref hrefValue, Helper.Strings.Href, String.Empty, Helper.SPM.Href, 0);
                    if (String.IsNullOrEmpty(hrefValue))
                    {
                        m_helper.LogRequiredAttributeMissing(Helper.Strings.Href, m_helper.Node.Name, ValidatorResources.FileHrefMissing);
                        m_location = null;
                    }
                    else
                    {
                        try
                        {
                            m_location = Helper.CombinePaths(m_xmlBase, hrefValue);
                        }
                        catch (LearningComponentsInternalException)
                        {
                            m_helper.LogBadAttribute(hrefValue, Helper.Strings.Href, m_helper.Node.Name, ValidatorResources.EmptyString,
                                ValidatorResources.BadUri);
                        }
                    }
                    m_gotLocation = true;
                }
                return m_location;
            }
        }

        /// <summary>
        /// Returns the &lt;metadata&gt; node child of the &lt;file&gt; node, or <c>null</c> if none exists.
        /// </summary>
        /// <remarks>
        /// In <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=true</c>, if there is more than one &lt;metadata&gt; node, a warning is put into the log
        /// and the first node is returned.
        /// </remarks>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        private XPathNavigator CreateMetadataNavigator()
        {
            return m_helper.CacheFirstChild(ref m_metadata, m_helper.Node, Helper.Strings.Metadata, m_helper.ImscpCurrentNamespace);
        }

        /// <summary>
        /// Return a reader of the file level metadata.
        /// </summary>
        /// <exception cref="InvalidPackageException">The parsing mode is <c><Typ>ManifestReaderSettings</Typ>.FixScormRequirementViolations=false</c> and there is more than
        /// one &lt;metadata&gt; node.</exception>
        public MetadataNodeReader Metadata
        {
            get
            {
                return MetadataNodeReader.Create(m_helper, m_packageReader, CreateMetadataNavigator(), m_xmlBase);
            }
        }

        /// <summary>
        /// True if this is the entry point of the parent &lt;resource&gt;.  False if it is not.
        /// </summary>
        /// <remarks>
        /// A file is the entry point of a resource if it is "launching point" of the resource. E.g. the &lt;file&gt;/href
        /// attribute matches the &lt;resource&gt;/href attribute.  Note that the &lt;resource&gt;/href attribute may be
        /// an external URL, and therefore no &lt;file&gt; will be the entry point.
        /// </remarks>
        public bool IsEntryPoint
        {
            get
            {
                if (m_isEntryPoint == null)
                {
                    if (m_resourceReader == null)
                    {
                        m_isEntryPoint = false;
                    }
                    else
                    {
                        if (m_resourceReader.EntryPoint == Location)
                        {
                            m_isEntryPoint = true;
                        }
                        else
                        {
                            m_isEntryPoint = false;
                        }
                    }
                }
                return m_isEntryPoint.Value;
            }
        }

        /// <summary>
        /// Return the XPathNavigator for this resource.
        /// </summary>
        /// <remarks>
        /// This allows an application to get custom namespaced data (e.g. "foo:bar") from the XML.
        /// </remarks>
        public XPathNavigator CreateNavigator()
        {
            return m_helper.Node.Clone();
        }
    }

    #endregion // ManifestReader classes

    /// <summary>
    /// Represents metadata within a manifest. 
    /// </summary>
    /// <remarks>
    /// This class does very little error checking or parsing of metadata, and is provided to
    /// simplify obtaining the title and description of a metadata block.  The first call to
    /// <Mth>GetTitle</Mth> or <Mth>GetDescriptions</Mth> will throw <Typ>InvalidPackageException</Typ>
    /// if the &lt;adlcp:location&gt; of the &lt;metadata&gt; points to a file that can not be
    /// obtained.  Thereafter, they return <c>String.Empty</c>.
    /// <para>
    /// When obtained via a <c>Metadata</c> property on <Typ>ManifestReader</Typ>, <Typ>OrganizationNodeReader</Typ>,
    /// <Typ>ActivityNodeReader</Typ>, <Typ>ResourceNodeReader</Typ>, or <Typ>FileNodeReader</Typ>, the
    /// <Mth>GetTitle</Mth> is called before the <Typ>MetadataNodeReader</Typ>
    /// is returned.  If a <Typ>InvalidPackageException</Typ> is thrown during this calls, it is caught
    /// and handled according to the chosen <Typ>ManifestReaderSettings</Typ> and package validation enforcement.
    /// </para>
    /// <para>
    /// This means that any additional calls to <Mth>GetTitle</Mth> or <Mth>GetDescriptions</Mth> will return <c>String.Empty</c>.
    /// </para>
    /// </remarks>
    public class MetadataNodeReader : IXPathNavigable
    {
        private XPathNavigator m_metadata;
        private XPathNavigator m_lom; // points to the <lom> node of the metadata. Only the Lom property should access this.
        private XmlNamespaceManager m_metadataNamespaces;
        private Uri m_xmlBase;
        private PackageReader m_packageReader;
        private Uri m_location;
        private bool m_gotLocation; // true if m_location is valid
        private bool m_exceptionThrown; // true if an InvalidPackageException has been thrown

        /// <summary>
        /// Create a Metadata object.
        /// </summary>
        /// <param name="packageReader">The package that contains any metadata files that are external to the 
        /// manifest.  If null, this <Typ>MetadataNodeReader</Typ> will be unable to retrieve metadata external
        /// to the manifest.</param>
        /// <param name="metadata">A &lt;metadata&gt; node from a manifest. </param>
        /// <param name="xmlBase">The xml:base to use when constructing the location for metadata external to
        /// the manifest.
        /// Set to <c>null</c> if there is no xml:base.</param>
        /// <remarks>
        /// There is no error checking done on the parameters.
        /// </remarks>
        private MetadataNodeReader(PackageReader packageReader, XPathNavigator metadata, Uri xmlBase)
        {
            if (metadata != null)
            {
                m_metadata = metadata.Clone();
            }
            m_packageReader = packageReader;
            m_xmlBase = xmlBase;
        }

        /// <summary>
        /// Create a Metadata object, with error handling during creation.
        /// </summary>
        /// <param name="helper">Helper object used to assist error handling.  If null, no error handling occurs.</param>
        /// <param name="packageReader">The package that contains any metadata files that are external to the 
        /// manifest.  If null, this <Typ>MetadataNodeReader</Typ> will be unable to retrieve metadata external
        /// to the manifest.</param>
        /// <param name="metadata">A &lt;metadata&gt; node from a manifest. </param>
        /// <param name="xmlBase">The xml:base to use when constructing the location for metadata external to
        /// the manifest.
        /// Set to <c>null</c> if there is no xml:base.</param>
        /// <remarks>
        /// After creating a new <Typ>MetadataNodeReader</Typ> using the constructor, this method then calls
        /// <Mth>GetTitle</Mth>, catching any <Typ>InvalidPackageException</Typ> that occurs.  It then uses the 
        /// <Typ>Helper</Typ> to handle the exception according to the <Typ>ManifestReaderSettings</Typ> and
        /// validation behavior.
        /// </remarks>
        internal static MetadataNodeReader Create(Helper helper, PackageReader packageReader, XPathNavigator metadata, Uri xmlBase)
        {
            MetadataNodeReader reader = new MetadataNodeReader(packageReader, metadata, xmlBase);
            // call GetTitle(), which throws InvalidPackageException the first time it is called
            // if the <location> points to a file in the package that doesn't exist.
            try
            {
                reader.GetTitle(CultureInfo.CurrentCulture);
            }
            catch (InvalidPackageException e)
            {
                if (helper == null)
                {
                    // just rethrow
                    throw;
                }
                else
                {
                    helper.LogInvalidNode(Helper.Strings.Metadata, e.Message);
                }
            }
            return reader;
        }

        /// <summary>
        /// Returns the version of scorm parsed by this reader.
        /// </summary>
        /// <remarks>
        /// Note that if there is no &lt;metadata&gt; node to parse, this internal method returns a scorm version of "v1p3"
        /// even if the package may be a "v1p2" package.
        /// </remarks>
        internal Helper.ScormVersion ScormVersion
        {
            get
            {
                if (m_metadata != null && String.CompareOrdinal(Helper.Strings.ImscpNamespaceV1p2, m_metadata.NamespaceURI) == 0)
                {
                    return Helper.ScormVersion.v1p2;
                }
                else
                {
                    return Helper.ScormVersion.v1p3;
                }
            }
        }

        /// <summary>
        /// Returns an object that can be used for resolving namespaces in an XPathNavigator.SelectSingleNode call.
        /// The metadata namespace will be identified by the "lom:" prefix.  The "adlcp:" namespace is also included
        /// because of the &lt;adlcp:location&gt; node.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For SCORM 2004, the metadata namespace is (http://ltsc.ieee.org/xsd/LOM).
        /// </para>
        /// <para>
        /// For SCORM 1.2, the metadata namespace is (http://www.imsglobal.org/xsd/imsmd_rootv1p2).
        /// </para>
        /// </remarks>
        internal XmlNamespaceManager MetadataNamespaces
        {
            get
            {
                if (m_metadataNamespaces == null)
                {
                    m_metadataNamespaces = new XmlNamespaceManager(m_metadata.NameTable);
                    // use the m_metadata namespace to determine which version of SCORM this metadata is in.  Default
                    // to SCORM 2004 if the metadata is not in SCORM 1.2.
                    if (ScormVersion == Helper.ScormVersion.v1p2)
                    {
                        m_metadataNamespaces.AddNamespace("lom", Helper.Strings.LomNamespaceV1p2);
                        m_metadataNamespaces.AddNamespace(Helper.Strings.Adlcp, Helper.Strings.AdlcpNamespaceV1p2);
                    }
                    else
                    {
                        m_metadataNamespaces.AddNamespace("lom", Helper.Strings.LomNamespaceV1p3);
                        m_metadataNamespaces.AddNamespace(Helper.Strings.Adlcp, Helper.Strings.AdlcpNamespaceV1p3);
                    }
                }
                return m_metadataNamespaces;
            }
        }

        /// <summary>
        /// Points to the &lt;lom&gt; node in the metadata. This may be in a different file than the manifest,
        /// if the <Mth>Location</Mth> exists.  Returns null if <Mth>Location</Mth> is a URI outside of the package.
        /// </summary>
        private XPathNavigator Lom
        {
            get
            {
                if (m_metadata == null) return null;
                if (m_lom == null)
                {
                    string xpath = "lom:lom";
                    // handle if location is another file
                    if (Location != null)
                    {
                        bool invalidLocation = false;
                        // If the location is a URI outside of the package, return null.
                        // Otherwise, use the package reader to read the file.
                        Uri fullLocation = Helper.CombinePaths(m_xmlBase, Location.OriginalString);
                        if (fullLocation.IsAbsoluteUri)
                        {
                            // Absolute Uri is technically ok, but is not supported by this reader.
                            return null;
                        }
                        else
                        {
                            string filePath = HttpUtility.UrlDecode(fullLocation.OriginalString);
                            if (m_packageReader.FileExists(filePath))
                            {
                                XmlDocument doc = new XmlDocument();
                                try
                                {
                                    using (Stream stream = m_packageReader.GetFileStream(filePath))
                                    {
                                        doc.Load(stream);
                                    }
                                    m_lom = doc.CreateNavigator().SelectSingleNode(xpath, MetadataNamespaces);
                                }
                                catch (XmlException)
                                {
                                    if (!m_exceptionThrown)
                                    {
                                        m_exceptionThrown = true;
                                        throw new InvalidPackageException(String.Format(CultureInfo.InvariantCulture,
                                            ValidatorResources.MetadataLocationBadXml, Location.OriginalString));
                                    }
                                }
                            }
                            else
                            {
                                invalidLocation = true;
                            }
                        }
                        if (invalidLocation && !m_exceptionThrown)
                        {
                            m_exceptionThrown = true;
                            throw new InvalidPackageException(String.Format(CultureInfo.InvariantCulture,
                                ValidatorResources.MetadataLocationNotFound, Location.OriginalString));
                        }
                    }
                    else
                    {
                        m_lom = m_metadata.SelectSingleNode(xpath, MetadataNamespaces);
                    }
                }
                return m_lom;
            }
        }

        /// <summary>
        /// Gets the title associated with this metadata that matches the current culture.
        /// </summary>
        /// <returns>The title that best matches the current culture or String.Empty if there is no title specified.
        /// </returns>
        /// <remarks>
        /// Note that an <Typ>InvalidPackageException</Typ> may be thrown on the first call to this method.  If this happens,
        /// subsequent calls do not throw additional exceptions.  Instead, <c>String.Empty</c> is returned.
        /// <para>
        /// If <Mth>Location</Mth> is an absolute URI, this method returns <c>String.Empty</c> because absolute URI
        /// is not supported.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">This is the first call to this method, and the metadata is 
        /// located in a metadata file that can not be found.</exception>
        public string GetTitle()
        {
            return GetTitle(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the general title associated with this metadata that best matches the specified culture.
        /// </summary>
        /// <param name="culture">The culture used to determine which title to return.</param>
        /// <returns>The title that best matches the specified <paramref name="culture"/> or String.Empty if there is no title specified.
        /// </returns>
        /// <remarks>
        /// Note that an <Typ>InvalidPackageException</Typ> may be thrown on the first call to this method.  If this happens,
        /// subsequent calls do not throw additional exceptions.  Instead, <c>String.Empty</c> is returned.
        /// <para>
        /// If <Mth>Location</Mth> is an absolute URI, this method returns <c>String.Empty</c> because absolute URI
        /// is not supported.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">This is the first call to this method, and the metadata is 
        /// located in a metadata file that can not be found.</exception>
        public string GetTitle(CultureInfo culture)
        {
            if (Lom == null) return String.Empty;
            else
            {
                // get XmlNode that maps to: metadata/lom/general/title
                string xpath = "lom:general/lom:title";
                XPathNavigator title = Lom.SelectSingleNode(xpath, MetadataNamespaces);
                if (title == null)
                {
                    return String.Empty;
                }
                else
                {
                    return Helper.ReplaceCarriageReturns(GetLangString(title, culture));
                }
            }
        }

        /// <summary>
        /// Gets a collection of general description values that matches the current culture.
        /// </summary>
        /// <returns>The descriptions that best match the current culture, or an empty collection if no
        /// descriptions exist.
        /// </returns>
        /// <remarks>
        /// This &lt;description&gt; blocks in the metadata should each contain consistent language strings,
        /// or unpredictable values may be returned from this method.
        /// <para>
        /// Note that an <Typ>InvalidPackageException</Typ> may be thrown on the first call to this method.  If this happens,
        /// subsequent calls do not throw additional exceptions.  Instead, an empty collection is returned.        
        /// </para>
        /// <para>
        /// If <Mth>Location</Mth> is an absolute URI, this method returns an empty collection because absolute URI
        /// is not supported.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">This is the first call to this method, and the metadata is 
        /// located in a metadata file that can not be found.</exception>
        public ReadOnlyCollection<string> GetDescriptions()
        {
            return GetDescriptions(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets a collection of general description values that most closely match the requested culture.
        /// </summary>
        /// <param name="culture">The culture used to determine which descriptions to return.</param>
        /// <returns>The description that best matches the specified <paramref name="culture"/>, or an empty collection if no
        /// descriptions exist.
        /// </returns>
        /// <remarks>
        /// This &lt;description&gt; blocks in the metadata should each contain consistent language strings,
        /// or unpredictable values may be returned from this method.
        /// <para>
        /// Note that an <Typ>InvalidPackageException</Typ> may be thrown on the first call to this method.  If this happens,
        /// subsequent calls do not throw additional exceptions.  Instead, <c>String.Empty</c> is returned.        
        /// </para>
        /// <para>
        /// If <Mth>Location</Mth> is an absolute URI, this method returns <c>String.Empty</c> because absolute URI
        /// is not supported.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidPackageException">This is the first call to this method, and the metadata is 
        /// located in a metadata file that can not be found.</exception>
        public ReadOnlyCollection<string> GetDescriptions(CultureInfo culture)
        {
            if (Lom == null) return new ReadOnlyCollection<string>(new List<string>());
            else
            {
                XPathNodeIterator itr = Lom.Select("lom:general/lom:description", MetadataNamespaces);
                List<string> list = new List<string>(itr.Count);
                while (itr.MoveNext())
                {
                    if (!String.IsNullOrEmpty(itr.Current.Value))
                    {
                        list.Add(Helper.ReplaceCarriageReturns(GetLangString(itr.Current, culture)));
                    }
                }
                return new ReadOnlyCollection<string>(list);
            }
        }

        /// <summary>
        /// Gets the location of the metadata information, if it is external to the manifest.
        /// If this value returns a location that indicates the metadata information is not in the package, 
        /// then other properties in this object will not read their values. If the location is 
        /// within the package or if there is no location specified, this object will provide properties to 
        /// read and parse those values.
        /// </summary>
        /// <remarks>
        /// Returns null if there is no &lt;adlcp:location&gt; node in the metadata.
        /// <para>
        /// If this is an absolute URI, <c>String.Empty</c> is returned from <Mth>GetTitle</Mth> and
        /// <Mth>GetDescriptions</Mth>.  Absolute URI's are valid according to SCORM, but are not supported
        /// by this reader.
        /// </para>
        /// </remarks>
        public Uri Location
        {
            get
            {
                if (!m_gotLocation)
                {
                    m_gotLocation = true;
                    if (m_metadata != null)
                    {
                        // get XmlNode that maps to: metadata/adlcp:location
                        string xpath = "adlcp:location";
                        XPathNavigator location = m_metadata.SelectSingleNode(xpath, MetadataNamespaces);
                        if (location != null)
                        {
                            m_location = Helper.CombinePaths(m_xmlBase, location.Value);
                        }
                    }
                }
                return m_location;
            }
        }

        /// <summary>
        /// Returns the langstring that matches the culture specified in the CultureInfo property, or String.Empty if one doesn't exist.
        /// </summary>
        private string GetLangString(XPathNavigator node, CultureInfo culture)
        {
            string str = null;
            XPathNodeIterator itr;
            if (ScormVersion == Helper.ScormVersion.v1p2)
            {
                itr = node.Select("lom:langstring", MetadataNamespaces);
            }
            else
            {
                itr = node.Select("lom:string", MetadataNamespaces);
            }
            while (itr.MoveNext())
            {
                // set str to the first element in the list so it gets returned if not overridden below
                if (str == null)
                {
                    // note that empty-string is a valid value.
                    str = itr.Current.Value;
                }
                string language = null; // the language of the current value
                if (ScormVersion == Helper.ScormVersion.v1p2)
                {
                    language = itr.Current.XmlLang;
                }
                else
                {
                    XPathNavigator nav = itr.Current.Clone();
                    if (nav.MoveToAttribute("language", String.Empty))
                    {
                        language = nav.Value;
                    }
                }
                if (language != null)
                {
                    // note that the following string comparisons ignore case, as per RFC 3066 (see http://tools.ietf.org/html/rfc3066).

                    // if there is an exact culture name match, return that
                    if (String.Compare(language, culture.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // note that empty-string is a valid value.
                        str = itr.Current.Value;
                        break;
                    }
                    // if there is a two letter ISO language name match, that is "good enough", but keep looking for
                    // an exact match
                    if (String.Compare(language, culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // note that empty-string is a valid value.
                        str = itr.Current.Value;
                    }
                }
            }
            // if there are no string elements under node, return String.Empty
            if (str == null) str = String.Empty;
            return str;
        }

        /// <summary>
        /// Gets metadata in <Typ>XPathNavigator</Typ> format.
        /// </summary>
        /// <returns>An <Typ>XPathNavigator</Typ> containing the &lt;metadata&gt; node, or <c>null</c> if there is no metadata node.</returns>
        public XPathNavigator CreateNavigator()
        {
            if (m_metadata == null) return null;
            else return m_metadata.Clone();
        }
    }
}
