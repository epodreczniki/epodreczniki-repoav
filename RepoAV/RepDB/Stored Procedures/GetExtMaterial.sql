CREATE PROCEDURE [dbo].[GetExtMaterial]
	@Id INT OUTPUT,
	@Title NVARCHAR(500) OUTPUT,
	@Duration INT OUTPUT,
	@AllowDistribution BIT OUTPUT,
	@MaterialType SMALLINT OUTPUT, 
    @PublicId VARCHAR(150) OUTPUT ,
	@CreatedDate DateTime OUTPUT ,
	@ModifyDate DateTime OUTPUT,
	@Status SMALLINT OUTPUT,
	@FormatGroupsCount INT OUTPUT,
	@Tags NVARCHAR(MAX) OUTPUT,
	@Metadata XML OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.Material WHERE PublicId = @PublicId;

	IF @Id IS NULL
		RETURN -41; --NotFound

	SELECT	@Title = m.Title, 
			@Duration = m.Duration, 
			@AllowDistribution = m.AllowDistribution, 
			@MaterialType = m.MaterialType, 
			@PublicId = m.PublicId,
			@CreatedDate = m.CreatedDate,
			@ModifyDate = m.ModifyDate,
			@Status = m.MaterialStatus,
			@FormatGroupsCount = (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id),
			@Metadata = Metadata,
			@Tags = dbo.fnctGetTags4MaterialAsString(m.Id)
		FROM dbo.Material m
		WHERE m.Id = @Id;

	RETURN 1;
END


