USE RepDB
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'ProcessFirstAudioOnly')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('ProcessFirstAudioOnly', 'False', 'Przetwarzanie tylko pierwszego strumienia audio w materiałach AV')
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'SelectRecoders')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('SelectRecoders', 'True', 'Czy wskazywać rekodery do zadań rekodowania')
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'OldMaterialRemovalInterval')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('OldMaterialRemovalInterval', -1, 'Czas w godzinach między usuwaniem starych materiałów')
GO

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'mp4_vlow_bl')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('mp4_vlow_bl','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="mp4_vlow_bl" StartOperation="Encode" ExecutionTimeout="720000">
		<tns:Operations>
		<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
		  %%tools\mp4_vlow_bl.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2, 'true;true')
	
	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('mp4_vlow_bl', IDENT_CURRENT('ProfileGroup'), 'video/mp4')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding, Profile, Level, FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 110000, 8, 'H.264/MPEG-4 AVC', 'Baseline', 1.3, 226, 400)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 40000, 'Advanced Audio Coding', 16, 32000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'mp4_low_bl')
BEGIN
	INSERT ProfileGroup(Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('mp4_low_bl','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="mp4_low_bl" StartOperation="Encode" ExecutionTimeout="720000">
	    <tns:Operations>
		  <tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			%%tools\mp4_low_bl.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>  
		</tns:Operations>
	  </tns:Task>
	</tns:Tasks>', 2, 'true;true')

	INSERT Profile(Name, Id_ProfileGroup, MinWidth, Mime)
	VALUES('mp4_low_bl', IDENT_CURRENT('ProfileGroup'), 432, 'video/mp4')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding, Profile, Level, FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 500000, 25, 'H.264/MPEG-4 AVC', 'Baseline', 3.0, 270, 480)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 40000, 'Advanced Audio Coding', 16, 32000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'mp4_med_ml')
BEGIN
	INSERT ProfileGroup(Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('mp4_med_ml','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="mp4_med_ml" StartOperation="Encode" ExecutionTimeout="720000">
	    <tns:Operations>
		  <tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			%%tools\mp4_med_ml.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2, 'true;true')

	INSERT Profile(Name, Id_ProfileGroup, MinWidth, Mime)
	VALUES('mp4_med_ml', IDENT_CURRENT('ProfileGroup'), 576, 'video/mp4')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding, Profile, Level, FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 1350000, 25, 'H.264/MPEG-4 AVC', 'Main', 3.0, 360, 640)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 96000, 'Advanced Audio Coding', 16, 44100)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'mp4_hi_hl')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('mp4_hi_hl','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="mp4_hi_hl" StartOperation="Encode" ExecutionTimeout="720000">
		<tns:Operations>
		<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			  %%tools\mp4_hi_hl.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2, 'true;true')

	INSERT Profile(Name, Id_ProfileGroup, MinHeight, Mime)
	VALUES('mp4_hi_hl', IDENT_CURRENT('ProfileGroup'), 648, 'video/mp4')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding, Profile, Level, FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 4000000, 25, 'H.264/MPEG-4 AVC', 'High', 4.1, 720, 1280)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 128000, 'Advanced Audio Coding', 16, 44100)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'webm_med')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('webm_med','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="webm_med" StartOperation="Encode" ExecutionTimeout="720000">
		<tns:Operations>
		<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			  %%tools\webm_med.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2, 'true;true')

	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('webm_med', IDENT_CURRENT('ProfileGroup'), 'video/webm')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding,  FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 1350000, 25, 'On2 VP8',  360, 640)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 96000, 'Vorbis', 16, 48000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'webm_hi')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType, DownloadSourceFiles)
	Values('webm_hi','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="webm_hi" StartOperation="Encode" ExecutionTimeout="720000">
		<tns:Operations>
		<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			  %%tools\webm_hi.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2, 'true;true')

	INSERT Profile(Name, Id_ProfileGroup, MinHeight, Mime)
	VALUES('webm_hi', IDENT_CURRENT('ProfileGroup'), 648, 'video/webm')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding,  FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 4000000, 25, 'On2 VP8',  360, 640)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 128000, 'Vorbis', 16, 48000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'audio_low_aac')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('audio_low_aac','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="audio_low_aac" StartOperation="Encode" ExecutionTimeout="720000">
	    <tns:Operations>
		  <tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			%%tools\audio_low_aac.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>	
	</tns:Task>
	</tns:Tasks>', 1)

	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('audio_low_acc', IDENT_CURRENT('ProfileGroup'), 'audio/mp4')

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 48000, 'Advanced Audio Coding', 16, 32000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'audio_med_aac')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('audio_med_aac','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="audio_med_aac" StartOperation="Encode" ExecutionTimeout="720000">
	    <tns:Operations>
		  <tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			%%tools\audio_med_aac.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 1)

	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('audio_med_acc', IDENT_CURRENT('ProfileGroup'), 'audio/mp4')

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 96000, 'Advanced Audio Coding', 16, 44100)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'audio_med_ogg')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('audio_med_ogg','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="audio_med_ogg" StartOperation="Encode" ExecutionTimeout="720000">
	    <tns:Operations>
		  <tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
			%%tools\audio_med_ogg.cmd "%%i1" "%%o" %%1 %%u "%%i2" 
		</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 1)

	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('audio_med_ogg', IDENT_CURRENT('ProfileGroup'), 'audio/ogg')

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 96000, 'Vorbis', 16, 44100)
END