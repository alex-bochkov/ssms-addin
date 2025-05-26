SELECT CONCAT(
    'ALTER DATABASE tempdb MODIFY FILE (',
      'NAME = ', QUOTENAME(f.[name]), ', ',
      'FILENAME = ', QUOTENAME(
                          CONCAT('Z:\MSSQL\DATA\', f.[name],
                            CASE WHEN f.type = 1 THEN '.ldf' ELSE '.mdf' END
                          )
                        , ''''), ', ',
      'SIZE = ', FORMAT(f.size * 8.0 / 1024, 'N0'), 'MB, ',
      'FILEGROWTH = ',
         CASE 
           WHEN f.is_percent_growth = 1 
             THEN CONCAT(f.growth, '%') 
           ELSE CONCAT(FORMAT(f.growth * 8.0 / 1024, 'N0'), 'MB') 
         END,
    ');'
) AS AlterCommand
FROM sys.master_files AS f
WHERE f.database_id = DB_ID(N'tempdb')
ORDER BY f.[file_id];
