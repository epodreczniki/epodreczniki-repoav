CREATE TABLE [dbo].[FormatGroup]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[MaterialId] INT NOT NULL,
	[SubtitleId] INT NULL,
	[SourceId] INT NULL,
	[AudioId] INT NULL, 
    CONSTRAINT [FK_FormatGroup_Material] FOREIGN KEY ([MaterialId]) REFERENCES [Material]([Id]),
	CONSTRAINT [FK_FormatGroup_SubtitleFormat] FOREIGN KEY ([SubtitleId]) REFERENCES [Format]([Id]),
	CONSTRAINT [FK_FormatGroup_SourceFormat] FOREIGN KEY ([SourceId]) REFERENCES [Format]([Id])
)

GO

CREATE INDEX [IX_FormatGroup_MaterialId] ON [dbo].[FormatGroup] ([MaterialId])
GO
