CREATE PROCEDURE [dbo].[SetGlobalData]
	@Key VARCHAR(50),
	@Value NVARCHAR(500),
	@Description NVARCHAR(500)
AS
BEGIN
	IF NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = @Key)
	BEGIN
		INSERT [dbo].[GlobalData]
			([Key], [Value], [Description])
			VALUES(@Key, @Value, @Description);
		RETURN 1;
	END
	ELSE
	BEGIN
		IF @Description IS NOT NULL
		BEGIN
			UPDATE [dbo].[GlobalData]
			SET [Value] = @Value, [Description] = @Description
			WHERE [Key] = @Key;

			RETURN 2;
		END
		ELSE
		BEGIN
			UPDATE [dbo].[GlobalData]
			SET [Value] = @Value
			WHERE [Key] = @Key;

			RETURN 2;
		END
	END
END