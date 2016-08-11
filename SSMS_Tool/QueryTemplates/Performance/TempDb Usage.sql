;WITH s AS
(
    SELECT 
        s.session_id,
        [pages] = SUM(s.user_objects_alloc_page_count 
          + s.internal_objects_alloc_page_count) 
    FROM sys.dm_db_session_space_usage AS s
    GROUP BY s.session_id
    HAVING SUM(s.user_objects_alloc_page_count 
      + s.internal_objects_alloc_page_count) > 0
)
SELECT s.session_id, s.[pages], t.[text], 
  [statement] = COALESCE(NULLIF(
    SUBSTRING(
        t.[text], 
        r.statement_start_offset / 2, 
        CASE WHEN r.statement_end_offset < r.statement_start_offset 
        THEN 0 
        ELSE( r.statement_end_offset - r.statement_start_offset ) / 2 END
      ), ''
    ), t.[text])
FROM s
LEFT OUTER JOIN 
sys.dm_exec_requests AS r
ON s.session_id = r.session_id
OUTER APPLY sys.dm_exec_sql_text(r.plan_handle) AS t
ORDER BY s.[pages] DESC;


   SELECT PAGES.session_id, PAGES.pages, r.num_reads, r.num_writes, sp.login_time, sp.last_batch, sp.cpu, sp.physical_io, sp.hostname, sp.program_name, t.text
    FROM sys.dm_exec_connections AS r
    LEFT OUTER JOIN master.sys.sysprocesses AS sp on sp.spid=r.session_id
    OUTER APPLY sys.dm_exec_sql_text(r.most_recent_sql_handle) AS t
    LEFT OUTER JOIN (
        SELECT s.session_id, [pages] = SUM(s.user_objects_alloc_page_count + s.internal_objects_alloc_page_count) 
        FROM sys.dm_db_session_space_usage AS s
        GROUP BY s.session_id
        HAVING SUM(s.user_objects_alloc_page_count + s.internal_objects_alloc_page_count) > 0
    ) PAGES ON PAGES.session_id = r.session_id
    WHERE PAGES.session_id IS NOT NULL AND PAGES.pages > 50
    ORDER BY PAGES.pages DESC;