CREATE PROCEDURE [dbo].[AddProfile]
	@Id INT OUTPUT,
	@Name NVARCHAR(200),
	@Id_ProfileGroup INT,	
	@MinHeight INT,
	@MinWidth INT,
	@Apect INT,
	@Mime VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT dbo.[Profile]
		([Name], Id_ProfileGroup, MinHeight,  MinWidth, Apect, Mime)
		VALUES(@Name, @Id_ProfileGroup,  @MinHeight,  @MinWidth, @Apect, @Mime);
	SET @Id = SCOPE_IDENTITY();

	RETURN 1;
END
