CREATE PROCEDURE [dbo].[GetProfilesFromGroup]
	@Id_ProfileGroup INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, [Name], MinHeight, MinWidth, Apect, Mime, Id_ProfileGroup
		FROM dbo.[Profile]
		WHERE Id_ProfileGroup = @Id_ProfileGroup
		ORDER BY [Name];

END
