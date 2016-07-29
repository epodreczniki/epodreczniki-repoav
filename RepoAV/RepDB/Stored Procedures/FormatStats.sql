CREATE  PROCEDURE [dbo].[FormatStats]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT a.MaterialType, a.Type, a.FormatCount, a.Size, a.Rank FROM
		(SELECT mt.Name as MaterialType, LOWER(ft.Name) as Type, COUNT(f.Id) as FormatCount, SUM(f.Size)/1024.0/1024 as Size, 1 as Rank
			FROM Format f
			INNER JOIN FormatType ft ON ft.Id = f.Type
			INNER JOIN FormatGroup fg on fg.Id = f.FormatGroupId
			INNER JOIN Material m on m.Id = fg.MaterialId
			INNER JOIN MaterialType mt on mt.Id = m.MaterialType
			WHERE f.Type NOT IN (1, 4)  -- Relate or Recoded
			GROUP BY mt.Name, ft.Name, ft.Id
		UNION
		SELECT mt.Name as MaterialType, 
			REPLACE(SUBSTRING(f.UniqueId, CHARINDEX(',', f.UniqueId, CHARINDEX(',',f.UniqueId,1)+1)+1, 100),')','') as Type,
			COUNT(f.Id) as FormatCount, SUM(f.Size)/1024.0/1024 as Size, 2 as Rank
			FROM Format f
			INNER JOIN FormatType ft ON ft.Id = f.Type
			INNER JOIN FormatGroup fg on fg.Id = f.FormatGroupId
			INNER JOIN Material m on m.Id = fg.MaterialId
			INNER JOIN MaterialType mt on mt.Id = m.MaterialType
			WHERE f.Type IN (1,4) --Related
			GROUP BY mt.Name, REPLACE(SUBSTRING(f.UniqueId, CHARINDEX(',', f.UniqueId, CHARINDEX(',',f.UniqueId,1)+1)+1, 100),')','')
			) a
	ORDER BY  a.MaterialType, a.Rank
		
END