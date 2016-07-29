CREATE PROCEDURE [dbo].[GetFileExtension4Profile]
	@Id_Profile INT ,
	@FileExtension VARCHAR(10) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Mime VARCHAR(50);

	SELECT 	@Mime = Mime
		FROM dbo.[Profile] p 
		WHERE p.Id = @Id_Profile;

	IF @Mime IS NULL
		RETURN -41;

	SET @FileExtension = dbo.fnctGetFileExt4Mime(@Mime);

	RETURN 1;
END
