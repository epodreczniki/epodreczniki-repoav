CREATE PROCEDURE [dbo].[GetFileExtension4Mime]
	@Mime VARCHAR(50),
	@FileExtension VARCHAR(10) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @FileExtension = dbo.fnctGetFileExt4Mime(@Mime);
END
