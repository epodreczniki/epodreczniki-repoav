CREATE TABLE [dbo].[Material]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Title] NVARCHAR(500) NOT NULL,
	[Duration] INT NULL,
	[CreatedDate] DATETIME NOT NULL DEFAULT GetDate(),
	[ModifyDate] DATETIME NULL,
	[AllowDistribution] BIT NOT NULL DEFAULT(0),
	[MaterialType] SMALLINT NOT NULL, 
    [PublicId] VARCHAR(150) NOT NULL, 
    [Deleted] BIT NOT NULL DEFAULT 0, 
	[Metadata] XML NULL,
    [MaterialStatus] SMALLINT NOT NULL DEFAULT 7, --Adding
    CONSTRAINT [FK_Material_MaterialType] FOREIGN KEY ([MaterialType]) REFERENCES [MaterialType]([Id])
)

GO

CREATE INDEX [IX_Material_Title] ON [dbo].[Material] ([Title])

GO

CREATE INDEX [IX_Material_PublicId] ON [dbo].[Material] ([PublicId])

GO

CREATE TRIGGER [dbo].[Trigger_Material]
    ON [dbo].[Material]
    FOR UPDATE
    AS
    BEGIN
        SET NoCount ON;
		IF UPDATE(Deleted)
		BEGIN
			UPDATE dbo.Material
				SET MaterialStatus = 5 --RemovePending 
				WHERE Id IN (SELECT Id FROM deleted)
					AND Deleted = 1;

			UPDATE dbo.Material
				SET MaterialStatus = [dbo].[fnctGetMaterialStatus](Id)
				WHERE Id IN (SELECT Id FROM deleted)
					AND Deleted = 0;
		END
    END