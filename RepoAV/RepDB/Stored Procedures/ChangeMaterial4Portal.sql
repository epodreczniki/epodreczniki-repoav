CREATE PROCEDURE [dbo].[ChangeMaterial4Portal]
	@Id INT,
	@Title NVARCHAR(500)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM dbo.Material WHERE [Title] = @Title AND Id <> @Id)
		RETURN -43; --AlreadyExists

	UPDATE dbo.Material
		SET Title = @Title, ModifyDate = GETDATE()
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;--NotFOund
END