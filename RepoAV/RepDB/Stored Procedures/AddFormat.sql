CREATE PROCEDURE [dbo].[AddFormat]
	@Id INT OUTPUT,
	@FormatGroupId INT,
	@ProfileId INT,
	@XmlMetadata NVARCHAR(MAX),
	@Type SMALLINT,
	@UniqueId VARCHAR(150),
	@Size BIGINT,
	@InternalStatus SMALLINT,
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

	IF EXISTS (SELECT * FROM dbo.Format WHERE UniqueId = @UniqueId)
		RETURN -43; -- AlreadyExists

	INSERT dbo.[Format]
		(FormatGroupId, ProfileId, XmlMetadata, [Type], UniqueId, Size, InternalStatus, Mime)
		VALUES (@FormatGroupId, @ProfileId, @XmlMetadata, @Type, @UniqueId, @Size, @InternalStatus, @Mime)
	SET @Id = SCOPE_IDENTITY();

	UPDATE Material SET ModifyDate = GETDATE() WHERE Id = (SELECT MaterialId FROM FormatGroup WHERE Id = @FormatGroupId)	

	RETURN 1;
END


