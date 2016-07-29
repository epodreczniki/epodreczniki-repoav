CREATE TABLE [dbo].[AudioStream]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[ProfileId] INT NOT NULL,
	[Bitrate] INT NULL,
	[Coding] NVARCHAR(150) NULL,
	[Sample] SMALLINT NULL,
	[SampleRate] INT NULL,
    CONSTRAINT [FK_AudioStream_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [Profile]([Id])

)
