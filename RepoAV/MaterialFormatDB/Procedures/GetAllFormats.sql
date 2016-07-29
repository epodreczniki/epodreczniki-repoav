CREATE PROCEDURE [dbo].[GetAllFormats]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id, UniqueId, Location, [Status], Size, Mime, AllowDistribution, RealSize
		FROM dbo.[Format]
		ORDER BY CreatedDate;
END
