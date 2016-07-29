CREATE PROCEDURE [dbo].[RemoveFormat]
	@Id INT,
	@UniqueId VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	IF @Id IS NULL
	BEGIN
		SELECT @Id = Id FROM dbo.Format WHERE UniqueId = @UniqueId;

		IF @Id IS NULL
			RETURN -41;--NotFound
	END

	UPDATE Material SET ModifyDate = GETDATE() 
	WHERE Id = ( SELECT MaterialId FROM FormatGroup fg
				 INNER JOIN Format f on f.FormatGroupId = fg.Id
			    WHERE f.Id = @Id)	

	DELETE dbo.[Format] WHERE Id = @Id;

	IF @@ROWCOUNT > 0
		RETURN 1;
	ELSE 
		RETURN -41;--NotFound
END
