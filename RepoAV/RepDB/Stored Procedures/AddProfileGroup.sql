CREATE PROCEDURE [dbo].[AddProfileGroup]
	@Id INT OUTPUT,
	@Name NVARCHAR(150),
	@OperationXML NVARCHAR(MAX),
	@Enabled BIT,
	@MaterialType SMALLINT,
	@TaskSubtype VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT dbo.[ProfileGroup]
		([Name], OperationXML, [Enabled], MaterialType, TaskSubtype)
		VALUES(@Name, @OperationXML, @Enabled, @MaterialType, @TaskSubtype);
	SET @Id = SCOPE_IDENTITY();

	RETURN 1;
END
