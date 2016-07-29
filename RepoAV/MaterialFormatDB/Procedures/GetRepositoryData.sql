CREATE PROCEDURE [dbo].[GetRepositoryData]
	@Size INT OUTPUT,
	@Path VARCHAR(200) OUTPUT,
	@TotalLoad BIGINT OUTPUT,
	@RepositoryMinFreeSpace TINYINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT @Size = CAST([Value] AS INT)
		FROM [dbo].[GlobalData]
		WHERE [Key] = 'RepositorySize';

	SELECT @Path = [Value]
		FROM [dbo].[GlobalData]
		WHERE [Key] = 'LocalRepositoryPath';

	SELECT @TotalLoad = ISNULL(sum(ISNULL(RealSize, Size)), 0)
		FROM dbo.[Format];

	SELECT @RepositoryMinFreeSpace = ISNULL([Value], 0)
		FROM [dbo].[GlobalData]
		WHERE [Key] = 'RepositoryMinFreeSpace';


END

