SELECT dbschemas.[name] as 'Schema', 
	dbtables.[name] as 'Table', 
	dbindexes.[name] as 'Index',
	--averaging this number across all partitions is not correct (page_count must bw accounted) but it is okay for my needs
	AVG(indexstats.avg_fragmentation_in_percent) AS avg_fragmentation_in_percent,
	SUM(indexstats.page_count) AS page_count,
	'ALTER INDEX ['+dbindexes.[name]+'] ON [' + dbschemas.name + '].['+dbtables.[name]+'] REORGANIZE;' AS CmdReorg,
	'ALTER INDEX ['+dbindexes.[name]+'] ON [' + dbschemas.name + '].['+dbtables.[name]+'] REBUILD WITH (FILLFACTOR = 90, ONLINE = ON, DATA_COMPRESSION = PAGE, SORT_IN_TEMPDB = ON);' AS CmdRebuild
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
	dbindexes.[name]
ORDER BY avg_fragmentation_in_percent DESC
