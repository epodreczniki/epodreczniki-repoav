CREATE PROCEDURE [dbo].[AddResponse2Send]
	@TaskId BIGINT,
	@ErrorCode INT,
	@Result NVARCHAR(max),
	@TaskFinishDate DateTime
AS
BEGIN
	SET NOCOUNT ON;

	INSERT dbo.Response2Send
		(TaskId, ErrorCode, Result, TaskFinishDate)
		VALUES(@TaskId, @ErrorCode, @Result, @TaskFinishDate);

	RETURN 1;
END
