CREATE PROCEDURE [dbo].[GetProfiles4XML]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	p.Id
			,p.Name
			,dbo.fnctGetFileExt4Mime(p.Mime) as FileFormat
			,(CASE WHEN pg.MaterialType = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END) as AudioOnly
			,v.FrameWidth
			,v.FrameHeight
			,v.Bitrate as VideoBitrate
			,v.Framerate
			,v.Coding as VideoCoding
			,a.Bitrate as AudioBitrate
			,a.Coding as AudioCoding
			,a.[Sample]
			,a.SampleRate
		FROM dbo.[Profile] p INNER JOIN
			dbo.ProfileGroup pg ON p.Id_ProfileGroup = pg.Id LEFT JOIN
			dbo.VideoStream v ON v.ProfileId = p.Id LEFT JOIN
			dbo.AudioStream a ON a.ProfileId = p.Id
		WHERE pg.[Enabled] = 1
		ORDER BY pg.Id, p.Id;

END
