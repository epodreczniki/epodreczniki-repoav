CREATE PROCEDURE [dbo].[GetMaterial]
	@Id INT OUTPUT,
	@Title NVARCHAR(500) OUTPUT,
	@Duration INT OUTPUT,
	@AllowDistribution BIT OUTPUT,
	@Deleted BIT OUTPUT,
	@MaterialType SMALLINT OUTPUT, 
    @PublicId VARCHAR(150) OUTPUT ,
	@CreatedDate DateTime OUTPUT ,
	@ModifyDate DateTime OUTPUT,
	@Status SMALLINT OUTPUT,
	@Tags NVARCHAR(MAX) OUTPUT,
	@Metadata XML OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.Material WHERE PublicId = @PublicId;

	IF @Id IS NULL
		RETURN -41; --NotFound

	SELECT	@Title = Title, 
			@Duration = Duration, 
			@AllowDistribution = AllowDistribution, 
			@Deleted = Deleted,
			@MaterialType = MaterialType, 
			@PublicId = PublicId,
			@CreatedDate = CreatedDate,
			@ModifyDate = ModifyDate,
			@Status = MaterialStatus,
			@Metadata = Metadata,
			@Tags = dbo.fnctGetTags4MaterialAsString(Id)
		FROM dbo.Material
		WHERE Id = @Id;

	RETURN 1;
END


