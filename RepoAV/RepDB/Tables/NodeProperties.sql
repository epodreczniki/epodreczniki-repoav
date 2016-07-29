CREATE TABLE [dbo].[NodeProperties]
(
	[Id_Node] INT NOT NULL , 
	[Name] VARCHAR(50) NOT NULL,
    [Value] VARCHAR(MAX) NULL, 

    CONSTRAINT [FK_NodeProperties_Node] FOREIGN KEY ([Id_Node]) REFERENCES [Node]([Id]), 

    PRIMARY KEY ([Id_Node], [Name])
)
