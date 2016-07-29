CREATE FUNCTION [dbo].[fnctGetMaterialStatus]
(
    @Id_Material INT
)
RETURNS SMALLINT
AS
BEGIN
	-- to już jest niepotrzebne, gdy metoda jest wołana w trigerach
	IF EXISTS (SELECT m.Id  FROM Material m WHERE m.Id = @Id_Material AND m.Deleted = 1)
		RETURN 5; --RemovePending 

	DECLARE @Formats TABLE(Id BIGINT, InternalStatus SMALLINT, Type SMALLINT)
	INSERT @Formats
		SELECT f.Id, f.InternalStatus, f.Type FROM dbo.[Format] f INNER JOIN 
			dbo.FormatGroup fg ON f.FormatGroupId = fg.Id
			WHERE fg.MaterialId = @Id_Material

	IF @@ROWCOUNT = 0
		RETURN 7; -- Adding
	IF EXISTS (SELECT * FROM @Formats WHERE InternalStatus = 7)--NotFound
		RETURN 2; -- NotFound
	IF EXISTS (SELECT * FROM @Formats WHERE InternalStatus = 6)--InvalidFile
		RETURN 1; -- InvalidFile
	IF EXISTS (SELECT * FROM @Formats WHERE InternalStatus = 1 AND Type <> 1) -- AddError for source
		RETURN 4; -- AddError
	IF EXISTS (SELECT * FROM @Formats WHERE (InternalStatus = 1 OR InternalStatus =3) AND  Type = 1) -- AddError or RecError for Recoded
		RETURN 3; -- RecError
	IF EXISTS( SELECT * FROM @Formats WHERE InternalStatus = 0 AND Type <> 1) --Adding for source
		RETURN 7; -- Adding
	IF EXISTS (SELECT * FROM @Formats WHERE (InternalStatus = 0 OR InternalStatus = 2) AND Type = 1)-- Adding or Recoding for Recoded
		RETURN 6; -- Recoding
	IF EXISTS (SELECT * FROM @Formats WHERE InternalStatus = 8)--Recoded
		RETURN 6; --Recoding
	IF EXISTS(SELECT * FROM @Formats WHERE InternalStatus = 4)--RemovePending
		RETURN 5; --RemovePending     
   IF NOT EXISTS (SELECT * FROM @Formats WHERE Type = 1) -- Recoded
		RETURN 8; -- Added
	
	RETURN 10; --Recoded
END
