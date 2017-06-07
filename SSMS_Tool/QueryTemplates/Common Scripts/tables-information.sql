WITH RowsStatistics
AS (SELECT SCHEMA_NAME(SO.schema_id) AS SchemaName,
           OBJECT_NAME(PS.object_id) AS TableName,
           PS.object_id AS ObjectId,
           SUM(row_count) AS RowsCount
    FROM sys.dm_db_partition_stats AS PS
         INNER JOIN
         sys.objects AS SO
         ON PS.OBJECT_ID = SO.object_id
    WHERE PS.index_id < 2
          AND PS.OBJECT_ID > 100
          AND SCHEMA_NAME(SO.schema_id) <> 'sys'
    GROUP BY SCHEMA_NAME(SO.schema_id), OBJECT_NAME(PS.object_id), PS.object_id),
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
       RS.RowsCount,
       TI.HasPK,
       TI.HasClusteredIndex,
       TI.PKisClustered,
       TI.is_replicated,
       L.[LastUserUpdate] AS LastWrite,
       COALESCE (L.LastUserScan, L.LastUserSeek, L.LastUserLookup) AS LastRead
FROM RowsStatistics AS RS
     FULL OUTER JOIN
     TableInformation AS TI
     ON RS.ObjectId = TI.ObjectId
     LEFT OUTER JOIN
     LastReadWrites AS L
     ON L.objectId = RS.ObjectId;
