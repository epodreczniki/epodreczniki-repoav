CREATE TABLE [dbo].[Task]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[Status] SMALLINT NOT NULL DEFAULT 0,
	[Result] NVARCHAR(MAX) NULL,
	[UniqueId] VARCHAR(150) NULL,
	[PublicId] VARCHAR(150) NULL,
	[ExecutingNodeId] INT NULL, -- relacja do wezla wykonujacego lub majacego wykonac to zadanie
	[CreatedDate] DATETIME NOT NULL DEFAULT GetDate(),
	[TakenDate] DATETIME NULL,
	[FinishDate] DATETIME NULL,
	[Type] SMALLINT NOT NULL, 
    [SupervisorId] INT NULL , 
    [ResultProcessed] BIT NOT NULL DEFAULT 0, 
	[BeginDate] DATETIME NULL, --żądana data rozpoczęcia
	[LastActivityDate] DATETIME NULL,
    [CanSkipPreferredNodes] BIT NOT NULL DEFAULT 1, 
	[TaskSubtype] VARCHAR(50) NULL, 

	CONSTRAINT [FK_Task_Node] FOREIGN KEY ([ExecutingNodeId]) REFERENCES [Node]([Id]),
    CONSTRAINT [FK_Task_TaskStatus] FOREIGN KEY ([Status]) REFERENCES [TaskStatus]([Id]),
	CONSTRAINT [FK_Task_TaskType] FOREIGN KEY ([Type]) REFERENCES [TaskType]([Id]),
	CONSTRAINT [FK_Task_SupervisingManager] FOREIGN KEY ([SupervisorId]) REFERENCES [Node]([Id])
)

GO

CREATE INDEX [IX_Task_Type] ON [dbo].[Task] ([Type]) INCLUDE ([TaskSubtype])


GO

CREATE INDEX [IX_Task_Status_Type_TaskSubtype] ON [dbo].[Task] ([Type], [Status], [TaskSubtype])

GO

CREATE INDEX [IX_Task_ExecutingNodeId] ON [dbo].[Task] ([ExecutingNodeId]) INCLUDE ([Type], [Status])

GO

CREATE INDEX [IX_Task_SupervisorId] ON [dbo].[Task] ([SupervisorId]) INCLUDE ([Type], [Status])

GO

CREATE INDEX [IX_Task_UniqueId] ON [dbo].[Task] ([UniqueId]) 

GO

