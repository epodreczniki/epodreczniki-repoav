/****** Object:  StoredProcedure [dbo].[ShowTasks]    Script Date: 2015-06-09 13:36:06 ******/
CREATE PROCEDURE [dbo].[ShowTasks]
	@TaskStatus VARCHAR(50),
	@TaskType VARCHAR(50),
	@PublicId  VARCHAR(150),
	@NodeId INT,
	@Limit INT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SQLString NVARCHAR(MAX);
	DECLARE @ParmDefinition NVARCHAR(500);
	DECLARE @Condition NVARCHAR(500);
  
	SET @Condition = 'WHERE ts.Name = ISNULL(@TaskStatus, ts.Name)					
					AND tt.Name =  ISNULL(@TaskType, tt.Name)' + 
					CASE WHEN @PublicId IS NOT NULL THEN 'AND (t.UniqueId LIKE @PublicId+''%'' OR t.PublicId = @PublicId)' ELSE '' END +
					CASE WHEN @NodeId IS NOT NULL THEN 'AND (t.ExecutingNodeId = @NodeId OR  CHARINDEX(CONVERT(VARCHAR(10), @NodeId), dbo.fnctGetNodePreferred4TaskAsString(t.Id)) > 0)' ELSE '' END			



	SET @SQLString = 'SELECT  * FROM 
			(SELECT TOP (ISNULL(@Limit,35)) t.Id, 
				ts.Name as Status, 
				t.Result, 
				t.UniqueId, 
				t.PublicId, 
				n.Name as ExecutingNode,
				tt.Name as Type, 
				n2.Name as Supervisor,
				t.ResultProcessed, 
				t.CreatedDate, 
				t.BeginDate, 
				t.FinishDate,
				t.TakenDate,
				t.LastActivityDate,
				--t.TaskSubtype,
				dbo.fnctGetNodePreferred4TaskAsString(t.Id) AS PreferredNodes
				FROM  Task t
				INNER JOIN TaskType tt on tt.Id = t.Type
				INNER JOIN TaskStatus ts on ts.Id = t.Status
				LEFT JOIN Node n on n.Id = t.ExecutingNodeId
				LEFT JOIN Node n2 on n2.Id = t.SupervisorId '
				+ @Condition +
				' ORDER BY CreatedDate DESC) a
				LEFT JOIN TaskData td on td.Id_Task = a.Id
			ORDER BY a.CreatedDate';


	SET @ParmDefinition = N'@TaskStatus VARCHAR(50),
							@TaskType VARCHAR(50),
							@PublicId  VARCHAR(150),
							@NodeId INT,
							@Limit INT';


	EXECUTE sp_executesql @SQLString, @ParmDefinition, @TaskStatus, @TaskType, @PublicId, @NodeId, @Limit
END

