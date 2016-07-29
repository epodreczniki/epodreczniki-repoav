CREATE PROCEDURE [dbo].[GetPreferredNodes4Tasks]
	@TaskIds VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT NodeId, Id_Task
		FROM dbo.TaskPreferredNode
		WHERE Id_Task IN (SELECT CAST(Part AS BIGINT) FROM dbo.fnctSplitString(@TaskIds, ';'))

	RETURN 1;
END