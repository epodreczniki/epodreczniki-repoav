CREATE PROCEDURE [dbo].[SetFormatSize]
	@UniqueId VARCHAR(150),
	@Size BIGINT,
	@RealSize BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.[Format]
		SET [Size] = @Size,
			[RealSize] = @RealSize
		WHERE UniqueId = @UniqueId;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFOund
END