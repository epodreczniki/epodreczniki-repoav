CREATE PROCEDURE [dbo].[AddTaskData]
	@Id_Task BIGINT,
	@Key VARCHAR(50),
	@Value NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Key IS NULL
		RETURN -1;--InvalidParameter

	IF NOT EXISTS(SELECT Id FROM dbo.Task WHERE Id = @Id_Task)
		RETURN -41; --NotFound

	IF EXISTS(SELECT Id_Task FROM dbo.TaskData WHERE Id_Task = @Id_Task AND [Key] = @Key)
		RETURN -43; --AlreadyExists


	INSERT dbo.TaskData
		(Id_Task, [Key], [Value])
		VALUES(@Id_Task, @Key, @Value);

	RETURN 1;
END
