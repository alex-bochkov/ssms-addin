/*
  Queue in DBA database to catch ErrorLog records in realtime
*/
ALTER DATABASE [DBA] SET ENABLE_BROKER;
GO
USE [DBA]
GO

CREATE SCHEMA Monitor;
GO

CREATE TABLE [Monitor].[AuditLoginFailed](
	[PK_Id] [int] IDENTITY(1,1) NOT NULL,
	[PostTime] [datetime] NULL,
	[SPID] [int] NULL,
	[TextData] [nvarchar](max) NULL,
	[DatabaseID] [int] NULL,
	[NTUserName] [nvarchar](128) NULL,
	[NTDomainName] [nvarchar](128) NULL,
	[HostName] [nvarchar](128) NULL,
	[ClientProcessID] [int] NULL,
	[ApplicationName] [nvarchar](128) NULL,
	[LoginName] [nvarchar](128) NULL,
	[StartTime] [datetime] NULL,
	[EventSubClass] [int] NULL,
	[Success] [int] NULL,
	[ServerName] [nvarchar](128) NULL,
	[State] [int] NULL,
	[Error] [int] NULL,
	[DatabaseName] [nvarchar](128) NULL,
	[RequestID] [int] NULL,
	[EventSequence] [int] NULL,
	[Type] [int] NULL,
	[IsSystem] [int] NULL,
	[SessionLoginName] [nvarchar](128) NULL,
 CONSTRAINT [PK_AuditLoginFailed] PRIMARY KEY CLUSTERED ([PK_Id])
)
GO

CREATE QUEUE FailedLoginNotificationQueue;
GO

CREATE SERVICE FailedLoginNotificationService
    ON QUEUE FailedLoginNotificationQueue 
   ([http://schemas.microsoft.com/SQL/Notifications/PostEventNotification]);
GO

CREATE EVENT NOTIFICATION FailedLoginNotification
    ON SERVER WITH FAN_IN
    FOR AUDIT_LOGIN_FAILED
    TO SERVICE 'FailedLoginNotificationService', 'current database';
GO

CREATE PROCEDURE Monitor.[ProcessEventNotificationsAuditLoginFiled]
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
			FROM dbo.FailedLoginNotificationQueue
	), TIMEOUT 1000  -- if the queue is empty for one second, give UPDATE and go away
	-- If we didn't get anything, bail out
	IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION
			BREAK
		END 

INSERT INTO [Monitor].[AuditLoginFailed]
           ([PostTime]
           ,[SPID]
           ,[TextData]
           ,[DatabaseID]
           ,[NTUserName]
           ,[NTDomainName]
           ,[HostName]
           ,[ClientProcessID]
           ,[ApplicationName]
           ,[LoginName]
           ,[StartTime]
           ,[EventSubClass]
           ,[Success]
           ,[ServerName]
           ,[State]
           ,[Error]
           ,[DatabaseName]
           ,[RequestID]
           ,[EventSequence]
           ,[Type]
           ,[IsSystem]
           ,[SessionLoginName])
		SELECT 
			 @message_body.value('(/EVENT_INSTANCE/PostTime)[1]', 'datetime') AS PostTime,
			 @message_body.value('(/EVENT_INSTANCE/SPID)[1]', 'int' ) AS SPID,
			 @message_body.value('(/EVENT_INSTANCE/TextData)[1]', 'varchar(max)' ) AS TextData,
			 @message_body.value('(/EVENT_INSTANCE/DatabaseID)[1]', 'int' ) AS DatabaseID,
			 @message_body.value('(/EVENT_INSTANCE/NTUserName)[1]', 'nvarchar(128)' ) AS NTUserName,
			 @message_body.value('(/EVENT_INSTANCE/NTDomainName)[1]', 'nvarchar(128)' ) AS NTDomainName,
			 @message_body.value('(/EVENT_INSTANCE/HostName)[1]', 'nvarchar(128)' ) AS HostName,
			 @message_body.value('(/EVENT_INSTANCE/ClientProcessID)[1]', 'int' ) AS ClientProcessID,
			 @message_body.value('(/EVENT_INSTANCE/ApplicationName)[1]', 'nvarchar(128)' ) AS ApplicationName,
			 @message_body.value('(/EVENT_INSTANCE/LoginName)[1]', 'nvarchar(128)' ) AS LoginName,
			 @message_body.value('(/EVENT_INSTANCE/StartTime)[1]', 'datetime' ) AS StartTime,
			 @message_body.value('(/EVENT_INSTANCE/EventSubClass)[1]', 'int' ) AS EventSubClass,
			 @message_body.value('(/EVENT_INSTANCE/Success)[1]', 'int' ) AS Success,
			 @message_body.value('(/EVENT_INSTANCE/ServerName)[1]', 'nvarchar(128)' ) AS ServerName,
			 @message_body.value('(/EVENT_INSTANCE/State)[1]', 'int' ) AS State,
			 @message_body.value('(/EVENT_INSTANCE/Error)[1]', 'int' ) AS Error,
			 @message_body.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'nvarchar(128)' ) AS DatabaseName,
			 @message_body.value('(/EVENT_INSTANCE/RequestID)[1]', 'int' ) AS RequestID,
			 @message_body.value('(/EVENT_INSTANCE/EventSequence)[1]', 'int' ) AS EventSequence,
			 @message_body.value('(/EVENT_INSTANCE/Type)[1]', 'int' ) AS Type,
			 @message_body.value('(/EVENT_INSTANCE/IsSystem)[1]', 'int' ) AS IsSystem,
			 @message_body.value('(/EVENT_INSTANCE/SessionLoginName)[1]', 'nvarchar(128)' ) AS SessionLoginName

	
--  Commit the transaction.  At any point before this, we could roll 
--  back - the received message would be back on the queue AND the response
--  wouldn't be sent.
	COMMIT TRANSACTION
END
GO

ALTER QUEUE FailedLoginNotificationQueue
WITH ACTIVATION
(
   STATUS = ON,
   PROCEDURE_NAME = [Monitor].[ProcessEventNotificationsAuditLoginFiled],
   MAX_QUEUE_READERS = 1,
   EXECUTE AS OWNER
);
GO

/* Test the Event Notification 
--try to login with incorrect username/password
--check new events:
SELECT * FROM [Monitor].[AuditLoginFailed];
*/
