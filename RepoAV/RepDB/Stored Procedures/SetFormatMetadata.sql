CREATE PROCEDURE [dbo].[SetFormatMetadata]
	@UniqueId VARCHAR(150),
	@XmlMetadata NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.[Format] WHERE UniqueId = @UniqueId)
		RETURN -41; --NotFOund

	UPDATE dbo.[Format]
		SET XmlMetadata = @XmlMetadata
		WHERE UniqueId = @UniqueId;

	UPDATE Material SET ModifyDate = GETDATE() 
		WHERE Id = (SELECT Material.Id FROM Material INNER JOIN FormatGroup fg ON fg.MaterialId = Material.Id INNER JOIN Format f on f.FormatGroupId = fg.Id WHERE f.UniqueId = @UniqueId)
	
	RETURN 1;
END


