/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 0)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (0, 'Adding')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 1)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (1, 'AddError')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 2)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (2, 'Recoding')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 3)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (3, 'RecError')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 4)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (4, 'RemovePending')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 5)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (5, 'Added')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 6)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (6, 'InvalidFile')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 7)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (7, 'NotFound')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatInternalStatus] WHERE [Id] = 8)
	INSERT INTO [dbo].[FormatInternalStatus]
			   ([Id], [Name])
			VALUES (8, 'Recoded')
GO





if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'TaskExecutionTimeout')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('TaskExecutionTimeout', '1800', 'Timeout podany w sekundach dla wykonania zadania')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'ResultProcessingTimeout')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('ResultProcessingTimeout', '600', 'Timeout podany w sekundach dla postprocessingu wyników zadania')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'TaskWaitingTimeout')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('TaskWaitingTimeout', '300', 'Timeout podany w sekundach dla zadań oczekujących na wykonanie, gdy podane są węzły preferowane, ale nie podejmują sie wykonania.')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryAccessNLB')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('RepositoryAccessNLB', 'http://IP/ReposytoryAccess/', 'Adres usługi współnej RepositoryAccess udostępnianej poprzez NLB.')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'APIService')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('APIService', 'RepAPI', 'Adres usługi Repository API udostępnianej poprzez NLB.')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'MinReplicaCount')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('MinReplicaCount', '1', 'Minimalna liczba przechowywanych kopii formatów')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'TargetReplicaCount')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('TargetReplicaCount', '2', 'Docelowa liczba przechowywanych kopii formatów')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'ManagerAPINLB')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('ManagerAPINLB', 'http://IP:8088/Manager/', 'Adres managera')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'MaxMaterialAge')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('MaxMaterialAge', '730', 'Wiek najstarszego materiału w dniach ')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'MaxTaskAge')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('MaxTaskAge', '60', 'Wiek najstarszego zadania w dniach ')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'ReplicaRepairInterval')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('ReplicaRepairInterval', '360', 'Czas w minutach między sprawdzeniami replikacji')
GO
if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'FormatRepairInterval')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('FormatRepairInterval', '120', 'Czas w minutach między sprawdzeniami formatów z błędami')
GO



if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 0)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (0, 'New')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 1)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (1, 'Ready')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 2)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (2, 'Removed')
GO



if NOT EXISTS(SELECT * FROM [dbo].[TaskStatus] WHERE [Id] = 0)
	INSERT INTO [dbo].[TaskStatus]
			   ([Id], [Name])
			VALUES (0, 'New')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskStatus] WHERE [Id] = 5)
	INSERT INTO [dbo].[TaskStatus]
			   ([Id], [Name])
			VALUES (5, 'Executing')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskStatus] WHERE [Id] = 10)
	INSERT INTO [dbo].[TaskStatus]
			   ([Id], [Name])
			VALUES (10, 'Success')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskStatus] WHERE [Id] = 66)
	INSERT INTO [dbo].[TaskStatus]
			   ([Id], [Name])
			VALUES (66, 'Failure')
GO



if NOT EXISTS(SELECT * FROM [dbo].[NodeRole] WHERE [Id] = 0)
	INSERT INTO [dbo].[NodeRole]
			   ([Id], [Name])
			VALUES (0, 'SNode')
GO

if NOT EXISTS(SELECT * FROM [dbo].[NodeRole] WHERE [Id] = 1)
	INSERT INTO [dbo].[NodeRole]
			   ([Id], [Name])
			VALUES (1, 'Recoder')
GO
if NOT EXISTS(SELECT * FROM [dbo].[NodeRole] WHERE [Id] = 2)
	INSERT INTO [dbo].[NodeRole]
			   ([Id], [Name])
			VALUES (2, 'Manager')
GO

if NOT EXISTS(SELECT * FROM [dbo].[NodeRole] WHERE [Id] = 3)
	INSERT INTO [dbo].[NodeRole]
			   ([Id], [Name])
			VALUES (3, 'SNodeRecoder')
GO


if NOT EXISTS(SELECT * FROM [dbo].[NodeRole] WHERE [Id] = 10)
	INSERT INTO [dbo].[NodeRole]
			   ([Id], [Name])
			VALUES (10, 'Other')
GO





if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 0)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (0, 'Download')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 1)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (1, 'Recode')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 2)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (2, 'Remove')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 3)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (3, 'RemoveFile')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 4)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (4, 'AddMaterial')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 5)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (5, 'RemoveMaterial')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 6)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (6, 'UpdateMaterial')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 7)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (7, 'SyncFormatMetadata')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 8)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (8, 'FixFormatErrors')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 9)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (9, 'FixReplication')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 10)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (10, 'RemoveOldMaterials')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 12)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (12, 'AddFormat')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 13)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (13, 'UpdateFormat')
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 14)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (14, 'RemoveFormat')
GO
GO
if NOT EXISTS(SELECT * FROM [dbo].[TaskType] WHERE [Id] = 15)
	INSERT INTO [dbo].[TaskType]
			   ([Id], [Name])
			VALUES (15, 'RemoveOldTasks')
GO




if NOT EXISTS(SELECT * FROM [dbo].[MaterialType] WHERE [Id] = 0)
	INSERT INTO [dbo].[MaterialType]
			   ([Id], [Name])
			VALUES (0, 'Unknown')
GO
if NOT EXISTS(SELECT * FROM [dbo].[MaterialType] WHERE [Id] = 1)
	INSERT INTO [dbo].[MaterialType]
			   ([Id], [Name])
			VALUES (1, 'Audio')
GO
if NOT EXISTS(SELECT * FROM [dbo].[MaterialType] WHERE [Id] = 2)
	INSERT INTO [dbo].[MaterialType]
			   ([Id], [Name])
			VALUES (2, 'Video')
GO



if NOT EXISTS(SELECT * FROM [dbo].[FormatType] WHERE [Id] = 0)
	INSERT INTO [dbo].[FormatType]
			   ([Id], [Name])
			VALUES (0, 'Source')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatType] WHERE [Id] = 1)
	INSERT INTO [dbo].[FormatType]
			   ([Id], [Name])
			VALUES (1, 'Recoded')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatType] WHERE [Id] = 2)
	INSERT INTO [dbo].[FormatType]
			   ([Id], [Name])
			VALUES (2, 'Subtitle')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatType] WHERE [Id] = 3)
	INSERT INTO [dbo].[FormatType]
			   ([Id], [Name])
			VALUES (3, 'Caption')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatType] WHERE [Id] = 4)
	INSERT INTO [dbo].[FormatType]
			   ([Id], [Name])
			VALUES (4, 'Related')
GO






if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/x-ms-wmv')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/x-ms-wmv', '.wmv')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'image/jpeg')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('image/jpeg', '.jpg')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'image/tiff')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('image/tiff', '.tif')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/x-wav')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/x-wav', '.wav')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/wav')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/wav', '.wav')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/mpeg')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/mpeg', '.mp3')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/m2ts')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/m2ts', '.m2ts')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/x-msvideo')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/x-msvideo', '.avi')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/x-ms-asf')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/x-ms-asf', '.wmv')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/ismv')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/ismv', '.ismv')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'text/ismc')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('text/ismc', '.ismc')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'text/ism')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('text/ism', '.ism')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/mp4')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/mp4', '.mp4')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/webm')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/webm', '.webm')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'video/m2ts')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('video/m2ts', '.m2ts')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/mpeg3')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/mpeg3', '.mp3')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/mp4')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/mp4', '.m4a')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'audio/ogg')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('audio/ogg', '.ogg')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'text/vtt')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('text/vtt', '.vtt')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = 'text/xml')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('text/xml', '.xml')
GO
if NOT EXISTS(SELECT * FROM [dbo].[Mime2Extension] WHERE [Mime] = '*')
	INSERT INTO [dbo].[Mime2Extension]
			   ([Mime], [FileExtension])
			VALUES ('*', '.bin')
GO


	UPDATE dbo.Material
		SET MaterialStatus = [dbo].[fnctGetMaterialStatus](Id);

GO
