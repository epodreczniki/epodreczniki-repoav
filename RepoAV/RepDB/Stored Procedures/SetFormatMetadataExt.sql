CREATE PROCEDURE [dbo].[SetFormatMetadataExt]
	@UniqueId VARCHAR(150),
	@XmlMetadata NVARCHAR(MAX),
	@Duration INT,
	@Size BIGINT,
	@Mime VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.[Format] WHERE UniqueId = @UniqueId)
		RETURN -41; --NotFOund

	UPDATE dbo.[Format]
		SET XmlMetadata = ISNULL(@XmlMetadata, XmlMetadata),
			Size = (CASE WHEN (@Size IS NOT NULL AND @Size > 0) THEN @Size ELSE Size END),
			Mime = ISNULL(@Mime, Mime)
		WHERE UniqueId = @UniqueId;

	IF @Duration IS NOT NULL AND @Duration > 0
	BEGIN
		DECLARE @Id_Material INT;
		SELECT @Id_Material = fg.MaterialId FROM dbo.[Format] f INNER JOIN dbo.FormatGroup fg ON f.FormatGroupId = fg.Id WHERE f.UniqueId = @UniqueId;

		IF @Id_Material IS NOT NULL
			UPDATE dbo.Material	
				SET Duration = @Duration
				WHERE Id = @Id_Material;
	END

	UPDATE Material SET ModifyDate = GETDATE() 
		WHERE Id = (SELECT Material.Id FROM Material INNER JOIN FormatGroup fg ON fg.MaterialId = Material.Id INNER JOIN Format f on f.FormatGroupId = fg.Id WHERE f.UniqueId = @UniqueId)

	RETURN 1;
END


