CREATE PROCEDURE [dbo].[GetTasksOfType]
	@Type SMALLINT,
	@TaskSubtype VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT t.Id, t.UniqueId, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.BeginDate, t.SupervisorId, t.FinishDate, t.TakenDate, t.CreatedDate, t.ExecutingNodeId, t.Result, t.CanSkipPreferredNodes, t.TaskSubtype
		FROM dbo.Task t
		WHERE (@Type IS NULL OR t.[Type] = @Type)
			AND (@TaskSubtype IS NULL OR t.TaskSubtype like @TaskSubtype)
		ORDER BY t.CreatedDate;

	RETURN 1;
END
