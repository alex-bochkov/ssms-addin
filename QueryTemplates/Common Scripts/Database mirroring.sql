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
CREATE CERTIFICATE DatabaseMirroring WITH SUBJECT = 'Certificate for database mirroring', EXPIRY_DATE = '2100-01-01';  
GO  

BACKUP CERTIFICATE DatabaseMirroring TO FILE = 'C:\Distr\DatabaseMirroring.cer'
  WITH PRIVATE KEY(ENCRYPTION BY PASSWORD='password', FILE='C:\Distr\DatabaseMirroring.pvk');

CREATE CERTIFICATE DatabaseMirroring FROM FILE ='C:\Distr\DatabaseMirroring.cer'
  WITH PRIVATE KEY(FILE='C:\Distr\DatabaseMirroring.pvk', DECRYPTION BY PASSWORD='password');
