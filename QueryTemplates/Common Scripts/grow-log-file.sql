DECLARE @INT INT = 2, @Cmd VARCHAR(max);
--target size 100GB
WHILE @INT <= 100
BEGIN

	
	SET @Cmd = 'ALTER DATABASE [db] MODIFY FILE ( NAME = N''db_log'', SIZE = '+ CAST(@INT * 1024 AS VARCHAR(10))+'MB )';
	PRINT @Cmd;
	EXEC(@CMD)
	SET @Int += 1;

END
