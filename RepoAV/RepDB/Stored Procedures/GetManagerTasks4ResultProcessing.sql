CREATE PROCEDURE [dbo].[GetManagerTasks4ResultProcessing]
	@Id_Node INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ResultProcessingTimeout INT;
	SELECT @ResultProcessingTimeout = CAST([Value] AS INT) FROM [dbo].[GlobalData] WHERE [Key] = 'ResultProcessingTimeout';

	DECLARE @Tmp TABLE(Id_Task BIGINT PRIMARY KEY);

	INSERT @Tmp
		(Id_Task)
		SELECT t.Id
			FROM dbo.Task t
			WHERE t.[Type] < 4 --Recode, Download, Remove, RemoveFile
				AND (SupervisorId IS NULL OR (@ResultProcessingTimeout IS NOT NULL AND DateAdd(second, @ResultProcessingTimeout, t.TakenDate) < GETDATE() ))
				AND t.[Status] >= 10 -- Success lub Failure
				AND t.ResultProcessed = 0;


	UPDATE dbo.Task
		SET SupervisorId = @Id_Node,
			TakenDate = GetDate()
		FROM dbo.Task t INNER JOIN @Tmp tm ON t.Id = tm.Id_Task
		WHERE t.[Status] >= 10 -- Success lub Failure
				AND t.ResultProcessed = 0;


	SELECT t.Id, t.UniqueId, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.TaskSubtype
		FROM dbo.Task t INNER JOIN	@Tmp tm ON t.Id = tm.Id_Task
		WHERE t.SupervisorId = @Id_Node
				AND t.[Status] >= 10 -- Success lub Failure
				AND t.ResultProcessed = 0
		ORDER BY t.FinishDate;

END
