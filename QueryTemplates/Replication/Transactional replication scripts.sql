--PUBLISHER
use master
exec sp_replicationdboption @dbname = '<DB Name>',
@optname = 'publish',
@value = 'true'
Go

USE [<DB Name>];
exec sp_addpublication @publication = N'<DB Name>_Publication'
, @description = N'Transactional publication from Publisher ''<Server Name>''.'
, @sync_method = N'concurrent', @retention = 0, @allow_push = N'true'
, @allow_pull = N'true', @allow_anonymous = N'true', @enabled_for_internet = N'false'
, @snapshot_in_defaultfolder = N'true', @compress_snapshot = N'false', @ftp_port = 21
, @ftp_login = N'anonymous', @allow_subscription_copy = N'false', @add_to_active_directory = N'false'
, @repl_freq = N'continuous', @status = N'active', @independent_agent = N'true'
, @immediate_sync = N'true', @allow_sync_tran = N'false', @autogen_sync_procs = N'false'
, @allow_queued_tran = N'false', @allow_dts = N'false', @replicate_ddl = 1
-- from backup
, @allow_initialize_from_backup = N'true', @enabled_for_p2p = N'false', @enabled_for_het_sub = N'false'

GO


IF EXISTS (SELECT *
           FROM sys.procedures
           WHERE name = 'miscAddArticle')
    DROP PROCEDURE miscAddArticle;
GO
CREATE PROCEDURE miscAddArticle
@schema VARCHAR (128),
@article VARCHAR (128)
AS
DECLARE @sql AS VARCHAR (8000);
SET @sql = 'sp_addarticle @publication = N''<DB Name>_Publication''
, @article = N''' + @article + ''', @source_owner = N''' + @schema + '''
, @source_object = N''' + @article + ''', @type = N''logbased''
, @description = N'''', @creation_script = N'''', @pre_creation_cmd = N''drop''
, @schema_option = 0x00000000080350DF, @identityrangemanagementoption = N''manual''
, @destination_table = N''' + @article + ''', @destination_owner = N''' + @schema + '''
, @status = 24, @vertical_partition = N''false'', @ins_cmd = N''CALL [sp_MSins_' + @article + ']''
, @del_cmd = N''CALL [sp_MSdel_' + @article + ']'', @upd_cmd = N''SCALL [sp_MSupd_' + @article + ']'' ';
EXECUTE (@sql);

GO

select 'exec miscAddArticle ''' + SCHEMA_NAME(schema_id) +''', ''' + name + ''' ', * 
from sys.tables
where name in (select object_name(Parent_object_id)  from sys.objects
				where type = 'PK'   )
and is_ms_shipped = 0
--and not SCHEMA_NAME(schema_id) = 'Meta'
--and schema_id = 1
order by 2

/*
  EXECUTE OUTPUT!
*/

exec sp_addsubscription 
	@publication = N'<DB Name>_Publication', 
	@subscriber = N'<Subscriber Server Name>', 
	@destination_db = N'<Subscriber Database Name>', 
	@sync_type = N'replication support only', 
	@subscription_type = N'pull', 
	@update_mode = N'read only'
GO

/*
  EXECUTE ON Subscriber 
*/
exec sp_addpullsubscription
	@publisher = N'<Server Name>', 
	@publication = N'<DB Name>_Publication', 
	@publisher_db = N'<DB Name>', 
	@independent_agent = N'True', 
	@subscription_type = N'pull', 
	@description = N'', 
	@update_mode = N'read only', 
	@immediate_sync = 1

exec sp_addpullsubscription_agent 
	@publisher = N'<Server Name>', 
	@publication = N'<DB Name>_Publication', 
	@publisher_db = N'<DB Name>', 
	@distributor = N'<Distributor Server Name>', 
	@distributor_security_mode = 1, 
	@distributor_login = N'', 
	@distributor_password = null,
	@enabled_for_syncmgr = N'False', 
	@frequency_type = 64, 
	@frequency_interval = 0, 
	@frequency_relative_interval = 0, 
	@frequency_recurrence_factor = 0, 
	@frequency_subday = 0, 
	@frequency_subday_interval = 0, 
	@active_start_time_of_day = 0, 
	@active_end_time_of_day = 235959, 
	@active_start_date = 20160606, 
	@active_end_date = 99991231, 
	@alt_snapshot_folder = N'', 
	@working_directory = N'', 
	@use_ftp = N'False', 
	@job_login = null, 
	@job_password = null, 
	@publication_type = 0
GO
