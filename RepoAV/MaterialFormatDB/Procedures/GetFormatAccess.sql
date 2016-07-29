CREATE PROCEDURE [dbo].[GetFormatAccess]
    @UniqueId  VARCHAR (150),
    @Location  VARCHAR (200) OUTPUT,
	@VirtualDir  VARCHAR (200) OUTPUT,
    @AllowDistribution BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT @Location = Location, @AllowDistribution = AllowDistribution
		FROM dbo.[Format]
		WHERE UniqueId = @UniqueId
			AND [Status] IN (0,1) -- Partial lub Full

	IF @Location IS NULL
		RETURN -41;
	ELSE
	BEGIN
		SELECT @VirtualDir = [Value] FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryVirtualDir';

		RETURN 1;
	END
END
