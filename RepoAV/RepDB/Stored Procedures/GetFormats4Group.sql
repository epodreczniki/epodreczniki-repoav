CREATE PROCEDURE [dbo].[GetFormats4Group]
	@FormatGroupId INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS(SELECT Id FROM dbo.FormatGroup WHERE Id = @FormatGroupId)
		RETURN -41; -- NotFound

	SELECT  f.Id,
			f.FormatGroupId, 
			f.ProfileId, 
			f.XmlMetadata, 
			f.[Type], 
			f.UniqueId, 
			f.Size, 
			f.CreateDate, 
			f.[Status], 
			f.InternalStatus,
			m.AllowDistribution,
			f.Mime
		FROM dbo.[Format] f INNER JOIN
			dbo.FormatGroup fg ON f.FormatGroupId = fg.Id INNER JOIN
			dbo.Material m ON fg.MaterialId = m.Id
		WHERE f.FormatGroupId = @FormatGroupId;

	RETURN 1;
END


