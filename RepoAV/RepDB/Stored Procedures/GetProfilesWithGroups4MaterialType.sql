CREATE PROCEDURE [dbo].[GetProfilesWithGroups4MaterialType]
	@MaterialType SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.Id, p.[Name], pg.MaterialType, p.MinHeight, p.MinWidth, p.Apect, p.Mime, p.Id_ProfileGroup, pg.OperationXML, pg.Name as GroupName, pg.[Enabled], pg.TaskSubtype, pg.DownloadSourceFiles
		FROM dbo.[Profile] p INNER JOIN
			dbo.ProfileGroup pg ON p.Id_ProfileGroup = pg.Id
		WHERE (@MaterialType IS NULL OR pg.MaterialType = @MaterialType)
			AND pg.[Enabled] = 1
		ORDER BY pg.Id, p.[Name];

END
