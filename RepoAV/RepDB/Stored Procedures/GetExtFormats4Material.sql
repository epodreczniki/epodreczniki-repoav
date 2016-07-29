﻿CREATE PROCEDURE [dbo].[GetExtFormats4Material]
	@PublicId VARCHAR(150) 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Id_Material INT;

	SELECT @Id_Material = Id FROM dbo.Material WHERE PublicId = @PublicId;

	IF @Id_Material IS NULL
		RETURN -52;--MaterialNotFound

	SELECT  f.Id,
			f.FormatGroupId, 
			f.ProfileId, 
			f.XmlMetadata, 
			f.[Type], 
			f.UniqueId, 
			f.Size, 
			f.CreateDate, 
			f.[Status], 
			f.InternalStatus,
			m.AllowDistribution,
			f.Mime,
			dbo.fnctGetFileExt4Mime(f.Mime) as FileFormat,
			v.Bitrate as VideoBitrate,
			v.Framerate as VideoFramerate,
			v.Coding as VideoCoding,
			v.FrameHeight as VideoFrameHeight,
			v.FrameWidth as VideoFrameWidth,
			v.[Profile] as VideoProfile,
			v.[Level] as VideoLevel,
			a.Bitrate as AudioBitrate,
			a.Coding as AudioCoding,
			a.[Sample] as AudioSample,
			a.SampleRate as AudioSampleRate
		FROM dbo.[Format] f INNER JOIN
			dbo.FormatGroup fg ON f.FormatGroupId = fg.Id INNER JOIN
			dbo.Material m ON fg.MaterialId = m.Id LEFT JOIN
			dbo.[Profile] p ON f.ProfileId = p.Id LEFT JOIN
			dbo.VideoStream v ON v.ProfileId = p.Id LEFT JOIN
			dbo.AudioStream a ON a.ProfileId = p.Id
		WHERE fg.MaterialId = @Id_Material;
	RETURN 1;
END

