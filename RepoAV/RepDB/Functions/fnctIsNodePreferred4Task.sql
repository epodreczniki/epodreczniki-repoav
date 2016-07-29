CREATE FUNCTION [dbo].[fnctIsNodePreferred4Task]
(
	@Id_Node INT,
	@Id_Task BIGINT
)
RETURNS BIT
AS
BEGIN
	IF (SELECT ISNULL(count(tpn.NodeId), 0) FROM dbo.TaskPreferredNode tpn WHERE tpn.Id_Task = @Id_Task) = 0
		RETURN 1;

	IF EXISTS (SELECT * FROM dbo.TaskPreferredNode tpn WHERE tpn.Id_Task = @Id_Task AND tpn.NodeId = @Id_Node)
		RETURN 1;

	RETURN 0;
END
