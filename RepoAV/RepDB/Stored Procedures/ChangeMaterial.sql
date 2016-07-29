CREATE PROCEDURE [dbo].[ChangeMaterial]
	@Id INT,
	@Title NVARCHAR(500),
	@Duration INT,
	@AllowDistribution BIT,
	@MaterialType SMALLINT,
	@Metadata XML
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.MaterialType WHERE Id = @MaterialType)
		RETURN -51; --MaterialTypeNotFound

	IF EXISTS (SELECT * FROM dbo.Material WHERE [Title] = @Title AND Id <> @Id)
		RETURN -43; --AlreadyExists

	UPDATE dbo.Material
		SET Title = @Title, 
			Duration = @Duration, 
			AllowDistribution = @AllowDistribution, 
			MaterialType = @MaterialType,
			ModifyDate = GETDATE(),
			Metadata = @Metadata
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;--NotFOund
END


