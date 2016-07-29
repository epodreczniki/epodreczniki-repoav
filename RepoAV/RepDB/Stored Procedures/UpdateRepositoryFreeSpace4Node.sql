CREATE PROCEDURE [dbo].[UpdateRepositoryFreeSpace4Node]
	@Id_Node int,
	@FreeSpace BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Node
		SET	FreeSpace = @FreeSpace
		WHERE Id = @Id_Node;
END
