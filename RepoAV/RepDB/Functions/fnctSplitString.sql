CREATE FUNCTION [dbo].[fnctSplitString]
(
	@InputString VARCHAR (MAX), 
	@Separator CHAR (1)
)
RETURNS 
@Result TABLE
(
	Part varchar(max),
	Idx int
)
AS
BEGIN
	DECLARE @Pos int,
			@Part varchar(max),
			@Idx int;
			
	SET @Idx = 0;
	WHILE (@InputString IS NOT NULL AND LEN(@InputString) > 0)
	BEGIN
		SET @Pos = CHARINDEX(@Separator, @InputString)

		SET @Part =
		CASE
			WHEN (@Pos > 0) THEN SUBSTRING(@InputString, 1, @Pos - 1)
			ELSE @InputString
		END
		SET @InputString =
		CASE
			WHEN (@Pos > 0) THEN SUBSTRING(@InputString, @Pos + LEN(@Separator), LEN(@InputString) - @Pos - LEN(@Separator) + 1)
			ELSE NULL
		END

		IF (@Part IS NOT NULL AND LEN(@Part) > 0)
		BEGIN
			INSERT INTO @Result
				(Part, Idx)
				VALUES (@Part, @Idx);
				
			SET @Idx = @Idx + 1;
			
		END
	END
	RETURN
END
