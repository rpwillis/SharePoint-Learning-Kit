USE SLKCourseManager
GO
/****** Object:  Table [dbo].[AssignedActivityStatuses]    Script Date: 06/10/2008 02:41:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssignedActivityStatuses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AssignedActivityStatuses](
	[GUID_aas] [varchar](50) NOT NULL,
	[Name_aas] [varchar](50) NOT NULL,
	[Description_aas] [varchar](100) NOT NULL,
	[StatusIcon_aas] [binary](1024) NULL,
 CONSTRAINT [PK_AssignedActivityStatuses] PRIMARY KEY CLUSTERED 
(
	[GUID_aas] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[SP_GetActivityStatusIconData]    Script Date: 06/10/2008 02:40:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetActivityStatusIconData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_GetActivityStatusIconData] 
	-- Add the parameters for the stored procedure here
	@ActivityStatusGUID varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT StatusIcon_ast FROM ActivityStatuses WHERE GUID_ast = @ActivityStatusGUID
END
' 
END
GO
/****** Object:  Table [dbo].[ActivityStatuses]    Script Date: 06/10/2008 02:41:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityStatuses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActivityStatuses](
	[GUID_ast] [varchar](50) NOT NULL,
	[Name_ast] [varchar](50) NOT NULL,
	[Description_ast] [varchar](100) NOT NULL,
	[StatusIcon_ast] [varbinary](max) NULL,
 CONSTRAINT [PK_CLSActivityStatus] PRIMARY KEY CLUSTERED 
(
	[GUID_ast] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UILayouts]    Script Date: 06/10/2008 02:41:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UILayouts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UILayouts](
	[GUID_ply] [varchar](50) NOT NULL,
	[Name_ply] [varchar](50) NOT NULL,
	[ObjectType_ply] [varchar](255) NOT NULL,
	[LayoutXML_ply] [text] NOT NULL,
 CONSTRAINT [PK_UILayouts] PRIMARY KEY CLUSTERED 
(
	[GUID_ply] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ConfigurationProfiles]    Script Date: 06/10/2008 02:41:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigurationProfiles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ConfigurationProfiles](
	[GUID_cfp] [varchar](50) NOT NULL,
	[Name_cfp] [varchar](25) NOT NULL,
 CONSTRAINT [PK_ConfigurationProfiles] PRIMARY KEY CLUSTERED 
(
	[GUID_cfp] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ConfigurationProperties]    Script Date: 06/10/2008 02:41:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfigurationProperties]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ConfigurationProperties](
	[GUID_cpr] [varchar](50) NOT NULL,
	[Name_cpr] [nvarchar](250) NOT NULL,
	[DefaultValue_cpr] [nvarchar](max) NOT NULL,
	[Encryption_cpr] [bit] NOT NULL,
 CONSTRAINT [PK_ConfigurationProperties] PRIMARY KEY CLUSTERED 
(
	[GUID_cpr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 06/10/2008 02:41:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserRoles](
	[GUID_uro] [varchar](50) NOT NULL,
	[Name_uro] [varchar](50) NULL,
 CONSTRAINT [PK_clsUserRole] PRIMARY KEY CLUSTERED 
(
	[GUID_uro] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ActivityGroupStatuses]    Script Date: 06/10/2008 02:41:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityGroupStatuses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActivityGroupStatuses](
	[GUID_ags] [varchar](50) NOT NULL,
	[Name_ags] [varchar](50) NOT NULL,
	[Description_ags] [varchar](100) NOT NULL,
	[Icon_ags] [binary](1024) NULL,
 CONSTRAINT [PK_ActivityGroupStatuses] PRIMARY KEY CLUSTERED 
(
	[GUID_ags] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Activities]    Script Date: 06/10/2008 02:40:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activities]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Activities](
	[GUID_act] [varchar](50) NOT NULL,
	[SLKGUID_act] [varchar](50) NULL,
	[Name_act] [varchar](50) NOT NULL,
	[AssignDate_act] [datetime] NOT NULL,
	[DueDate_act] [datetime] NOT NULL,
	[MaxScore_act] [int] NOT NULL,
	[Gradeable_act] [bit] NOT NULL,
	[Weight_act] [int] NOT NULL,
	[ActivityGroupGUID_act] [varchar](50) NOT NULL,
	[ActivityStatusGUID_act] [varchar](50) NOT NULL,
	[Description_act] [varchar](255) NOT NULL,
	[FileURL_act] [varchar](255) NOT NULL,
 CONSTRAINT [PK_clsActivity] PRIMARY KEY CLUSTERED 
(
	[GUID_act] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Profiles_Properties]    Script Date: 06/10/2008 02:41:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Profiles_Properties]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Profiles_Properties](
	[GUID_ppr] [varchar](50) NOT NULL,
	[MasterGUID_ppr] [varchar](50) NOT NULL,
	[DetailGUID_ppr] [varchar](50) NOT NULL,
	[Value_ppr] [varchar](max) NOT NULL,
	[Encryption_ppr] [bit] NOT NULL,
 CONSTRAINT [PK_Profiles_Properties] PRIMARY KEY CLUSTERED 
(
	[GUID_ppr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ActivityGroups]    Script Date: 06/10/2008 02:40:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityGroups]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActivityGroups](
	[GUID_agr] [varchar](50) NOT NULL,
	[Name_agr] [varchar](50) NOT NULL,
	[Outcome_agr] [varchar](255) NOT NULL,
	[WeekFrom_agr] [int] NOT NULL,
	[WeekTo_agr] [int] NOT NULL,
	[CourseGUID_agr] [varchar](50) NOT NULL,
	[ActivityGroupStatusGUID_agr] [varchar](50) NOT NULL,
	[Priority_agr] [int] NOT NULL,
 CONSTRAINT [PK_clsActivityGroup] PRIMARY KEY CLUSTERED 
(
	[GUID_agr] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[SP_ReIndexActivityGroupPriority]    Script Date: 06/10/2008 02:40:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_ReIndexActivityGroupPriority]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SP_ReIndexActivityGroupPriority]
	@p_CourseGUID varchar(50)
AS

declare @i int, @rowCount int;

declare @ActivityGroupStore as Table( ID_ags int identity(1,1),
									  GUID_ags varchar(50),
									  Priority_ags int);


insert into @ActivityGroupStore (GUID_ags,Priority_ags)
select GUID_agr,Priority_agr
from ActivityGroups
where CourseGUID_agr = @p_CourseGUID
order by Priority_agr;

set @rowCount = @@ROWCOUNT;
set @i = 1;

while (@i <= @rowCount)
begin
	update ActivityGroups
	set Priority_agr = @i				
	where GUID_agr = (select GUID_ags 
					  from @ActivityGroupStore
					  where ID_ags = @i); 
	set @i = @i+1;
end
' 
END
GO
/****** Object:  ForeignKey [FK_Activities_ActivityGroups]    Script Date: 06/10/2008 02:40:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Activities_ActivityGroups]') AND parent_object_id = OBJECT_ID(N'[dbo].[Activities]'))
ALTER TABLE [dbo].[Activities]  WITH CHECK ADD  CONSTRAINT [FK_Activities_ActivityGroups] FOREIGN KEY([ActivityGroupGUID_act])
REFERENCES [dbo].[ActivityGroups] ([GUID_agr])
GO
ALTER TABLE [dbo].[Activities] CHECK CONSTRAINT [FK_Activities_ActivityGroups]
GO
/****** Object:  ForeignKey [FK_Activities_ActivityStatuses]    Script Date: 06/10/2008 02:40:54 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Activities_ActivityStatuses]') AND parent_object_id = OBJECT_ID(N'[dbo].[Activities]'))
ALTER TABLE [dbo].[Activities]  WITH CHECK ADD  CONSTRAINT [FK_Activities_ActivityStatuses] FOREIGN KEY([ActivityStatusGUID_act])
REFERENCES [dbo].[ActivityStatuses] ([GUID_ast])
GO
ALTER TABLE [dbo].[Activities] CHECK CONSTRAINT [FK_Activities_ActivityStatuses]
GO
/****** Object:  ForeignKey [FK_ActivityGroups_ActivityGroupStatuses]    Script Date: 06/10/2008 02:40:59 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ActivityGroups_ActivityGroupStatuses]') AND parent_object_id = OBJECT_ID(N'[dbo].[ActivityGroups]'))
ALTER TABLE [dbo].[ActivityGroups]  WITH CHECK ADD  CONSTRAINT [FK_ActivityGroups_ActivityGroupStatuses] FOREIGN KEY([ActivityGroupStatusGUID_agr])
REFERENCES [dbo].[ActivityGroupStatuses] ([GUID_ags])
GO
ALTER TABLE [dbo].[ActivityGroups] CHECK CONSTRAINT [FK_ActivityGroups_ActivityGroupStatuses]
GO
/****** Object:  ForeignKey [FK_Profiles_Properties_ConfigurationProfiles]    Script Date: 06/10/2008 02:41:12 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Profiles_Properties_ConfigurationProfiles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Profiles_Properties]'))
ALTER TABLE [dbo].[Profiles_Properties]  WITH CHECK ADD  CONSTRAINT [FK_Profiles_Properties_ConfigurationProfiles] FOREIGN KEY([MasterGUID_ppr])
REFERENCES [dbo].[ConfigurationProfiles] ([GUID_cfp])
GO
ALTER TABLE [dbo].[Profiles_Properties] CHECK CONSTRAINT [FK_Profiles_Properties_ConfigurationProfiles]
GO
/****** Object:  ForeignKey [FK_Profiles_Properties_ConfigurationProperties]    Script Date: 06/10/2008 02:41:12 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Profiles_Properties_ConfigurationProperties]') AND parent_object_id = OBJECT_ID(N'[dbo].[Profiles_Properties]'))
ALTER TABLE [dbo].[Profiles_Properties]  WITH CHECK ADD  CONSTRAINT [FK_Profiles_Properties_ConfigurationProperties] FOREIGN KEY([DetailGUID_ppr])
REFERENCES [dbo].[ConfigurationProperties] ([GUID_cpr])
GO
ALTER TABLE [dbo].[Profiles_Properties] CHECK CONSTRAINT [FK_Profiles_Properties_ConfigurationProperties]
GO
/****** User Interface Layouts ***********/
INSERT INTO UILayouts VALUES('b3568ce1-3097-4d73-8048-e21d0f4ea449', 'MonitorAndAssessGrid', 'Axelerate.SlkCourseManagerLogicalLayer.clsUsers', '<FORM><TOOLBAR Width="800px" CssClass="ms-menutoolbar"><GROUP Width="100%"><COLUMN><Command Type="ImageButton" Text="Save Grades" PostbackURL="" MouseOutImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save" PressedImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save" MouseOverImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save"  Command="update" CommandArgs="" UseImageButtonComposer="False" /></COLUMN><COLUMN><Command Type="LinkButton" Text="Save" PostbackURL="" Command="update" CommandArgs="" Style="text-decoration:none;color:black" FontBold="True"/></COLUMN><COLUMN><Command Type="ImageButton" Text="Return all graded assignments to students for the selected task" PostbackURL="" MouseOverImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_returntolearners" PressedImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_returntolearners" MouseOutImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_returntolearners" Command="ReturnToLearner" CommandArgs="" Target="Column" UseImageButtonComposer="False" /></COLUMN><COLUMN><Command Type="LinkButton" Text="Return To Learners"  CausesValidation="False" PostbackURL=""  Command="ReturnToLearner" CommandArgs="" Target="Column" Style="text-decoration:none;color:black" FontBold="True"/></COLUMN><COLUMN Width="552px"></COLUMN></GROUP></TOOLBAR><COLLECTIONLAYOUT><BAND SelectRow="false" PageSize="8" GridLines="both" AllowPaging="false" AlternateRowCssClass="ms-alternating" SelectedRowCssClass="ms-menutoolbar" EditionRowCssClass="ms-menutoolbar" GridHeaderCssClass="ms-vh" CssClass="" RowCssClass="ms-viewselect" Width="800px" Height="100%" CommandErrorMessages="ReturnToLearner:You must select an assignment column before clicking: Return to Learners" ><COLUMN Header="Name" ToolTip="{Name}" MaxChars="25" Width="225px"  Content="{Name}" ColumnPropertyType="HTML" EditBehavior="NoEditable" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" FixedColumn="true" HeaderStyle="text-align:center; vertical-align:center; padding: 0px 0px 0px 0px" Style="background-color:#C3CDDE" HeaderCssClass="ms-vh" /><COLUMN Header="Course Total" ToolTip="{Grade}" MaxChars="10"  Width="100px" Content="Grade" ColumnPropertyType="Property" EditBehavior="NoEditable" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" FixedColumn="true" HeaderStyle="text-align:center; vertical-align:center; padding: 0px 0px 0px 0px" Style="background-color:#C3CDDE; text-align:right" HeaderCssClass="ms-vh" /><REPEATABLECOLUMNS Header="" DSClassName="Axelerate.SlkCourseManagerLogicalLayer.clsSLKAssignments, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4" FactoryMethod="GetSLKAssignmentsForMA" FactoryParam="" Width="100px" HeaderPropertyType="PROPERTY" PropertyName="AssignedActivities" Content="NameWithLink" HeaderStyle="height:15px; text-align:center;vertical-align:middle; valign:center; padding: 0px 0px 0px 0px" HeaderCssClass="ms-vh"><COLUMN Width="70px" NullContent="Unassigned" Content="FinalPointsToString" ColumnPropertyType="Property" EditBehavior="Editable" KeyProperty="AssignmentGUID" EditorType="fieldcontrol" IsLink="False" TargetUrl="" Style="text-align:right; vertical-align:middle; padding: 0px 0px 0px 0px" IsHidden="False" HeaderCssClass="ms-vh"/><COLUMN Width="30px" NullContent="&lt;img src=''/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_unassigned'' alt=''Unassigned'' /&gt;" Content="{AssignedActivityStatus.StatusImageHTMLTag}" ColumnPropertyType="HTML" EditBehavior="NoEditable" KeyProperty="AssignmentGUID" IsLink="False" TargetUrl="" Style="text-align:right; vertical-align:middle; padding: 0px 0px 0px 0px" IsHidden="False" HeaderCssClass="ms-vh" /></REPEATABLECOLUMNS></BAND></COLLECTIONLAYOUT></FORM>')
GO
INSERT INTO UILayouts VALUES('a7142980-6866-4f75-9f71-e1ca853f86a6', 'PlanAndAssignActivityGrid', 'Axelerate.SlkCourseManagerLogicalLayer.clsActivities', '<FORM><COLLECTIONLAYOUT><BAND SelectRow="false" PageSize="8" GridLines="none" AllowPaging="false" AlternateRowCssClass="ms-alternating" CssClass="ms-vh" GridHeaderCssClass="ms-vh" RowCssClass="ACMWebRowStyle" Width="100%" Height="100%" SelectedRowCssClass="ms-menutoolbar" EditionRow="Start a new activity" EditionRowAddTooltip="Add activity" OffsetEditionRow="15px" EditionRowCssClass="ms-menutoolbar" ><COLUMN Header="Activity" Width="15%" ToolTip="{Name}" MaxChars="14" Content="Name" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="Document" Width="20%" Content="FileURL" ColumnPropertyType="Property" EditBehavior="Editable" IsLink="False" TargetUrl="" Style="white-space:nowrap" IsHidden="False" DisplayValue="&lt;table border=''0'' cellpadding=''5'' cellspacing=''0'' width=''100%''&gt;&lt;tr&gt;&lt;td style=''width:auto'' &gt;&lt;span title=''{FileURL}''&gt;&amp;nbsp;{FileName}&lt;/span&gt;&lt;/td&gt;&lt;td style=''width:48px'' &gt;&lt;input type=''button'' value=''...'' title=''Select Document'' disabled=''disabled'' /&gt;&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;" EditorType="Custom" ><CUSTOMVALUEEDITOR ClassName="Axelerate.SlkCourseManagerLogicalLayer.WebControls.SPDocumentSelector, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5BBD900FCDE291A4" Style="background-color:#F2F8FF" Format="10"/></COLUMN><COLUMN Header="Description" Width="16%" ToolTip="{Description}" MaxChars="14" Content="Description" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="Gradable" Width="7%" ToolTip="{Gradeable}" MaxChars="4" Content="Gradeable" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" EditorStyle="background-color:#f2f8ff; text-align:center; vertical-align:middle" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="Weight" Width="8%" ToolTip="{Weight}" MaxChars="3" Content="Weight" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" IsLink="False" Style="text-align:center" TargetUrl="" IsHidden="False" /><COLUMN Header="Assign Date" Width="10%" Content="AssignDate" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" Style="text-align:right" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="Due Date" Width="8%" Content="DueDate" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" Style="text-align:right" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="Max Score" Width="8%" ToolTip="{MaxScore}" MaxChars="14" Content="MaxScore" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" /><COLUMN Header="" Width="8%" Content="" ColumnPropertyType="ACTIONS" EditBehavior="NoEditable" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="ACTIONS" FixedColumn="false"><TOOLBAR Width="100%" Condition="''{ActivityStatus.GUID}''==''1''"><GROUP Width="100%"><COLUMN Width="50%"><Command Type="LinkButton" Text="Assign" PostbackURL="" Command="Assign" CommandArgs="" /></COLUMN></GROUP></TOOLBAR><TOOLBAR Width="100%" Condition="''{ActivityStatus.GUID}''!==''1''"><GROUP Width="100%"><COLUMN Width="50%"><Command Type="LinkButton" Text="Unassign" PostbackURL="" Command="Unassign" CommandArgs="" /></COLUMN></GROUP></TOOLBAR></COLUMN></BAND></COLLECTIONLAYOUT></FORM>')
GO
INSERT INTO UILayouts VALUES('48d9d5ac-e404-4270-b881-49818fb74a06', 'PlanAndAssignGrid', 'Axelerate.SlkCourseManagerLogicalLayer.clsActivityGroups', '<FORM><TOOLBAR Width="100%" CssClass="ms-menutoolbar"><GROUP Width="100%"><COLUMN><Command Type="ImageButton" Text="Save" PostbackURL="" MouseOutImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save" MouseOverImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save" PressedImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_save" CausesValidation="False" Command="update" CommandArgs="" UseImageButtonComposer="False" /></COLUMN><COLUMN><Command Type="LinkButton" Text="Save" PostbackURL="" Command="update" CommandArgs="" Style="text-decoration:none;color:black" FontBold="True"/></COLUMN><COLUMN ><Command Type="ImageButton" Text="Delete Activity" ConfirmMessage="Are you sure you want to delete the selected row?" PostbackURL="" MouseOutImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_delete" MouseOverImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_delete" PressedImageUrl="/Images.asmx?ObjectClass=Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4&amp;FixedImageURL=slk_delete" Command="DeleteSelectedRow"  Target="Row"  CommandArgs="" UseImageButtonComposer="False" /></COLUMN><COLUMN><Command Type="LinkButton" Text="Delete" ConfirmMessage="Are you sure you want to delete the selected row?" PostbackURL="" Command="DeleteSelectedRow" Target="Row" CommandArgs="" Style="text-decoration:none;color:black" FontBold="True"/></COLUMN><COLUMN Width="80%"></COLUMN></GROUP></TOOLBAR><COLLECTIONLAYOUT><BAND SelectRow="false" PageSize="8" GridLines="none" AllowPaging="false" AlternateRowCssClass="ms-alternating" GridHeaderCssClass="ms-vh" CssClass="ms-vh" RowCssClass="ms-viewselect" Width="100%" Height="100%" EditionRow="Start a new activity group" OffsetEditionRow="15px" EditionRowAddTooltip="Add activity group" SelectedRowCssClass="ms-menutoolbar" EditionRowCssClass="ms-menutoolbar" RowHeight="30px" ><CHILDBAND PropertyName="Activities" IsCollapsed="false" ClassName="Axelerate.SlkCourseManagerLogicalLayer.clsActivities, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4" LayoutName="PlanAndAssignActivityGrid" AutoGenerateSelectButton="true" GridHeaderCssClass="ms-vh"  /><COLUMN Header="Activity Group" Width="32%" ToolTip="{Name}" MaxChars="33" Content="Name" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="Outcomes" Width="48%" ToolTip="{Outcome}" MaxChars="51" Content="Outcome" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="Start Week" Width="10%" ToolTip="{WeekFrom}" MaxChars="3" Content="WeekFrom" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="End Week" Width="10%" ToolTip="{WeekTo}" MaxChars="3" Content="WeekTo" ColumnPropertyType="Property" EditBehavior="Editable" EditorType="fieldcontrol" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /></BAND></COLLECTIONLAYOUT></FORM>')
GO
INSERT INTO UILayouts VALUES('lh125h45-e234-497d-9273-asd98723b8sa', 'SLKActivitiesGrid', 'Axelerate.SlkCourseManagerLogicalLayer.clsSLKAssignments', '<FORM><TOOLBAR Width="100%" CssClass="ms-menutoolbar"><GROUP Width="100%"><COLUMN Height="10px"><LABEL Text="&amp;nbsp;"></LABEL></COLUMN></GROUP></TOOLBAR><COLLECTIONLAYOUT><BAND SelectRow="false" PageSize="8" GridLines="none" AllowPaging="false" AlternateRowCssClass="ms-alternating" GridHeaderCssClass="ms-vh" CssClass="ms-vh" RowCssClass="ms-viewselect" Width="100%" Height="100%" SelectedRowCssClass="ms-menutoolbar" RowHeight="30px" ><COLUMN Header="Title" ToolTip="{Name}" MaxChars="33" Width="30%" Content="Name" ColumnPropertyType="Property" EditBehavior="NoEditable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" ></COLUMN><COLUMN Header="Description" ToolTip="{Description}" MaxChars="50" Width="40%" Content="Description" ColumnPropertyType="Property" EditBehavior="NoEditable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="Points" Width="10%" ToolTip="{PointsPossible}" MaxChars="3"  Content="PointsPossible" ColumnPropertyType="Property" EditBehavior="NoEditable" EditorType="fieldcontrol" IsLink="False" TargetUrl="" Style="text-align:center" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="Start Date" Width="10%" Content="StartDateToString" ColumnPropertyType="Property" EditBehavior="NoEditable" EditorType="fieldcontrol" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /><COLUMN Header="Due Date" Width="10%" Content="DueDateToString" ColumnPropertyType="Property" EditBehavior="NoEditable" EditorType="fieldcontrol" Style="text-align:center" IsLink="False" TargetUrl="" IsHidden="False" HeaderPropertyType="TEXT" /></BAND></COLLECTIONLAYOUT><TOOLBAR Width="100%" CssClass="ms-menutoolbar"><GROUP Width="100%"><COLUMN Height="10px"><LABEL Text="&amp;nbsp;"></LABEL></COLUMN></GROUP></TOOLBAR></FORM>')
GO
/****** ActivityGroup Statuses ***********/
INSERT INTO ActivityGroupStatuses VALUES('1','Pending Start', 'The activity has not been started', NULL)
GO
INSERT INTO ActivityGroupStatuses VALUES('2','Started', 'The activity has been started', NULL)
GO
INSERT INTO ActivityGroupStatuses VALUES('3','Completed', 'The activity is completed', NULL)
GO
/****** Activity Statuses ****************/
INSERT INTO ActivityStatuses VALUES('1', 'Unassigned', 'The activity is not assigned', NULL)
GO
INSERT INTO ActivityStatuses VALUES('2', 'Assigned', 'The activity is assigned', NULL)
GO
INSERT INTO ActivityStatuses VALUES('3', 'Completed', 'The activity is completed', NULL)
GO
/***** Assigned Activity Statuses ********/
INSERT INTO AssignedActivityStatuses VALUES('1', 'Pending Submit', 'The submit of the activity is pending', NULL)
GO
INSERT INTO AssignedActivityStatuses VALUES('2', 'Sitting on inbox', 'The activity is waiting to be checked', NULL)
GO
INSERT INTO AssignedActivityStatuses VALUES('3', 'Marked and ready to return','The activity is ready to be returned to the student', NULL)
GO
INSERT INTO AssignedActivityStatuses VALUES('4', 'Returned to student', 'The activity has been returned to the student', NULL)
GO
/******** User Roles **********/
INSERT INTO UserRoles VALUES('1', 'Instructor')
GO
INSERT INTO UserRoles VALUES('2', 'Learner')
GO
INSERT INTO UserRoles VALUES('3', 'Observer')
