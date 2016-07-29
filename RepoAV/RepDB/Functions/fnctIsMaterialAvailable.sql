CREATE FUNCTION [dbo].[fnctIsMaterialAvailable]
(
	@Id_Material INT
)
RETURNS BIT
AS
BEGIN
	IF EXISTS ((SELECT f.Id FROM Format f
				LEFT JOIN Location l on l.FormatId = f.Id
				INNER JOIN FormatGroup fg on fg.Id = f.FormatGroupId
				INNER JOIN Material m on m.id = fg.MaterialId
				LEFT JOIN Node n on n.Id = l.NodeId AND dbo.fnctIsNodeOnline(n.Id) = 1
				WHERE m.Id = @Id_Material
				GROUP BY f.Id
				HAVING COUNT(n.Id) < 1) )
		RETURN 0;
	RETURN 1;
END
