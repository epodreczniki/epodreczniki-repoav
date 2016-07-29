CREATE PROCEDURE [dbo].[SetNodeEnabled]
	@Id_Node int,
	@Enabled BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CurrStat SMALLINT;
	IF NOT EXISTS(SELECT Id FROM dbo.Node WHERE Id = @Id_Node)
		RETURN -41;

	UPDATE dbo.Node
		SET	[Enabled] = @Enabled
		WHERE Id = @Id_Node;

	RETURN 1;
END
