CREATE TABLE [dbo].[Node]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Role] SMALLINT NOT NULL,
	[ExternalAddress] VARCHAR(500) NULL,
	[InternalAddress] VARCHAR(500) NULL,
	[Url] VARCHAR(500) NULL, 
    [FreeSpace] BIGINT NULL, 
    [CheckTime] DATETIME NULL, 
	[TotalSpace] BIGINT NULL, 
	[Name] VARCHAR(50),

    [Enabled] BIT NOT NULL DEFAULT 1, 
    [ProcaPortNumber] INT NOT NULL DEFAULT 8088, 
    CONSTRAINT [FK_Node_NodeRole] FOREIGN KEY ([Role]) REFERENCES [NodeRole]([Id])
)
