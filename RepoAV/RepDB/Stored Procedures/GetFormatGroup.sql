CREATE PROCEDURE [dbo].[GetFormatGroup]
	@Id INT,
	@MaterialId INT OUTPUT,
	@SubtitleId VARCHAR(150) OUTPUT,
	@SourceId VARCHAR(150) OUTPUT,
	@AudioId INT OUTPUT,
	@PublicId VARCHAR(150) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	@MaterialId = m.Id, 
			@SubtitleId = fSub.UniqueId, 
			@SourceId = fSource.UniqueId, 
			@AudioId = fg.AudioId,
			@PublicId = m.PublicId
		FROM dbo.FormatGroup fg INNER JOIN
			dbo.Material m ON m.Id = fg.MaterialId LEFT JOIN
			dbo.[Format] fSub ON fg.SubtitleId = fSub.Id LEFT JOIN
			dbo.[Format] fSource ON fSource.Id = fg.SourceId
		WHERE fg.Id = @Id;

	IF @MaterialId IS NULL
		RETURN -41;
	ELSE
		RETURN 1;
END


