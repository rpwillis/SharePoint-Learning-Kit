CREATE PROCEDURE DropConstraints
(
    @table      NVARCHAR(255),
    @type       NVARCHAR(50)
)
AS
BEGIN
    DECLARE @sql nvarchar(255)
    WHILE EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE table_name = @table AND CONSTRAINT_TYPE = @TYPE)
    BEGIN
        SELECT    @sql = 'ALTER TABLE ' + @table + ' DROP CONSTRAINT ' + CONSTRAINT_NAME 
        FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE    table_name = @table
                AND CONSTRAINT_TYPE = @TYPE
        EXEC    sp_executesql @sql
    END
END
GO

exec DropConstraints 'ActivityAttemptItem', 'CHECK'

ALTER TABLE AssignmentItem ADD EmailChanges BIT NOT NULL CONSTRAINT DF_AssignmentItem_EmailChanges DEFAULT 0
GO
ALTER TABLE AssignmentItem ADD CONSTRAINT DF_AssignmentItem_AutoReturn DEFAULT 0 FOR AutoReturn
GO
ALTER TABLE AssignmentItem ADD CONSTRAINT DF_AssignmentItem_ShowAnswersToLearners DEFAULT 0 FOR ShowAnswersToLearners
GO

ALTER TABLE LearnerAssignmentItem ADD Grade NVARCHAR(20) NULL
GO


CREATE TABLE UserItemSite (
    Id bigint IDENTITY NOT NULL CONSTRAINT UNQ_UserItemSite UNIQUE,
    UserId  BIGINT NOT NULL CONSTRAINT FK_UserItemSite_UserId  REFERENCES UserItem (Id) ON DELETE CASCADE,
    [SPSiteGuid] uniqueidentifier NOT NULL,
    SPUserId BIGINT NOT NULL,
    CONSTRAINT PK_UserItemSite PRIMARY KEY  CLUSTERED  (UserId, SPSiteGuid)
)
GO
GRANT SELECT, INSERT, DELETE, UPDATE ON [UserItemSite] TO LearningStore
GO

-- Create function that implements the default view for the UserItem item type
CREATE FUNCTION [UserItemSite$DefaultView](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT Id, UserId, SPSiteGuid, SPUserId
    FROM [UserItemSite]
)
GO
GRANT SELECT ON [UserItemSite$DefaultView] TO LearningStore
GO


DROP PROCEDURE DropConstraints
GO

DECLARE @schema varchar(max)
SET @schema = '<StoreSchema>'
SET @schema = @schema +
    '<ItemType Name="ActivityAttemptItem" ViewFunction="ActivityAttemptItem$DefaultView">' + 
        '<Property Name="AttemptId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="AttemptItem"/>' +
        '<Property Name="ActivityPackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="CompletionStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="CompletionStatus"/>' +
        '<Property Name="AttemptCount" TypeCode="9" Nullable="true" HasDefault="true"/>' +
        '<Property Name="DataModelCache" TypeCode="7" Nullable="true" HasDefault="true"/>' +
        '<Property Name="EvaluationPoints" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Exit" TypeCode="8" Nullable="true" HasDefault="true" EnumName="ExitMode"/>' +
        '<Property Name="LessonStatus" TypeCode="8" Nullable="true" HasDefault="true" EnumName="LessonStatus"/>' +
        '<Property Name="Location" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MinScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MaxScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ProgressMeasure" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="RandomPlacement" TypeCode="9" Nullable="true" HasDefault="true"/>' +
        '<Property Name="RawScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ScaledScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SequencingDataCache" TypeCode="7" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SessionStartTimestamp" TypeCode="4" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SessionTime" TypeCode="6" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SuccessStatus" TypeCode="8" Nullable="true" HasDefault="true" EnumName="SuccessStatus"/>' +
        '<Property Name="SuspendData" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="TotalTime" TypeCode="6" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="ActivityObjectiveItem" ViewFunction="ActivityObjectiveItem$DefaultView">' + 
        '<Property Name="ActivityPackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="IsPrimaryObjective" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="Key" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="MinNormalizedMeasure" TypeCode="5" Nullable="false" HasDefault="true"/>' +
        '<Property Name="SatisfiedByMeasure" TypeCode="3" Nullable="false" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="ActivityPackageItem" ViewFunction="ActivityPackageItem$DefaultView">' + 
        '<Property Name="ActivityIdFromManifest" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="OriginalPlacement" TypeCode="9" Nullable="false" HasDefault="false"/>' +
        '<Property Name="ParentActivityId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="PackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="PackageItem"/>' +
        '<Property Name="PrimaryObjectiveId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityObjectiveItem"/>' +
        '<Property Name="ResourceId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ResourceItem"/>' +
        '<Property Name="PrimaryResourceFromManifest" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="DataModelCache" TypeCode="7" Nullable="true" HasDefault="true"/>' +
        '<Property Name="CompletionThreshold" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Credit" TypeCode="3" Nullable="true" HasDefault="true"/>' +
        '<Property Name="HideContinue" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="HidePrevious" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="HideExit" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="HideAbandon" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="IsVisibleInContents" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="LaunchData" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MasteryScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MaxAttempts" TypeCode="9" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MaxTimeAllowed" TypeCode="6" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ResourceParameters" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ScaledPassingScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="TimeLimitAction" TypeCode="8" Nullable="true" HasDefault="true" EnumName="TimeLimitAction"/>' +
        '<Property Name="Title" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="ObjectivesGlobalToSystem" TypeCode="3" Nullable="false" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="PackageGlobalObjectiveItem" ViewFunction="PackageGlobalObjectiveItem$DefaultView">' + 
        '<Property Name="LearnerId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="GlobalObjectiveId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="GlobalObjectiveItem"/>' +
        '<Property Name="ScaledScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SuccessStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="SuccessStatus"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="AttemptItem" ViewFunction="AttemptItem$DefaultView">' + 
        '<Property Name="LearnerId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="RootActivityId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="CompletionStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="CompletionStatus"/>' +
        '<Property Name="CurrentActivityId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="SuspendedActivityId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="PackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="PackageItem"/>' +
        '<Property Name="AttemptStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="AttemptStatus"/>' +
        '<Property Name="FinishedTimestamp" TypeCode="4" Nullable="true" HasDefault="true"/>' +
        '<Property Name="LogDetailSequencing" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="LogFinalSequencing" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="LogRollup" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="StartedTimestamp" TypeCode="4" Nullable="true" HasDefault="false"/>' +
        '<Property Name="SuccessStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="SuccessStatus"/>' +
        '<Property Name="TotalPoints" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="LearnerAssignmentId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="LearnerAssignmentItem"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="AttemptObjectiveItem" ViewFunction="AttemptObjectiveItem$DefaultView">' + 
        '<Property Name="ActivityAttemptId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Property Name="ActivityObjectiveId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityObjectiveItem"/>' +
        '<Property Name="CompletionStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="CompletionStatus"/>' +
        '<Property Name="Description" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="IsPrimaryObjective" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="Key" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="LessonStatus" TypeCode="8" Nullable="true" HasDefault="true" EnumName="LessonStatus"/>' +
        '<Property Name="RawScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MinScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MaxScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ProgressMeasure" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ScaledScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SuccessStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="SuccessStatus"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="CommentFromLearnerItem" ViewFunction="CommentFromLearnerItem$DefaultView">' + 
        '<Property Name="ActivityAttemptId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Property Name="Comment" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Location" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Ordinal" TypeCode="9" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Timestamp" TypeCode="2" Nullable="true" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="CommentFromLmsItem" ViewFunction="CommentFromLmsItem$DefaultView">' + 
        '<Property Name="ActivityPackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="Comment" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Location" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Timestamp" TypeCode="2" Nullable="true" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="CorrectResponseItem" ViewFunction="CorrectResponseItem$DefaultView">' + 
        '<Property Name="InteractionId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="InteractionItem"/>' +
        '<Property Name="ResponsePattern" TypeCode="2" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="EvaluationCommentItem" ViewFunction="EvaluationCommentItem$DefaultView">' + 
        '<Property Name="InteractionId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="InteractionItem"/>' +
        '<Property Name="Comment" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Location" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Ordinal" TypeCode="9" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Timestamp" TypeCode="2" Nullable="true" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="ExtensionDataItem" ViewFunction="ExtensionDataItem$DefaultView">' + 
        '<Property Name="ActivityAttemptId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Property Name="InteractionId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Property Name="AttemptObjectiveId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="AttemptObjectiveItem"/>' +
        '<Property Name="Name" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="AttachmentGuid" TypeCode="11" Nullable="true" HasDefault="true"/>' +
        '<Property Name="AttachmentValue" TypeCode="10" Nullable="true" HasDefault="true"/>' +
        '<Property Name="BoolValue" TypeCode="3" Nullable="true" HasDefault="true"/>' +
        '<Property Name="DateTimeValue" TypeCode="4" Nullable="true" HasDefault="true"/>' +
        '<Property Name="DoubleValue" TypeCode="6" Nullable="true" HasDefault="true"/>' +
        '<Property Name="IntValue" TypeCode="9" Nullable="true" HasDefault="true"/>' +
        '<Property Name="StringValue" TypeCode="2" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="GlobalObjectiveItem" ViewFunction="GlobalObjectiveItem$DefaultView">' + 
        '<Property Name="OrganizationId" TypeCode="1" Nullable="true" HasDefault="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="Key" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Description" TypeCode="2" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="InteractionItem" ViewFunction="InteractionItem$DefaultView">' + 
        '<Property Name="ActivityAttemptId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Property Name="InteractionIdFromCmi" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="InteractionType" TypeCode="8" Nullable="true" HasDefault="true" EnumName="InteractionType"/>' +
        '<Property Name="Timestamp" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Weighting" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ResultState" TypeCode="8" Nullable="true" HasDefault="true" EnumName="InteractionResultState"/>' +
        '<Property Name="ResultNumeric" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Latency" TypeCode="6" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Description" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="LearnerResponseBool" TypeCode="3" Nullable="true" HasDefault="true"/>' +
        '<Property Name="LearnerResponseString" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="LearnerResponseNumeric" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="ScaledScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="RawScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MinScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="MaxScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="EvaluationPoints" TypeCode="5" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="InteractionObjectiveItem" ViewFunction="InteractionObjectiveItem$DefaultView">' + 
        '<Property Name="InteractionId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="InteractionItem"/>' +
        '<Property Name="AttemptObjectiveId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="AttemptObjectiveItem"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="LearnerGlobalObjectiveItem" ViewFunction="LearnerGlobalObjectiveItem$DefaultView">' + 
        '<Property Name="LearnerId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="GlobalObjectiveId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="GlobalObjectiveItem"/>' +
        '<Property Name="ScaledScore" TypeCode="5" Nullable="true" HasDefault="true"/>' +
        '<Property Name="SuccessStatus" TypeCode="8" Nullable="false" HasDefault="true" EnumName="SuccessStatus"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="MapActivityObjectiveToGlobalObjectiveItem" ViewFunction="MapActivityObjectiveToGlobalObjectiveItem$DefaultView">' + 
        '<Property Name="ActivityObjectiveId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="ActivityObjectiveItem"/>' +
        '<Property Name="GlobalObjectiveId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="GlobalObjectiveItem"/>' +
        '<Property Name="ReadSatisfiedStatus" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="ReadNormalizedMeasure" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="WriteSatisfiedStatus" TypeCode="3" Nullable="false" HasDefault="true"/>' +
        '<Property Name="WriteNormalizedMeasure" TypeCode="3" Nullable="false" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="PackageItem" ViewFunction="PackageItem$DefaultView">' + 
        '<Property Name="PackageFormat" TypeCode="8" Nullable="false" HasDefault="false" EnumName="PackageFormat"/>' +
        '<Property Name="Location" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Manifest" TypeCode="7" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Warnings" TypeCode="7" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="RubricItem" ViewFunction="RubricItem$DefaultView">' + 
        '<Property Name="InteractionId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="InteractionItem"/>' +
        '<Property Name="Ordinal" TypeCode="9" Nullable="false" HasDefault="false"/>' +
        '<Property Name="IsSatisfied" TypeCode="3" Nullable="true" HasDefault="true"/>' +
        '<Property Name="Points" TypeCode="5" Nullable="true" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="ResourceItem" ViewFunction="ResourceItem$DefaultView">' + 
        '<Property Name="PackageId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="PackageItem"/>' +
        '<Property Name="ResourceXml" TypeCode="7" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="SequencingLogEntryItem" ViewFunction="SequencingLogEntryItem$DefaultView">' + 
        '<Property Name="AttemptId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="AttemptItem"/>' +
        '<Property Name="ActivityAttemptId" TypeCode="1" Nullable="true" HasDefault="false" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Property Name="EventType" TypeCode="8" Nullable="false" HasDefault="false" EnumName="SequencingEventType"/>' +
        '<Property Name="Message" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="NavigationCommand" TypeCode="8" Nullable="false" HasDefault="false" EnumName="NavigationCommand"/>' +
        '<Property Name="Timestamp" TypeCode="4" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="UserItem" ViewFunction="UserItem$DefaultView">' + 
        '<Property Name="Key" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="Name" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="AudioCaptioning" TypeCode="8" Nullable="false" HasDefault="true" EnumName="AudioCaptioning"/>' +
        '<Property Name="AudioLevel" TypeCode="5" Nullable="false" HasDefault="true"/>' +
        '<Property Name="DeliverySpeed" TypeCode="5" Nullable="false" HasDefault="true"/>' +
        '<Property Name="Language" TypeCode="2" Nullable="false" HasDefault="true"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="UserItemSite" ViewFunction="UserItemSite$DefaultView" DeleteSecurityFunction="UserItemSite$DeleteSecurity">' + 
        '<Property Name="UserId" TypeCode="1" Nullable="true" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="SPSiteGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SPUserId" TypeCode="9" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="SiteSettingsItem" ViewFunction="SiteSettingsItem$DefaultView">' + 
        '<Property Name="SiteGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SettingsXml" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SettingsXmlLastModified" TypeCode="4" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="AssignmentItem" ViewFunction="AssignmentItem$DefaultView" DeleteSecurityFunction="AssignmentItem$DeleteSecurity">' + 
        '<Property Name="SPSiteGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SPWebGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="RootActivityId" TypeCode="1" Nullable="true" HasDefault="false" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Property Name="NonELearningLocation" TypeCode="2" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Title" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="StartDate" TypeCode="4" Nullable="false" HasDefault="false"/>' +
        '<Property Name="DueDate" TypeCode="4" Nullable="true" HasDefault="false"/>' +
        '<Property Name="PointsPossible" TypeCode="5" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Description" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="AutoReturn" TypeCode="3" Nullable="false" HasDefault="false"/>' +
        '<Property Name="EmailChanges" TypeCode="3" Nullable="false" HasDefault="false"/>' +
        '<Property Name="ShowAnswersToLearners" TypeCode="3" Nullable="false" HasDefault="false"/>' +
        '<Property Name="CreatedBy" TypeCode="1" Nullable="true" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="DateCreated" TypeCode="4" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="InstructorAssignmentItem" ViewFunction="InstructorAssignmentItem$DefaultView">' + 
        '<Property Name="AssignmentId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Property Name="InstructorId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="LearnerAssignmentItem" ViewFunction="LearnerAssignmentItem$DefaultView">' + 
        '<Property Name="GuidId" TypeCode="11" Nullable="true" HasDefault="true"/>' +
        '<Property Name="AssignmentId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Property Name="LearnerId" TypeCode="1" Nullable="false" HasDefault="false" ReferencedItemTypeName="UserItem"/>' +
        '<Property Name="IsFinal" TypeCode="3" Nullable="false" HasDefault="false"/>' +
        '<Property Name="NonELearningStatus" TypeCode="8" Nullable="true" HasDefault="false" EnumName="AttemptStatus"/>' +
        '<Property Name="FinalPoints" TypeCode="5" Nullable="true" HasDefault="false"/>' +
        '<Property Name="Grade" TypeCode="2" Nullable="true" HasDefault="true"/>' +
        '<Property Name="InstructorComments" TypeCode="2" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<ItemType Name="UserWebListItem" ViewFunction="UserWebListItem$DefaultView">' + 
        '<Property Name="OwnerKey" TypeCode="2" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SPSiteGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="SPWebGuid" TypeCode="11" Nullable="false" HasDefault="false"/>' +
        '<Property Name="LastAccessTime" TypeCode="4" Nullable="false" HasDefault="false"/>' +
    '</ItemType>'
SET @schema = @schema +
    '<Enum Name="AudioCaptioning">' + 
        '<Value Name="Off" Value="-1"/>' + 
        '<Value Name="NoChange" Value="0"/>' + 
        '<Value Name="On" Value="1"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="CompletionStatus">' + 
        '<Value Name="Unknown" Value="0"/>' + 
        '<Value Name="Completed" Value="1"/>' + 
        '<Value Name="Incomplete" Value="2"/>' + 
        '<Value Name="NotAttempted" Value="3"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="ExitMode">' + 
        '<Value Name="Undetermined" Value="0"/>' + 
        '<Value Name="Logout" Value="1"/>' + 
        '<Value Name="Normal" Value="2"/>' + 
        '<Value Name="TimeOut" Value="3"/>' + 
        '<Value Name="Suspended" Value="4"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="DisplayMode">' + 
        '<Value Name="Normal" Value="0"/>' + 
        '<Value Name="Browse" Value="1"/>' + 
        '<Value Name="Review" Value="2"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="SuccessStatus">' + 
        '<Value Name="Unknown" Value="0"/>' + 
        '<Value Name="Failed" Value="1"/>' + 
        '<Value Name="Passed" Value="2"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="TimeLimitAction">' + 
        '<Value Name="ContinueNoMessage" Value="0"/>' + 
        '<Value Name="ContinueWithMessage" Value="1"/>' + 
        '<Value Name="ExitNoMessage" Value="2"/>' + 
        '<Value Name="ExitWithMessage" Value="3"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="InteractionType">' + 
        '<Value Name="Other" Value="0"/>' + 
        '<Value Name="FillIn" Value="1"/>' + 
        '<Value Name="Likert" Value="2"/>' + 
        '<Value Name="LongFillIn" Value="3"/>' + 
        '<Value Name="Matching" Value="4"/>' + 
        '<Value Name="MultipleChoice" Value="5"/>' + 
        '<Value Name="Numeric" Value="6"/>' + 
        '<Value Name="Performance" Value="7"/>' + 
        '<Value Name="Sequencing" Value="8"/>' + 
        '<Value Name="TrueFalse" Value="9"/>' + 
        '<Value Name="Essay" Value="10"/>' + 
        '<Value Name="Attachment" Value="11"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="InteractionResultState">' + 
        '<Value Name="None" Value="0"/>' + 
        '<Value Name="Correct" Value="1"/>' + 
        '<Value Name="Incorrect" Value="2"/>' + 
        '<Value Name="Neutral" Value="3"/>' + 
        '<Value Name="Numeric" Value="4"/>' + 
        '<Value Name="Unanticipated" Value="5"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="PackageFormat">' + 
        '<Value Name="Lrm" Value="0"/>' + 
        '<Value Name="V1p2" Value="1"/>' + 
        '<Value Name="V1p3" Value="2"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="LessonStatus">' + 
        '<Value Name="NotAttempted" Value="0"/>' + 
        '<Value Name="Browsed" Value="1"/>' + 
        '<Value Name="Completed" Value="2"/>' + 
        '<Value Name="Failed" Value="3"/>' + 
        '<Value Name="Incomplete" Value="4"/>' + 
        '<Value Name="Passed" Value="5"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="NavigationCommand">' + 
        '<Value Name="None" Value="0"/>' + 
        '<Value Name="Abandon" Value="1"/>' + 
        '<Value Name="AbandonAll" Value="2"/>' + 
        '<Value Name="ChoiceStart" Value="3"/>' + 
        '<Value Name="Choose" Value="4"/>' + 
        '<Value Name="Continue" Value="5"/>' + 
        '<Value Name="ExitAll" Value="6"/>' + 
        '<Value Name="Previous" Value="7"/>' + 
        '<Value Name="ResumeAll" Value="8"/>' + 
        '<Value Name="Start" Value="9"/>' + 
        '<Value Name="SuspendAll" Value="10"/>' + 
        '<Value Name="UnqualifiedExit" Value="11"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="SequencingEventType">' + 
        '<Value Name="FinalNavigation" Value="0"/>' + 
        '<Value Name="IntermediateNavigation" Value="1"/>' + 
        '<Value Name="Rollup" Value="2"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="AttemptStatus">' + 
        '<Value Name="Active" Value="0"/>' + 
        '<Value Name="Abandoned" Value="1"/>' + 
        '<Value Name="Completed" Value="2"/>' + 
        '<Value Name="Suspended" Value="3"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<Enum Name="LearnerAssignmentState">' + 
        '<Value Name="NotStarted" Value="0"/>' + 
        '<Value Name="Active" Value="1"/>' + 
        '<Value Name="Completed" Value="2"/>' + 
        '<Value Name="Final" Value="3"/>' + 
    '</Enum>'
SET @schema = @schema +
    '<View Name="SeqNavOrganizationGlobalObjectiveView" Function="SeqNavOrganizationGlobalObjectiveView">' + 
        '<Column Name="OrganizationId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="Key" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="ScaledScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="SuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavLearnerGlobalObjectiveView" Function="SeqNavLearnerGlobalObjectiveView">' + 
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="Key" TypeCode="2" Nullable="true"/>' +
        '<Column Name="ScaledScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="SuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavActivityPackageView" Function="SeqNavActivityPackageView">' + 
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackagePath" TypeCode="2" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavActivityTreeView" Function="SeqNavActivityTreeView">' + 
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="ParentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="DataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="ObjectivesGlobalToSystem" TypeCode="3" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptView" Function="SeqNavAttemptView">' + 
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="LogDetailSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="LogFinalSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="LogRollup" TypeCode="3" Nullable="true"/>' +
        '<Column Name="CurrentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="SuspendedActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackagePath" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="LearnerName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerAudioCaptioning" TypeCode="8" Nullable="true" EnumName="AudioCaptioning"/>' +
        '<Column Name="LearnerAudioLevel" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerDeliverySpeed" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerLanguage" TypeCode="2" Nullable="true"/>' +
        '<Column Name="StartedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="FinishedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="TotalPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="SuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
        '<Column Name="CompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavActivityAttemptView" Function="SeqNavActivityAttemptView">' + 
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="DataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="SequencingDataCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="RandomPlacement" TypeCode="9" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ActivityPackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="StaticDataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="ParentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="ObjectivesGlobalToSystem" TypeCode="3" Nullable="true"/>' +
        '<Column Name="CompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
        '<Column Name="AttemptCount" TypeCode="9" Nullable="true"/>' +
        '<Column Name="EvaluationPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Exit" TypeCode="8" Nullable="true" EnumName="ExitMode"/>' +
        '<Column Name="LessonStatus" TypeCode="8" Nullable="true" EnumName="LessonStatus"/>' +
        '<Column Name="Location" TypeCode="2" Nullable="true"/>' +
        '<Column Name="MinScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="MaxScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ProgressMeasure" TypeCode="5" Nullable="true"/>' +
        '<Column Name="RawScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ScaledScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="SessionStartTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="SessionTime" TypeCode="6" Nullable="true"/>' +
        '<Column Name="SuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
        '<Column Name="SuspendData" TypeCode="2" Nullable="true"/>' +
        '<Column Name="TotalTime" TypeCode="6" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavCurrentActivityAttemptView" Function="SeqNavCurrentActivityAttemptView">' + 
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="DataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="SequencingDataCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="RandomPlacement" TypeCode="9" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="StaticDataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="ObjectivesGlobalToSystem" TypeCode="3" Nullable="true"/>' +
        '<Column Name="Credit" TypeCode="3" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavCurrentCommentFromLmsView" Function="SeqNavCurrentCommentFromLmsView">' + 
        '<Column Name="Comment" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Location" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Timestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="DataModelCache" TypeCode="7" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptObjectiveView" Function="SeqNavAttemptObjectiveView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptObjectiveId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptObjectiveItem"/>' +
        '<Column Name="ActivityAttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="CompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
        '<Column Name="Description" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IsPrimaryObjective" TypeCode="3" Nullable="true"/>' +
        '<Column Name="Key" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LessonStatus" TypeCode="8" Nullable="true" EnumName="LessonStatus"/>' +
        '<Column Name="RawScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="MinScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="MaxScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ProgressMeasure" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ScaledScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="SuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptCommentFromLearnerView" Function="SeqNavAttemptCommentFromLearnerView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ActivityAttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="CommentFromLearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="CommentFromLearnerItem"/>' +
        '<Column Name="Comment" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Location" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Timestamp" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Ordinal" TypeCode="9" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptInteractionView" Function="SeqNavAttemptInteractionView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ActivityAttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="InteractionIdFromCmi" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InteractionType" TypeCode="8" Nullable="true" EnumName="InteractionType"/>' +
        '<Column Name="Timestamp" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Weighting" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ResultState" TypeCode="8" Nullable="true" EnumName="InteractionResultState"/>' +
        '<Column Name="ResultNumeric" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Latency" TypeCode="6" Nullable="true"/>' +
        '<Column Name="Description" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerResponseBool" TypeCode="3" Nullable="true"/>' +
        '<Column Name="LearnerResponseString" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerResponseNumeric" TypeCode="5" Nullable="true"/>' +
        '<Column Name="ScaledScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="RawScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="MinScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="MaxScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="EvaluationPoints" TypeCode="5" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptEvaluationCommentLearnerView" Function="SeqNavAttemptEvaluationCommentLearnerView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="EvaluationCommentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="EvaluationCommentItem"/>' +
        '<Column Name="Comment" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Location" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Timestamp" TypeCode="2" Nullable="true"/>' +
        '<Column Name="Ordinal" TypeCode="9" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptCorrectResponseView" Function="SeqNavAttemptCorrectResponseView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="CorrectResponseId" TypeCode="1" Nullable="true" ReferencedItemTypeName="CorrectResponseItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="ResponsePattern" TypeCode="2" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptInteractionObjectiveView" Function="SeqNavAttemptInteractionObjectiveView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="InteractionObjectiveId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionObjectiveItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="AttemptObjectiveId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptObjectiveItem"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptExtensionDataView" Function="SeqNavAttemptExtensionDataView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ActivityAttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityAttemptItem"/>' +
        '<Column Name="ExtensionDataId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ExtensionDataItem"/>' +
        '<Column Name="Name" TypeCode="2" Nullable="true"/>' +
        '<Column Name="StringValue" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IntValue" TypeCode="9" Nullable="true"/>' +
        '<Column Name="BoolValue" TypeCode="3" Nullable="true"/>' +
        '<Column Name="DoubleValue" TypeCode="6" Nullable="true"/>' +
        '<Column Name="DateTimeValue" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttachmentGuid" TypeCode="11" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptObjectiveExtensionDataView" Function="SeqNavAttemptObjectiveExtensionDataView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ExtensionDataId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ExtensionDataItem"/>' +
        '<Column Name="AttemptObjectiveId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptObjectiveItem"/>' +
        '<Column Name="Name" TypeCode="2" Nullable="true"/>' +
        '<Column Name="StringValue" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IntValue" TypeCode="9" Nullable="true"/>' +
        '<Column Name="BoolValue" TypeCode="3" Nullable="true"/>' +
        '<Column Name="DoubleValue" TypeCode="6" Nullable="true"/>' +
        '<Column Name="DateTimeValue" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttachmentGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AttachmentValue" TypeCode="10" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptInteractionExtensionDataView" Function="SeqNavAttemptInteractionExtensionDataView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="ExtensionDataId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ExtensionDataItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="Name" TypeCode="2" Nullable="true"/>' +
        '<Column Name="StringValue" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IntValue" TypeCode="9" Nullable="true"/>' +
        '<Column Name="BoolValue" TypeCode="3" Nullable="true"/>' +
        '<Column Name="DoubleValue" TypeCode="6" Nullable="true"/>' +
        '<Column Name="DateTimeValue" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttachmentGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AttachmentValue" TypeCode="10" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="SeqNavAttemptRubricView" Function="SeqNavAttemptRubricView">' + 
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="RubricItemId" TypeCode="1" Nullable="true" ReferencedItemTypeName="RubricItem"/>' +
        '<Column Name="InteractionId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InteractionItem"/>' +
        '<Column Name="Ordinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="IsSatisfied" TypeCode="3" Nullable="true"/>' +
        '<Column Name="Points" TypeCode="5" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="LearnerAssignmentView" Function="LearnerAssignmentView">' + 
        '<Column Name="LearnerAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="LearnerAssignmentItem"/>' +
        '<Column Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptGradedPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerAssignmentState" TypeCode="8" Nullable="true" EnumName="LearnerAssignmentState"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="AssignmentPropertiesView" Function="AssignmentPropertiesView" SecurityFunction="AssignmentPropertiesView$Security">' + 
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentDateCreated" TypeCode="4" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Parameter Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Parameter Name="IsInstructor" TypeCode="3" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="AssignmentListForInstructors" Function="AssignmentListForInstructors" SecurityFunction="AssignmentListForInstructors$Security">' + 
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentDateCreated" TypeCode="4" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="CountTotal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotStarted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountActive" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountCompleted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountStarted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotStartedOrActive" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountCompletedOrFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="MinGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MaxGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="AvgGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MinFinalPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MaxFinalPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="AvgFinalPoints" TypeCode="6" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="InstructorAssignmentListForInstructors" Function="InstructorAssignmentListForInstructors" SecurityFunction="InstructorAssignmentListForInstructors$Security">' + 
        '<Column Name="InstructorAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InstructorAssignmentItem"/>' +
        '<Column Name="InstructorId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="InstructorName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InstructorKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageManifest" TypeCode="7" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="InstructorAssignmentList" Function="InstructorAssignmentList" SecurityFunction="InstructorAssignmentList$Security">' + 
        '<Column Name="InstructorAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="InstructorAssignmentItem"/>' +
        '<Column Name="InstructorId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="InstructorName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InstructorKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="SPUserId" TypeCode="9" Nullable="true"/>' +
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageManifest" TypeCode="7" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="LearnerAssignmentListForLearners" Function="LearnerAssignmentListForLearners" SecurityFunction="LearnerAssignmentListForLearners$Security">' + 
        '<Column Name="LearnerAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="LearnerAssignmentItem"/>' +
        '<Column Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="LearnerName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="SPUserId" TypeCode="9" Nullable="true"/>' +
        '<Column Name="IsFinal" TypeCode="3" Nullable="true"/>' +
        '<Column Name="NonELearningStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="FinalPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Grade" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InstructorComments" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentDateCreated" TypeCode="4" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptCurrentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptSuspendedActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="AttemptFinishedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptLogDetailSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogFinalSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogRollup" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptStartedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptCompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
        '<Column Name="AttemptSuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
        '<Column Name="AttemptGradedPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerAssignmentState" TypeCode="8" Nullable="true" EnumName="LearnerAssignmentState"/>' +
        '<Column Name="HasInstructors" TypeCode="3" Nullable="true"/>' +
        '<Column Name="FileSubmissionState" TypeCode="2" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="LearnerAssignmentListForObservers" Function="LearnerAssignmentListForObservers" SecurityFunction="LearnerAssignmentListForObservers$Security">' + 
        '<Column Name="LearnerAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="LearnerAssignmentItem"/>' +
        '<Column Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="LearnerName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IsFinal" TypeCode="3" Nullable="true"/>' +
        '<Column Name="NonELearningStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="FinalPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Grade" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InstructorComments" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentDateCreated" TypeCode="4" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="CountTotal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotStarted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountActive" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountCompleted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountStarted" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotStartedOrActive" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountCompletedOrFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="CountNotFinal" TypeCode="9" Nullable="true"/>' +
        '<Column Name="MinGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MaxGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="AvgGradedPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MinFinalPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="MaxFinalPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="AvgFinalPoints" TypeCode="6" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptCurrentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptSuspendedActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="AttemptFinishedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptLogDetailSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogFinalSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogRollup" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptStartedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptCompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
        '<Column Name="AttemptSuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
        '<Column Name="AttemptGradedPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerAssignmentState" TypeCode="8" Nullable="true" EnumName="LearnerAssignmentState"/>' +
        '<Column Name="HasInstructors" TypeCode="3" Nullable="true"/>' +
        '<Column Name="FileSubmissionState" TypeCode="2" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="LearnerAssignmentListForInstructors" Function="LearnerAssignmentListForInstructors" SecurityFunction="LearnerAssignmentListForInstructors$Security">' + 
        '<Column Name="LearnerAssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="LearnerAssignmentItem"/>' +
        '<Column Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
        '<Column Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="SPUserId" TypeCode="9" Nullable="true"/>' +
        '<Column Name="LearnerName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="LearnerKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="IsFinal" TypeCode="3" Nullable="true"/>' +
        '<Column Name="NonELearningStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="FinalPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Grade" TypeCode="2" Nullable="true"/>' +
        '<Column Name="InstructorComments" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AssignmentItem"/>' +
        '<Column Name="AssignmentSPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentSPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="AssignmentNonELearningLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentTitle" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentStartDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentDueDate" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AssignmentPointsPossible" TypeCode="5" Nullable="true"/>' +
        '<Column Name="AssignmentDescription" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentAutoReturn" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentEmailChanges" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentShowAnswersToLearners" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedById" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
        '<Column Name="AssignmentCreatedByName" TypeCode="2" Nullable="true"/>' +
        '<Column Name="AssignmentCreatedByKey" TypeCode="2" Nullable="true"/>' +
        '<Column Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageManifest" TypeCode="7" Nullable="true"/>' +
        '<Column Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
        '<Column Name="AttemptCurrentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptSuspendedActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="AttemptStatus" TypeCode="8" Nullable="true" EnumName="AttemptStatus"/>' +
        '<Column Name="AttemptFinishedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptLogDetailSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogFinalSequencing" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptLogRollup" TypeCode="3" Nullable="true"/>' +
        '<Column Name="AttemptStartedTimestamp" TypeCode="4" Nullable="true"/>' +
        '<Column Name="AttemptCompletionStatus" TypeCode="8" Nullable="true" EnumName="CompletionStatus"/>' +
        '<Column Name="AttemptSuccessStatus" TypeCode="8" Nullable="true" EnumName="SuccessStatus"/>' +
        '<Column Name="AttemptGradedPoints" TypeCode="5" Nullable="true"/>' +
        '<Column Name="LearnerAssignmentState" TypeCode="8" Nullable="true" EnumName="LearnerAssignmentState"/>' +
        '<Column Name="HasInstructors" TypeCode="3" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="UserWebList" Function="UserWebList" SecurityFunction="UserWebList$Security">' + 
        '<Column Name="SPSiteGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="SPWebGuid" TypeCode="11" Nullable="true"/>' +
        '<Column Name="LastAccessTime" TypeCode="4" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<View Name="ActivityPackageItemView" Function="ActivityPackageItemView">' + 
        '<Column Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
        '<Column Name="PackageFormat" TypeCode="8" Nullable="true" EnumName="PackageFormat"/>' +
        '<Column Name="PackageLocation" TypeCode="2" Nullable="true"/>' +
        '<Column Name="PackageManifest" TypeCode="7" Nullable="true"/>' +
        '<Column Name="Id" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="ActivityIdFromManifest" TypeCode="2" Nullable="true"/>' +
        '<Column Name="OriginalPlacement" TypeCode="9" Nullable="true"/>' +
        '<Column Name="ParentActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Column Name="PrimaryObjectiveId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityObjectiveItem"/>' +
        '<Column Name="ResourceId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ResourceItem"/>' +
        '<Column Name="PrimaryResourceIdFromManifest" TypeCode="2" Nullable="true"/>' +
        '<Column Name="CompletionThreshold" TypeCode="5" Nullable="true"/>' +
        '<Column Name="Credit" TypeCode="3" Nullable="true"/>' +
        '<Column Name="IsVisibleInContents" TypeCode="3" Nullable="true"/>' +
        '<Column Name="LaunchData" TypeCode="2" Nullable="true"/>' +
        '<Column Name="MaxAttempts" TypeCode="9" Nullable="true"/>' +
        '<Column Name="MaxTimeAllowed" TypeCode="6" Nullable="true"/>' +
        '<Column Name="ResourceParameters" TypeCode="2" Nullable="true"/>' +
        '<Column Name="ScaledPassingScore" TypeCode="5" Nullable="true"/>' +
        '<Column Name="TimeLimitAction" TypeCode="8" Nullable="true" EnumName="TimeLimitAction"/>' +
        '<Column Name="Title" TypeCode="2" Nullable="true"/>' +
        '<Column Name="ObjectivesGlobalToSystem" TypeCode="3" Nullable="true"/>' +
    '</View>'
SET @schema = @schema +
    '<Right Name="AddPackageReferenceRight">' + 
    '</Right>'
SET @schema = @schema +
    '<Right Name="RemovePackageReferenceRight">' + 
        '<Parameter Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="ReadPackageRight">' + 
        '<Parameter Name="PackageId" TypeCode="1" Nullable="true" ReferencedItemTypeName="PackageItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="CreateAttemptRight">' + 
        '<Parameter Name="RootActivityId" TypeCode="1" Nullable="true" ReferencedItemTypeName="ActivityPackageItem"/>' +
        '<Parameter Name="LearnerId" TypeCode="1" Nullable="true" ReferencedItemTypeName="UserItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="DeleteAttemptRight">' + 
        '<Parameter Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="ExecuteSessionRight" SecurityFunction="ExecuteSessionRight">' + 
        '<Parameter Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="ReviewSessionRight" SecurityFunction="ReviewSessionRight">' + 
        '<Parameter Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="RandomAccessSessionRight" SecurityFunction="RandomAccessSessionRight">' + 
        '<Parameter Name="AttemptId" TypeCode="1" Nullable="true" ReferencedItemTypeName="AttemptItem"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="StartAttemptOnLearnerAssignmentRight" SecurityFunction="StartAttemptOnLearnerAssignmentRight">' + 
        '<Parameter Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="FinishLearnerAssignmentRight" SecurityFunction="FinishLearnerAssignmentRight">' + 
        '<Parameter Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="CompleteLearnerAssignmentRight" SecurityFunction="CompleteLearnerAssignmentRight">' + 
        '<Parameter Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="FinalizeLearnerAssignmentRight" SecurityFunction="FinalizeLearnerAssignmentRight">' + 
        '<Parameter Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
    '</Right>'
SET @schema = @schema +
    '<Right Name="ActivateLearnerAssignmentRight" SecurityFunction="ActivateLearnerAssignmentRight">' + 
        '<Parameter Name="LearnerAssignmentGuidId" TypeCode="11" Nullable="true"/>' +
    '</Right>'
SET @schema = @schema + '</StoreSchema>'
DELETE FROM Configuration
INSERT INTO Configuration (
    EngineVersion,
    SchemaDefinition
) VALUES (
    1,@schema
)
GO

ALTER FUNCTION [LearnerAssignmentView](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from LearnerAssignmentItem -----
    lai.Id                          LearnerAssignmentId,
    lai.GuidId			LearnerAssignmentGuidId,
    lai.LearnerId                   LearnerId,
    ----- from AssignmentItem -----
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.RootActivityId              RootActivityId,
    ----- from AttemptItem -----
    ati.Id AttemptId,
    ati.TotalPoints                 AttemptGradedPoints,
    ----- computed LearnerAssignmentState -----
    dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) LearnerAssignmentState
    -----
    FROM LearnerAssignmentItem lai
    INNER JOIN AssignmentItem asi ON lai.AssignmentId = asi.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
)
GO

ALTER FUNCTION [AssignmentPropertiesView](@UserKey nvarchar(250),@AssignmentId bigint=NULL,@IsInstructor bit=NULL)
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    asi.DateCreated                 AssignmentDateCreated,
    asi.RootActivityId              RootActivityId,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation
    -----
    FROM AssignmentItem asi
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki ON api.PackageId = pki.Id
    WHERE asi.Id = @AssignmentId
)
GO

ALTER FUNCTION [AssignmentListForInstructors](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    asi.DateCreated                 AssignmentDateCreated,
    asi.RootActivityId              RootActivityId,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    ----- from learner assignment computed info -----
    ISNULL(la.CountTotal,0)         CountTotal,
    ISNULL(la.CountNotStarted,0)    CountNotStarted,
    ISNULL(la.CountActive,0)        CountActive,
    ISNULL(la.CountCompleted,0)     CountCompleted,
    ISNULL(la.CountFinal,0)         CountFinal,
    ISNULL(la.CountStarted,0)       CountStarted,
    ISNULL(la.CountNotStartedOrActive,0) CountNotStartedOrActive,
    ISNULL(la.CountCompletedOrFinal,0) CountCompletedOrFinal,
    ISNULL(la.CountNotFinal,0)      CountNotFinal,
    la.MinGradedPoints              MinGradedPoints,
    la.MaxGradedPoints              MaxGradedPoints,
    la.AvgGradedPoints              AvgGradedPoints,
    la.MinFinalPoints               MinFinalPoints,
    la.MaxFinalPoints               MaxFinalPoints,
    la.AvgFinalPoints               AvgFinalPoints
    -----
    FROM AssignmentItem asi
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    INNER JOIN InstructorAssignmentItem iai ON asi.Id = iai.AssignmentId
    INNER JOIN UserItem iui ON iai.InstructorId = iui.Id
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki ON api.PackageId = pki.Id
    LEFT OUTER JOIN (
    SELECT
    ----- from Assignment -----
    lai.AssignmentId,
    ----- computed CountTotal -----
    COUNT(lai.Id) CountTotal,
    ----- computed CountNotStarted -----
    COUNT(CASE WHEN dbo.IsLearnerAssignmentStarted(asi.RootActivityId, lai.NonELearningStatus, ati.AttemptStatus) = 0 THEN 1 ELSE NULL END) CountNotStarted,
    ----- computed CountActive -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) = 1 THEN 1 ELSE NULL END) CountActive,
    ----- computed CountCompleted -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) = 2 THEN 1 ELSE NULL END) CountCompleted,
    ----- computed CountFinal -----
    COUNT(CASE WHEN lai.IsFinal = 1 THEN 1 ELSE NULL END) CountFinal,
    ----- computed CountStarted -----
    COUNT(CASE WHEN dbo.IsLearnerAssignmentStarted(asi.RootActivityId, lai.NonELearningStatus, ati.AttemptStatus) <> 0 THEN 1 ELSE NULL END) CountStarted,
    ----- computed CountNotStartedOrActive -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) IN (0,1) THEN 1 ELSE NULL END) CountNotStartedOrActive,
    ----- computed CountCompletedOrFinal -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) IN (2,3) THEN 1 ELSE NULL END) CountCompletedOrFinal,
    ----- computed CountNotFinal -----
    COUNT(CASE WHEN lai.IsFinal = 0 THEN 1 ELSE NULL END) CountNotFinal,
    ----- computed MinGradedPoints -----
    MIN(ati.TotalPoints)              MinGradedPoints,
    ----- computed MaxGradedPoints -----
    MAX(ati.TotalPoints)              MaxGradedPoints,
    ----- computed AvgGradedPoints -----
    AVG(ati.TotalPoints)              AvgGradedPoints,
    ----- computed MinFinalPoints -----
    MIN(lai.FinalPoints)              MinFinalPoints,
    ----- computed MaxFinalPoints -----
    MAX(lai.FinalPoints)              MaxFinalPoints,
    ----- computed AvgFinalPoints -----
    AVG(lai.FinalPoints)              AvgFinalPoints
    -----
    FROM LearnerAssignmentItem lai
    INNER JOIN AssignmentItem asi ON lai.AssignmentId = asi.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
    INNER JOIN InstructorAssignmentItem iai ON asi.Id = iai.AssignmentId
    INNER JOIN UserItem iui ON iai.InstructorId = iui.Id
    WHERE iui.[Key] = @UserKey
    GROUP BY lai.AssignmentId) AS la ON asi.Id = la.AssignmentId
    WHERE iui.[Key] = @UserKey
)
GO

ALTER FUNCTION [InstructorAssignmentListForInstructors](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from InstructorAssignmentItem -----
    iai.Id                          InstructorAssignmentId,
    iai.InstructorId                InstructorId,
    iui.[Name]                      InstructorName,
    iui.[Key]                       InstructorKey,
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    pki.Manifest                    PackageManifest
    -----
    FROM InstructorAssignmentItem iai
    INNER JOIN AssignmentItem asi ON iai.AssignmentId = asi.Id
    INNER JOIN UserItem iui ON iui.Id = iai.InstructorId
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki on api.PackageId = pki.Id
    WHERE EXISTS
    (
    SELECT *
    FROM InstructorAssignmentItem iai2
    INNER JOIN UserItem ui2
    ON iai2.InstructorId = ui2.Id
    WHERE iai2.AssignmentId = asi.Id
    AND ui2.[Key] = @UserKey
    )
)
GO

ALTER FUNCTION [InstructorAssignmentList](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from InstructorAssignmentItem -----
    iai.Id                          InstructorAssignmentId,
    iai.InstructorId                InstructorId,
    iui.[Name]                      InstructorName,
    iui.[Key]                       InstructorKey,
    luis.SPUserId                   SPUserId,
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    pki.Manifest                    PackageManifest
    -----
    FROM InstructorAssignmentItem iai
    INNER JOIN AssignmentItem asi ON iai.AssignmentId = asi.Id
    INNER JOIN UserItem iui ON iui.Id = iai.InstructorId
    LEFT JOIN UserItemSite luis ON iui.Id = luis.UserId AND
                                    asi.SPSiteGuid = luis.SPSiteGuid
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki on api.PackageId = pki.Id
)
GO

ALTER FUNCTION [LearnerAssignmentListForLearners](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from LearnerAssignmentItem -----
    lai.Id                          LearnerAssignmentId,
    lai.GuidId			LearnerAssignmentGuidId,
    lai.LearnerId                   LearnerId,
    lui.[Name]                      LearnerName,
    lui.[Key]                       LearnerKey,
    luis.SPUserId                   SPUserId,
    lai.IsFinal                     IsFinal,
    lai.NonELearningStatus          NonELearningStatus,
    CASE WHEN lai.IsFinal = 1 THEN lai.FinalPoints ELSE NULL END FinalPoints,
    CASE WHEN lai.IsFinal = 1 THEN lai.Grade ELSE NULL END Grade,
    lai.InstructorComments          InstructorComments,
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    asi.DateCreated                 AssignmentDateCreated,
    asi.RootActivityId              RootActivityId,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    ----- from AttemptItem -----
    ati.Id AttemptId,
    ati.CurrentActivityId           AttemptCurrentActivityId,
    ati.SuspendedActivityId         AttemptSuspendedActivityId,
    ati.AttemptStatus               AttemptStatus,
    ati.FinishedTimestamp           AttemptFinishedTimestamp,
    ati.LogDetailSequencing         AttemptLogDetailSequencing,
    ati.LogFinalSequencing          AttemptLogFinalSequencing,
    ati.LogRollup                   AttemptLogRollup,
    ati.StartedTimestamp            AttemptStartedTimestamp,
    ati.CompletionStatus            AttemptCompletionStatus,
    ati.SuccessStatus               AttemptSuccessStatus,
    ati.TotalPoints                 AttemptGradedPoints,
    ----- computed FileSubmissionState -----
    dbo.GetLearnerFileSubmissionState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus) FileSubmissionState,
    ----- computed LearnerAssignmentState -----
    dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) LearnerAssignmentState,
    ----- computed HasInstructors -----
    CASE WHEN EXISTS
    (
    SELECT *
    FROM InstructorAssignmentItem iaiH
    WHERE iaiH.AssignmentId = asi.Id
    ) THEN 1 ELSE 0 END             HasInstructors
    -----
    FROM LearnerAssignmentItem lai
    INNER JOIN AssignmentItem asi ON lai.AssignmentId = asi.Id
    INNER JOIN UserItem lui ON lui.Id = lai.LearnerId
    LEFT JOIN UserItemSite luis ON lui.Id = luis.UserId AND
                                    asi.SPSiteGuid = luis.SPSiteGuid
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki on api.PackageId = pki.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
    WHERE lui.[Key] = @UserKey
    AND GETUTCDATE() >= asi.StartDate
)
GO

ALTER FUNCTION [LearnerAssignmentListForObservers](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from LearnerAssignmentItem -----
    
    lai.Id                          LearnerAssignmentId,
    lai.GuidId                      LearnerAssignmentGuidId,
    lai.LearnerId                   LearnerId,
    lui.[Name]                      LearnerName,
    lui.[Key]                       LearnerKey,
    lai.IsFinal                     IsFinal,
    lai.NonELearningStatus          NonELearningStatus,
    CASE WHEN lai.IsFinal = 1 THEN lai.FinalPoints ELSE NULL END FinalPoints,
    CASE WHEN lai.IsFinal = 1 THEN lai.Grade ELSE NULL END Grade,
    lai.InstructorComments          InstructorComments,
    
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    asi.DateCreated                 AssignmentDateCreated,
    asi.RootActivityId              RootActivityId,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    ----- from learner assignment computed info -----
    ISNULL(la.CountTotal,0)         CountTotal,
    ISNULL(la.CountNotStarted,0)    CountNotStarted,
    ISNULL(la.CountActive,0)        CountActive,
    ISNULL(la.CountCompleted,0)     CountCompleted,
    ISNULL(la.CountFinal,0)         CountFinal,
    ISNULL(la.CountStarted,0)       CountStarted,
    ISNULL(la.CountNotStartedOrActive,0) CountNotStartedOrActive,
    ISNULL(la.CountCompletedOrFinal,0) CountCompletedOrFinal,
    ISNULL(la.CountNotFinal,0)      CountNotFinal,
    la.MinGradedPoints              MinGradedPoints,
    la.MaxGradedPoints              MaxGradedPoints,
    la.AvgGradedPoints              AvgGradedPoints,
    la.MinFinalPoints               MinFinalPoints,
    la.MaxFinalPoints               MaxFinalPoints,
    la.AvgFinalPoints               AvgFinalPoints,
    ----- from AttemptItem -----
    ati.Id AttemptId,
    ati.CurrentActivityId           AttemptCurrentActivityId,
    ati.SuspendedActivityId         AttemptSuspendedActivityId,
    ati.AttemptStatus               AttemptStatus,
    ati.FinishedTimestamp           AttemptFinishedTimestamp,
    ati.LogDetailSequencing         AttemptLogDetailSequencing,
    ati.LogFinalSequencing          AttemptLogFinalSequencing,
    ati.LogRollup                   AttemptLogRollup,
    ati.StartedTimestamp            AttemptStartedTimestamp,
    ati.CompletionStatus            AttemptCompletionStatus,
    ati.SuccessStatus               AttemptSuccessStatus,
    ati.TotalPoints                 AttemptGradedPoints,
    ----- computed FileSubmissionState -----
    dbo.GetObserverFileSubmissionState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus) FileSubmissionState,
    ----- computed LearnerAssignmentState -----
    dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) LearnerAssignmentState,
    ----- computed HasInstructors -----
    CASE WHEN EXISTS
    (
    SELECT *
    FROM InstructorAssignmentItem iaiH
    WHERE iaiH.AssignmentId = asi.Id
    ) THEN 1 ELSE 0 END             HasInstructors
    ------------------------------------
    FROM AssignmentItem asi
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    INNER JOIN LearnerAssignmentItem lai ON asi.Id = lai.AssignmentId
    INNER JOIN UserItem lui ON lai.LearnerId = lui.Id
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki ON api.PackageId = pki.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
    INNER JOIN (
    SELECT
    ----- from Assignment -----
    lai.AssignmentId,
    ----- computed CountTotal -----
    COUNT(lai.Id) CountTotal,
    ----- computed CountNotStarted -----
    COUNT(CASE WHEN dbo.IsLearnerAssignmentStarted(asi.RootActivityId, lai.NonELearningStatus, ati.AttemptStatus) = 0 THEN 1 ELSE NULL END) CountNotStarted,
    ----- computed CountActive -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) = 1 THEN 1 ELSE NULL END) CountActive,
    ----- computed CountCompleted -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) = 2 THEN 1 ELSE NULL END) CountCompleted,
    ----- computed CountFinal -----
    COUNT(CASE WHEN lai.IsFinal = 1 THEN 1 ELSE NULL END) CountFinal,
    ----- computed CountStarted -----
    COUNT(CASE WHEN dbo.IsLearnerAssignmentStarted(asi.RootActivityId, lai.NonELearningStatus, ati.AttemptStatus) <> 0 THEN 1 ELSE NULL END) CountStarted,
    ----- computed CountNotStartedOrActive -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) IN (0,1) THEN 1 ELSE NULL END) CountNotStartedOrActive,
    ----- computed CountCompletedOrFinal -----
    COUNT(CASE WHEN dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) IN (2,3) THEN 1 ELSE NULL END) CountCompletedOrFinal,
    ----- computed CountNotFinal -----
    COUNT(CASE WHEN lai.IsFinal = 0 THEN 1 ELSE NULL END) CountNotFinal,
    ----- computed MinGradedPoints -----
    MIN(ati.TotalPoints)              MinGradedPoints,
    ----- computed MaxGradedPoints -----
    MAX(ati.TotalPoints)              MaxGradedPoints,
    ----- computed AvgGradedPoints -----
    AVG(ati.TotalPoints)              AvgGradedPoints,
    ----- computed MinFinalPoints -----
    MIN(lai.FinalPoints)              MinFinalPoints,
    ----- computed MaxFinalPoints -----
    MAX(lai.FinalPoints)              MaxFinalPoints,
    ----- computed AvgFinalPoints -----
    AVG(lai.FinalPoints)              AvgFinalPoints
    -----
    FROM LearnerAssignmentItem lai
    INNER JOIN AssignmentItem asi ON lai.AssignmentId = asi.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
    INNER JOIN LearnerAssignmentItem iai ON asi.Id = iai.AssignmentId
    INNER JOIN UserItem iui ON iai.LearnerId = iui.Id
    GROUP BY lai.AssignmentId) AS la ON asi.Id = la.AssignmentId
    WHERE lui.[Key] = @UserKey
)
GO

ALTER FUNCTION [LearnerAssignmentListForInstructors](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT
    ----- from LearnerAssignmentItem -----
    lai.Id                          LearnerAssignmentId,
    lai.GuidId			LearnerAssignmentGuidId,
    lai.LearnerId                   LearnerId,
    lui.[Name]                      LearnerName,
    lui.[Key]                       LearnerKey,
    luis.SPUserId                   SPUserId,
    lai.IsFinal                     IsFinal,
    lai.NonELearningStatus          NonELearningStatus,
    lai.FinalPoints                 FinalPoints,
    lai.Grade                       Grade,
    lai.InstructorComments          InstructorComments,
    ----- from AssignmentItem -----
    asi.Id                          AssignmentId,
    asi.SPSiteGuid                  AssignmentSPSiteGuid,
    asi.SPWebGuid                   AssignmentSPWebGuid,
    asi.NonELearningLocation        AssignmentNonELearningLocation,
    asi.Title                       AssignmentTitle,
    asi.StartDate                   AssignmentStartDate,
    asi.DueDate                     AssignmentDueDate,
    asi.PointsPossible              AssignmentPointsPossible,
    asi.Description                 AssignmentDescription,
    asi.AutoReturn                  AssignmentAutoReturn,
    asi.EmailChanges                AssignmentEmailChanges,
    asi.ShowAnswersToLearners       AssignmentShowAnswersToLearners,
    asi.CreatedBy                   AssignmentCreatedById,
    cbui.[Name]                     AssignmentCreatedByName,
    cbui.[Key]                      AssignmentCreatedByKey,
    asi.RootActivityId              RootActivityId,
    ----- from PackageItem -----
    pki.Id                          PackageId,
    pki.PackageFormat               PackageFormat,
    pki.Location                    PackageLocation,
    pki.Manifest                    PackageManifest,
    ----- from AttemptItem -----
    ati.Id AttemptId,
    ati.CurrentActivityId           AttemptCurrentActivityId,
    ati.SuspendedActivityId         AttemptSuspendedActivityId,
    ati.AttemptStatus               AttemptStatus,
    ati.FinishedTimestamp           AttemptFinishedTimestamp,
    ati.LogDetailSequencing         AttemptLogDetailSequencing,
    ati.LogFinalSequencing          AttemptLogFinalSequencing,
    ati.LogRollup                   AttemptLogRollup,
    ati.StartedTimestamp            AttemptStartedTimestamp,
    ati.CompletionStatus            AttemptCompletionStatus,
    ati.SuccessStatus               AttemptSuccessStatus,
    ati.TotalPoints                 AttemptGradedPoints,
    ----- computed LearnerAssignmentState -----
    dbo.GetLearnerAssignmentState(asi.RootActivityId, lai.IsFinal, lai.NonELearningStatus, ati.AttemptStatus) LearnerAssignmentState,
    ----- computed HasInstructors -----
    CASE WHEN EXISTS
    (
    SELECT *
    FROM InstructorAssignmentItem iaiH
    WHERE iaiH.AssignmentId = asi.Id
    ) THEN 1 ELSE 0 END             HasInstructors
    -----
    FROM LearnerAssignmentItem lai
    INNER JOIN AssignmentItem asi ON lai.AssignmentId = asi.Id
    INNER JOIN UserItem lui ON lui.Id = lai.LearnerId
    LEFT JOIN UserItemSite luis ON lui.Id = luis.UserId AND
                                    asi.SPSiteGuid = luis.SPSiteGuid
    INNER JOIN UserItem cbui ON cbui.Id = asi.CreatedBy
    INNER JOIN InstructorAssignmentItem iai ON lai.AssignmentId = iai.AssignmentId
    INNER JOIN UserItem iui ON iai.InstructorId = iui.Id
    LEFT OUTER JOIN ActivityPackageItem api ON asi.RootActivityId = api.Id
    LEFT OUTER JOIN PackageItem pki ON api.PackageId = pki.Id
    LEFT OUTER JOIN AttemptItem ati ON ati.LearnerAssignmentId = lai.Id
    WHERE iui.[Key] = @UserKey
)
GO

ALTER FUNCTION [AssignmentItem$DefaultView](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT Id, [SPSiteGuid], [SPWebGuid], [RootActivityId], [NonELearningLocation], [Title], [StartDate], [DueDate], [PointsPossible], [Description], [AutoReturn], [EmailChanges], [ShowAnswersToLearners], [CreatedBy], [DateCreated]
    FROM [AssignmentItem]
)
GO

ALTER FUNCTION [LearnerAssignmentItem$DefaultView](@UserKey nvarchar(250))
RETURNS TABLE
AS
RETURN (
    SELECT Id, [GuidId], [AssignmentId], [LearnerId], [IsFinal], [NonELearningStatus], [FinalPoints], [Grade], [InstructorComments]
    FROM [LearnerAssignmentItem]
)
GO
GRANT SELECT ON [LearnerAssignmentItem$DefaultView] TO LearningStore
GO

