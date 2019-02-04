EXEC sp_msforeachtable @command1 = '
declare @int int
set @int =object_id("?")
EXEC sys.sp_identitycolumnforreplication @int, 1'



--smarter way to do it
SELECT 
  s.name AS schemaName,
  o.name AS tableName,
  ic.name AS columnName,
'declare @int int
set @int =object_id(''[' + s.name + '].[' + o.name + ']'')
EXEC sys.sp_identitycolumnforreplication @int, 1;
GO' AS command
FROM sys.identity_columns ic 
	INNER JOIN sys.tables o
		ON ic.object_id = o.object_id
	INNER JOIN sys.schemas s
		ON o.schema_id = s.schema_id
WHERE ic.is_not_for_replication = 0
	AND o.is_ms_shipped = 0
