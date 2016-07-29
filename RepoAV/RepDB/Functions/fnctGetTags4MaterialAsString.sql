CREATE FUNCTION [dbo].[fnctGetTags4MaterialAsString]
(
	@Id_Material INT
)
RETURNS NVARCHAR(MAX)
AS
BEGIN

	DECLARE @Res NVARCHAR(MAX);

	SET @Res = '';
	
	SELECT @Res = @Res + t2m.Tag + ';'
		FROM dbo.Tag2Material t2m
		WHERE t2m.Id_Material = @Id_Material

	IF LEN(@Res) > 0
		SET @Res = SUBSTRING(@Res, 1, LEN(@Res) - 1)

	IF LEN(@Res ) < 1
		SET @Res = NULL;

	RETURN @Res;
END
