CREATE PROCEDURE [dbo].[GetFormats4Sync]
	@Id_Node INT, 
	@Offset INT, 
	@Count INT, 
	@Total INT OUTPUT
AS
BEGIN
	SET @Total = 0;
	
	SELECT	@Total = count(f.Id)
		FROM dbo.[Format] f INNER JOIN 
			dbo.Location l ON l.FormatId = f.Id
		WHERE l.NodeId = @Id_Node;
		
		
	SELECT	f.UniqueId, f.Size, m.AllowDistribution, f.[Status]
		FROM dbo.[Format] f INNER JOIN 
			dbo.Location l ON l.FormatId = f.Id LEFT JOIN
			dbo.FormatGroup fg ON fg.Id = f.FormatGroupId LEFT JOIN
			dbo.Material m ON fg.MaterialId = m.Id
		WHERE l.NodeId = @Id_Node
		ORDER BY f.CreateDate ASC
		OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY;
END
