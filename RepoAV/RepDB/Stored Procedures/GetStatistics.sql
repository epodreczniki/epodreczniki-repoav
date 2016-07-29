CREATE PROCEDURE [dbo].[GetStatistics]
	@MaterialsCount INT OUTPUT,
	@VideoMaterialsCount INT OUTPUT,
	@AudioMaterialCount INT OUTPUT,
	@UncompletedMaterialsCount INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
		
	SELECT @MaterialsCount = count(Id)
		FROM dbo.Material 
		WHERE Deleted = 0;;

	SELECT @VideoMaterialsCount = count(Id)
		FROM dbo.Material
		WHERE MaterialType = 2 AND Deleted = 0;

	SELECT @AudioMaterialCount = count(Id)
		FROM dbo.Material
		WHERE MaterialType = 1 AND Deleted = 0;;

	SELECT @UncompletedMaterialsCount = count(m.Id)
		FROM dbo.Material m
		WHERE EXISTS(SELECT f.Id FROM dbo.[Format] f INNER JOIN dbo.FormatGroup fg ON f.FormatGroupId = fg.Id WHERE fg.MaterialId = m.Id AND f.InternalStatus <> 5)
		 AND Deleted = 0;

	RETURN 0
END
