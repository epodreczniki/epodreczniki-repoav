CREATE PROCEDURE [dbo].[GetFormat]
	@Id INT OUTPUT,
	@FormatGroupId INT OUTPUT,
	@ProfileId INT OUTPUT,
	@XmlMetadata NVARCHAR(MAX) OUTPUT,
	@Type SMALLINT OUTPUT,
	@UniqueId VARCHAR(150) OUTPUT,
	@Size BIGINT OUTPUT,
	@CreatedDate DATETIME OUTPUT,
	@Status SMALLINT OUTPUT,
	@InternalStatus SMALLINT OUTPUT,
	@AllowDistribution BIT OUTPUT,
	@Mime VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;

	SELECT @FormatGroupId = f.FormatGroupId, 
			@ProfileId = f.ProfileId, 
			@XmlMetadata = f.XmlMetadata, 
			@Type = f.[Type], 
			@UniqueId = f.UniqueId, 
			@Size = f.Size, 
			@CreatedDate = f.CreateDate, 
			@Status = f.[Status], 
			@InternalStatus = f.InternalStatus,
			@AllowDistribution = m.AllowDistribution,
			@Mime = f.Mime
		FROM dbo.[Format] f INNER JOIN
			dbo.FormatGroup fg on f.FormatGroupId = fg.Id INNER JOIN
			dbo.Material m ON fg.MaterialId = m.Id
		WHERE f.Id = @Id;

	IF @UniqueId IS NULL
		RETURN -41;

	RETURN 1;
END


