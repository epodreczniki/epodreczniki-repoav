CREATE PROCEDURE [dbo].[GetAllGlobalData]
AS
BEGIN
	SET NOCOUNT ON;
	SELECT [Value], [Description], [Key]
		FROM [dbo].[GlobalData]
		ORDER BY [Key];
END