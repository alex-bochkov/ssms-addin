/*
  Queue in DBA database to catch ErrorLog records in realtime
*/
ALTER DATABASE [DBA] SET ENABLE_BROKER;
GO
USE [DBA]
GO

CREATE SCHEMA Monitor;
GO

CREATE TABLE [Monitor].[ErrorLog](
	[PK_Id] [int] IDENTITY(1,1) NOT NULL,
	[PostTime] [datetime] NULL,
	[SPID] [int] NULL,
	[TextData] [nvarchar](max) NULL,
	[DatabaseID] [int] NULL,
	[TransactionID] [bigint] NULL,
	[NTUserName] [nvarchar](128) NULL,
	[NTDomainName] [nvarchar](128) NULL,
	[HostName] [nvarchar](128) NULL,
	[ClientProcessID] [int] NULL,
	[ApplicationName] [nvarchar](128) NULL,
	[LoginName] [nvarchar](128) NULL,
	[StartTime] [datetime] NULL,
	[Severity] [int] NULL,
	[ServerName] [nvarchar](128) NULL,
	[State] [int] NULL,
	[Error] [int] NULL,
	[DatabaseName] [nvarchar](128) NULL,
	[LoginSid] [image] NULL,
	[RequestID] [int] NULL,
	[EventSequence] [int] NULL,
	[IsSystem] [int] NULL,
	[SessionLoginName] [nvarchar](128) NULL,
 CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED ([PK_Id])
)
GO

CREATE QUEUE EventNotificationQueue;

-- Create a service broker service receive the events
CREATE SERVICE EventNotificationService
 ON QUEUE EventNotificationQueue ([http://schemas.microsoft.com/SQL/Notifications/PostEventNotification])
GO

-- Create the event notification for ERRORLOG trace events on the service
CREATE EVENT NOTIFICATION CaptureErrorLogEvents
 ON SERVER
 WITH FAN_IN
 FOR ERRORLOG
 TO SERVICE 'EventNotificationService', 'current database';
GO

CREATE PROCEDURE Monitor.[ProcessEventNotifications]
  WITH EXECUTE AS OWNER
AS 
SET NOCOUNT ON
DECLARE @message_body xml 
DECLARE @email_message nvarchar(MAX)
WHILE (1 = 1)
BEGIN
	BEGIN TRANSACTION
	-- Receive the next available message FROM the queue
	WAITFOR (
		RECEIVE TOP(1) -- just handle one message at a time
			@message_body=message_body
			FROM dbo.EventNotificationQueue
	), TIMEOUT 1000  -- if the queue is empty for one second, give UPDATE and go away
	-- If we didn't get anything, bail out
	IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION
			BREAK
		END 

		INSERT INTO [Monitor].[ErrorLog]
           ([PostTime]
           ,[SPID]
           ,[TextData]
           ,[DatabaseID]
           ,[TransactionID]
           ,[NTUserName]
           ,[NTDomainName]
           ,[HostName]
           ,[ClientProcessID]
           ,[ApplicationName]
           ,[LoginName]
           ,[StartTime]
           ,[Severity]
           ,[ServerName]
           ,[State]
           ,[Error]
           ,[DatabaseName]
           ,[LoginSid]
           ,[RequestID]
           ,[EventSequence]
           ,[IsSystem]
           ,[SessionLoginName])
		SELECT 
			 @message_body.value('(/EVENT_INSTANCE/PostTime)[1]', 'datetime') AS PostTime,
			 @message_body.value('(/EVENT_INSTANCE/SPID)[1]', 'int' ) AS SPID,
			 @message_body.value('(/EVENT_INSTANCE/TextData)[1]', 'varchar(max)' ) AS TextData,
			 @message_body.value('(/EVENT_INSTANCE/DatabaseID)[1]', 'int' ) AS DatabaseID,
			 @message_body.value('(/EVENT_INSTANCE/TransactionID)[1]', 'bigint' ) AS TransactionID,
			 @message_body.value('(/EVENT_INSTANCE/NTUserName)[1]', 'nvarchar(128)' ) AS NTUserName,
			 @message_body.value('(/EVENT_INSTANCE/NTDomainName)[1]', 'nvarchar(128)' ) AS NTDomainName,
			 @message_body.value('(/EVENT_INSTANCE/HostName)[1]', 'nvarchar(128)' ) AS HostName,
			 @message_body.value('(/EVENT_INSTANCE/ClientProcessID)[1]', 'int' ) AS ClientProcessID,
			 @message_body.value('(/EVENT_INSTANCE/ApplicationName)[1]', 'nvarchar(128)' ) AS ApplicationName,
			 @message_body.value('(/EVENT_INSTANCE/LoginName)[1]', 'nvarchar(128)' ) AS LoginName,
			 @message_body.value('(/EVENT_INSTANCE/StartTime)[1]', 'datetime' ) AS StartTime,
			 @message_body.value('(/EVENT_INSTANCE/Severity)[1]', 'int' ) AS Severity,
			 @message_body.value('(/EVENT_INSTANCE/ServerName)[1]', 'nvarchar(128)' ) AS ServerName,
			 @message_body.value('(/EVENT_INSTANCE/State)[1]', 'int' ) AS State,
			 @message_body.value('(/EVENT_INSTANCE/Error)[1]', 'int' ) AS Error,
			 @message_body.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'nvarchar(128)' ) AS DatabaseName,
			 @message_body.value('(/EVENT_INSTANCE/LoginSid)[1]', 'binary' ) AS LoginSid,
			 @message_body.value('(/EVENT_INSTANCE/RequestID)[1]', 'int' ) AS RequestID,
			 @message_body.value('(/EVENT_INSTANCE/EventSequence)[1]', 'int' ) AS EventSequence,
			 @message_body.value('(/EVENT_INSTANCE/IsSystem)[1]', 'int' ) AS IsSystem,
			 @message_body.value('(/EVENT_INSTANCE/SessionLoginName)[1]', 'nvarchar(128)' ) AS SessionLoginName

	
--  Commit the transaction.  At any point before this, we could roll 
--  back - the received message would be back on the queue AND the response
--  wouldn't be sent.
	COMMIT TRANSACTION
END
GO

--  Alter the Queue to add Activation Procedure
ALTER QUEUE EventNotificationQueue
WITH 
 ACTIVATION -- Setup Activation Procedure
	(STATUS=ON,
	 PROCEDURE_NAME = [Monitor].[ProcessEventNotifications],  -- Procedure to execute
	 MAX_QUEUE_READERS = 1, -- maximum concurrent executions of the procedure
	 EXECUTE AS OWNER) -- account to execute procedure under
GO

/* Test the Event Notification by raising an Error
RAISERROR (N'Test ERRORLOG Event!!', 26, 1) WITH LOG;
GO

SELECT * FROM Monitor.ErrorLog;
*/
