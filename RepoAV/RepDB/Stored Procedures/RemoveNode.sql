CREATE PROCEDURE [dbo].[RemoveNode]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Task 
		SET [Status] = 0,
			ExecutingNodeId = NULL
		WHERE [Status] IN (0, 5)
			AND ExecutingNodeId = @Id;

	UPDATE dbo.Task 
		SET ExecutingNodeId = NULL
		WHERE [Status] IN (10, 66)
			AND ExecutingNodeId = @Id;
						
	DELETE dbo.Node WHERE Id = @Id;
	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
