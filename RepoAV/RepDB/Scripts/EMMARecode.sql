if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'ProcessFirstAudioOnly')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('ProcessFirstAudioOnly', '1', 'Przetwarzanie tylko pierwszego strumienia audio w materiałach AV')
GO

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'audio_aac')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('audio_aac','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<tns:Task Name="audio_aac" StartOperation="Encode" ExecutionTimeout="720000">
			<tns:Operations>
				<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
					%%tools\audio_aac.cmd "%%i1" "%%o"
			</tns:Operation>
		</tns:Operations>
		</tns:Task>
		</tns:Tasks>', 1)

	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('audio_aac', IDENT_CURRENT('ProfileGroup'), 'audio/mp4')

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 96000, 'Advanced Audio Coding', 16, 44100)
END
GO

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'audio_arm')
BEGIN
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('audio_arm','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="arm_audio" StartOperation="Recognize" ExecutionTimeout="720000">
		<tns:Operations>
			<tns:Operation Name="Recognize" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="Rename" OnFailure="">
				%%tools\Mowa\asrcmd.exe -wave:"%%i1" -for_xml /outPath:%%do
			</tns:Operation>
			<tns:Operation Name="Rename" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
				move "%%do\%%iw1.xml" "%%o"
			</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 1)
	
	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('arm_audio', IDENT_CURRENT('ProfileGroup'), 'text/xml')
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'video_mp4')
BEGIN
	INSERT ProfileGroup(Name,  OperationXML, MaterialType)
	Values('video_mp4','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="video_mp4" StartOperation="Encode" ExecutionTimeout="720000">
		<tns:Operations>
			<tns:Operation Name="Encode" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
				%%tools\mp4.cmd "%%i1" "%%o"
			</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2)

	INSERT Profile(Name, Id_ProfileGroup, MinWidth, Mime)
	VALUES('video_mp4', IDENT_CURRENT('ProfileGroup'), NULL, 'video/mp4')

	INSERT VideoStream (ProfileId, Bitrate, Framerate, Coding, Profile, Level, FrameHeight, FrameWidth)
	VALUES(IDENT_CURRENT('Profile'), 500000, 25, 'H.264/MPEG-4 AVC', 'Baseline', 3.0, 576, 720)

	INSERT AudioStream (ProfileId, Bitrate, Coding, Sample, SampleRate)
	VALUES (IDENT_CURRENT('Profile'), 40000, 'Advanced Audio Coding', 16, 32000)
END

IF NOT EXISTS(SELECT * FROM [dbo].[ProfileGroup] WHERE [Name] = 'video_arm')
BEGIN	
	INSERT ProfileGroup (Name,  OperationXML, MaterialType)
	Values('video_arm','<tns:Tasks xmlns:tns="http://schemas.psnc.pl/Recoder/RecoderTaskDefinition/1.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<tns:Task Name="video_arm" StartOperation="Recognize" ExecutionTimeout="720000">
	    <tns:Operations>
			<tns:Operation Name="Recognize" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="Rename" OnFailure="">
				%%tools\Mowa\asrcmd.exe -wave:"%%i1" -for_xml /outPath:%%do
			</tns:Operation>
			<tns:Operation Name="Rename" Skip="0" SkipResult="StopExecutionWithSuccess" Type="CmdLine" OnSuccess="" OnFailure="">
				move "%%do\%%iw1.xml" "%%o"
			</tns:Operation>
		</tns:Operations>
	</tns:Task>
	</tns:Tasks>', 2)
 
	INSERT Profile(Name, Id_ProfileGroup, Mime)
	VALUES('arm_video', IDENT_CURRENT('ProfileGroup'), 'text/xml')
END

