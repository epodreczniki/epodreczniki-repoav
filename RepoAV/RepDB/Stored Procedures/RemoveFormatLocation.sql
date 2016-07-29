CREATE PROCEDURE [dbo].[RemoveFormatLocation]
	@UniqueId VARCHAR(150),
	@NodeId INT,
	@FreeSpace BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @FormatId INT;
	SELECT @FormatId = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @FormatId IS NULL
		RETURN -53; --FormatNotFound

	IF NOT EXISTS(SELECT * FROM dbo.Node WHERE Id = @NodeId)
		RETURN -61; -- NodeNotFound

	DELETE dbo.Location
	WHERE FormatId = @FormatId
		AND NodeId = @NodeId;

	IF @FreeSpace IS NOT NULL
		UPDATE dbo.Node
			SET FreeSpace = @FreeSpace
			WHERE Id = @NodeId;

	RETURN 1;
END


