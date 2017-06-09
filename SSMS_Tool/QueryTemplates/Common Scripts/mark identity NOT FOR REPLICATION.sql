EXEC sp_msforeachtable @command1 = '
declare @int int
set @int =object_id("?")
EXEC sys.sp_identitycolumnforreplication @int, 1'
