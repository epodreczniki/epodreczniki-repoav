CREATE PROCEDURE [dbo].[SetTaskStatus]
	@Id_Task bigint,
	@NewStatus SMALLINT,
	@OldStatus SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CurrStat SMALLINT;
	SELECT @CurrStat = [Status] FROM dbo.Task WHERE Id = @Id_Task;

	IF @CurrStat IS NULL
		RETURN -41;

	IF @CurrStat <> @OldStatus
		RETURN -44;--StateChangedMeanwhile

	UPDATE dbo.Task
		SET	[Status] = @NewStatus
		WHERE Id = @Id_Task 
			AND [Status] = @OldStatus;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -44;--StateChangedMeanwhile
END
