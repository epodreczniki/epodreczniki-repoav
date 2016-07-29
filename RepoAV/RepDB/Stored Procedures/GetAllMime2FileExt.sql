CREATE PROCEDURE [dbo].[GetAllMime2FileExt]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Mime, FileExtension
		FROM dbo.Mime2Extension
		ORDER BY Mime;
END
