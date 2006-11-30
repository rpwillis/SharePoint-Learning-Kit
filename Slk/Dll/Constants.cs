/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Constants required for the application.
    /// </summary>
    internal static class Constants
    {
        #region Public Members
        /// <summary>
        /// Character Hyphen '-'
        /// </summary>
        public const char CharHyphen = '-';

        /// <summary>
        /// Character Colon ':'
        /// </summary>
        public const char CharColon = ':';

        /// <summary>
        /// String Hyphen '-'
        /// </summary>
        public const String Hyphen = "-";

        /// <summary>
        /// String Comma ','
        /// </summary>
        public const String Comma = ",";

        /// <summary>
        /// String Colon ':'
        /// </summary>
        public const String Colon = ":";

        /// <summary>
        /// String SemiColon ':'
        /// </summary>
        public const String SemiColon = ";";

        /// <summary>
        /// Equal To
        /// </summary>
        public const String EqualTo = "=";

        /// <summary>
        /// String Mark
        /// </summary>
        public const String QuestionMark = "?";

        /// <summary>
        /// Dollor Sign
        /// </summary>
        public const String DollorSign = "$";

        /// <summary>
        /// UnderScore
        /// </summary>
        public const String UnderScore = "_";
        
        /// <summary>
        /// Default Value for the LearnerQuerySet
        /// </summary>
        public const string DefaultLearnerQuerySet = "LearnerQuerySet";

        /// <summary>
        /// Default Value for the LearnerQuerySet
        /// </summary>
        public const string DefaultInstructorQuerySet = "InstructorQuerySet";

        /// <summary>
        /// Default Value for Height of the Assignmentlist Web Part
        /// </summary>
        public const string WebPartHeight = "250px";

        /// <summary>
        /// Default Summary Frame Width Value
        /// </summary>
        public const double SummaryFrameWidth = 150;

        /// <summary>
        /// Key for opening Target in new window
        /// </summary>
        public const string TargetBlank = "_blank";

        /// <summary>
        /// The round-trip  specifier "R" for float or double guarantees that a numeric value 
        /// converted to a string will be parsed back into the same numeric value.  
        /// </summary>
        public const string RoundTrip = "R";
        /// <summary>
        /// The number is converted to a string of the form "-ddd.ddd…" where 
        /// each 'd' indicates a digit (0-9). The string starts with a minus sign 
        /// if the number is negative.
        /// </summary>
        public const string FixedPoint = "F";
       
        //Slk App Web Pages and Framset Page 

        public const string QuerySetPage = "QuerySet.aspx";
        public const string QuerySummaryPage = "QuerySummary.aspx";
        public const string QueryResultPage = "QueryResults.aspx";
        public const string GradingPage = "Grading.aspx";
        public const string FrameSetPage = "Frameset/Frameset.aspx";

        public const string SlkUrlPath = "/_layouts/SharePointLearningKit/";
        public const string ImagePath = "/_layouts/SharePointLearningKit/Images/";
        
        public const string InfoIcon = "InfoIcon.gif";
        public const string ErrorIcon = "ErrorIcon.gif";
        public const string WarningIcon = "WarningIcon.gif";
        public const string MarkCompleteIcon = "MarkCompleteIcon.gif";
        public const string NewDocumentIcon = "NewDocumentIcon.gif";
        public const string DeleteIcon = "DeleteIcon.gif";
        public const string SubmitIcon = "SubmitIcon.gif";
        public const string CollectAllIcon = "CollectAllIcon.gif";
        public const string EditIcon = "EditIcon.gif";
        public const string ReturnAllIcon = "ReturnAllIcon.gif";
        public const string SaveIcon = "SaveIcon.gif";
        public const string GradingSatisfiedIcon = "GradingSatisfied.gif";
        public const string GradingUnSatisfiedIcon = "GradingUnsatisfied.gif";
        public const string WaitIcon = "WaitIcon.gif";

        //Blank Gif Path
        public const string BlankGifUrl = ImagePath + "Blank.gif";

        // Class to use for toolbars
        public const string ToolbarClass = "ms-toolbar";        

        #endregion

        #region Html
        /// <summary>
        /// Blank html space
        /// </summary>
        public const string NonBreakingSpace = "&nbsp;";

        public const string Space = " "; 

        /// <summary>
        /// Html Line Break
        /// </summary>
        public const string LineBreak = "<br/>";

        /// <summary>
        /// Html Carriage Return and Line Feed
        /// </summary>
        public const string CRLF  = "\r\n";

        /// <summary>
        /// Character new Line '\n'
        /// </summary>
        public const char NewLineChar = '\n';
        #endregion
    }
}
