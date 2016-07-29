CREATE PROCEDURE [dbo].[SetFormatStatus]
	@UniqueId VARCHAR(150),
	@OldStatus SMALLINT,
	@NewStatus SMALLINT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT * FROM dbo.[Format] WHERE UniqueId = @UniqueId)
		RETURN -41; --NotFOund

	UPDATE dbo.[Format]
		SET [Status] = @NewStatus
		WHERE UniqueId = @UniqueId
			AND [Status] = @OldStatus;

	IF @@ROWCOUNT > 0
	BEGIN
	UPDATE Material SET ModifyDate = GETDATE() 
			WHERE Id = (SELECT Material.Id FROM Material INNER JOIN FormatGroup fg ON fg.MaterialId = Material.Id INNER JOIN Format f on f.FormatGroupId = fg.Id WHERE f.UniqueId = @UniqueId)
		RETURN 1;
	END
	ELSE 
		RETURN -44;--StateChangedMeanwhile
END


