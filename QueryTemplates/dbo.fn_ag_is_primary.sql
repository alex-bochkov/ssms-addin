USE master;
GO

CREATE OR ALTER FUNCTION dbo.fn_ag_is_primary(@AGName sysname NULL)
RETURNS bit
AS
BEGIN
	DECLARE @ServerName NVARCHAR(256)  = @@SERVERNAME 
	DECLARE @RoleDesc NVARCHAR(60)

	IF @AGName IS NULL
	BEGIN
		IF (SELECT COUNT(*) FROM sys.availability_groups) > 1
		BEGIN
			RETURN 0;
		END

		SELECT @AGName = AG.name FROM sys.availability_replicas AR 
			INNER JOIN sys.availability_groups AG
				ON AR.group_id = AG.group_id
		WHERE replica_server_name = @ServerName
	END
	ELSE
	BEGIN
		IF NOT EXISTS(SELECT 1 FROM sys.availability_groups WHERE name = @AGName)
		BEGIN
			RETURN 0;
		END
	END

	SELECT @RoleDesc = a.role_desc
		FROM sys.dm_hadr_availability_replica_states AS a
		JOIN sys.availability_replicas AS b
			ON b.replica_id = a.replica_id
		INNER JOIN sys.availability_groups AG
			ON a.group_id = AG.group_id
	WHERE b.replica_server_name = @ServerName 
		AND  AG.name = @AGName

	IF @RoleDesc = 'PRIMARY'
	BEGIN
		RETURN 1;
	END

	RETURN 0;
END
