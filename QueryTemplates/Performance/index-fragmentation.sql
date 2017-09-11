SELECT dbschemas.[name] as 'Schema', 
	dbtables.[name] as 'Table', 
	dbindexes.[name] as 'Index',
	indexstats.avg_fragmentation_in_percent,
	indexstats.page_count,
	'ALTER INDEX ['+dbindexes.[name]+'] ON [' + dbschemas.name + '].['+dbtables.[name]+'] REORGANIZE;'
FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS indexstats
INNER JOIN sys.tables dbtables on dbtables.[object_id] = indexstats.[object_id]
INNER JOIN sys.schemas dbschemas on dbtables.[schema_id] = dbschemas.[schema_id]
INNER JOIN sys.indexes AS dbindexes ON dbindexes.[object_id] = indexstats.[object_id]
AND indexstats.index_id = dbindexes.index_id
WHERE indexstats.database_id = DB_ID()
AND indexstats.page_count > 100
AND dbindexes.[name] IS NOT NULL
ORDER BY indexstats.avg_fragmentation_in_percent DESC