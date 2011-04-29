using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Xunit;

namespace Microsoft.SharePointLearningKit.Test
{
    /// <summary>Tests the SlkSettings object.</summary>
    public class SlkSettingsTest
    {
        const string minimalEmailSettings = @"<Settings xmlns='urn:schemas-microsoft-com:sharepoint-learning-kit:settings' xmlns:slk='urn:schemas-microsoft-com:sharepoint-learning-kit:settings'
    ApprovedAttachmentTypes='bmp, doc, docx, gif, htm, html, jpeg, jnt, jpg, lit, mdi, mix, pdf, png, ppt, pptx, pub, rtf, txt, wdb, wks, xlr, xls, xlsx, zip'
    ELearningIisCompatibilityModeExtensions='mp3, swf'
    LoggingOptions='None' 
    MaxAttachmentKilobytes='10240'
    NonELearningIisCompatibilityModeExtensions=''
    PackageCacheExpirationMinutes='4320'
    PackageCacheLocation='%TEMP%\SLK_PC'
    UserWebListMruSize='6'>

  <MimeTypeMapping Extension='.xml' MimeType='text/xml' />

  <EmailSettings>
      <NewAssignment Subject='Test Subject'>
          <slk:Body xmlns=''><p>First para.</p><p>second para</p></slk:Body>
      </NewAssignment>
  </EmailSettings>

  <Query Name='OverdueLearner' Title='Overdue' ViewName='LearnerAssignmentListForLearners' CountViewColumnName='IsFinal'>
    <Column Title='Site' RenderAs='SPWebName' ViewColumnName='AssignmentSPWebGuid' ViewColumnName2='AssignmentSPSiteGuid' />
    <Column Title='Assignment' RenderAs='Link' ViewColumnName='AssignmentTitle' ViewColumnName2='LearnerAssignmentId' NullDisplayString='Untitled' />
    <Column Title='Due' RenderAs='UtcAsLocalDateTime' ViewColumnName='AssignmentDueDate' CellFormat='d' NullDisplayString='--' ToolTipFormat='Due: {0:D}, {0:t}' Wrap='false' />
    <Column Title='File Submission' ViewColumnName='FileSubmissionState'/>
    <Column Title='Status' RenderAs='LearnerAssignmentStatus' ViewColumnName='LearnerAssignmentState' Wrap='false' />
    <Column Title='Score' RenderAs='ScoreAndPossible' ViewColumnName='FinalPoints' ViewColumnName2='AssignmentPointsPossible' ToolTipFormat='Score: {0}' Wrap='false' />
    <Condition ViewColumnName='AssignmentSPWebGuid' Operator='Equal' MacroName='SPWebScope' NoConditionOnNull='true' />
    <Condition ViewColumnName='AssignmentStartDate' Operator='LessThanEqual' MacroName='Now'/>
    <Condition ViewColumnName='AssignmentDueDate' Operator='IsNotNull' />
    <Condition ViewColumnName='AssignmentDueDate' Operator='LessThan' MacroName='Now'/>
    <Condition ViewColumnName='IsFinal' Operator='NotEqual' Value='1' /><!-- helps SQL perf? -->
    <Condition ViewColumnName='LearnerAssignmentState' Operator='LessThan' Value='2'/><!-- i.e. unsubmitted -->
    <Sort ViewColumnName='AssignmentDueDate' Ascending='true'/>
    <Sort ViewColumnName='LearnerAssignmentState' Ascending='true'/>
  </Query>

<QuerySet Name='LearnerQuerySet' Title='Learner Query Set' DefaultQueryName='OverdueLearner'>
    <IncludeQuery QueryName='OverdueLearner' />
  </QuerySet>

</Settings>";

#region tests
        /// <summary>Tests that email settings are loaded.</summary>
        [Fact]
        public void LoadSettings_IncludeEmailSettings_AreParsedCorrectly()
        {
            using (StringReader stringReader = new StringReader(minimalEmailSettings))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    SlkSettings settings = new SlkSettings(reader, DateTime.Now);

                    Assert.True(settings.EmailSettings != null);
                    EmailSettings emailSettings = settings.EmailSettings;
                    Assert.True(emailSettings.NewAssignment != null);
                    Assert.Equal("Test Subject", emailSettings.NewAssignment.Subject);
                    string expectedBody = @"<p>First para.</p><p>second para</p>";
                    Assert.Equal(expectedBody, emailSettings.NewAssignment.Body);
                }
            }

        }

        /// <summary>Tests that email settings are loaded.</summary>
        [Fact]
        public void LoadSettings_MinimalSettings_QueryIsLoaded()
        {
            using (StringReader stringReader = new StringReader(minimalEmailSettings))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    SlkSettings settings = new SlkSettings(reader, DateTime.Now);
                    Assert.Equal(1, settings.QueryDefinitions.Count);
                    Assert.Equal("OverdueLearner", settings.QueryDefinitions[0].Name);
                }
            }
        }

        /// <summary>Tests that the standard settings load successfully.</summary>
        [Fact]
        public void LoadSettings_StandardSettings_LoadsSuccessfully()
        {
            using (StreamReader stringReader = new StreamReader("../../slksettings.xml"))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    SlkSettings settings = new SlkSettings(reader, DateTime.Now);
                    Assert.Equal(22, settings.QueryDefinitions.Count);
                }
            }
        }

        /// <summary>Validate that the standard settings file validates according to the schema.</summary>
        [Fact]
        public void LoadSettings_StandardSettingsValidateToSchema_ValidateSuccessfully()
        {
            using (StreamReader schemaReader = new StreamReader("../../slksettings.xsd"))
            {
                System.Xml.Schema.XmlSchema xmlSchema = XmlSchema.Read(schemaReader,
                        delegate(object sender2, ValidationEventArgs e2)
                        {
                            XmlSchemaException schemaException = e2.Exception;
                            string messageFormat = "[{0}:{1}] {2}";
                            string message = string.Format(CultureInfo.InvariantCulture, messageFormat, schemaException.LineNumber, schemaException.LinePosition, schemaException.Message);
                            throw new InvalidOperationException(message);
                        });
                using (StreamReader stringReader = new StreamReader("../../slksettings.xml"))
                {
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    xmlSettings.Schemas.Add(xmlSchema);
                    xmlSettings.ValidationType = ValidationType.Schema;
                    using (XmlReader reader = XmlReader.Create(stringReader, xmlSettings))
                    {
                        while (reader.Read())
                        {
                        }
                    }
                }
            }
        }
#endregion tests
    }
}

