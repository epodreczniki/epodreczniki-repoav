CREATE PROCEDURE [dbo].[GetPublicIds4ChangedMaterialsSince]
	@From DateTime,
	@To DateTime
AS
	SET NOCOUNT ON;

	SELECT PublicId
		FROM dbo.Material
		WHERE (@From IS NULL OR ModifyDate >= @From)
			AND (@To IS NULL OR ModifyDate <= @To);
RETURN 0
