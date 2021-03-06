
--:setvar folder "f:\RepoAV\Database"
--:setvar dbname "MaterialFormatDB"

IF (EXISTS (SELECT name 
FROM master.dbo.sysdatabases 
WHERE ('[' + name + ']' = '$(dbname)'
OR name = '$(dbname)')))
BEGIN
	PRINT '$(dbname)' + ' już istnieje'
END
ELSE
BEGIN
	PRINT 'Tworzenie bazy danych $(folder)\Database\$(dbname).mdf...'

	CREATE DATABASE [$(dbname)]
	 CONTAINMENT = NONE
	 ON  PRIMARY 
	( NAME = N'$(dbname)_Data', FILENAME = N'$(folder)\Database\$(dbname).mdf' , SIZE = 4160KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
	 LOG ON 
	( NAME = N'$(dbname)_Log', FILENAME = N'$(folder)\Database\$(dbname).ldf' , SIZE = 1536KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)

END

GO