CREATE TABLE [dbo].[ProfileGroup]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] NVARCHAR(150) NOT NULL,
	[OperationXML] NVARCHAR(MAX) NULL, 
    [Enabled] BIT NOT NULL DEFAULT 1,
	[MaterialType] SMALLINT NOT NULL,
	[TaskSubtype] VARCHAR(50) NULL, 

	[DownloadSourceFiles] VARCHAR(150) NOT NULL DEFAULT 'true', 
    CONSTRAINT [FK_ProfileGroup_MaterialType] FOREIGN KEY ([MaterialType]) REFERENCES [MaterialType]([Id])
)

GO

CREATE INDEX [IX_ProfileGroup_MaterialType] ON [dbo].[ProfileGroup] ([MaterialType])
