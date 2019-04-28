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
    @article = N'all'
