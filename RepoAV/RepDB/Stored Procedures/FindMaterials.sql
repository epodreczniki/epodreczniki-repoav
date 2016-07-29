CREATE PROCEDURE [dbo].[FindMaterials]
	@Title NVARCHAR(500),
	@DurationFrom INT,
	@DurationTo INT,
	@AllowDistribution BIT,
	@MaterialType SMALLINT, 
    @PublicId VARCHAR(150),
	@CreatedDateFrom DateTime,
	@CreatedDateTo DateTime,
	@ModifyDateFrom DateTime,
	@ModifyDateTo DateTime,
	@MaterialStatus SMALLINT,
	@SortOrder TINYINT,
	@Offset INT,
	@Count INT,
	@Total int OUTPUT,
	@Tag NVARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	IF @PublicId IS NULL 
		SET @PublicId = @Title


	SELECT @Total = count(m.Id)
		FROM dbo.Material m
		WHERE ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
			AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
			AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
			AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
			AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
			--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
			AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
			AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
			AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
			AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
			AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
			AND (m.Deleted = 0)
			AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			;



	if ISNULL(@SortOrder, 0) = 0
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 1
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.Title DESC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 2
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.PublicId ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 3
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.PublicId DESC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 4
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.Duration ASC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 5
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.Duration DESC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 6
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.CreatedDate ASC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 7
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.CreatedDate DESC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 8
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.ModifyDate ASC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 9
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.ModifyDate DESC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 10
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY FormatGroupsCount ASC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 11
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY FormatGroupsCount DESC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 12
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.MaterialType ASC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
	ELSE if @SortOrder = 13
		SELECT	m.Id, m.Title, m.Duration, m.AllowDistribution, m.MaterialType, m.PublicId, m.CreatedDate, m.ModifyDate, (SELECT count(*) FROM dbo.FormatGroup fg WHERE fg.MaterialId = m.Id) as FormatGroupsCount,
				m.MaterialStatus as [Status], m.Metadata
			FROM dbo.Material m
			WHERE  ((@Title IS NULL OR m.Title like @Title) OR (@PublicId IS NULL OR m.PublicId like @PublicId))
				AND (@DurationFrom IS NULL OR m.Duration >= @DurationFrom)
				AND (@DurationTo IS NULL OR m.Duration <= @DurationTo)
				AND (@AllowDistribution IS NULL OR m.AllowDistribution = @AllowDistribution)
				AND (@MaterialType IS NULL OR m.MaterialType = @MaterialType)
				--AND (@PublicId IS NULL OR m.PublicId like @PublicId)
				AND (@CreatedDateFrom IS NULL OR m.CreatedDate >= @CreatedDateFrom)
				AND (@CreatedDateTo IS NULL OR m.CreatedDate <= @CreatedDateTo)
				AND (@ModifyDateFrom IS NULL OR m.ModifyDate >= @ModifyDateFrom)
				AND (@ModifyDateTo IS NULL OR m.ModifyDate <= @ModifyDateTo)
				AND (@MaterialStatus IS NULL OR m.MaterialStatus = @MaterialStatus)
				AND (m.Deleted = 0)
				AND (@Tag IS NULL OR EXISTS(SELECT t2m.Id FROM dbo.Tag2Material t2m WHERE t2m.Id_Material = m.Id AND t2m.Tag like @Tag))
			ORDER BY m.MaterialType DESC, m.Title ASC
				OFFSET @Offset ROWS FETCH NEXT @Count ROWS ONLY
			;
END