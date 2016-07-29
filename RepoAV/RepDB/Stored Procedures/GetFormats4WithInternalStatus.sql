CREATE PROCEDURE [dbo].[GetFormats4WithInternalStatus]
	@FormatInternalStatus SMALLINT,
	@Offset INT, 
	@Count INT, 
	@Total INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS(SELECT Id FROM dbo.FormatInternalStatus WHERE Id = @FormatInternalStatus)
		RETURN -41; -- NotFound

	SELECT @Total = count(f.Id)
		FROM dbo.[Format] f 
		WHERE f.InternalStatus = @FormatInternalStatus;

	SELECT  f.Id,
			f.FormatGroupId, 
			f.ProfileId, 
			f.XmlMetadata, 
			f.[Type], 
			f.UniqueId, 
			f.Size, 
			f.CreateDate AS CreatedDate, 
			f.[Status], 
			f.InternalStatus,
			m.AllowDistribution,
			f.Mime
		FROM dbo.[Format] f INNER JOIN
			dbo.FormatGroup fg ON f.FormatGroupId = fg.Id INNER JOIN
			dbo.Material m ON fg.MaterialId = m.Id
		WHERE f.InternalStatus = @FormatInternalStatus
		ORDER BY f.CreateDate ASC
			OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY;

	RETURN 1;
END


