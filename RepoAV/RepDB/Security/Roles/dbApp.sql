CREATE ROLE [db_app]
GO


GRANT EXECUTE TO [db_app]
GO

ALTER ROLE  [db_app] ADD MEMBER App;
GO
