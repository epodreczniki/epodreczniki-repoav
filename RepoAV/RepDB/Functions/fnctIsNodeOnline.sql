CREATE FUNCTION [dbo].[fnctIsNodeOnline]
(
	@Id_Node int
)
RETURNS BIT
AS
BEGIN
	IF EXISTS(SELECT * FROM dbo.Node WHERE Id = @Id_Node AND Enabled = 1 AND DATEDIFF(second, CheckTime, GetDate()) < 120)
		RETURN 1;
	RETURN 0;
END
