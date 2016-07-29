CREATE PROCEDURE [dbo].[AddFormat]
	@Id INT,
    @UniqueId  VARCHAR (150),
    @Location  VARCHAR (200),
    @Status	SMALLINT,
    @Size BIGINT,
	@RealSize BIGINT,
    @Mime VARCHAR(50), 
    @AllowDistribution BIT
AS
BEGIN
	SET NOCOUNT ON;

	If EXISTS(SELECT Id FROM dbo.[Format] WHERE Id = @Id OR UniqueId = @UniqueId)
		RETURN -43; --AlreadyExists

	INSERT dbo.[Format]
		(Id, UniqueId, Location, [Status], Size, Mime, AllowDistribution, RealSize)
		VALUES(@Id, @UniqueId, @Location, @Status, @Size, @Mime, @AllowDistribution, @RealSize);

	RETURN 1;
END
