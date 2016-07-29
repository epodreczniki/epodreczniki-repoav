CREATE PROCEDURE [dbo].[RepeatTask]
	@Id_Task bigint

AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Type SMALLINT, @UniqueId VARCHAR(150), @PublicId VARCHAR(150), @CanSkipPreferredNodes BIT, @TaskSubtype VARCHAR(50), @Id BIGINT

	SELECT @UniqueId = UniqueId, @PublicId = PublicId, @Type = Type, @TaskSubtype = TaskSubtype, @CanSkipPreferredNodes = CanSkipPreferredNodes
		FROM Task WHERE Id = @Id_Task

	IF @Type IS NULL
		RETURN -41;  --NotFound

    INSERT TASK (UniqueId, PublicId, Type, TaskSubtype, CanSkipPreferredNodes)
		VALUES(@UniqueId, @PublicId, @Type, @TaskSubtype, @CanSkipPreferredNodes)

	SET @Id = SCOPE_IDENTITY();
	
	INSERT INTO TaskData 
		SELECT @Id, [Key], Value FROM TaskData 
			WHERE Id_Task = @Id_Task AND [Key] <> 'Repeated'

	INSERT INTO TaskData
		VALUES(@Id, 'Repeated', CAST(@Id_Task as VARCHAR(150)))


   INSERT INTO TaskPreferredNode
	SELECT @Id, NodeId FROM TaskPreferredNode WHERE Id_Task = @Id_Task

END
GO


