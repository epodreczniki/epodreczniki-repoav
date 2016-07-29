CREATE PROCEDURE [dbo].[GetNodesInRole]
	@Role SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	[Role],
			Id,
			[ExternalAddress],
			[InternalAddress],
			[Url],			
			[FreeSpace],
			[Enabled],
			[Name],
			[TotalSpace],
			dbo.fnctIsNodeOnline(Id) as IsOnline,
			[ProcaPortNumber]
		FROM dbo.Node
		WHERE @Role IS NULL 
			OR [Role] = @Role;
END
