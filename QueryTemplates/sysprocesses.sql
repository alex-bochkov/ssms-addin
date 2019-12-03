SELECT hostname,
       db_name(dbid) AS DatabaseName,
       loginame,
       count(*) AS ConnectionCount
FROM sys.sysprocesses
GROUP BY hostname, db_name(dbid), loginame
ORDER BY 2;

SELECT ec.client_net_address,
       es.[program_name],
       es.[host_name],
       es.login_name,
       DB_NAME(es.database_id) as DatabaseName,
       COUNT(ec.session_id) AS [connection count],
       SUM(CASE WHEN encrypt_option = 'TRUE' THEN 1 ELSE 0 END) AS [connection count (enc)]
FROM sys.dm_exec_sessions AS es WITH (NOLOCK)
     INNER JOIN
     sys.dm_exec_connections AS ec WITH (NOLOCK)
     ON es.session_id = ec.session_id
GROUP BY ec.client_net_address, es.[program_name], es.[host_name], es.login_name, DB_NAME(es.database_id)
ORDER BY ec.client_net_address, es.[program_name]
OPTION (RECOMPILE);
