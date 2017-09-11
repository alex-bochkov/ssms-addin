CREATE EVENT SESSION FailedLogins
ON SERVER
 ADD EVENT sqlserver.error_reported
 (
   ACTION 
   (
     sqlserver.client_app_name,
     sqlserver.client_hostname,
     sqlserver.nt_username
    )
    WHERE severity = 14
      AND (error_number = 18452 
		OR error_number = 18456 
		OR error_number = 18470)
      AND state > 1 -- removes redundant state 1 event
  )
  ADD TARGET package0.ring_buffer
  WITH (STARTUP_STATE=ON)
GO

ALTER EVENT SESSION FailedLogins ON SERVER
  STATE = START;
GO


DECLARE @target_data XML;
SELECT @target_data = CAST(target_data AS XML)
FROM sys.dm_xe_sessions AS s 
JOIN sys.dm_xe_session_targets AS t 
    ON t.event_session_address = s.address
WHERE s.name = N'FailedLogins';

;WITH RawData
AS (SELECT --n.query('.') AS event_data,
	n.value('(@timestamp)[1]', 'datetime') AS [EventDateTime],
	n.value('(data[@name="error_number"]/value)[1]', 'int') AS [ErrorNumber],
	n.value('(data[@name="severity"]/value)[1]', 'int') AS [Severity],
	n.value('(data[@name="message"]/value)[1]', 'varchar(max)') AS [ErrorMessage],
	n.value('(action[@name="nt_username"]/value)[1]', 'varchar(max)') AS [nt_username],
	n.value('(action[@name="client_hostname"]/value)[1]', 'varchar(max)') AS [client_hostname],
	n.value('(action[@name="client_app_name"]/value)[1]', 'varchar(max)') AS [client_app_name]
    FROM @target_data.nodes('RingBufferTarget/event[@name=''error_reported'']') AS q(n))
SELECT 
	*
FROM RawData;
