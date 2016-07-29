CREATE PROCEDURE [dbo].[GetOperationXml4Profile]
	@Id_Profile INT,
	@OperationXML NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT @OperationXML = pg.OperationXML
		FROM dbo.[Profile] p INNER JOIN
			dbo.ProfileGroup pg ON p.Id_ProfileGroup = pg.Id
		WHERE p.Id = @Id_Profile;
END
