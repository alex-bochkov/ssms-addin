CREATE EVENT SESSION [track-ddl-changes] ON SERVER 
ADD EVENT sqlserver.object_altered
    (
    ACTION (sqlserver.client_app_name,
            sqlserver.client_hostname,
            sqlserver.sql_text,
            sqlserver.username)
    WHERE (--[database_id] <> (4) AND --exclude all objects from TempDB
			[object_id] > 0 --exclude temp tables
			AND (object_type = 8277 --table
			     OR [object_type]=(21076) --trigger
			     OR [object_type]=(19543) --winlogin
			     OR [object_type]=(21847) --winuser
			     OR [object_type]=(18263) -- wingroup
			     OR [object_type]=(8278) --view
			     OR [object_type]=(21333) -- user
			     OR [object_type]=(20051) --synonim
			     OR [object_type]=(21843) --sql user
			     OR [object_type]=(17235) --schema
			     OR [object_type]=(8272) --proc
			     OR [object_type]=(22604) -- login
			     OR [object_type]=(22601) --index
			     OR [object_type]=(20038) --function
			     OR [object_type]=(16964) --database
				 )
			)
    ), 
ADD EVENT sqlserver.object_created
    (
    ACTION (sqlserver.client_app_name,
            sqlserver.client_hostname,
            sqlserver.sql_text,
            sqlserver.username)
    WHERE (--[database_id] <> (4) AND --exclude all objects from TempDB
			[object_id] > 0 --exclude temp tables
			AND (object_type = 8277 --table
			     OR [object_type]=(21076) --trigger
			     OR [object_type]=(19543) --winlogin
			     OR [object_type]=(21847) --winuser
			     OR [object_type]=(18263) -- wingroup
			     OR [object_type]=(8278) --view
			     OR [object_type]=(21333) -- user
			     OR [object_type]=(20051) --synonim
			     OR [object_type]=(21843) --sql user
			     OR [object_type]=(17235) --schema
			     OR [object_type]=(8272) --proc
			     OR [object_type]=(22604) -- login
			     OR [object_type]=(22601) --index
			     OR [object_type]=(20038) --function
			     OR [object_type]=(16964) --database
				 )
			)
    ), 
ADD EVENT sqlserver.object_deleted
    (
    ACTION (sqlserver.client_app_name,
            sqlserver.client_hostname,
            sqlserver.sql_text,
            sqlserver.username)
    WHERE (--[database_id] <> (4) AND --exclude all objects from TempDB
			[object_id] > 0 --exclude temp tables
			AND (object_type = 8277 --table
			     OR [object_type]=(21076) --trigger
			     OR [object_type]=(19543) --winlogin
			     OR [object_type]=(21847) --winuser
			     OR [object_type]=(18263) -- wingroup
			     OR [object_type]=(8278) --view
			     OR [object_type]=(21333) -- user
			     OR [object_type]=(20051) --synonim
			     OR [object_type]=(21843) --sql user
			     OR [object_type]=(17235) --schema
			     OR [object_type]=(8272) --proc
			     OR [object_type]=(22604) -- login
			     OR [object_type]=(22601) --index
			     OR [object_type]=(20038) --function
			     OR [object_type]=(16964) --database
				 )
			)
    ) 
ADD TARGET package0.asynchronous_file_target
    (SET filename = 'ddl-changes.xel', max_file_size = 100, max_rollover_files = 9999)
WITH  (STARTUP_STATE = ON);

GO
ALTER EVENT SESSION [track-ddl-changes] ON SERVER STATE = START;
GO
ALTER EVENT SESSION [track-ddl-changes] ON SERVER STATE = STOP;
GO
DROP EVENT SESSION [track-ddl-changes] ON SERVER;
GO




;WITH RawData
AS (SELECT --xevents.event_data,
		   xevents.event_data.value('(event/@timestamp)[1]', 'datetime') AS [EventDateTime],
		   xevents.event_data.value('(event/data[@name="database_id"]/value)[1]', 'int') AS [database_id],
           xevents.event_data.value('(event/data[@name="object_id"]/value)[1]', 'int') AS [object_id],
           xevents.event_data.value('(event/data[@name="object_type"]/value)[1]', 'int') AS [object_type],
           xevents.event_data.value('(event/data[@name="ddl_phase"]/text)[1]', 'varchar(10)') AS [ddl_phase],
           xevents.event_data.value('(event/data[@name="object_name"]/value)[1]', 'varchar(max)') AS [object_name],
           xevents.event_data.value('(event/action[@name="client_app_name"]/value)[1]', 'varchar(max)') AS [client_app_name],
           xevents.event_data.value('(event/action[@name="client_hostname"]/value)[1]', 'varchar(max)') AS [client_hostname],
           xevents.event_data.value('(event/action[@name="username"]/value)[1]', 'varchar(max)') AS [username],
           xevents.event_data.value('(event/action[@name="sql_text"]/value)[1]', 'varchar(max)') AS [sql_text]
    FROM sys.fn_xe_file_target_read_file('ddl-changes*.xel', DEFAULT, DEFAULT, DEFAULT) 
	CROSS APPLY (SELECT CAST (event_data AS XML) AS event_data) AS xevents)
SELECT 
	DB_NAME([database_id]) as DatabaseName,
	*
FROM RawData
order by [EventDateTime] desc;
