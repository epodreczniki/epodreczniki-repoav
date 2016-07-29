CREATE TABLE [dbo].[Format]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[FormatGroupId] INT NOT NULL,
	[ProfileId] INT NULL,
	[XmlMetadata] NVARCHAR(MAX) NULL,
	[CreateDate] DATETIME NOT NULL DEFAULT GetDate(),
	[Type] SMALLINT NOT NULL,
	[Status] SMALLINT NOT NULL DEFAULT 0,
	[InternalStatus] SMALLINT NOT NULL DEFAULT 0, 
    [UniqueId] VARCHAR(150) NOT NULL, 
    [Size] BIGINT NOT NULL , 
	[Mime] VARCHAR(50) NULL,
    CONSTRAINT [FK_Format_FormatType] FOREIGN KEY ([Type]) REFERENCES [FormatType]([Id]), 
    CONSTRAINT [FK_Format_FormatStatus] FOREIGN KEY ([Status]) REFERENCES [FormatStatus]([Id]),
	CONSTRAINT [FK_Format_FormatInternalStatus] FOREIGN KEY ([InternalStatus]) REFERENCES [FormatInternalStatus]([Id]),
	CONSTRAINT [FK_Format_FormatGroup] FOREIGN KEY ([FormatGroupId]) REFERENCES [FormatGroup]([Id]),
	CONSTRAINT [FK_Format_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [Profile]([Id])
)

GO

CREATE INDEX [IX_Format_UniqueId] ON [dbo].[Format] ([UniqueId])
go


CREATE TRIGGER [dbo].[Trigger_Format_Update]
    ON [dbo].[Format]
    FOR UPDATE
    AS
    BEGIN
        SET NoCount ON;

		IF UPDATE(InternalStatus) OR UPDATE(Type)
		BEGIN
			UPDATE dbo.Material
				SET MaterialStatus = [dbo].[fnctGetMaterialStatus](Id)
				WHERE Id IN (SELECT fg.MaterialId FROM deleted d INNER JOIN dbo.FormatGroup fg ON fg.Id = d.FormatGroupId
								UNION 
							SELECT fg.MaterialId FROM inserted d INNER JOIN dbo.FormatGroup fg ON fg.Id = d.FormatGroupId);
		END
    END

Go

CREATE TRIGGER [dbo].[Trigger_Format_InsertDelete]
    ON [dbo].[Format]
    FOR DELETE, INSERT
    AS
    BEGIN
        SET NoCount ON;

		UPDATE dbo.Material
			SET MaterialStatus = [dbo].[fnctGetMaterialStatus](Id)
			WHERE Id IN (SELECT fg.MaterialId FROM deleted d INNER JOIN dbo.FormatGroup fg ON fg.Id = d.FormatGroupId
							UNION 
						SELECT fg.MaterialId FROM inserted d INNER JOIN dbo.FormatGroup fg ON fg.Id = d.FormatGroupId);
    END
GO