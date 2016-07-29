CREATE PROCEDURE [dbo].[SetTaskResultProcessed]
	@Id_Task bigint,
	@ResultProcessed BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CurrStat SMALLINT;
	IF NOT EXISTS (SELECT Id FROM dbo.Task WHERE Id = @Id_Task)
		RETURN -41;

	UPDATE dbo.Task
		SET	ResultProcessed = @ResultProcessed
		WHERE Id = @Id_Task;

	RETURN 1;
END
