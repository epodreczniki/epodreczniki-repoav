CREATE PROCEDURE [dbo].[RemoveTask]
	@Id BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE dbo.TaskData WHERE Id_Task = @Id;
	DELETE dbo.TaskPreferredNode WHERE Id_Task = @Id;
	DELETE dbo.Task WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
