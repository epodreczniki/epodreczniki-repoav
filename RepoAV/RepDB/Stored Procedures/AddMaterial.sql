CREATE PROCEDURE [dbo].[AddMaterial]
	@Id INT OUTPUT,
	@Title NVARCHAR(500),
	@Duration INT,
	@AllowDistribution BIT,
	@MaterialType SMALLINT, 
    @PublicId VARCHAR(150) ,
	@Tags NVARCHAR(MAX),
	@Metadata XML 
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.MaterialType WHERE Id = @MaterialType)
		RETURN -51; --MaterialTypeNotFound

	IF EXISTS (SELECT * FROM dbo.Material WHERE PublicId = @PublicId)
		RETURN -43; --AlreadyExists

	BEGIN TRY
	BEGIN TRANSACTION T1;
		INSERT dbo.Material
			(Title, Duration, AllowDistribution, MaterialType, PublicId, Metadata)
			VALUES (@Title, @Duration, @AllowDistribution, @MaterialType, @PublicId, @Metadata)
		SET @Id = SCOPE_IDENTITY();

		IF @Tags IS NOT NULL
		BEGIN
			INSERT dbo.Tag2Material
				(Id_Material, Tag)
				SELECT @Id, fss.Part FROM dbo.fnctSplitString(@Tags, ';') fss
		END
		COMMIT TRANSACTION T1;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION T1;
	END CATCH

	RETURN 1;
END


