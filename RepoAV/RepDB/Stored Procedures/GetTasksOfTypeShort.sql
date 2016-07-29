CREATE PROCEDURE [dbo].[GetTasksOfTypeShort]
    @Type SMALLINT 
AS 
BEGIN 
    SET NOCOUNT ON; 
    SELECT t.Id, t.PublicId, t.[Type], t.[Status], t.ResultProcessed, t.UniqueId
        FROM dbo.Task t 
        WHERE (@Type IS NULL OR t.[Type] = @Type) 
        ORDER BY t.CreatedDate; 
END
