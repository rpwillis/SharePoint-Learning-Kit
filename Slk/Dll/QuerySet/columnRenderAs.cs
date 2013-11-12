using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Specifies how to render the a column defined by a <c>ColumnDefinition</c>.
    /// </summary>
    ///
    [SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    public enum ColumnRenderAs
    {
        /// <summary>
        /// Render the column as a string, converted from the column specified by
        /// <c>ColumnDefinition.ViewColumnName</c>.
        /// </summary>
        Default,

        /// <summary>
        /// Render the column as a string, converted from the column specified by
        /// <c>ColumnDefinition.ViewColumnName</c>, which must be a date/time value represented in the
        /// UTC (GMT) time zone.  The date/time is converted to local time for display purposes.
        /// </summary>
        UtcAsLocalDateTime,

        /// <summary>
        /// Render the name of an SPWeb.  The column specified by
        /// <c>ColumnDefinition.ViewColumnName</c> must contain the SPWeb GUID, and the column
        /// specified by <c>ColumnDefinition.ViewColumnName2</c> must contain the SPSite GUID.
        /// </summary>
        SPWebName,

        /// <summary>
        /// Render a link to another page.  The column specified by
        /// <c>ColumnDefinition.ViewColumnName</c> must contain the title of the link, and the column
        /// specified by <c>ColumnDefinition.ViewColumnName2</c> must contain the
        /// <c>LearningStoreItemIdentifier</c> of the information to display in the linked-to page.
        /// </summary>
        Link,

        /// <summary>
        /// Render the status of a learner assignment.  The column specified by
        /// <c>ColumnDefinition.ViewColumnName</c> must contain a <c>bool</c> value: <c>true</c> if the
        /// learner assignment has been completed, <c>false</c> if not.  The column specified by
        /// <c>ColumnDefinition.ViewColumnName2</c> must contain the <c>AttemptStatus</c> of the
        /// learner assignment.
        /// </summary>
        LearnerAssignmentStatus,

        /// <summary>
        /// Render the number of points the learner received (if applicable) and the number of points
        /// possible for the assignment (if applicable); for example, "7/10".  The column specified by
        /// <c>ColumnDefinition.ViewColumnName</c> must contain the number of points the learner
        /// received (e.g. FinalPoints column in LearnerAssignmentLearnerView); this value may be NULL
        /// if the assignment does not award points, or if the assignment hasn't been graded.
        /// The column specified by <c>ViewColumnName2</c> must contain the nominal maximum number of
        /// points that learners may receive on the assignment (e.g. AssignmentPointsPossible column
        /// in LearnerAssignmentLearnerView); may be NULL if not applicable.
        /// </summary>
        ScoreAndPossible,


        /// <summary>
        /// Render the number of learner assignments submitted and the number of learner assignments
        /// for the assignment; for example, "10/12".  Each value is an integer.  Neither value can be
        /// NULL.
        /// </summary>
        Submitted,
    }
}

