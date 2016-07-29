CREATE TABLE [dbo].[Format]
(
	[Id] INT NOT NULL PRIMARY KEY,
    [UniqueId]  VARCHAR (150)  NOT NULL,
    [Location]  VARCHAR (200)  NOT NULL,
    [Status]	SMALLINT       NOT NULL,
    [Size]      BIGINT         NOT NULL,
    [Mime]   VARCHAR(50)       NULL, 
    [AllowDistribution] BIT NOT NULL DEFAULT ((1)),
	[CreatedDate] DATETIME NOT NULL DEFAULT GetDate(), 
    [RealSize] BIGINT NOT NULL, 
    CONSTRAINT [FK_Format_FormatStatus] FOREIGN KEY ([Status]) REFERENCES [FormatStatus]([Id])
)
