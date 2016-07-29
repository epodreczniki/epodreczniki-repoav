CREATE PROCEDURE [dbo].[GetFormatsPossibleToRemove]
	@OlderThenDays SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT UniqueId, Location, [Status], RealSize, Size
		FROM dbo.[Format]
		WHERE [Status] < 2 -- Partial or Full
			AND (ISNULL(@OlderThenDays, 0) <= 0 OR DATEADD(day, @OlderThenDays, CreatedDate) >= GetDate())
		ORDER BY CreatedDate;
END
