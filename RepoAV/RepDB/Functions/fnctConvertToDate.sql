CREATE FUNCTION [dbo].[fnctConvertToDate] 
(
	@date VARCHAR(32)
)
RETURNS DATETIME 
AS
BEGIN	
	RETURN CAST(CONCAT(SUBSTRING(@date,1,4), '-', 
					SUBSTRING(@date,5,2), '-',
					SUBSTRING(@date,7,2), ' ', 
					SUBSTRING(@date,9,2),':', 
					SUBSTRING(@date,11,2) ,':',
					SUBSTRING(@date,13,2) )
					AS DATETIME)

END