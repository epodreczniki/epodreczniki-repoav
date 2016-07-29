CREATE PROCEDURE [dbo].[SetProfileGroupEnabled]
	@Id INT,
	@Enabled BIT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.[ProfileGroup] WHERE Id = @Id)
		RETURN -41; --NotFOund

	UPDATE dbo.[ProfileGroup]
		SET [Enabled] = @Enabled
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFOund
END


