CREATE PROCEDURE [dbo].[FinishTask]
	@Id_Task bigint,
	@Result NVARCHAR(MAX),
	@Success BIT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Task
		SET	[Status] = (CASE WHEN @Success = 0 THEN 66 ELSE 10 END),
			Result = @Result,
			FinishDate = GetDate()
		WHERE Id = @Id_Task;


	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;--NotFOund
END
