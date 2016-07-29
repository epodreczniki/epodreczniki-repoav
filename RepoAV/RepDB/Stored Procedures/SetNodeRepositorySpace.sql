CREATE PROCEDURE [dbo].[SetNodeRepositorySpace]
	@Id_Node int,
	@FreeSpace BIGINT,
	@TotalSpace BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CurrStat SMALLINT;
	IF NOT EXISTS(SELECT Id FROM dbo.Node WHERE Id = @Id_Node)
		RETURN -41;

	UPDATE dbo.Node
		SET	[FreeSpace] = @FreeSpace,
			TotalSpace = @TotalSpace
		WHERE Id = @Id_Node ;

	RETURN 1;
END
