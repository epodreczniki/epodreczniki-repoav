CREATE PROCEDURE [dbo].[ChangeNode]
	@Id INT,
	@Role SMALLINT,
	@ExternalAddress VARCHAR(500),
	@InternalAddress VARCHAR(500),
	@Url VARCHAR(500),
	@Name VARCHAR(50),
	@ProcaPortNumber INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Node
		SET [Role] = @Role, 
			[ExternalAddress] = @ExternalAddress, 
			[InternalAddress] = @InternalAddress, 
			[Url] = @Url,
			[Name] = @Name,
			[ProcaPortNumber] = @ProcaPortNumber
		WHERE Id =@Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;
END
