CREATE PROCEDURE [dbo].[MaterialStats] 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT  CASE WHEN GROUPING(a.Month) = 1  THEN '9999-1-01' ELSE a.Month END AS Month
		,CASE WHEN GROUPING(mt.Name) = 1 THEN 'Wszystkie' ELSE mt.Name END as MaterialType
		,COUNT(a.Id) as MaterialCount
		,SUM(a.Duration) as TotalDuration
		,SUM(a.FormatGroupCount) as FormatGroupCount
		,SUM(a.FormatCount)as FormatCount
		,SUM(Size) as TotalSize
	FROM
	(SELECT  m.MaterialType,  m.Id, m.Duration/1000.0/60/60 as Duration , Count(distinct fg.Id) as FormatGroupCount, Count(f.Id) as FormatCount, Sum(f.size)/1024.0/1024 as Size,
		CAST(YEAR(m.CreatedDate) as CHAR(4))+'-'+ CAST(MONTH(m.CreatedDate) AS CHAR(2))+'-01' as Month
		FROM Material m
			INNER JOIN MaterialType mt on mt.Id = m.MaterialType
			INNER JOIN FormatGroup fg on fg.MaterialId = m.Id
			INNER JOIN Format f on f.FormatGroupId = fg.Id
			GROUP BY m.MaterialType, m.Id, m.Duration, YEAR(m.CreatedDate), MONTH(m.CreatedDate)) a
	INNER JOIN MaterialType mt on mt.Id = a.MaterialType
	GROUP BY CUBE (a.Month, mt.Name)

END