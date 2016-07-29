CREATE PROCEDURE [dbo].[GetTasksWithStatus]
	@Status SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT t.Id, t.UniqueId, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.TaskSubtype
		FROM dbo.Task t 
		WHERE (@Status IS NULL OR t.[Status] = @Status)
		ORDER BY t.CreatedDate;

END
