CREATE PROCEDURE [dbo].[SetFormatLocation]
	@UniqueId VARCHAR(150),
	@Id INT,
	@Location VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
		SELECT @Id = Id FROM dbo.[Format] WHERE UniqueId = @UniqueId;

	IF @Id IS NULL
		RETURN -41; --NotFOund

	UPDATE dbo.[Format]
		SET Location = @Location
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFOund
END