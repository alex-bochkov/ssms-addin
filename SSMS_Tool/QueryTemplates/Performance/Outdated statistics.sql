-------------------------------
SELECT OBJECT_NAME(id),name,STATS_DATE(id, indid),rowmodctr
FROM sys.sysindexes
WHERE STATS_DATE(id, indid)<=DATEADD(DAY,-1,GETDATE()) 
AND rowmodctr>0 
AND id IN (SELECT object_id FROM sys.tables)

--DBCC FREEPROCCACHE

SELECT 'UPDATE STATISTICS '+ OBJECT_NAME(id), SUM(rowmodctr)
FROM sys.sysindexes
WHERE STATS_DATE(id, indid)<=DATEADD(DAY,-1,GETDATE()) 
AND rowmodctr>0 
AND id IN (SELECT object_id FROM sys.tables)
GROUP BY 'UPDATE STATISTICS '+ OBJECT_NAME(id)
ORDER BY SUM(rowmodctr)
