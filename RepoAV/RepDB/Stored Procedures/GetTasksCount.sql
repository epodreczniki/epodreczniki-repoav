CREATE PROCEDURE [dbo].[GetTasksCount]
	@Type SMALLINT,
	@Statuses VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Tmp TABLE(Stat SMALLINT);

	IF @Statuses IS NOT NULL
		INSERT @Tmp
			(Stat)
			SELECT CAST(Part as SMALLINT) FROM dbo.fnctSplitString(@Statuses, ';');


	SELECT count(Id) as Number, ISNULL(ExecutingNodeId, -1) as Id_Node
		FROM dbo.Task
		WHERE (@Type IS NULL OR [Type] = @Type)
			AND (@Statuses IS NULL OR EXISTS(SELECT Stat FROM @Tmp WHERE Stat = [Status]))
		GROUP BY ISNULL(ExecutingNodeId, -1);

END
