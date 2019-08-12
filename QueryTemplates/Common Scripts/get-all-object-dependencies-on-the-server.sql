DROP TABLE IF EXISTS master..dependencies;

select db_NAME() as databaseName, * 
INTO master..dependencies 
FROM sys.sql_expression_dependencies dep
INNER JOIN sys.objects obj
  on dep.referencing_id = obj.object_id
WHERE 1 = 0;

EXEC sp_msforeachdb N' use [?];
INSERT INTO  master..dependencies
select db_NAME() as databaseName, * from sys.sql_expression_dependencies dep
INNER JOIN sys.objects obj
on dep.referencing_id = obj.object_id
'
SELECT * FROM master..dependencies where referenced_database_name IN ('db1', 'db2')
