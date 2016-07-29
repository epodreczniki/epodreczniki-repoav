CREATE PROCEDURE [dbo].[RemoveProfile]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT * FROM dbo.[Format] WHERE ProfileId = @Id)
		RETURN -54;--FormatWithProfileExists

	DELETE dbo.[Profile] WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
