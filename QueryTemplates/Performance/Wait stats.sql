/*
SQL Server Wait Information from sys.dm_os_wait_stats
Copyright (C) 2014, Brent Ozar Unlimited.
See http://BrentOzar.com/go/eula for the End User Licensing Agreement.
*/
/*********************************
Let's build a list of waits we can safely ignore.
*********************************/
IF OBJECT_ID('tempdb..#ignorable_waits') IS NOT NULL
DROP TABLE #ignorable_waits;
GO
create table #ignorable_waits (wait_type nvarchar(256) PRIMARY KEY);
GO
/* We aren't using row constructors to be SQL 2005 compatible */
set nocount on;
insert #ignorable_waits (wait_type) VALUES ('REQUEST_FOR_DEADLOCK_SEARCH');
insert #ignorable_waits (wait_type) VALUES ('SQLTRACE_INCREMENTAL_FLUSH_SLEEP');
insert #ignorable_waits (wait_type) VALUES ('SQLTRACE_BUFFER_FLUSH');
insert #ignorable_waits (wait_type) VALUES ('LAZYWRITER_SLEEP');
insert #ignorable_waits (wait_type) VALUES ('XE_TIMER_EVENT');
insert #ignorable_waits (wait_type) VALUES ('XE_DISPATCHER_WAIT');
insert #ignorable_waits (wait_type) VALUES ('FT_IFTS_SCHEDULER_IDLE_WAIT');
insert #ignorable_waits (wait_type) VALUES ('LOGMGR_QUEUE');
insert #ignorable_waits (wait_type) VALUES ('CHECKPOINT_QUEUE');
insert #ignorable_waits (wait_type) VALUES ('BROKER_TO_FLUSH');
insert #ignorable_waits (wait_type) VALUES ('BROKER_TASK_STOP');
insert #ignorable_waits (wait_type) VALUES ('BROKER_EVENTHANDLER');
insert #ignorable_waits (wait_type) VALUES ('SLEEP_TASK');
insert #ignorable_waits (wait_type) VALUES ('WAITFOR');
insert #ignorable_waits (wait_type) VALUES ('DBMIRROR_DBM_MUTEX')
insert #ignorable_waits (wait_type) VALUES ('DBMIRROR_EVENTS_QUEUE')
insert #ignorable_waits (wait_type) VALUES ('DBMIRRORING_CMD');
insert #ignorable_waits (wait_type) VALUES ('DISPATCHER_QUEUE_SEMAPHORE');
insert #ignorable_waits (wait_type) VALUES ('BROKER_RECEIVE_WAITFOR');
insert #ignorable_waits (wait_type) VALUES ('CLR_AUTO_EVENT');
insert #ignorable_waits (wait_type) VALUES ('DIRTY_PAGE_POLL');
insert #ignorable_waits (wait_type) VALUES ('HADR_FILESTREAM_IOMGR_IOCOMPLETION');
insert #ignorable_waits (wait_type) VALUES ('ONDEMAND_TASK_QUEUE');
insert #ignorable_waits (wait_type) VALUES ('FT_IFTSHC_MUTEX');
insert #ignorable_waits (wait_type) VALUES ('CLR_MANUAL_EVENT');
insert #ignorable_waits (wait_type) VALUES ('SP_SERVER_DIAGNOSTICS_SLEEP');
insert #ignorable_waits (wait_type) VALUES ('QDS_CLEANUP_STALE_QUERIES_TASK_MAIN_LOOP_SLEEP');
insert #ignorable_waits (wait_type) VALUES ('QDS_PERSIST_TASK_MAIN_LOOP_SLEEP');
GO
/* Want to manually exclude an event and recalculate?*/
/* insert #ignorable_waits (wait_type) VALUES (''); */
/*********************************
What are the highest overall waits since startup?
*********************************/
SELECT TOP 25
os.wait_type,
SUM(os.wait_time_ms) OVER (PARTITION BY os.wait_type) as sum_wait_time_ms,
CAST(
100.* SUM(os.wait_time_ms) OVER (PARTITION BY os.wait_type)
/ (1. * SUM(os.wait_time_ms) OVER () )
AS NUMERIC(12,1)) as pct_wait_time,
SUM(os.waiting_tasks_count) OVER (PARTITION BY os.wait_type) AS sum_waiting_tasks,
CASE WHEN SUM(os.waiting_tasks_count) OVER (PARTITION BY os.wait_type) > 0
THEN
CAST(
SUM(os.wait_time_ms) OVER (PARTITION BY os.wait_type)
/ (1. * SUM(os.waiting_tasks_count) OVER (PARTITION BY os.wait_type))
AS NUMERIC(12,1))
ELSE 0 END AS avg_wait_time_ms,
CURRENT_TIMESTAMP as sample_time
FROM sys.dm_os_wait_stats os
LEFT JOIN #ignorable_waits iw on
os.wait_type=iw.wait_type
WHERE
iw.wait_type is null
ORDER BY sum_wait_time_ms DESC;
GO
/*********************************
What are the higest waits *right now*?
*********************************/
/* Note: this is dependent on the #ignorable_waits table created earlier. */
if OBJECT_ID('tempdb..#wait_batches') is not null
drop table #wait_batches;
if OBJECT_ID('tempdb..#wait_data') is not null
drop table #wait_data;
GO
CREATE TABLE #wait_batches (
batch_id int identity primary key,
sample_time datetime not null
);
CREATE TABLE #wait_data
( batch_id INT NOT NULL ,
wait_type NVARCHAR(256) NOT NULL ,
wait_time_ms BIGINT NOT NULL ,
waiting_tasks BIGINT NOT NULL
);
CREATE CLUSTERED INDEX cx_wait_data on #wait_data(batch_id);
GO
/*
This temporary procedure records wait data to a temp table.
*/
if OBJECT_ID('tempdb..#get_wait_data') IS NOT NULL
DROP procedure #get_wait_data;
GO
CREATE PROCEDURE #get_wait_data
@intervals tinyint = 2,
@delay char(12)='00:00:30.000' /* 30 seconds*/
AS
DECLARE @batch_id int,
@current_interval tinyint,
@msg nvarchar(max);
SET NOCOUNT ON;
SET @current_interval=1;
WHILE @current_interval <= @intervals
BEGIN
INSERT #wait_batches(sample_time)
SELECT CURRENT_TIMESTAMP;
SELECT @batch_id=SCOPE_IDENTITY();
INSERT #wait_data (batch_id, wait_type, wait_time_ms, waiting_tasks)
SELECT
@batch_id,
os.wait_type,
SUM(os.wait_time_ms) OVER (PARTITION BY os.wait_type) as sum_wait_time_ms,
SUM(os.waiting_tasks_count) OVER (PARTITION BY os.wait_type) AS sum_waiting_tasks
FROM sys.dm_os_wait_stats os
LEFT JOIN #ignorable_waits iw on
os.wait_type=iw.wait_type
WHERE
iw.wait_type is null
ORDER BY sum_wait_time_ms DESC;
set @msg= CONVERT(char(23),CURRENT_TIMESTAMP,121)+ N': Completed sample '
+ cast(@current_interval as nvarchar(4))
+ N' of ' + cast(@intervals as nvarchar(4)) +
'.'
RAISERROR (@msg,0,1) WITH NOWAIT;
SET @current_interval=@current_interval+1;
if @current_interval <= @intervals
WAITFOR DELAY @delay;
END
GO
/*
Let's take two samples 30 seconds apart
*/
exec #get_wait_data @intervals=2, @delay='00:00:30.000';
GO
/*
What were we waiting on?
This query compares the most recent two samples.
*/
with max_batch as (
select top 1 batch_id, sample_time
from #wait_batches
order by batch_id desc
)
SELECT
b.sample_time as [Second Sample Time],
datediff(ss,wb1.sample_time, b.sample_time) as [Sample Duration in Seconds],
wd1.wait_type,
cast((wd2.wait_time_ms-wd1.wait_time_ms)/1000. as numeric(12,1)) as [Wait Time (Seconds)],
(wd2.waiting_tasks-wd1.waiting_tasks) AS [Number of Waits],
CASE WHEN (wd2.waiting_tasks-wd1.waiting_tasks) > 0
THEN
cast((wd2.wait_time_ms-wd1.wait_time_ms)/
(1.0*(wd2.waiting_tasks-wd1.waiting_tasks)) as numeric(12,1))
ELSE 0 END AS [Avg ms Per Wait]
FROM max_batch b
JOIN #wait_data wd2 on
wd2.batch_id=b.batch_id
JOIN #wait_data wd1 on
wd1.wait_type=wd2.wait_type AND
wd2.batch_id - 1 = wd1.batch_id
join #wait_batches wb1 on
wd1.batch_id=wb1.batch_id
WHERE (wd2.waiting_tasks-wd1.waiting_tasks) > 0
ORDER BY [Wait Time (Seconds)] DESC;
GO