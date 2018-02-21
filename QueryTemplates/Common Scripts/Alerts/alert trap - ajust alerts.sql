USE [msdb]
GO
DECLARE @JobId nvarchar(50);
SELECT @JobId = job_id FROM sysjobs WHERE name = '[DBA] - Alert Trap'

EXEC msdb.dbo.sp_update_alert @name=N'Severity 016', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 017', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 018', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 019', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 020', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 021', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 022', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 023', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 024', @job_id=@JobId;
EXEC msdb.dbo.sp_update_alert @name=N'Severity 025', @job_id=@JobId;
