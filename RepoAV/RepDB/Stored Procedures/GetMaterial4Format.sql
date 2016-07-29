CREATE PROCEDURE [dbo].[GetMaterial4Format]
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

	SELECT	@Id = m.Id,
			@Title = Title, 
			@Duration = Duration, 
			@AllowDistribution = AllowDistribution, 
			@Deleted = Deleted,
			@MaterialType = MaterialType, 
			@PublicId = PublicId,
			@CreatedDate = CreatedDate,
			@ModifyDate = ModifyDate,
			@Status = MaterialStatus,
			@Tags = dbo.fnctGetTags4MaterialAsString(m.Id),
			@Metadata = m.Metadata
		FROM dbo.Material m
		INNER JOIN dbo.FormatGroup fg on fg.MaterialId = m.Id
		INNER JOIN dbo.Format f on f.FormatGroupId = fg.Id
		WHERE f.Id = @Id;

	RETURN 1;
END

