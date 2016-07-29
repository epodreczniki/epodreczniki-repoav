CREATE TABLE [dbo].[Profile]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] NVARCHAR(200) NOT NULL,		
	[MinHeight] INT NULL,
	[MinWidth] INT NULL,
	[Apect] REAL NULL,
	[Id_ProfileGroup] INT NOT NULL,
	[Mime] VARCHAR(50) NULL,
	
	CONSTRAINT [FK_Profile_ProfileGorup] FOREIGN KEY ([Id_ProfileGroup]) REFERENCES [ProfileGroup]([Id])
)
