SET NOCOUNT ON;

DROP TABLE IF EXISTS #AllFiles;
DROP TABLE IF EXISTS #AllDatabases;

SELECT
	DB_NAME(database_id) as DatabaseName,
	name
INTO #AllFiles
FROM sys.master_files 
WHERE type = 0 and database_id > 4

SELECT 
	DatabaseName,
	ROW_NUMBER() OVER (ORDER BY DatabaseName) AS RN
INTO #AllDatabases
FROM #AllFiles
GROUP BY DatabaseName

DECLARE @MaxRow int = 0, @RN int = 1, @TheResults NVARCHAR(max) = N'', @DbName NVARCHAR(50);

Declare @vbCrLf CHAR(2) 
SET @vbCrLf = CHAR(13) + CHAR(10) 

SELECT @MaxRow = MAX(RN) FROM #AllDatabases

WHILE @RN <= @MaxRow
BEGIN

	SELECT @DbName = DatabaseName FROM #AllDatabases WHERE RN = @RN;

	SET @TheResults = @TheResults + @vbCrLf + 'CREATE DATABASE [' + @DbName + '.ss] ON'

	SELECT @TheResults = @TheResults + @vbCrLf + '	( NAME = ' + name + ', FILENAME = ''Z:\Snapshots\' + @DbName + '.' + name + '.ss''),' 
	FROM #AllFiles
	WHERE DatabaseName = @DbName;

	SET @TheResults = LEFT(@TheResults, LEN(@TheResults) - 1)
	
	SET @TheResults = @TheResults + @vbCrLf + '	AS SNAPSHOT OF ' + @DbName + ';' + @vbCrLf;

	SET @RN = @RN + 1;

END

;WITH E01(N) AS (SELECT 1 UNION ALL SELECT 1 UNION ALL   
                    SELECT 1 UNION ALL SELECT 1 UNION ALL   
                    SELECT 1 UNION ALL SELECT 1 UNION ALL   
                    SELECT 1 UNION ALL SELECT 1 UNION ALL   
                    SELECT 1 UNION ALL SELECT 1), --         10 or 10E01 rows   
         E02(N) AS (SELECT 1 FROM E01 a, E01 b),  --        100 or 10E02 rows   
         E04(N) AS (SELECT 1 FROM E02 a, E02 b),  --     10,000 or 10E04 rows   
         E08(N) AS (SELECT 1 FROM E04 a, E04 b),  --100,000,000 or 10E08 rows   
         --E16(N) AS (SELECT 1 FROM E08 a, E08 b),  --10E16 or more rows than you'll EVER need,   
         Tally(N) AS (SELECT ROW_NUMBER() OVER (ORDER BY N) FROM E08),   
       ItemSplit(   
                 ItemOrder,   
                 Item   
                ) as (   
                      SELECT N,   
                        SUBSTRING(@vbCrLf + @TheResults + @vbCrLf,N + DATALENGTH(@vbCrLf),CHARINDEX(@vbCrLf,@vbCrLf + @TheResults + @vbCrLf,N + DATALENGTH(@vbCrLf)) - N - DATALENGTH(@vbCrLf))   
                      FROM Tally   
                      WHERE N < DATALENGTH(@vbCrLf + @TheResults)   
                      --WHERE N < DATALENGTH(@vbCrLf + @INPUT) -- REMOVED added @vbCrLf   
                        AND SUBSTRING(@vbCrLf + @TheResults + @vbCrLf,N,DATALENGTH(@vbCrLf)) = @vbCrLf --Notice how we find the delimiter   
                     )   
  select   
    row_number() over (order by ItemOrder) as ItemID,   
    Item   
  from ItemSplit   
