CREATE PROCEDURE [dbo].[AddNode]
	@Id INT,
	@Role SMALLINT,
	@ExternalAddress VARCHAR(500),
	@InternalAddress VARCHAR(500),
	@Url VARCHAR(500),
	@Enabled BIT,
	@Name VARCHAR(50),
	@ProcaPortNumber INT
AS
BEGIN
	SET NOCOUNT ON;


	IF NOT EXISTS (SELECT * FROM dbo.Node WHERE Id = @Id)
	BEGIN
		INSERT dbo.Node
			([Id], [Role], [ExternalAddress], [InternalAddress], [Url],  [Enabled], [Name], [ProcaPortNumber])
			VALUES(@Id, @Role, @ExternalAddress, @InternalAddress, @Url,  @Enabled, @Name, @ProcaPortNumber);
	END
	ELSE
	BEGIN
		EXEC dbo.ChangeNode
			@Id = @Id,
			@Role = @Role,
			@ExternalAddress = @ExternalAddress,
			@InternalAddress = @InternalAddress,
			@Url = @Url,
			@Name = @Name,
			@ProcaPortNumber = @ProcaPortNumber
	END
	RETURN 1;
END
