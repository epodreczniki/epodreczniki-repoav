CREATE PROCEDURE [dbo].[UpdateFormat]
	@UniqueId VARCHAR(150),
	@Size BIGINT,
	@RealSize BIGINT,
	@Status SMALLINT,
	@Mime VARCHAR(50),
	@Location VARCHAR (200) 
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.[Format]
		SET [Size] = @Size,
			[RealSize] = @RealSize,
			Mime = ISNULL(@Mime, Mime),
			Location = ISNULL(@Location, Location)
		WHERE UniqueId = @UniqueId;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFOund
END