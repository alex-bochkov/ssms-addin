with Blocking as
(
SELECT [Spid] = session_Id
	, percent_complete
	, blocked ,waittime
	, [Database] = DB_NAME(sp.dbid)
	, [User] = nt_username
	, [Status] = er.status
	, [Wait] = wait_type
	, [Individual Query] = SUBSTRING (qt.text, er.statement_start_offset/2,
	(CASE WHEN er.statement_end_offset = -1 THEN LEN(CONVERT(NVARCHAR(MAX), qt.text)) * 2 ELSE er.statement_end_offset END - er.statement_start_offset)/2)
	,[Parent Query] = qt.text
	, Program = program_name
	, Hostname
	, cpu_time
	, reads
	, start_time, sp.login_time, sp.last_batch, sp.cmd
FROM sys.dm_exec_requests er
INNER JOIN sys.sysprocesses sp ON er.session_id = sp.spid
CROSS APPLY sys.dm_exec_sql_text(er.sql_handle)as qt
WHERE session_Id > 50 -- Ignore system spids.
AND session_Id NOT IN (@@SPID) -- Ignore this current statement.
)

select * 
from blocking 
where blocked <> 0
union all
select * from blocking where spid in (select blocked from blocking where blocked <> 0)