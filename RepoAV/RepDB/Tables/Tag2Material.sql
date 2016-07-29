CREATE TABLE [dbo].[Tag2Material]
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
	[Id_Material] INT NOT NULL,
	[Tag] NVARCHAR(150) NOT NULL,
    CONSTRAINT [FK_Tag2Material_Material] FOREIGN KEY ([Id_Material]) REFERENCES [Material]([Id]) ON DELETE CASCADE
)

GO

CREATE INDEX [IX_Tag2Material_Id_Material] ON [dbo].[Tag2Material] ([Id_Material])

GO
