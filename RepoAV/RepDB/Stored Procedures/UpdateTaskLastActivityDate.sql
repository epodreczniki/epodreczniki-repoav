CREATE PROCEDURE [dbo].[UpdateTaskLastActivityDate]
	@Id_Task bigint
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Task
		SET	LastActivityDate = GetDate()
		WHERE Id = @Id_Task 
			AND [Status] = 5;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -44;--StateChangedMeanwhile
END
