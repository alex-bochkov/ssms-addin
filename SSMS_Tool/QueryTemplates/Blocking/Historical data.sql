SELECT TOP 100 * 
FROM dba_scrap_book.dbo.blocking_status AS bs
ORDER BY ts desc

SELECT TOP 100 * 
FROM dba_scrap_book.dbo.blocking_process AS bs
ORDER BY ts desc

;WITH AllProcesses AS (
SELECT CAST(EventInfo AS NVARCHAR(4000)) AS EventInfo, spid, MAX(ts) AS ts
  FROM [dba_scrap_book].[dbo].[blocking_process]
WHERE ts > DATEADD(hour, -3, GETDATE())
Group BY CAST(EventInfo AS NVARCHAR(4000)), spid
)
SELECT *
  FROM [dba_scrap_book].[dbo].[blocking_status] S
  LEFT JOIN AllProcesses P ON S.spid = P.spid
WHERE S.ts > DATEADD(hour, -3, GETDATE())
ORDER BY S.ts DESC