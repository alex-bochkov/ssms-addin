USE [master]
GO

DROP ENDPOINT [Mirroring]
GO

CREATE ENDPOINT [Mirroring] 
	STATE=STARTED
	AS TCP (LISTENER_PORT = 5022, LISTENER_IP = ALL)
	FOR DATA_MIRRORING (ROLE = PARTNER, AUTHENTICATION = CERTIFICATE DatabaseMirroring
, ENCRYPTION = DISABLED)
GO

CREATE MASTER KEY ENCRYPTION BY PASSWORD = '<password>';  
GO  

USE master;  
CREATE CERTIFICATE DatabaseMirroring WITH SUBJECT = 'Certificate for database mirroring';  
GO  

BACKUP CERTIFICATE DatabaseMirroring TO FILE = 'C:\DatabaseMirroring.cer'
  WITH PRIVATE KEY(ENCRYPTION BY PASSWORD='<password>', FILE='C:\DatabaseMirroring.pvk');

CREATE CERTIFICATE DatabaseMirroring FROM FILE ='C:\DatabaseMirroring.cer'
  WITH PRIVATE KEY(FILE='C:\DatabaseMirroring.pvk', DECRYPTION BY PASSWORD='<password>');
