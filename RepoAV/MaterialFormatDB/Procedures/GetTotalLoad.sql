CREATE PROCEDURE [dbo].[GetTotalLoad]
	@TotalLoad BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT @TotalLoad = ISNULL(sum(ISNULL(RealSize, ISNULL(Size, 0) )), 0)
		FROM dbo.[Format];
END
