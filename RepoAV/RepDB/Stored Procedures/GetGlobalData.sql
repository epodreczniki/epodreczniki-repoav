CREATE PROCEDURE [dbo].[GetGlobalData]
	@Key VARCHAR(50),
	@Value NVARCHAR(MAX) OUTPUT,
	@Description NVARCHAR(500) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT @Value = [Value], @Description = [Description]
		FROM [dbo].[GlobalData]
		WHERE [Key] = @Key
END

