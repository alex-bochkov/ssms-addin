-------------------------------
SELECT OBJECT_NAME(id),name,STATS_DATE(id, indid),rowmodctr
FROM sys.sysindexes
WHERE STATS_DATE(id, indid)<=DATEADD(DAY,-1,GETDATE()) 
AND rowmodctr>0 
AND id IN (SELECT object_id FROM sys.tables)

--DBCC FREEPROCCACHE

;WITH AllChanges
AS (SELECT ST.name,
           Schema_name(st.schema_id) AS SchemaName,
           SUM(rowmodctr) AS ChangesCount
    FROM sys.sysindexes AS SI
         INNER JOIN
         sys.tables AS ST
         ON SI.id = ST.object_id
    WHERE STATS_DATE(id, indid) <= DATEADD(DAY, -1, GETDATE())
          AND rowmodctr > 0
    GROUP BY ST.name, Schema_name(st.schema_id))
SELECT 'UPDATE STATISTICS [' + SchemaName + '].[' + name + '];',
       ChangesCount
FROM AllChanges
ORDER BY 2 DESC;
