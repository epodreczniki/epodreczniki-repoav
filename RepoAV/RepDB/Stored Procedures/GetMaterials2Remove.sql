CREATE PROCEDURE [dbo].[GetMaterials2Remove]
	@Offset INT,
	@Count INT,
	@Total int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @MaxMaterialAge SMALLINT;
	SELECT @MaxMaterialAge = CAST(ISNULL([Value], -1) aS smallint) 
		FROM dbo.GlobalData
		WHERE [Key] = 'MaxMaterialAge';

	IF @MaxMaterialAge < 0
		RETURN -63;--WrongConfiguration

	SELECT @Total = count(m.Id)
		FROM dbo.Material m
		WHERE (m.Deleted = 0)
			AND DATEADD(day, @MaxMaterialAge, m.CreatedDate) < GetDate();

	SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
			m.MaterialStatus as [Status], m.Metadata
		FROM dbo.Material m
		WHERE (m.Deleted = 0)
			AND DATEADD(day, @MaxMaterialAge, m.CreatedDate) < GetDate()
		ORDER BY m.Title ASC
			OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY;
END