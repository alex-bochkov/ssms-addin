DROP EVENT SESSION [Catch_All_Write_Queries] ON SERVER;
GO
CREATE EVENT SESSION [Catch_All_Write_Queries] ON SERVER 
ADD EVENT sqlserver.sp_statement_completed(
    ACTION(sqlserver.client_hostname,
          sqlserver.sql_text,
          sqlserver.username)
    WHERE (source_database_id = 10 --filter by database
            AND writes > 0
            AND [sqlserver].[client_hostname]<>N'<filtered host>'
            --AND ([sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%UPDATE%') --For +SQL2012
            --	OR [sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%INSERT%')
            --	OR [sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%DELETE%'))
		)),
ADD EVENT sqlserver.sql_statement_completed(
    ACTION(sqlserver.client_hostname,
          sqlserver.sql_text,
          sqlserver.username)
    WHERE (source_database_id = 10 --filter by database
            AND writes > 0
            AND [sqlserver].[client_hostname]<>N'<filtered host>'
            --AND ([sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%UPDATE%') --For +SQL2012
            --	OR [sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%INSERT%')
            --	OR [sqlserver].[like_i_sql_unicode_string]([sqlserver].[sql_text],N'%DELETE%'))
ADD TARGET  package0.asynchronous_file_target(
     SET filename='D:\ExtendedEvent\catch_all_writes_.xel', max_file_size = 1000, max_rollover_files = 9999)
WITH (STARTUP_STATE=ON)

GO

ALTER EVENT SESSION [Catch_All_Write_Queries] 
ON SERVER 
STATE = START;

GO

ALTER EVENT SESSION [Catch_All_Write_Queries] 
ON SERVER 
STATE = STOP;

GO
-- How to select data
;WITH RawData
AS (SELECT xevents.event_data.value('(event/data[@name="source_database_id"]/value)[1]', 'int') AS [database_id],
           xevents.event_data.value('(event/data[@name="object_id"]/value)[1]', 'int') AS [object_id],
           xevents.event_data.value('(event/data[@name="duration"]/value)[1]', 'int') AS [duration],
           xevents.event_data.value('(event/action[@name="client_hostname"]/value)[1]', 'varchar(max)') AS [client_hostname],
           xevents.event_data.value('(event/action[@name="username"]/value)[1]', 'varchar(max)') AS [username],
           xevents.event_data.value('(event/action[@name="sql_text"]/value)[1]', 'varchar(max)') AS [sql_text]
    FROM sys.fn_xe_file_target_read_file('D:\ExtendedEvent\catch_all_write*.xel', 
									'D:\ExtendedEvent\catch_all_write*.xem', NULL, NULL) 
	CROSS APPLY (SELECT CAST (event_data AS XML) AS event_data) AS xevents)
SELECT 
	DB_NAME([database_id]) as DatabaseName,
	[client_hostname],
	[username],
	[sql_text]
FROM RawData;
