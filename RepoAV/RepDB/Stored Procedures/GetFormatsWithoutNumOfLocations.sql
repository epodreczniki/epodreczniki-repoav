CREATE PROCEDURE [dbo].[GetFormatsWithoutNumOfLocations]
	@MinNumOfLocations INT,
	@Offset INT, 
	@Count INT, 
	@Total INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT @Total = count(f.Id)
		FROM dbo.[Format] f
		WHERE f.[Status] <> 2 AND f.InternalStatus = 5 --Added
			AND (SELECT count(l.NodeId) FROM dbo.Location l INNER JOIN dbo.Node n ON l.NodeId = n.Id WHERE l.FormatId = f.Id AND n.[Role] IN (0, 3)) < @MinNumOfLocations;

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
		WHERE f.[Status] <> 2 AND f.InternalStatus = 5 --Added
			AND (SELECT count(l.NodeId) FROM dbo.Location l INNER JOIN dbo.Node n ON l.NodeId = n.Id WHERE l.FormatId = f.Id AND n.[Role] IN (0, 3)) < @MinNumOfLocations
		ORDER BY f.CreateDate ASC
			OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY;

	RETURN 1;
END


