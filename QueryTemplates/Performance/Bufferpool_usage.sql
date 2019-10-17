DECLARE @total_buffer INT;
SELECT @total_buffer = cntr_value   FROM sys.dm_os_performance_counters
WHERE RTRIM([object_name]) LIKE '%Buffer Manager'   AND counter_name = 'Total Pages';
;WITH src AS(   SELECT        database_id, db_buffer_pages = COUNT_BIG(*) 
FROM sys.dm_os_buffer_descriptors       --WHERE database_id BETWEEN 5 AND 32766       
GROUP BY database_id)SELECT   [db_name] = CASE [database_id] WHEN 32767        THEN 'Resource DB'        ELSE DB_NAME([database_id]) END,   db_buffer_pages,   db_buffer_MB = db_buffer_pages / 128,   db_buffer_percent = CONVERT(DECIMAL(6,3),        db_buffer_pages * 100.0 / @total_buffer)
FROM src
ORDER BY db_buffer_MB DESC;

--then drill down into memory used by objects in database of your choice

USE <db_with_most_memory>;

WITH src AS(   SELECT       [Object] = o.name,       [Type] = o.type_desc,       [Index] = COALESCE(i.name, ''),       [Index_Type] = i.type_desc,       p.[object_id],       p.index_id,       au.allocation_unit_id   
FROM       sys.partitions AS p   INNER JOIN       sys.allocation_units AS au       ON p.hobt_id = au.container_id   INNER JOIN       sys.objects AS o       ON p.[object_id] = o.[object_id]   INNER JOIN       sys.indexes AS i       ON o.[object_id] = i.[object_id]       AND p.index_id = i.index_id   WHERE       au.[type] IN (1,2,3)       AND o.is_ms_shipped = 0)
SELECT   src.[Object],   src.[Type],   src.[Index],   src.Index_Type,   buffer_pages = COUNT_BIG(b.page_id),   buffer_mb = COUNT_BIG(b.page_id) / 128
FROM   src
INNER JOIN   sys.dm_os_buffer_descriptors AS b  
 ON src.allocation_unit_id = b.allocation_unit_id
WHERE   b.database_id = DB_ID()
GROUP BY   src.[Object],   src.[Type],   src.[Index],   src.Index_Type
ORDER BY   buffer_pages DESC;
----------------------------------------------------------------

SELECT DB_NAME(dobd.database_id) as dbname
  ,dobd.database_id
  ,dobd.allocation_unit_id
  ,dobd.page_type
INTO #buffer_pool
FROM sys.dm_os_buffer_descriptors dobd
WHERE dobd.database_id = DB_ID(' < database > ')
 AND allocation_unit_id IS NOT NULL

SELECT dbname AS DB
	,objname AS db_object_name
	,COUNT(dbname) AS cache_page_count
	,COUNT('x') * 8.0 / 1024 AS size_mb
FROM (
	SELECT allocation_units.dbname
		,allocation_units.database_id
		,allocation_units.allocation_unit_id AS au_unit_id
		,allocation_units.page_type
		,object_details.allocation_unit_id
		,object_details.type
		,object_details.type_desc
		,object_details.name AS objname
		,object_details.index_id
	FROM #buffer_pool allocation_units
	LEFT OUTER JOIN (
		SELECT au.allocation_unit_id
			,au.type
			,au.type_desc
			,au.total_pages
			,o.object_id
			,o.name
			,p.index_id
		FROM sys.allocation_units au
		INNER JOIN sys.partitions p ON au.container_id = p.hobt_id 
		INNER JOIN sys.objects o ON p.object_id = o.object_id
		) AS object_details ON allocation_units.allocation_unit_id = object_details.allocation_unit_id
	) tab
GROUP BY dbname
	,objname
ORDER BY size_mb DESC
OPTION (MAXDOP 0)
