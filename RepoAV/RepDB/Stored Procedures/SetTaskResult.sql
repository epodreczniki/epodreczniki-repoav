CREATE PROCEDURE [dbo].[SetTaskResult]
	@Id_Task bigint,
	@Result NVARCHAR(MAX),
	@Status SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CurrStat SMALLINT;
	SELECT @CurrStat = [Status] FROM dbo.Task WHERE Id = @Id_Task;

	IF @CurrStat IS NULL
		RETURN -41;

	IF @CurrStat = @Status
		RETURN 1;

	UPDATE dbo.Task
		SET	[Status] = @Status,
			Result = @Result,
			FinishDate = GetDate()
		WHERE Id = @Id_Task;
	RETURN 1;

END
