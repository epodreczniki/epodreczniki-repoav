CREATE PROCEDURE [dbo].[GetFormatLocationsExt]
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id INT;
	SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;

	SELECT l.NodeId, n.[Role], dbo.fnctIsNodeOnline(l.NodeId) AS IsOnLine
		FROM dbo.Location l INNER JOIN
			dbo.Node n ON l.NodeId = n.Id
		WHERE FormatId = @Id
		ORDER BY n.[Role];

	RETURN 1;
END


