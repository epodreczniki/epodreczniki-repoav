CREATE PROCEDURE [dbo].[GetResponses2Send]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT TaskId, Result, ErrorCode, TaskFinishDate
		FROM dbo.Response2Send
		ORDER BY TaskFinishDate;
	RETURN 1;
END
