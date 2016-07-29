CREATE PROCEDURE [dbo].[ChangeFormat]
	@Id INT,
	@FormatGroupId INT,
	@ProfileId INT,
	@XmlMetadata NVARCHAR(MAX),
	@Type SMALLINT,
	@UniqueId VARCHAR(150),
	@Size BIGINT,
	@Mime VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.FormatGroup WHERE Id = @FormatGroupId)
		RETURN -58; --FormatGroupNotFound

	IF @ProfileId IS NOT NULL
		IF NOT EXISTS (SELECT * FROM dbo.[Profile] WHERE Id = @ProfileId)
			RETURN -59; --ProfileNotFound

	IF NOT EXISTS (SELECT * FROM dbo.[FormatType] WHERE Id = @Type)
		RETURN -60; --FormatTypeNotFound

	IF EXISTS (SELECT * FROM dbo.Format WHERE UniqueId = @UniqueId AND Id <> @Id)
		RETURN -43; -- AlreadyExists

	UPDATE dbo.[Format]
		SET FormatGroupId = @FormatGroupId, 
			ProfileId = @ProfileId, 
			XmlMetadata = @XmlMetadata, 
			[Type] = @Type, 
			UniqueId = @UniqueId, 
			Size = @Size,
			Mime = @Mime
		WHERE Id = @Id;
			
	UPDATE Material SET ModifyDate = GETDATE() WHERE Id = (SELECT MaterialId FROM FormatGroup WHERE Id = @FormatGroupId)	
	
	RETURN 1;
END


