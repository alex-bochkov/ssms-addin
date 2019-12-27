-- Enter the file name you want to shrink and target file size
-- File will be shrinked in a loop by one gigabyte at a time
DECLARE @FileName VARCHAR(100)  = 'file_name';
DECLARE @DesiredSize INT        = 30000;

DECLARE @CurrentSize INT;
SELECT @CurrentSize = ROUND(size * 8 / 1024, -3) FROM sys.database_files WHERE name = @FileName

WHILE (@CurrentSize > @DesiredSize)
BEGIN

  SET LOCK_TIMEOUT 5000; -- this supposed to help against heavy blocking (but I don't think it works)

  DBCC SHRINKFILE (@FileName , @CurrentSize);

  PRINT @FileName + ' new size is: ' + FORMAT(@CurrentSize, 'N0');

  SET @CurrentSize = @CurrentSize - 1000;  

END
