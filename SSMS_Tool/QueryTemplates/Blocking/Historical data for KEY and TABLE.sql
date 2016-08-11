;WITH T1 AS (
	SELECT 
		REPLACE([waitresource], 'KEY: ' + CAST (DB_ID() AS NVARCHAR (2)) + ':', '') AS LockKey, 
		DATEADD(hour, DATEDIFF(hour, 0, ts), 0) AS ts
	FROM [dba_scrap_book].[dbo].[blocking_status] 
	WHERE waitresource LIKE 'KEY: ' + CAST (DB_ID() AS NVARCHAR (2)) + ':%'
		AND ts > GETDATE() - 10
),
AllStats AS (
	SELECT 
		 SUBSTRING(LockKey, 1, CHARINDEX('(', LockKey) - 2) AS LockKey,
		 ts
	FROM T1
)
SELECT ts, S.ObjName, COUNT(*) AS RowsCount FROM AllStats AS L
OUTER APPLY (SELECT o.name AS ObjName, i.name, OBJECT_SCHEMA_NAME(p.object_id, 9) AS Sc
                                FROM sys.partitions AS p
                                INNER JOIN sys.indexes AS i ON p.index_id = i.index_id AND p.[object_id] = i.[object_id]
                                INNER JOIN sys.objects AS o ON o.object_id = i.object_id WHERE hobt_id = L.LockKey) AS S
GROUP BY ts, S.ObjName
ORDER BY TS DESC						
								
								
;WITH AllTables
AS (SELECT REPLACE([waitresource], 'TAB: ' + CAST (DB_ID() AS NVARCHAR (2)) + ':', '') AS TableID,
           DATEADD(hour, DATEDIFF(hour, 0, ts), 0) AS ts
    FROM [dba_scrap_book].[dbo].[blocking_status]
    WHERE waitresource LIKE 'TAB: ' + CAST (DB_ID() AS NVARCHAR (2)) + ':%'
          AND ts > GETDATE() - 10
),
 RowsStats
AS (SELECT CAST (SUBSTRING(TableID, 1, CHARINDEX(':', TableID, 1) - 1) AS BIGINT) AS ID,
           ts,
           COUNT(*) AS RowsCount
    FROM AllTables
    GROUP BY CAST (SUBSTRING(TableID, 1, CHARINDEX(':', TableID, 1) - 1) AS BIGINT), ts)
SELECT RS.ts,
       RS.RowsCount,
       T.name
FROM RowsStats AS RS OUTER APPLY (SELECT name
                                  FROM sys.tables AS t
                                  WHERE t.[object_id] = RS.ID) AS T
ORDER BY RS.ts DESC;

