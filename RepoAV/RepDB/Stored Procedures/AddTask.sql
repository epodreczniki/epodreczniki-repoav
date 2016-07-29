CREATE PROCEDURE [dbo].[AddTask]
	@Id BIGINT OUTPUT,
	@UniqueId VARCHAR(150),
	@PublicId VARCHAR(150),
	@PreferredNodeIds VARCHAR(150),
	@Type SMALLINT,
	@SupervisorId INT,
	@BeginDate DateTime,
	@CanSkipPreferredNodes BIT,
	@TaskSubtype VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Type IN (8,9,10,15) -- FixFormats, FixReplication, RemoveOldMaterials, RemoveOldTasks
	BEGIN
		if EXISTS(SELECT Id FROM dbo.Task WHERE [Type] = @Type AND [Status] IN (0,5) AND ISNULL([TaskSubtype], '') = ISNULL(@TaskSubtype, ''))
			RETURN -43; -- AlreadyExists
	END
	
	IF @UniqueId IS NOT NULL AND @Type NOT IN (12, 13) -- AddFormat, UpdateFormat
		IF NOT EXISTS(SELECT Id FROM dbo.Format WHERE UniqueId = @UniqueId)
				RETURN -53; --FormatNotFound

	IF @UniqueId IS NOT NULL AND  @PublicId IS NULL
		SET @PublicId = (SELECT PublicID FROM Material m
						INNER JOIN FormatGroup fg ON fg.MaterialId = m.Id
						INNER JOIN Format f ON f.FormatGroupId = fg.Id
						WHERE f.UniqueId = @UniqueId)
	
	DECLARE @ExistingTaskId BIGINT
	DECLARE @ExistingTaskType VARCHAR(50)
	
	IF @Type IN  (12, 13, 14) -- AddFormat, UpdateFormat, RemoveFormat
	BEGIN	
		IF NOT EXISTS(SELECT Id FROM Material WHERE PublicId = @PublicId AND Deleted = 0)
			RETURN -52; -- MaterialNotFolund
			
		IF EXISTS(SELECT t.Id FROM Task t  WHERE t.PublicId = @PublicId AND t.[Type] in (4,5,6) AND t.Status IN (0,5)) -- exists task from Material
			RETURN -45 -- inCompatibleState

		IF @Type = 12 AND EXISTS(SELECT f.id FROM Format f WHERE f.UniqueId = @UniqueId AND f.InternalStatus <> 4)
			RETURN  -64 --FormatExists

		SELECT @ExistingTaskId = Id, @ExistingTaskType = [Type] FROM Task WHERE UniqueId = @UniqueId AND  Status = 0 AND  [TYPE] IN  (12, 13, 14) 
		IF @ExistingTaskId IS NOT NULL 
		BEGIN
			IF @Type = 12 AND (@ExistingTaskType = 12 OR @ExistingTaskType = 13) -- error -can't add exisiting format
				RETURN  -64 --FormatExists
			IF @Type = 13 AND (@ExistingTaskType = 12 OR @ExistingTaskType = 13) --replace add or update with update
				EXEC dbo.RemoveTask @Id = @ExistingTaskId 
			IF @Type = 14 AND  @ExistingTaskType = 12 --replace Add with Remove
				EXEC dbo.RemoveTask @Id = @ExistingTaskId
			IF @Type = 14 AND @ExistingTaskType = 13 --replace Update with Remove	
				EXEC dbo.RemoveTask @Id = @ExistingTaskId
			IF @Type = 14 AND @ExistingTaskType = 14 --remove already submitted
				RETURN -43 -- AlreadyExists
			END
		END

		IF @TYPE IN (4, 5, 6) 	-- AddMaterial. UpdateMaterial, RemoveMaterial
		BEGIN
			SELECT @ExistingTaskId = Id, @ExistingTaskType = [Type]  FROM Task WHERE PublicId = @PublicId AND Status = 0  AND  [TYPE] IN  (4,5,6)
			IF @ExistingTaskId IS NOT NULL 
			BEGIN
				IF @Type = 4 -- AddMaterial submitted only by manager
					RETURN -43  -- skip 				
				IF @Type = 6 AND @ExistingTaskType = 6 --remove already submitted
					RETURN -43  -- AlreadyExists				
				EXEC dbo.RemoveTask @Id = @ExistingTaskId --replace old update or remove with new task
			END
					
			IF @Type IN (5,6) -- RemoveMaterial, UpdateMaterial
			BEGIN	-- remove all New tasks for this material's formats
				DECLARE @Tmp TABLE(Id_Task BIGINT PRIMARY KEY);

				INSERT @Tmp (Id_Task)
					SELECT  tI.Id 
						FROM dbo.Task tI 
						WHERE tI.UniqueId iS NOT NULL 
							AND tI.PublicId = @PublicId
							AND tI.Status = 0 --New

				DELETE from TaskData WHERE Id_Task IN (SELECT * FROM @Tmp)
				DELETE from TaskPreferredNode WHERE Id_Task IN (SELECT * FROM @Tmp)
				DELETE from Task WHERE Id IN (SELECT * FROM @Tmp)	
			END

			IF @Type = 5 --RemoveMaterial			
				EXEC dbo.SetMaterialDeleted @Id = NULL, @PublicId = @PublicId, @Deleted = 1
		END
	

	INSERT dbo.Task
		(UniqueId, PublicId, [Type], SupervisorId, BeginDate, CanSkipPreferredNodes, TaskSubtype)
		VALUES(@UniqueId, @PublicId, @Type, @SupervisorId, @BeginDate, @CanSkipPreferredNodes, @TaskSubtype);
	SET @Id = SCOPE_IDENTITY();

	if @PreferredNodeIds IS NOT NULL
	BEGIN
		INSERT dbo.TaskPreferredNode
			(Id_Task, NodeId)
			SELECT @Id, CAST(Part AS INT) FROM dbo.fnctSplitString(@PreferredNodeIds, ';');
	END

	RETURN 1;
END