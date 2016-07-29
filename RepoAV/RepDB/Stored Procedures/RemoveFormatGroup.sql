CREATE PROCEDURE [dbo].[RemoveFormatGroup]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS(SELECT * FROM dbo.FormatGroup WHERE Id = @Id)
		RETURN -41;--NotFound

	IF EXISTS(SELECT * FROM dbo.[Format] WHERE FormatGroupId = @Id)
		RETURN -57;--Format4GroupExists

	UPDATE Material SET ModifyDate = GETDATE() WHERE Id = (SELECT MaterialId FROM FormatGroup WHERE Id = @Id)	

	DELETE dbo.FormatGroup WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
