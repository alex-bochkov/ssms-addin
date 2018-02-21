USE msdb;

IF OBJECT_ID('dbo.ServerErrors') IS NULL
BEGIN
	CREATE TABLE ServerErrors (
		id INT IDENTITY PRIMARY KEY CLUSTERED,
		ErrorDateTime datetime, 
		ErrorNumber int, 
		Severity int, 
		DatabaseName nvarchar(128), 
		ErrorMessage nvarchar(max)
	)

END;

GO

CREATE OR ALTER PROCEDURE spTrapAlert
	@error_num int,
	@severity int,
	@database_name nvarchar(128),
	@error_msg ntext
AS
	INSERT INTO ServerErrors (ErrorDateTime, ErrorNumber, Severity, DatabaseName, ErrorMessage)
	SELECT 
		GETDATE(),
		@error_num,
		@severity,
		@database_name,
		@error_msg;
GO
