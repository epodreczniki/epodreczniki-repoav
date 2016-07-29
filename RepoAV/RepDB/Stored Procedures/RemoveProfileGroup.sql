CREATE PROCEDURE [dbo].[RemoveProfileGroup]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT * FROM dbo.[Format] WHERE ProfileId IN (SELECT Id FROM dbo.Profile WHERE Id_ProfileGroup = @Id))
		RETURN -54;--FormatWithProfileExists

	DELETE dbo.[Profile] WHERE Id_ProfileGroup = @Id;
	DELETE dbo.[ProfileGroup] WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
