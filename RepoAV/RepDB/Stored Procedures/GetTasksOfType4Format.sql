CREATE PROCEDURE [dbo].[GetTasksOfType4Format]
	@Type SMALLINT,
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT t.Id, t.UniqueId, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.BeginDate, t.SupervisorId, t.FinishDate, t.TakenDate, t.CreatedDate, t.ExecutingNodeId, t.Result, t.CanSkipPreferredNodes, t.TaskSubtype
		FROM dbo.Task t
		WHERE (@Type IS NULL OR t.[Type] = @Type)
			AND t.UniqueId = @UniqueId
		ORDER BY t.CreatedDate;

	RETURN 1;
END
