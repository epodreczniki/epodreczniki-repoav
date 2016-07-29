CREATE PROCEDURE [dbo].[GetSourceUrl4Format]
	@UniqueId VARCHAR(150),
	@SourceUrl VARCHAR(500) OUTPUT,
	@SourceNodeId INT OUTPUT,
	@SourceNodeIP VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id INT;
	SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;

	SELECT TOP(1)	@SourceNodeId = l.NodeId, 
					@SourceUrl = dbo.fnctBuildAccessUrl(n.Id, 1, @UniqueId),
					@SourceNodeIP = n.InternalAddress
		FROM dbo.Location l INNER JOIN
			dbo.Node n ON l.NodeId = n.Id
		WHERE FormatId = @Id
		ORDER BY n.[Role];--SNode preferowany

	RETURN 1;
END


