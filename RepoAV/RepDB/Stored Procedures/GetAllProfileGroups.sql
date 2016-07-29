CREATE PROCEDURE [dbo].[GetAllProfileGroups]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, [Name], OperationXML, [Enabled], MaterialType, TaskSubtype
		FROM dbo.[ProfileGroup]
		ORDER BY [Name];

END
