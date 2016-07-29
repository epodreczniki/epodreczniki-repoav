CREATE FUNCTION [dbo].[fnctGetNodePreferred4TaskAsString]
(
	@Id_Task BIGINT
)
RETURNS VARCHAR(150)
AS
BEGIN

	DECLARE @Res VARCHAR(150);

	SET @Res = '';
	
	SELECT @Res = @Res + CAST(tpn.NodeId AS VARCHAR(15)) + ';'
		FROM dbo.TaskPreferredNode tpn
		WHERE tpn.Id_Task = @Id_Task
		ORDER BY tpn.NodeId;

	IF LEN(@Res) > 0
		SET @Res = SUBSTRING(@Res, 1, LEN(@Res) - 1)

	IF LEN(@Res ) < 1
		SET @Res = NULL;

	RETURN @Res;
END
