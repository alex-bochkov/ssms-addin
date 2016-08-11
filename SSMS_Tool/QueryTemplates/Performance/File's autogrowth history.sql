DECLARE @filename NVARCHAR(1000);
DECLARE @bc INT;
DECLARE @ec INT;
DECLARE @bfn VARCHAR(1000);
DECLARE @efn VARCHAR(10);

-- Get the name of the current default trace
SELECT @filename = CAST(value AS NVARCHAR(1000))
FROM ::fn_trace_getinfo(DEFAULT)
WHERE traceid = 1 AND property = 2;

-- rip apart file name into pieces
SET @filename = REVERSE(@filename);
SET @bc = CHARINDEX('.',@filename);
SET @ec = CHARINDEX('_',@filename)+1;
SET @efn = REVERSE(SUBSTRING(@filename,1,@bc));
SET @bfn = REVERSE(SUBSTRING(@filename,@ec,LEN(@filename)));

-- set filename without rollover number
SET @filename = @bfn + @efn

-- process all trace files
SELECT
  ftg.StartTime
,te.name AS EventName
,DB_NAME(ftg.databaseid) AS DatabaseName  
,ftg.Filename
,(ftg.IntegerData*8)/1024.0 AS GrowthMB
,(ftg.duration/1000)AS DurMS
FROM ::fn_trace_gettable(@filename, DEFAULT) AS ftg
INNER JOIN sys.trace_events AS te ON ftg.EventClass = te.trace_event_id  
WHERE (ftg.EventClass = 92  -- Date File Auto-grow
    OR ftg.EventClass = 93) -- Log File Auto-grow
ORDER BY ftg.StartTime