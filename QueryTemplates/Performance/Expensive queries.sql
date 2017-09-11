SELECT TOP 100
       SUBSTRING(qt.TEXT, (qs.statement_start_offset / 2) + 1,
					((CASE qs.statement_end_offset
                        WHEN -1 THEN DATALENGTH(qt.TEXT)
                        ELSE qs.statement_end_offset
                   END - qs.statement_start_offset) / 2) + 1) AS QueryText,
       qp.query_plan,
       qs.execution_count,
       qs.total_logical_reads,
       qs.last_logical_reads,
       qs.total_logical_reads / qs.execution_count AS avg_logical_read,
       qs.total_logical_writes,
       qs.last_logical_writes,
       qs.total_worker_time,
       qs.last_worker_time,
       qs.total_elapsed_time / 1000000     total_elapsed_time_in_S,
       qs.last_elapsed_time / 1000000      last_elapsed_time_in_S,
       qs.last_execution_time,
       DB_NAME(qt.dbid)                 AS DBName
FROM   sys.dm_exec_query_stats qs
       CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) qp
ORDER BY qs.total_logical_reads DESC -- logical reads
--ORDER BY qs.total_logical_writes DESC -- logical writes
--ORDER BY qs.total_worker_time DESC -- CPU time
--ORDER BY qs.execution_count DESC