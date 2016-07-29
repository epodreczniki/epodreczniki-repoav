CREATE TABLE [dbo].[GlobalData]
(
	[Key] [varchar](50) NOT NULL,
	[Value] NVARCHAR(500) NULL,
	[Description] NVARCHAR(500) NULL, 
    CONSTRAINT [PK_GlobalData] PRIMARY KEY ([Key]),

)
