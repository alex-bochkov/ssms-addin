sp_msforeachdb N'
USE [?];
PRINT ''USE [?]'';
With AllIdent as (SELECT     
	CAST(last_value as bigint) + 1 as NextValue, OBJECT_NAME(T.object_id) as TName, SCHEMA_NAME(T.schema_id) as SName
FROM sys.identity_columns IC
inner join sys.tables T on IC.object_id = T.object_id 
where last_value is not null
)
select 
''DBCC CHECKIDENT ('' + QUOTENAME(SName +''.''+ TName, '''''''') + '', RESEED, '' + CAST(NextValue  as VARCHAR(15))+ '');  ''
FROM  AllIdent
where DB_ID() > 4
'
