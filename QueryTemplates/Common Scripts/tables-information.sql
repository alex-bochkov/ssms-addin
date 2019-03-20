WITH RowsStatistics
AS (SELECT SCHEMA_NAME(SO.schema_id) AS SchemaName,
           OBJECT_NAME(PS.object_id) AS TableName,
           PS.object_id AS ObjectId,
           SO.create_date,
           COUNT(DISTINCT ps.partition_number) as PartitionCount,
           SUM(row_count) AS RowsCount
    FROM sys.dm_db_partition_stats AS PS
         INNER JOIN
         sys.objects AS SO
         ON PS.OBJECT_ID = SO.object_id
    WHERE PS.index_id < 2
          AND PS.OBJECT_ID > 100
          AND SCHEMA_NAME(SO.schema_id) <> 'sys'
    GROUP BY SCHEMA_NAME(SO.schema_id), OBJECT_NAME(PS.object_id), PS.object_id, SO.create_date),
 TableSizes
AS (SELECT t.object_id AS ObjectId,
           CAST (ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC (36, 2)) AS UsedSpaceMB,
           CAST (ROUND(((SUM(CASE WHEN p.data_compression > 0 THEN a.used_pages ELSE 0 END) * 8) / 1024.00), 2) AS NUMERIC (36, 2)) AS UsedSpaceMB_Compressed
    FROM sys.tables AS t
         INNER JOIN
         sys.indexes AS i
         ON t.OBJECT_ID = i.object_id
         INNER JOIN
         sys.partitions AS p
         ON i.object_id = p.OBJECT_ID
            AND i.index_id = p.index_id
         INNER JOIN
         sys.allocation_units AS a
         ON p.partition_id = a.container_id
    GROUP BY t.object_id),
 TableInformation
AS (SELECT NAME,
           object_id AS ObjectId,
           CASE WHEN NAME IN (SELECT object_name(Parent_object_id)
                              FROM sys.objects
                              WHERE type = 'PK') THEN 1 ELSE 0 END AS HasPK,
           CASE WHEN NAME IN (SELECT OBJECT_NAME(OBJECT_ID)
                              FROM sys.indexes AS i
                              WHERE i.type_desc = 'CLUSTERED'
                                    AND i.object_id = T.object_id) THEN 1 ELSE 0 END AS HasClusteredIndex,
           CASE WHEN NAME IN (SELECT object_name(Parent_object_id)
                              FROM sys.objects
                              WHERE type = 'PK'
                                    AND object_name(object_id) IN (SELECT name
                                                                   FROM sys.indexes AS i
                                                                   WHERE i.type_desc = 'CLUSTERED'
                                                                         AND i.object_id = T.object_id)) THEN 1 ELSE 0 END AS PKisClustered,
           is_replicated
    FROM sys.tables AS T
    WHERE is_ms_shipped = 0),
 LastReadWrites
AS (SELECT object_id AS ObjectID,
           MAX(last_user_update) AS [LastUserUpdate],
           MAX(last_user_seek) AS [LastUserSeek],
           MAX(last_user_scan) AS [LastUserScan],
           MAX(last_user_lookup) AS [LastUserLookup]
    FROM sys.dm_db_index_usage_stats
    WHERE database_id = DB_ID()
    GROUP BY object_id)
SELECT DB_NAME() AS DatabaseName,
       rs.SchemaName,
       rs.TableName,
       FORMAT(RS.RowsCount, 'N0') AS RowsCount,
       FORMAT(TS.UsedSpaceMB, 'N0') AS UsedSpaceMB,
       FORMAT(TS.UsedSpaceMB_Compressed, 'N0') AS UsedSpaceMB_Compressed,
       RS.PartitionCount,
       TI.HasPK,
       TI.HasClusteredIndex,
       TI.PKisClustered,
       TI.is_replicated,
       L.[LastUserUpdate] AS LastWrite,
       COALESCE (L.LastUserScan, L.LastUserSeek, L.LastUserLookup) AS LastRead,
       RS.Create_date as CreateDate
FROM RowsStatistics AS RS
     FULL OUTER JOIN
     TableInformation AS TI
     ON RS.ObjectId = TI.ObjectId
     LEFT OUTER JOIN
     LastReadWrites AS L
     ON L.objectId = RS.ObjectId
     LEFT OUTER JOIN
     TableSizes AS TS
     ON TS.ObjectId = TI.ObjectId
ORDER BY TS.UsedSpaceMB DESC;

