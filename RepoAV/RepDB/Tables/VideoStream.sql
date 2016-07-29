CREATE TABLE [dbo].[VideoStream]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[ProfileId] INT NOT NULL,
	[Bitrate] INT NULL,
	[Framerate] INT NULL,
	[Coding] NVARCHAR(150) NULL,
	[Profile] NVARCHAR(50) NULL,
	[Level] REAL NULL,
	[FrameHeight] INT NULL,
	[FrameWidth] INT NULL, 
    CONSTRAINT [FK_VideoStream_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [Profile]([Id])
)
