SELECT dbschemas.[name] as 'Schema', 
	dbtables.[name] as 'Table', 
	dbindexes.[index_id] as 'IndexID',
	dbindexes.[name] as 'Index',
	indexstats.partition_number,
	FORMAT(AVG(indexstats.avg_fragmentation_in_percent), 'N2') AS avg_fragmentation_in_percent,
	FORMAT(SUM(indexstats.page_count), 'N0') AS page_count,
	'ALTER INDEX ['+dbindexes.[name]+'] ON [' + dbschemas.name + '].['+dbtables.[name]+'] REORGANIZE PARTITION = ' 
		+ CASE WHEN EXISTS(SELECT TOP 1 1 FROM sys.partition_schemes s WHERE s.data_space_id = dbindexes.data_space_id) THEN CAST(indexstats.partition_number AS VARCHAR(3)) ELSE 'ALL' END + ';' AS CmdReorg,
	'ALTER INDEX ['+dbindexes.[name]+'] ON [' + dbschemas.name + '].['+dbtables.[name]+'] REBUILD PARTITION = ' 
		+ CASE WHEN EXISTS(SELECT TOP 1 1 FROM sys.partition_schemes s WHERE s.data_space_id = dbindexes.data_space_id) THEN CAST(indexstats.partition_number AS VARCHAR(3)) ELSE 'ALL' END
		+ ' WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = SELF)), '
		+ 'SORT_IN_TEMPDB = ON);' AS CmdRebuild
FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS indexstats
INNER JOIN sys.tables dbtables on dbtables.[object_id] = indexstats.[object_id]
INNER JOIN sys.schemas dbschemas on dbtables.[schema_id] = dbschemas.[schema_id]
INNER JOIN sys.indexes AS dbindexes ON dbindexes.[object_id] = indexstats.[object_id]
	AND indexstats.index_id = dbindexes.index_id
WHERE indexstats.database_id = DB_ID()
	AND indexstats.page_count > 100
	AND dbindexes.[name] IS NOT NULL
GROUP BY dbschemas.[name], 
	dbtables.[name], 
	dbindexes.[index_id],
	dbindexes.[name],
	indexstats.partition_number,
	dbindexes.data_space_id
ORDER BY avg_fragmentation_in_percent DESC
