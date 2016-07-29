CREATE PROCEDURE [dbo].[RemoveFormat]
	@Id INT,
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.Format WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41;--NotFound

	DELETE dbo.[Format] WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
