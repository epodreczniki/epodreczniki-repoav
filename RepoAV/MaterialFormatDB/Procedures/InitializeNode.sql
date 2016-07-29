CREATE PROCEDURE [dbo].[InitializeNode]
	@LocalRepositoryPath VARCHAR(200),
	@RepositoryVirtualDir VARCHAR(200),
	@RepositorySize BIGINT,
	@RepositoryMinFreeSpace TINYINT
AS
BEGIN
	if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'LocalRepositoryPath')
	BEGIN
		INSERT INTO [dbo].[GlobalData]
				   ([Key], [Value], [Description])
				VALUES ('LocalRepositoryPath', @LocalRepositoryPath, 'Ścieżka do głównego katalogu repozytorium plikowego.')
	END
	ELSE
	BEGIN
		UPDATE [dbo].[GlobalData] SET [Value] = @LocalRepositoryPath WHERE [Key] = 'LocalRepositoryPath'
	END

	if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryVirtualDir')
	BEGIN
		INSERT INTO [dbo].[GlobalData]
				   ([Key], [Value], [Description])
				VALUES ('RepositoryVirtualDir', @RepositoryVirtualDir, 'Wirtualny katalog używany przez IISa.')
	END
	ELSE
	BEGIN
		UPDATE [dbo].[GlobalData] SET [Value] = @RepositoryVirtualDir WHERE [Key] = 'RepositoryVirtualDir'
	END

	if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositorySize')
	BEGIN
		INSERT INTO [dbo].[GlobalData]
				   ([Key], [Value], [Description])
				VALUES ('RepositorySize', CAST(@RepositorySize AS VARCHAR(150)), 'Rozmiar repozytorium dyskowego wyrażony w GB.')
	END
	ELSE
	BEGIN
		UPDATE [dbo].[GlobalData] SET [Value] = @RepositorySize WHERE [Key] = 'RepositorySize'
	END

	if NOT EXISTS(SELECT * FROM [dbo].[GlobalData] WHERE [Key] = 'RepositoryMinFreeSpace')
	BEGIN
		INSERT INTO [dbo].[GlobalData]
				   ([Key], [Value], [Description])
				VALUES ('RepositoryMinFreeSpace', CAST(@RepositoryMinFreeSpace AS VARCHAR(150)), 'Żądany rozmiar wolnej przetrzeni dyskowej w repozytorium podany w procentach.')
	END
	ELSE
	BEGIN
		UPDATE [dbo].[GlobalData] SET [Value] = @RepositoryMinFreeSpace WHERE [Key] = 'RepositoryMinFreeSpace'
	END

	RETURN 0
END
