CREATE PROCEDURE [dbo].[RemoveResponse2Send]
	@Id_Task BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE dbo.Response2Send WHERE TaskId = @Id_Task;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
