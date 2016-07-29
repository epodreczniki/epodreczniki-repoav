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
if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 0)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (0, 'Partial')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 1)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (1, 'Full')
GO
if NOT EXISTS(SELECT * FROM [dbo].[FormatStatus] WHERE [Id] = 2)
	INSERT INTO [dbo].[FormatStatus]
			   ([Id], [Name])
			VALUES (2, 'Removed')
GO



if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'LocalRepositoryPath')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('LocalRepositoryPath', 'e:\Repository', 'Ścieżka do głównego katalogu repozytorium plikowego.')
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryVirtualDir')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('RepositoryVirtualDir', 'Repository', 'Wirtualny katalog używany przez IISa.')
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositorySize')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('RepositorySize', '30', 'Rozmiar repozytorium dyskowego wyrażony w GB.')
GO

if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryMinFreeSpace')
	INSERT INTO [dbo].[GlobalData]
			   ([Key], [Value], [Description])
			VALUES ('RepositoryMinFreeSpace', '0', 'Żądany rozmiar wolnej przetrzeni dyskowej w repozytorium podany w procentach.')
GO


