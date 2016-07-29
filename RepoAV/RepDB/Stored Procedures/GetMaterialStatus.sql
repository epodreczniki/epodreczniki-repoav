CREATE PROCEDURE [dbo].[GetMaterialStatus]
    @PublicId VARCHAR(150),
	@Status SMALLINT OUTPUT 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id_Material INT, @Deleted BIT;

	SELECT @Id_Material = m.Id,
			@Status = m.MaterialStatus,
			@Deleted = m.Deleted
		FROM dbo.Material m
		WHERE m.PublicId = @PublicId;
	
	IF @Id_Material IS NULL
		RETURN -41; -- NotFound

	IF @Status IS NULL
	BEGIN
		IF @Deleted = 1
			SET @Status = 5; --RemovePending 
		ELSE
			SET @Status = dbo.fnctGetMaterialStatus(@Id_Material);
	END

	RETURN 1;
END


