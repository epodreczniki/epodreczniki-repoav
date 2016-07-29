CREATE PROCEDURE [dbo].[SNodeStats]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @total BIGINT


	SELECT @total = SUM(TotalSpace) FROM Node WHERE Role <> 2

	SELECT n.Name , mt.Name as MaterialType, COUNT(f.Id) as FormatCount, SUM(ISNULL(f.Size,0))/1024.0/1024 as FormatSize, n.TotalSpace
		FROM Node n
		INNER JOIN Location l ON l.NodeId = n.Id
		LEFT JOIN Format f ON f.Id = l.FormatId
		LEFT JOIN FormatGroup fg ON fg.Id = f.FormatGroupId
		LEFT JOIN Material m ON m.Id = fg.MaterialId
		LEFT JOIN MaterialType mt ON mt.Id = m.MaterialType
		WHERE n.Role in (0,3)
		GROUP BY n.Name, mt.Name, n.TotalSpace
   UNION
	SELECT Name, 'Wolne' as MaterialType, 0 as FormatCount, FreeSpace as FormatSize, TotalSpace
		FROM Node 
		WHERE Role in (0,3)
   UNION
	SELECT  'Razem', mt.Name, COUNT(f.Id) as FormatCount, SUM(ISNULL(f.Size,0))/1024.0/1024 as FormatSize, @total as TotalSpace
	FROM Format f
	LEFT JOIN Location l ON l.FormatId = f.Id
	INNER JOIN FormatGroup fg ON fg.Id = f.FormatGroupId
	INNER JOIN Material m ON m.Id = fg.MaterialId
	INNER JOIN MaterialType mt ON mt.Id = m.MaterialType
	where l.NodeId > 1
	GROUP BY mt.Name
    UNION
	SELECT 'Razem', 'Wolne', 0, Sum(FreeSpace), Sum(TotalSpace) 
		FROM Node where Role <> 2
END