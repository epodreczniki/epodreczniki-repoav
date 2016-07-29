CREATE PROCEDURE [dbo].[UpdateNodeCheckTime]
	@Id_Node int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Node
		SET	CheckTime = GetDate()
		WHERE Id = @Id_Node 

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;--NotFound
END
