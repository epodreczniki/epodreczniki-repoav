CREATE PROCEDURE [dbo].[ChangeProfileGroup]
	@Id INT,
	@Name NVARCHAR(150),
	@OperationXML NVARCHAR(MAX),
	@Enabled BIT,
	@MaterialType SMALLINT,
	@TaskSubtype VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.[ProfileGroup]
		SET [Name] = @Name, 
			OperationXML = @OperationXML,
			[Enabled] = @Enabled,
			MaterialType = @MaterialType,
			TaskSubtype = @TaskSubtype
		WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE
		RETURN -41;
END
