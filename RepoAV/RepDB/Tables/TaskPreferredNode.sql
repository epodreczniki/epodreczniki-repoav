CREATE TABLE [dbo].[TaskPreferredNode]
(
	[Id_Task] BIGINT NOT NULL,
	[NodeId] INT NOT NULL, -- relacja do wezla majacego wykonac to zadanie

	CONSTRAINT [FK_TaskPreferredNode_Node] FOREIGN KEY ([NodeId]) REFERENCES [Node]([Id]),
    CONSTRAINT [FK_TaskPreferredNode_Task] FOREIGN KEY ([Id_Task]) REFERENCES [Task]([Id]), 
    CONSTRAINT [PK_TaskPreferredNode] PRIMARY KEY ([Id_Task], [NodeId])
)

GO

CREATE INDEX [IX_TaskPreferredNode_Id_Task] ON [dbo].[TaskPreferredNode] ([Id_Task])

GO
