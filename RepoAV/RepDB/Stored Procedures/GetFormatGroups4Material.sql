CREATE PROCEDURE [dbo].[GetFormatGroups4Material]
	@PublicId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	fg.Id,
			fg.MaterialId, 
			fSub.UniqueId as SubtitleId, 
			fSource.UniqueId as SourceId, 
			fg.AudioId,
			m.PublicId
		FROM dbo.FormatGroup fg INNER JOIN
			dbo.Material m ON m.Id = fg.MaterialId LEFT JOIN
			dbo.[Format] fSub ON fg.SubtitleId = fSub.Id LEFT JOIN
			dbo.[Format] fSource ON fSource.Id = fg.SourceId
		WHERE m.PublicId = @PublicId;
END


