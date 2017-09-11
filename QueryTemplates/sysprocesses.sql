select hostname, db_name(dbid) as DatabaseName, loginame, count(*) as ConnectionCount
from sys.sysprocesses 
group by hostname, db_name(dbid), loginame
order by 2
