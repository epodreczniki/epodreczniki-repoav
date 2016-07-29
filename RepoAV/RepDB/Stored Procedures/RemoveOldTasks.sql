CREATE PROCEDURE [dbo].[RemoveOldTasks]
	@KeepTasksWithError BIT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @OlderThen DateTime
	SELECT @OlderThen =  DATEADD(day,-CAST(ISNULL([Value], -1) AS smallint), GetDate())
		FROM dbo.GlobalData
		WHERE [Key] = 'MaxTaskAge'
	
	IF @KeepTasksWithError = 1
	BEGIN
		DELETE dbo.TaskData WHERE Id_Task IN (SELECT Id FROM dbo.Task WHERE [Status] = 10 AND FinishDate < @OlderThen);
		DELETE dbo.TaskPreferredNode WHERE Id_Task IN (SELECT Id FROM dbo.Task WHERE [Status] = 10 AND FinishDate < @OlderThen);
		DELETE dbo.Task WHERE [Status] = 10 AND FinishDate < @OlderThen;
	END
	ELSE
	BEGIN
		DELETE dbo.TaskData WHERE Id_Task IN (SELECT Id FROM dbo.Task WHERE [Status] > 5 AND ISNULL(FinishDate, CreatedDate) < @OlderThen);
		DELETE dbo.TaskPreferredNode WHERE Id_Task IN (SELECT Id FROM dbo.Task WHERE [Status] > 5 AND ISNULL(FinishDate, CreatedDate) < @OlderThen);
		DELETE dbo.Task WHERE [Status] > 5 AND ISNULL(FinishDate, CreatedDate) < @OlderThen;
	END

END
