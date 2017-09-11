DROP EVENT SESSION [ErrorCapture] ON SERVER 
GO
CREATE EVENT SESSION [ErrorCapture] ON SERVER 
ADD EVENT sqlserver.error_reported
    (
    ACTION (sqlserver.client_hostname,
            sqlserver.database_id,
            sqlserver.sql_text,
            sqlserver.username)
    WHERE ([severity] >= (11))
    ) 
ADD TARGET package0.asynchronous_file_target
    (
    SET filename = 'D:\ExtendedEvents\ErrorCapture.xel'
    )
WITH  (
        MAX_MEMORY = 4096 KB,
        EVENT_RETENTION_MODE = ALLOW_SINGLE_EVENT_LOSS,
        MAX_DISPATCH_LATENCY = 10 SECONDS,
        MAX_EVENT_SIZE = 0 KB,
        MEMORY_PARTITION_MODE = NONE,
        TRACK_CAUSALITY = OFF,
        STARTUP_STATE = ON
      );
      
ALTER EVENT SESSION [ErrorCapture] 
ON SERVER 
STATE = START;

ALTER EVENT SESSION [ErrorCapture] 
ON SERVER 
STATE = STOP;

;WITH events_cte
AS (SELECT DATEADD(mi, DATEDIFF(mi, GETUTCDATE(), CURRENT_TIMESTAMP), xevents.event_data.value('(event/@timestamp)[1]', 'datetime2')) AS [err_timestamp],
           DB_NAME(xevents.event_data.value('(event/action[@name="database_id"]/value)[1]', 'bigint')) AS [database],
           xevents.event_data.value('(event/data[@name="error"]/value)[1]', 'bigint') AS [error],
           xevents.event_data.value('(event/data[@name="severity"]/value)[1]', 'bigint') AS [err_severity],
           xevents.event_data.value('(event/data[@name="error_number"]/value)[1]', 'bigint') AS [err_number],
           xevents.event_data.value('(event/data[@name="message"]/value)[1]', 'nvarchar(512)') AS [err_message],
           xevents.event_data.value('(event/action[@name="client_hostname"]/value)[1]', 'nvarchar(max)') AS [client_hostname],
           xevents.event_data.value('(event/action[@name="sql_text"]/value)[1]', 'nvarchar(max)') AS [sql_text],
           xevents.event_data
    FROM sys.fn_xe_file_target_read_file('D:\ExtendedEvents\ErrorCapture*.xel', 
									'D:\ExtendedEvents\ErrorCapture*.xem', NULL, NULL) 
		CROSS APPLY (SELECT CAST (event_data AS XML) AS event_data) AS xevents)
SELECT 
  *
FROM events_cte
ORDER BY err_timestamp;
