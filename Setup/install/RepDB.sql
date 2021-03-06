
 --:setvar folder "f:\RepoAV\Database"
 --:setvar dbname "RepoDB"

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
	( NAME = N'RepoDB_Data', FILENAME = N'$(folder)\Database\$(dbname).mdf' , SIZE = 4160KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
	 LOG ON 
	( NAME = N'RepoDB_Log', FILENAME = N'$(folder)\Database\$(dbname).ldf' , SIZE = 1536KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)

END

GO