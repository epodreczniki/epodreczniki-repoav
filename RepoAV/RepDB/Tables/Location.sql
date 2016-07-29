CREATE TABLE [dbo].[Location]
(
	[NodeId] INT NOT NULL,
	[FormatId] INT NOT NULL, 
    CONSTRAINT [PK_Location] PRIMARY KEY ([NodeId], [FormatId]), 
    CONSTRAINT [FK_Location_Node] FOREIGN KEY ([NodeId]) REFERENCES [Node]([Id]),
	CONSTRAINT [FK_Location_Format] FOREIGN KEY ([FormatId]) REFERENCES [Format]([Id]) ON DELETE CASCADE
)
