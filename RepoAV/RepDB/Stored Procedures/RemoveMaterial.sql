CREATE PROCEDURE [dbo].[RemoveMaterial]
	@Id INT,
	@PublicId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT * FROM dbo.FormatGroup WHERE MaterialId = @Id)
		RETURN -55;--FormatGroup4MaterialExists

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.Material WHERE PublicId = @PublicId;

	IF @Id IS NULL
		RETURN -41;

	DELETE dbo.[Tag2Material] WHERE Id_Material = @Id;
	DELETE dbo.[Material] WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
