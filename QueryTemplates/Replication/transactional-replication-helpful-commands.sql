-- Initialize subscriber from backup. The biggest benefit - no need to take snapshots on publisher database.
-- Subscriber is ready immidiately.
exec sp_addsubscription @publication = N'db_Publication',
        @sync_type = N'initialize with backup',
        @backupdevicetype='Disk',
        @backupdevicename='\\192.168.1.1\d$\db\db_20171214223004.trn',
        @subscriber = N'full server name', 
        @subscriber_type = 0,
        @destination_db = N'dmTraffic', 
        @subscription_type = N'push', 
        @update_mode = N'read only',
        @article = N'all';

-- Increase number of threads that writing data to subscriber
sp_changesubscription @publication =  'db_Publication'  
        ,  @article =  'all'  
        ,  @subscriber =  'server name'  
        ,  @destination_db =  'db'  
        ,  @property =  'subscriptionstreams'  
        ,  @value =  '8'

--***********************************************************************************
-- to skip errors about duplicated or non-existing records add this parameter into subscriber job
--  -SkipErrors 2601:2627:20598
--  From <https://www.mssqltips.com/sqlservertip/2469/handling-data-consistency-errors-in-sql-server-transactional-replication/> 


--***********************************************************************************
-- select replication commands
select * from dbo.MSarticles
where article_id in (
        select article_id from MSrepl_commands
        where xact_seqno = 0x000A4562000031F4027B00000001)

exec sp_browsereplcmds
        @xact_seqno_start = '0x000A4562000031F4027B00000001',
        @xact_seqno_end = '0x000A4562000031F4027B00000001'
