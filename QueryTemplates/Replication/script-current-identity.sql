SET NOCOUNT ON;

DROP TABLE IF EXISTS #AllTables;

CREATE TABLE #AllTables (tableName varchar(300), identityValue VARCHAR(50));

DECLARE @AllDatabases AS TABLE (databaseName VARCHAR(128), RN int);
INSERT INTO @AllDatabases
SELECT name, ROW_NUMBER() OVER (ORDER BY name) AS RN
FROM sys.databases
WHERE database_id > 4

DECLARE @RowsTotal INT, @Row INT, @Cmd VARCHAR(MAX), @Database VARCHAR(128);

SELECT @RowsTotal = MAX(RN), @Row = 1 FROM @AllDatabases;

WHILE @Row <= @RowsTotal
BEGIN
	
	SELECT @Database = databaseName,
		@Cmd = '
		USE [' + databaseName + '];
		INSERT INTO #AllTables
		SELECT 
			''['' + SCHEMA_NAME(schema_id) + ''].['' + name + '']'' AS FullTableName,
			IDENT_CURRENT(SCHEMA_NAME(schema_id) + ''.'' + name) AS CurrentValue
		FROM sys.tables
		WHERE OBJECTPROPERTY(object_id, ''TableHasIdentity'') = 1
		'
	FROM @AllDatabases WHERE RN = @Row

	EXEC(@Cmd);	
	
	SELECT 'USE ['+@Database+']';
	SELECT 'DBCC CHECKIDENT (''' + tableName + ''', RESEED, ' + identityValue + ')'
	FROM #AllTables

	DELETE FROM #AllTables;

	SET @Row = @Row + 1;

END
