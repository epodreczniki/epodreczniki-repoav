CREATE TABLE [dbo].[Response2Send]
(
    [ErrorCode] INT            NOT NULL ,
    [TaskId] BIGINT  NOT NULL,
    [Result] NVARCHAR(MAX)  NULL,
    [TaskFinishDate]   DATETIME       NOT NULL, 
    CONSTRAINT [PK_ExecutedTask] PRIMARY KEY ([TaskId])
)

