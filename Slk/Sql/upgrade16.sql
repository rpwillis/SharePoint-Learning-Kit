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



GO


DROP PROCEDURE DropConstraints
GO
