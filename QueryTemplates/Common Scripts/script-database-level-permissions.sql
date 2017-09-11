/*
From http://www.sqlservercentral.com/Forums/Topic1560182-1550-1.aspx
*/
SET NOCOUNT ON 
DECLARE @sql NVARCHAR(MAX)
SET @sql = ''
SELECT @sql = 
'--======================================================================================' + CHAR(10) +
'--==== IMPORTANT: Before executing these scripts check the  details to ensure they  ====' + CHAR(10) +
'--==== are valid. For instance when crossing domains                                ====' + CHAR(10) +
'--======================================================================================' + CHAR(10)
PRINT @sql
SET @sql = ''
--========================================================
--script any certificates in the database
--========================================================
IF (SELECT COUNT(*) FROM sys.certificates) = 0
BEGIN
		SELECT @sql = @sql + '/*No certificates found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
		SELECT @sql = '/*Scripting all user certificates' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13) + CHAR(13)
		SELECT @sql = @sql + 'CREATE CERTIFICATE ' + name + 
		' ENCRYPTION BY PASSWORD = ''P@ssw0rd1''
		WITH SUBJECT = ''' + issuer_name + ''', 
		EXPIRY_DATE = ''' + CONVERT(NVARCHAR(25), expiry_date, 120) + '''' + CHAR(13)
		FROM sys.certificates
PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--======================================================
--Script the database users
--======================================================
SELECT principal_id INTO #users FROM sys.database_principals WHERE type IN ('U', 'G', 'S') AND principal_id > 4
IF (SELECT COUNT(*) FROM #users) = 0
BEGIN
		SELECT @sql = @sql + '/*No database users found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
	SET CONCAT_NULL_YIELDS_NULL OFF
	DECLARE @uid INT
	SELECT @sql = '/*Scripting all database users and schemas' + CHAR(10) + 
	'===================================================================================' + CHAR(13) +
	'Note: these are the users found in the database, but they may not all be valid, check them first*/' +
	CHAR(13) + CHAR(13)
	--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(13) + CHAR(10) + CHAR(13)
	WHILE (SELECT TOP 1 principal_id FROM #users) IS NOT NULL
		BEGIN
			SELECT TOP 1 @uid = principal_id FROM #users			
			SELECT @sql = @sql + 'IF (SELECT name FROM sys.database_principals WHERE name = ''' + dp.name + ''') IS NULL' + CHAR(13) + 'BEGIN' + CHAR(13) +
			'CREATE USER ' + QUOTENAME(dp.name) + 
			/*CASE 
			WHEN SUSER_SID(dp.name) IS NULL THEN ''
			ELSE ' FOR LOGIN ' + QUOTENAME(dp.name)
			END +*/
			CASE
			WHEN SUSER_SNAME(dp.sid) IS NULL THEN ' WITHOUT LOGIN'
			ELSE ' FOR LOGIN ' + QUOTENAME(SUSER_SNAME(dp.sid)) 
			END + 
			CASE 			
			WHEN dp.default_schema_name IS NULL AND dp.type <> 'G' THEN ' WITH DEFAULT_SCHEMA = [dbo]'
			ELSE ' WITH DEFAULT_SCHEMA = [' + dp.default_schema_name + ']'
			END + CHAR(13) + 'END' 
			FROM sys.database_principals dp LEFT OUTER JOIN
			sys.schemas sch ON dp.principal_id = sch.principal_id
			WHERE dp.principal_id = @uid AND dp.TYPE IN ('U', 'G', 'S') AND dp.principal_id > 4			
			
			PRINT @sql + CHAR(10) 
			DELETE FROM #users WHERE principal_id = @uid
			
			SELECT @sql = ''
		END
		
		DROP TABLE #users
END
SELECT @sql = ''
--========================================================
--Script any users that are protected by a cert
--========================================================
IF (SELECT count(*) FROM sys.database_principals dp INNER JOIN sys.certificates c ON dp.sid = c.sid
	WHERE dp.type = 'C' AND dp.principal_id > 4) = 0
BEGIN
		SELECT @sql = @sql + '/*No certificated users found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
			SELECT @sql = '/*Scripting all certificated database users' + CHAR(10) + 
			'===================================================================================*/' + CHAR(13)
			--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(13) --+ 'GO' + CHAR(10) 
			SELECT @sql = @sql + 'CREATE USER ' + QUOTENAME(dp.name) + ' FOR CERTIFICATE ' + c.name
	    	FROM sys.database_principals dp INNER JOIN sys.certificates c ON dp.sid = c.sid
	  		WHERE dp.type = 'C' AND dp.principal_id > 4
	  		
	  		PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--=======================================================
--script all schemas
--=======================================================
		SELECT @sql = '/*Scripting all user schema permissions' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)	
		
		--Script the permission grants on the schemas		
		SELECT @sql = @sql + CHAR(13) + dp.state_desc COLLATE latin1_general_ci_as + ' ' + 
		dp.permission_name + ' ON ' + dp.class_desc + '::' + QUOTENAME(sch.name) + 
		' TO ' + QUOTENAME(dp2.name) + ' AS ' + QUOTENAME(dp3.name) 
		FROM sys.database_permissions dp 
		INNER JOIN sys.schemas sch ON dp.grantor_principal_id = sch.principal_id
		INNER JOIN sys.database_principals dp2 ON dp.grantee_principal_id = dp2.principal_id
		INNER JOIN sys.database_principals dp3 ON dp.grantor_principal_id = dp3.principal_id
		WHERE dp.class = 3  --dp.major_id BETWEEN 1 AND 8
		
		PRINT @sql + CHAR(13) + CHAR(13) 
SET @sql = ''
--========================================================
--script database roles from the database
--========================================================
IF (SELECT COUNT(*) FROM sys.database_principals WHERE type = 'R' AND is_fixed_role <> 1 AND principal_id > 4) = 0
BEGIN
		SELECT @sql = @sql + '/*No database roles found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
			SELECT @sql = '/*Scripting all database roles' + CHAR(10) + 
			'===================================================================================*/' + CHAR(13)
			--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(10) --+ 'GO' + CHAR(10)
			SELECT @sql = @sql + 'CREATE ROLE ' + QUOTENAME(dp.name) + ' AUTHORIZATION ' + QUOTENAME(dp2.name) + CHAR(13)
			FROM sys.database_principals dp INNER JOIN sys.database_principals dp2 
			ON dp.owning_principal_id = dp2.principal_id
			WHERE dp.type = 'R' AND dp.is_fixed_role <> 1 AND dp.principal_id > 4
			
			PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--=========================================================
--script Application roles from the database
--=========================================================
IF (SELECT COUNT(*) FROM sys.database_principals WHERE type = 'A') = 0
BEGIN
		SELECT @sql = @sql + '/*No application roles found*/'
		PRINT @sql + CHAR(13) + CHAR(13) 
END 
ELSE
BEGIN
		SELECT @sql = '/*Scripting all application roles' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)
		--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(10) --+ 'GO' + CHAR(10)
		SELECT @sql = @sql + 'CREATE APPLICATION ROLE ' + dp.name + ' WITH DEFAULT_SCHEMA = ' + 
		QUOTENAME(dp.default_schema_name) + ', PASSWORD = N''P@ssw0rd1''' + CHAR(10)
		FROM sys.database_principals dp
		WHERE dp.type = 'A' AND dp.is_fixed_role <> 1 AND dp.principal_id > 4
PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--===============================================================
--got the roles so now we need to get any nested role permissions
--===============================================================
IF (SELECT COUNT(*) from sys.database_principals dp inner join sys.database_role_members drm
		ON dp.principal_id = drm.member_principal_id inner join sys.database_principals dp2 
		ON drm.role_principal_id = dp2.principal_id WHERE dp.type = 'R') = 0
BEGIN
		SELECT @sql = + '/*No nested roles found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
		SELECT @sql = '/*Scripting all nested roles' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)
		--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(10) --+ 'GO' + CHAR(10)
		SELECT @sql = @sql + 'EXEC sp_addrolemember ''' + dp2.name + ''', ''' + dp.name + '''' + CHAR(10)
		FROM sys.database_principals dp 
		INNER JOIN sys.database_role_members drm
		ON dp.principal_id = drm.member_principal_id 
		INNER JOIN sys.database_principals dp2 
		ON drm.role_principal_id = dp2.principal_id
		WHERE dp.type = 'R'
PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--================================================================
--Scripting all user connection grants
--================================================================
IF		(SELECT COUNT(*) FROM sys.database_permissions dpm INNER JOIN sys.database_principals dp 
		ON dpm.grantee_principal_id = dp.principal_id WHERE dp.principal_id > 4 AND dpm.class = 0 AND dpm.type = 'CO') = 0
BEGIN
		SELECT @sql = + '/*No database connection GRANTS found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
		SELECT @sql = '/*Scripting all database and connection GRANTS' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)
		--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(10) --+ 'GO' + CHAR(10)
		SELECT @sql = @sql + dpm.state_desc COLLATE Latin1_General_CI_AS + ' ' + 
		dpm.permission_name COLLATE Latin1_General_CI_AS + ' TO ' + QUOTENAME(dp.name) + CHAR(13)
		FROM sys.database_permissions dpm INNER JOIN sys.database_principals dp 
		ON dpm.grantee_principal_id = dp.principal_id
		WHERE dp.principal_id > 4 AND dpm.class = 0 --AND dpm.type = 'CO'
PRINT @sql + CHAR(13) + CHAR(13)
END
SET @sql = ''
--=================================================================
--Now all the object level permissions
--=================================================================
IF		(SELECT	COUNT(*) FROM sys.database_permissions dbpe INNER JOIN sys.database_principals dbpr 
		ON dbpr.principal_id = dbpe.grantee_principal_id INNER JOIN sys.objects obj 
		ON dbpe.major_id = obj.object_id WHERE obj.type NOT IN ('IT','S','X')) = 0
BEGIN
		SELECT @sql = + '/*No database user object GRANTS found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
	
		SELECT @sql = '/*Scripting all database user object GRANTS' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)
		--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(13) --+ 'GO' 
		PRINT @sql --+ CHAR(10)
		SET @sql = ''
		
		IF OBJECT_ID('tempdb..#objgrants') IS NOT NULL
		BEGIN
			DROP TABLE #objgrants
		END
		CREATE TABLE #objgrants(
				state_desc	VARCHAR(60)
				, perm_name		NVARCHAR(128)
				, sch_name		NVARCHAR(128)	
				, maj_ID		NVARCHAR(128)
				, name			NVARCHAR(128)	
				, pr_name		NVARCHAR(128)
				)
		
		DECLARE @state_desc VARCHAR(60)
		DECLARE @perm_name NVARCHAR(128), @sch_name NVARCHAR(128), @maj_ID NVARCHAR(128)
		DECLARE @name NVARCHAR(128), @pr_name NVARCHAR(128)		
					
		INSERT INTO #objgrants
		SELECT CASE dbpe.[state] WHEN 'W' THEN 'GRANT'
		ELSE dbpe.state_desc COLLATE Latin1_General_CI_AS
		END AS [state_desc]
		, dbpe.permission_name COLLATE Latin1_General_CI_AS AS perm_name
		, sch.name AS sch_name
		, OBJECT_NAME(dbpe.major_id) AS maj_ID
		, dbpr.name AS name
		, CASE dbpe.[state] WHEN 'W' THEN '] WITH GRANT OPTION'
		ELSE ']' END AS pr_name
		FROM sys.database_permissions dbpe INNER JOIN sys.database_principals dbpr 
		ON dbpr.principal_id = dbpe.grantee_principal_id
		INNER JOIN sys.objects obj ON dbpe.major_id = obj.object_id
		INNER JOIN sys.schemas sch ON obj.schema_id = sch.schema_id
		WHERE obj.type NOT IN ('IT','S','X') 
		ORDER BY dbpr.name, obj.name
			
		WHILE (SELECT COUNT(*) FROM #objgrants) > 0
		BEGIN
			
			SELECT TOP 1 @state_desc = state_desc, @perm_name = perm_name, @sch_name = sch_name, 
			@maj_ID = maj_ID, @name = name, @pr_name = pr_name FROM #objgrants
			
				SELECT @sql = @sql + @state_desc + ' ' + @perm_name +
				' ON [' + @sch_name + '].[' + @maj_ID + '] TO [' + @name + @pr_name
				PRINT @sql 	
				SET @sql = ''			
DELETE FROM #objgrants WHERE state_desc = @state_desc AND perm_name = @perm_name 
			AND sch_name = @sch_name AND maj_ID = @maj_ID AND name = @name AND pr_name = @pr_name
			 
		END
		PRINT CHAR(13)
DROP TABLE #objgrants
END
		
SET @sql = ''
--=================================================================
--Now script all the database roles the user have permissions to
--=================================================================
IF		(SELECT COUNT(*) FROM sys.database_principals dp
		INNER JOIN sys.database_role_members drm ON dp.principal_id = drm.role_principal_id
		INNER JOIN sys.database_principals dp2 ON drm.member_principal_id = dp2.principal_id
		WHERE dp2.principal_id > 4 AND dp2.type <> 'R') = 0
BEGIN
		SELECT @sql = + '/*No database user role GRANTS found*/'
		PRINT @sql + CHAR(13) + CHAR(13)
END 
ELSE
BEGIN
	
		SELECT @sql = '/*Scripting all database user role permissions' + CHAR(10) + 
		'===================================================================================*/' + CHAR(13)
		--SELECT @sql = @sql + 'USE ' + QUOTENAME(DB_NAME(DB_ID())) + CHAR(13) + CHAR(10)	
		SELECT @sql = @sql + 'EXEC sp_addrolemember ''' + dp.name + ''', ''' + dp2.name + '''' + CHAR(13)
		FROM sys.database_principals dp
		INNER JOIN sys.database_role_members drm ON dp.principal_id = drm.role_principal_id
		INNER JOIN sys.database_principals dp2 ON drm.member_principal_id = dp2.principal_id
		WHERE dp2.principal_id > 4 AND dp2.type <> 'R'
		
		PRINT @sql + CHAR(13) + CHAR(13)
END
	
SET @sql = ''
PRINT '--Finished!'
