CREATE PROCEDURE [dbo].[GetFormat]
	@Id INT OUTPUT,
    @UniqueId  VARCHAR (150) OUTPUT,
    @Location  VARCHAR (200) OUTPUT,
    @Status	SMALLINT OUTPUT,
    @Size BIGINT OUTPUT,
    @Mime VARCHAR(50) OUTPUT, 
    @AllowDistribution BIT OUTPUT,
	@RealSize BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;

	SELECT @UniqueId = UniqueId, @Location = Location, @Status = [Status], @Size = Size, @Mime = Mime, @AllowDistribution = AllowDistribution, @RealSize = RealSize
		FROM dbo.[Format]
		WHERE Id = @Id;

	IF @UniqueId IS NULL
		RETURN -41;
	ELSE
		RETURN 1;
END
