CREATE PROCEDURE [dbo].[SetMaterialDeleted]
	@Id INT,
	@PublicId VARCHAR(150),
	@Deleted BIT
AS
BEGIN
	SET NOCOUNT ON;

	If @Id IS NULL
	BEGIN
		SELECT @Id = Id FROM dbo.Material WHERE PublicId = @PublicId;
		IF @Id IS NULL
			RETURN -41; --NotFOund
	END
	ELSE
		IF NOT EXISTS (SELECT * FROM dbo.Material WHERE Id = @Id)
			RETURN -41; --NotFOund

	UPDATE dbo.Material
		SET Deleted = @Deleted, ModifyDate = GETDATE()
		WHERE Id = @Id;

	RETURN 1;
END

