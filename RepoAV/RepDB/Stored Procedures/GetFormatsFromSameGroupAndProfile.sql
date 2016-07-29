CREATE PROCEDURE [dbo].[GetFormatsFromSameGroupAndProfile]
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id_FormatGroup INT;
	DECLARE @Id_ProfileGroup INT;

	SELECT @Id_FormatGroup = f.FormatGroupId, @Id_ProfileGroup = p.Id_ProfileGroup 
		FROM dbo.[Format] f LEFT JOIN dbo.[Profile] p ON f.ProfileId = p.Id
		WHERE f.UniqueId = @UniqueId;

	IF @Id_FormatGroup IS NULL AND @Id_ProfileGroup IS NULL
		RETURN -53;--FormatNotFound

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
			dbo.[Profile] p ON f.ProfileId = p.Id INNER JOIN
			dbo.FormatGroup fg ON f.FormatGroupId = fg.Id INNER JOIN
			dbo.Material m ON fg.MaterialId = m.Id
		WHERE f.FormatGroupId = @Id_FormatGroup
			AND p.Id_ProfileGroup = @Id_ProfileGroup;

	RETURN 1;
END


