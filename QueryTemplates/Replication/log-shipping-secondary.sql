DECLARE @DatabaseName NVARCHAR(128) = ' < database name > ';
DECLARE @PrimaryServer NVARCHAR(128) = ' < server name > ';

DECLARE @SourceDir NVARCHAR(128) = '\\ < server name > \s$\backups$\' + @DatabaseName + '\LOG\';
DECLARE @TargetDir NVARCHAR(128) = 'X:\LS\' + @DatabaseName;
DECLARE @CopyJobName NVARCHAR(128) = N'LSCopy_' + @PrimaryServer + '_' + @DatabaseName;
DECLARE @RestoreJobName NVARCHAR(128) = N'LSRestore_' + @PrimaryServer + '_' + @DatabaseName;





DECLARE @LS_Secondary__CopyJobId	AS uniqueidentifier
DECLARE @LS_Secondary__RestoreJobId	AS uniqueidentifier
DECLARE @LS_Secondary__SecondaryId	AS uniqueidentifier
DECLARE @LS_Add_RetCode	As int

EXEC @LS_Add_RetCode = master.dbo.sp_add_log_shipping_secondary_primary
	@primary_server = @PrimaryServer
	,@primary_database = @DatabaseName
	,@backup_source_directory = @SourceDir
	,@backup_destination_directory = @TargetDir
	,@copy_job_name = @CopyJobName
	,@restore_job_name = @RestoreJobName
	,@file_retention_period = 4320
	,@overwrite = 1
	,@copy_job_id = @LS_Secondary__CopyJobId OUTPUT
	,@restore_job_id = @LS_Secondary__RestoreJobId OUTPUT
	,@secondary_id = @LS_Secondary__SecondaryId OUTPUT

IF (@@ERROR = 0 AND @LS_Add_RetCode = 0)
BEGIN

	DECLARE @LS_SecondaryCopyJobScheduleUID	As uniqueidentifier
	DECLARE @LS_SecondaryCopyJobScheduleID	AS int

	EXEC msdb.dbo.sp_add_schedule
	@schedule_name =N'DefaultCopyJobSchedule'
	,@enabled = 1
	,@freq_type = 4
	,@freq_interval = 1
	,@freq_subday_type = 4
	,@freq_subday_interval = 15
	,@freq_recurrence_factor = 0
	,@active_start_date = 20110127
	,@active_end_date = 99991231
	,@active_start_time = 500
	,@active_end_time = 235959
	,@schedule_uid = @LS_SecondaryCopyJobScheduleUID OUTPUT
	,@schedule_id = @LS_SecondaryCopyJobScheduleID OUTPUT

	EXEC msdb.dbo.sp_attach_schedule
	@job_id = @LS_Secondary__CopyJobId
	,@schedule_id = @LS_SecondaryCopyJobScheduleID

	DECLARE @LS_SecondaryRestoreJobScheduleUID	As uniqueidentifier
	DECLARE @LS_SecondaryRestoreJobScheduleID	AS int

	EXEC msdb.dbo.sp_add_schedule
	@schedule_name =N'DefaultRestoreJobSchedule'
	,@enabled = 1
	,@freq_type = 4
	,@freq_interval = 1
	,@freq_subday_type = 4
	,@freq_subday_interval = 15
	,@freq_recurrence_factor = 0
	,@active_start_date = 20110127
	,@active_end_date = 99991231
	,@active_start_time = 1000
	,@active_end_time = 235959
	,@schedule_uid = @LS_SecondaryRestoreJobScheduleUID OUTPUT
	,@schedule_id = @LS_SecondaryRestoreJobScheduleID OUTPUT

	EXEC msdb.dbo.sp_attach_schedule @job_id = @LS_Secondary__RestoreJobId, @schedule_id = @LS_SecondaryRestoreJobScheduleID

END

DECLARE @LS_Add_RetCode2	As int

IF (@@ERROR = 0 AND @LS_Add_RetCode = 0)
BEGIN
	EXEC @LS_Add_RetCode2 = master.dbo.sp_add_log_shipping_secondary_database
	@secondary_database = @DatabaseName
	,@primary_server = @PrimaryServer
	,@primary_database = @DatabaseName
	,@restore_delay = 0
	,@restore_mode = 1
	,@disconnect_users	= 1
	,@restore_threshold = 1440
	,@threshold_alert_enabled = 1
	,@history_retention_period	= 5760
	,@overwrite = 1
END

IF (@@error = 0 AND @LS_Add_RetCode = 0)
BEGIN
	EXEC msdb.dbo.sp_update_job @job_id = @LS_Secondary__CopyJobId, @enabled = 1
	EXEC msdb.dbo.sp_update_job @job_id = @LS_Secondary__RestoreJobId, @enabled = 1
END
