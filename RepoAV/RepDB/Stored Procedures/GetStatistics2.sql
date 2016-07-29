CREATE PROCEDURE [dbo].[GetStatistics2]

AS
	SELECT * FROM
		(SELECT CONCAT('Liczba materiałów', CASE WHEN a.Status IS NOT NULL THEN ' w stanie ' END,
			CASE WHEN a.Status = 1 THEN 'InvalidFile' 
		 		 WHEN a.Status = 2 THEN 'NotFound' 
				 WHEN a.Status = 3 THEN 'RecError' 
				 WHEN a.Status = 4 THEN 'AddError'
				 WHEN a.Status = 5 THEN 'RmPending'
				 WHEN a.Status = 6 THEN 'Recoding'
				 WHEN a.Status = 7 THEN 'Adding'
				 WHEN a.Status = 8 THEN 'Added'
				 WHEN a.Status = 10 THEN 'Recoded' END) AS Name
				, COUNT(a.Id) AS Count FROM
		(SELECT m.Id, m.MaterialStatus AS Status
		FROM Material m) a
		GROUP BY ROLLUP(a.Status) 
	UNION
	SELECT CASE WHEN a.LocCount IS NULL THEN 'Liczba formatów' ELSE  CONCAT('Liczba formatów z liczbą replik ', a.LocCount) END as Name, 
		COUNT(a.Id) AS Count FROM 
		(SELECT f.Id as Id, COUNT(l.NodeId) as LocCount
			FROM Format f
			LEFT JOIN dbo.Location l on l.FormatId = f.Id
			LEFT JOIN dbo.Node n on n.Id = l.NodeId AND dbo.fnctIsNodeOnline(n.Id) = 1
			WHERE  (n.Role in (0,3) or n.Role IS NULL)
		GROUP BY f.Id) a
	GROUP BY ROLLUP(LocCount)
	UNION
	SELECT 'Czas pozostały do naprawy (h)' as Name,  DATEDIFF(minute, GETDATE(),  MAX(BeginDate)) as Count
		FROM Task t WHERE t.Type = 8
	UNION 
	SELECT 'Czas pozostały do replikacji (h)' as Name,  DATEDIFF(minute, GETDATE(),  MAX(BeginDate)) as Count
		FROM Task t  WHERE t.Type = 9) c
	ORDER BY c.Name DESC

