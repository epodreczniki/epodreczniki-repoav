CREATE PROCEDURE [dbo].[AddFormatGroup]
	@Id INT OUTPUT,
	@MaterialId INT,
	@SubtitleId VARCHAR(150),
	@SourceId VARCHAR(150),
	@AudioId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Source INT, @Subtitle INT

	IF NOT EXISTS (SELECT * FROM dbo.Material WHERE Id = @MaterialId)
		RETURN -52; --MaterialNotFound

	IF @SubtitleId IS NOT NULL
	BEGIN
		SELECT @Subtitle = Id FROM dbo.[Format] WHERE UniqueId = @SubtitleId
		IF @Subtitle IS NULL
			RETURN -56; --SubtitleFormatNotFound
	END

	IF @SourceId IS NOT NULL
	BEGIN
		SELECT @Source = Id FROM dbo.[Format] WHERE UniqueId = @SourceId
		IF @Source IS NULL
			RETURN -53; --FormatNotFound
	END

	INSERT dbo.FormatGroup
		(MaterialId, SubtitleId, SourceId, AudioId)
		VALUES (@MaterialId, @Subtitle, @Source, @AudioId)
	SET @Id = SCOPE_IDENTITY();
	
	UPDATE Material SET ModifyDate = GETDATE() WHERE Id = @MaterialId

	RETURN 1;

END


