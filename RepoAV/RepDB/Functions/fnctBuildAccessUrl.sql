CREATE FUNCTION [dbo].[fnctBuildAccessUrl]
(
	@Id_Node INT,
	@Internal BIT,
	@UniqueId VARCHAR(150)
)
RETURNS VARCHAR(500)
AS
BEGIN
	DECLARE @Url VARCHAR(500);

	SELECT @Url = REPLACE(REPLACE(n.Url, '{IpAddress}', (CASE WHEN @Internal = 1 THEN n.InternalAddress ELSE n.ExternalAddress END)), '{UniqueId}', @UniqueId)
		FROM dbo.Node n 
		WHERE n.Id = @Id_Node;

	RETURN @Url;
END
