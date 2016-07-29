CREATE PROCEDURE [dbo].[GetManagerTasks2Execute]
	@Id_Node INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExecutionTimeout INT;
	SELECT @ExecutionTimeout = CAST([Value] AS INT) FROM [dbo].[GlobalData] WHERE [Key] = 'TaskExecutionTimeout';

	DECLARE @Tmp TABLE(Id_Task BIGINT PRIMARY KEY);

	INSERT @Tmp
		(Id_Task)
		SELECT t.Id
			FROM dbo.Task t
			WHERE t.[Type] IN (4,5,6,8,9,10,12,13,14,15)--AddMaterial,UpdateMaterial, RemoveMaterial, FixFormatErrors, FixReplication, RemoveOldMaterials, AddFormat, UpdateFormat, RemvoeFormat
				AND (t.BeginDate IS NULL OR t.BeginDate <= DATEADD(minute, 1, GetDate()))
				AND (  (ISNULL(ExecutingNodeId, @Id_Node) = @Id_Node
							AND t.[Status] = 0 -- New)
						OR (@ExecutionTimeout IS NOT NULL 
							AND t.[Status] = 5 
							AND DateAdd(second, @ExecutionTimeout, t.LastActivityDate) < GETDATE())
							)
					)				
				AND NOT EXISTS(SELECT tI.Id  
								FROM dbo.Task tI 	
								WHERE tI.UniqueId iS NOT NULL 
									AND tI.PublicId = t.PublicId
									AND tI.[Type] NOT IN (12, 13, 14)
									AND tI.ResultProcessed = 0
									AND tI.Id <> t.Id)
				AND (t.[Status] = 5 OR NOT EXISTS(SELECT tI.Id 
													FROM dbo.Task tI
													WHERE tI.PublicId iS NOT NULL 
														AND tI.PublicId = t.PublicId
														AND tI.[Type] IN (4,5,6)
														AND tI.Id <> t.Id
														AND tI.[Status] = 5
													)
					)		


	UPDATE dbo.Task
		SET [Status] = 5, -- Executing
			TakenDate = GetDate(),
			LastActivityDate = GetDate(),
			ExecutingNodeId = @Id_Node,
			FinishDate = NULL
		FROM dbo.Task t INNER JOIN @Tmp tm ON t.Id = tm.Id_Task
		WHERE t.[Status] = 0
			AND NOT EXISTS(SELECT tI.Id 
							FROM dbo.Task tI 
							WHERE tI.UniqueId iS NOT NULL 
								AND tI.PublicId = t.PublicId
								AND tI.[Type] NOT IN (12, 13, 14)
								AND tI.ResultProcessed = 0
								AND tI.Id <> t.Id)
			AND NOT EXISTS(SELECT tI.Id 
							FROM dbo.Task tI
							WHERE tI.PublicId iS NOT NULL 
								AND tI.PublicId = t.PublicId
								AND tI.[Type] IN (4,5,6)
								AND tI.Id <> t.Id
								AND tI.[Status] IN (0, 5))


	SELECT t.Id, t.UniqueId, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.TaskSubtype
		FROM dbo.Task t INNER JOIN	@Tmp tm ON t.Id = tm.Id_Task
		WHERE t.[Type] IN (4,5,6,8,9,10,12,13,14,15)--AddMaterial,UpdateMaterial, RemoveMaterial, FixFormatErrors, FixReplication, RemoveOldMaterials, AddFormat, UpdateFormat, RemvoeFormat
			AND t.[Status] = 5
			AND t.ExecutingNodeId = @Id_Node
		ORDER BY t.CreatedDate;

END