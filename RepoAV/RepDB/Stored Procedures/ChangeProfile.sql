CREATE PROCEDURE [dbo].[ChangeProfile]
	@Id INT,
	@Name NVARCHAR(200),	
	@MinHeight INT,
	@MinWidth INT,
	@Apect INT,
	@Id_ProfileGroup INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.[Profile]
		SET [Name] = @Name, 			
			MinHeight = @MinHeight,
			MinWidth = @MinWidth,
			Apect = @Apect,
			Id_ProfileGroup = @Id_ProfileGroup
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;
END
