CREATE PROCEDURE [dbo].[GetFormatLocations]
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id INT;
	SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;

	SELECT NodeId
		FROM dbo.Location
		WHERE FormatId = @Id;

	RETURN 1;
END


