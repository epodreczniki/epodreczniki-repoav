CREATE FUNCTION [dbo].[fnctGetFileExt4Mime]
(
	@Mime VARCHAR(50)
)
RETURNS VARCHAR(10)
AS
BEGIN
	DECLARE @FileExtension VARCHAR(10);

	SELECT @FileExtension = FileExtension 
		FROM dbo.Mime2Extension
		WHERE Mime = @Mime

	IF @FileExtension IS NULL
		SELECT @FileExtension = FileExtension 
			FROM dbo.Mime2Extension
			WHERE Mime = '*';

	RETURN @FileExtension;
END
