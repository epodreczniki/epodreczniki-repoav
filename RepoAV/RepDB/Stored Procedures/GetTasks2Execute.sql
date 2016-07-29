CREATE PROCEDURE [dbo].[GetTasks2Execute]
	@Id_Node INT,
	@Types VARCHAR(100),
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @En BIT;

	SELECT @En = [Enabled] FROM dbo.Node WHERE Id = @Id_Node;
	if @En IS NULL
		RETURN -61; -- NodeNotFound

	IF @En = 1
	BEGIN
		DECLARE @ExecutionTimeout INT;
		SELECT @ExecutionTimeout = CAST([Value] AS INT) FROM [dbo].[GlobalData] WHERE [Key] = 'TaskExecutionTimeout';

		DECLARE @WaitingTimeout INT;
		SELECT @WaitingTimeout = CAST([Value] AS INT) FROM [dbo].[GlobalData] WHERE [Key] = 'TaskWaitingTimeout';

		DECLARE @Tmp TABLE(Id_Task BIGINT PRIMARY KEY);
		DECLARE @TypesTable TABLE(TaskType SMALLINT, TaskSubtype VARCHAR(50));

		INSERT @TypesTable
			(TaskType, TaskSubtype)
			SELECT (SELECT CAST(fI.Part AS SMALLINT) FROM dbo.fnctSplitString(fss.Part, ':') fI WHERE fI.Idx = 0),
				(SELECT fI2.Part FROM dbo.fnctSplitString(fss.Part, ':') fI2 WHERE fI2.Idx = 1)
				FROM dbo.fnctSplitString(@Types, ';') fss;

		INSERT @Tmp
			(Id_Task)
			SELECT TOP(@Count) t.Id
				FROM dbo.Task t
				WHERE 
					(@Types IS NULL OR EXISTS(SELECT tts.TaskType FROM @TypesTable tts WHERE tts.TaskType = t.[Type] AND (tts.TaskSubtype IS NULL OR t.TaskSubtype = tts.TaskSubtype)))
						AND
					(dbo.fnctIsNodePreferred4Task(@Id_Node, t.Id) = 1 OR (@WaitingTimeout IS NOT NULL AND t.CanSkipPreferredNodes = 1 AND DATEDIFF(second, t.CreatedDate, GetDate()) >= @WaitingTimeout))
						AND				
					(		(t.[Status] = 0	AND (t.BeginDate IS NULL OR t.BeginDate <= DATEADD(second, 5, GetDate())) )
								OR 
							(@ExecutionTimeout IS NOT NULL AND t.[Status] = 5 AND DateAdd(second, @ExecutionTimeout, t.LastActivityDate) < GETDATE())
					)
				ORDER BY t.BeginDate, t.CreatedDate;

		UPDATE dbo.Task
			SET ExecutingNodeId = @Id_Node,
				[Status] = 5, -- Executing
				TakenDate = GetDate(),
				LastActivityDate = GetDate()
			FROM dbo.Task t INNER JOIN @Tmp tm ON t.Id = tm.Id_Task
			WHERE t.[Status] = 0;


		SELECT t.Id, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.UniqueId
			FROM dbo.Task t INNER JOIN	
				@Tmp tm ON t.Id = tm.Id_Task
			WHERE t.[Status] = 5
				AND t.ExecutingNodeId = @Id_Node
			ORDER BY t.CreatedDate;
	END

	RETURN 1;
END
