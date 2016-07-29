CREATE PROCEDURE [dbo].[GetMaterialByMetadata]
	 @StartTime DATETIME, 
	 @EndTime DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
   
	SELECT a.MaterialType, a.Channel, a.Programme, Min(a.StartTime) as StartTime, Max(a.EndTime) as EndTime, COUNT(a.Id) AS Count, SUM(a.Size/1024/1024) as Size, SUM(Duration/1000/60) as Duration FROM
	(SELECT m.Id, m.MaterialType,  Metadata.value('(//channel/display-name)[1]','varchar(128)') AS Channel, 
		Metadata.value('(//programme/title)[1]','varchar(128)') AS Programme,
		dbo.fnctConvertToDate(Metadata.value('(//programme/@start)[1]','varchar(32)')) AS StartTime,
		dbo.fnctConvertToDate(Metadata.value('(//programme/@stop)[1]','varchar(32)')) AS EndTime,
		Size,
		Duration
		FROM Material m
		INNER JOIN 
			(SELECT fg.MaterialId as Id, SUM(f.Size) as Size FROM Format F
			INNER JOIN FormatGroup fg ON fg.Id = f.FormatGroupId
			GROUP BY fg.MaterialId) b ON b.Id = m.Id
		WHERE Metadata IS NOT NULL 
		) a
	WHERE a.StartTime >= ISNULL(@StartTime, a.StartTime)
		AND a.EndTime <= ISNULL(@EndTime, a.EndTime)	
		GROUP BY a.MaterialType, a.Channel, a.Programme
	
END