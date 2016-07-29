CREATE TABLE [dbo].[TaskData]
(
	[Id_Task] BIGINT NOT NULL,
	[Key] VARCHAR(50) NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL
	CONSTRAINT [FK_TaskData_Task] FOREIGN KEY ([Id_Task]) REFERENCES [Task]([Id]), 
    CONSTRAINT [PK_TaskData] PRIMARY KEY ([Id_Task], [Key])
)
