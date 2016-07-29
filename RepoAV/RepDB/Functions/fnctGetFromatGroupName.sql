
CREATE FUNCTION [dbo].[fnctGetFormatGroupName]
(
	@Id INT
	
)
RETURNS VARCHAR(64)
AS
BEGIN
	DECLARE @Name VARCHAR(64) 

	SELECT @Name = CASE WHEN fg.SourceId IS NULL THEN 'source'  
						WHEN fg.SubtitleId IS NULL THEN  CASE WHEN fg.AudioId IS NULL THEN  'recoded' ELSE CONCAT('recoded: audio ',CAST(fg.AudioId+1 as varchar(2))) END
						ELSE CASE WHEN fg.AudioId IS NULL THEN CONCAT('recoded: ', LOWER(ft.Name)) ELSE CONCAT('recoded: audio ',CAST(fg.AudioId+1 as varchar(2)), ', ', LOWER(ft.Name)) END END
	FROM FormatGroup fg
		LEFT JOIN Format f ON f.Id = fg.SubtitleId
		LEFT JOIN FormatType ft ON ft.Id = f.Type
	WHERE fg.Id = @Id

	RETURN @Name
END