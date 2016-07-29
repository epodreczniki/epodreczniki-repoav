CREATE PROCEDURE [dbo].[GetNode]
	@Id INT,
	@Role SMALLINT OUTPUT,
	@ExternalAddress VARCHAR(500) OUTPUT,
	@InternalAddress VARCHAR(500) OUTPUT,
	@Url VARCHAR(500) OUTPUT,
	@Enabled BIT OUTPUT,
	@FreeSpace BIGINT OUTPUT,
	@IsOnline BIT OUTPUT,
	@Name VARCHAR(50) OUTPUT,
	@TotalSpace BIGINT OUTPUT,
	@ProcaPortNumber INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	@Role = [Role],
			@ExternalAddress = [ExternalAddress],
			@InternalAddress = [InternalAddress],
			@Url = [Url],
			@FreeSpace = [FreeSpace],
			@Enabled = [Enabled],
			@IsOnline = dbo.fnctIsNodeOnline(@Id),
			@Name = [Name],
			@TotalSpace = TotalSpace,
			@ProcaPortNumber = @ProcaPortNumber
		FROM dbo.Node
		WHERE Id = @Id;

	IF @Role IS NULL
		RETURN -41; --NotFound

	RETURN 1;
END
