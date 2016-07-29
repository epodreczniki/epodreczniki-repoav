CREATE PROCEDURE [dbo].[GetFormatsFromSameMaterial]
	@UniqueId VARCHAR(150) 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id_Material INT;

	SELECT @Id_Material = fg.MaterialId FROM dbo.[Format] f INNER JOIN dbo.FormatGroup fg ON f.FormatGroupId = fg.Id WHERE f.UniqueId = @UniqueId;

	IF @Id_Material IS NULL
		RETURN -52;--MaterialNotFound

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
		WHERE fg.MaterialId = @Id_Material;
	RETURN 1;
END


