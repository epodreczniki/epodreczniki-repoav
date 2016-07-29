CREATE PROCEDURE [dbo].[GetProfile]
	@Id INT ,
	@Name NVARCHAR(200) OUTPUT,		
	@MinHeight INT OUTPUT,
	@MinWidth INT OUTPUT,
	@Apect INT OUTPUT,
	@Mime VARCHAR(50) OUTPUT,
	@Id_ProfileGroup INT OUTPUT,
	@Enabled BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT @Name = p.[Name],
			@MinHeight = MinHeight,
			@MinWidth = MinWidth,
			@Apect = Apect,
			@Mime = Mime,
			@Id_ProfileGroup = Id_ProfileGroup,
			@Enabled = pg.[Enabled]
		FROM dbo.[Profile] p INNER JOIN
			dbo.ProfileGroup pg ON p.Id_ProfileGroup = pg.Id
		WHERE p.Id = @Id;

	IF @Name IS NULL
		RETURN -41;

	RETURN 1;
END
