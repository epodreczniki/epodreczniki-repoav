CREATE PROCEDURE [dbo].[RenewTask]
	@Id_Task bigint,
	@ExecutingNodeId INT
AS
BEGIN
	SET NOCOUNT ON;

	IF @ExecutingNodeId IS NOT NULL
		IF NOT EXISTS (SELECT Id FROM dbo.Node WHERE Id = @ExecutingNodeId)
			RETURN -61;--NodeNotFound

	UPDATE dbo.Task
		SET	[Status] = 0,
			Result = NULL,
			FinishDate = NULL,
			TakenDate = NULL,
			LastActivityDate = NULL,
			ExecutingNodeId = @ExecutingNodeId,
			SupervisorId = NULL,
			CreatedDate = GetDate(),
			ResultProcessed = 0
		WHERE Id = @Id_Task;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;--NotFOund
END
