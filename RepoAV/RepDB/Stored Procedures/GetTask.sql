CREATE PROCEDURE [dbo].[GetTask]
	@Id BIGINT,
	@Status SMALLINT OUTPUT,
	@Result NVARCHAR(MAX) OUTPUT,
	@UniqueId VARCHAR(150) OUTPUT,
	@PublicId VARCHAR(150) OUTPUT,
	@ExecutingNodeId INT OUTPUT,
	@CreatedDate DATETIME OUTPUT,
	@TakenDate DATETIME OUTPUT,
	@FinishDate DATETIME OUTPUT,
	@Type SMALLINT OUTPUT, 
    @SupervisorId INT OUTPUT,
    @ResultProcessed BIT OUTPUT, 
	@BeginDate DATETIME OUTPUT,
	@PreferredNodeIds VARCHAR(150) OUTPUT,
	@CanSkipPreferredNodes BIT OUTPUT,
	@TaskSubtype VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	@Status = t.[Status],
			@Result = t.Result,
			@UniqueId = t.UniqueId,
			@PublicId = t.PublicId,
			@ExecutingNodeId = t.ExecutingNodeId,
			@CreatedDate = t.CreatedDate,
			@TakenDate = t.TakenDate,
			@FinishDate = t.FinishDate,
			@Type = t.[Type],
			@SupervisorId = t.SupervisorId,
			@ResultProcessed = t.ResultProcessed,
			@BeginDate = t.BeginDate,
			@PreferredNodeIds = dbo.fnctGetNodePreferred4TaskAsString(t.Id),
			@CanSkipPreferredNodes = t.CanSkipPreferredNodes,
			@TaskSubtype = t.TaskSubtype
		FROM dbo.Task t 
		WHERE t.Id = @Id;

	IF @Status IS NULL
		RETURN -41;

	SELECT [Key], Value
		FROM dbo.TaskData
		WHERE Id_Task = @Id;

	RETURN 1;
END
