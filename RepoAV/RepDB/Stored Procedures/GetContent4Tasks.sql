CREATE PROCEDURE [dbo].[GetContent4Tasks]
	@TaskIds VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Key], Value, Id_Task
		FROM dbo.TaskData
		WHERE Id_Task IN (SELECT CAST(Part AS BIGINT) FROM dbo.fnctSplitString(@TaskIds, ';'))

	RETURN 1;
END
