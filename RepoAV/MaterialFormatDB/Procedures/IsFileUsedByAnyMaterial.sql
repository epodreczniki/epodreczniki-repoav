CREATE PROCEDURE [dbo].[IsFileUsedByAnyMaterial]
	@FilePath VARCHAR (250),
	@IsInUse BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT Id FROM dbo.[Format] m WHERE @FilePath = m.Location)
		SET @IsInUse = 1;
	ELSE
		SET @IsInUse = 0;
END

