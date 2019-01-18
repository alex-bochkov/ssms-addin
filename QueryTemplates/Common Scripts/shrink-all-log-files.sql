/* this is terrible idea, don't do it */

SELECT 'USE [' + DB_NAME(database_id) + ']
GO
DBCC SHRINKFILE (N''' + name + ''' , 0, TRUNCATEONLY)
GO
'
FROM sys.master_files
WHERE type_desc = 'LOG'
      AND database_id > 4;
