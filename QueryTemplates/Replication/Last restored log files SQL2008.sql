SELECT SD.[secondary_database],
       LEFT(RIGHT(SD.last_restored_file, CHARINDEX('\', REVERSE(SD.last_restored_file)) - 1), 50) AS LastRestoredFile,
       MS.last_copied_date,
       SD.[last_restored_date],
       MS.last_restored_latency,
       DATEADD(minute, -MS.last_restored_latency + 7 * 60, SD.[last_restored_date]) AS DatabaseTimestamp,
       DATEDIFF(minute, DATEADD(minute, -MS.last_restored_latency + 7 * 60, SD.[last_restored_date]), GETDATE()) AS ActualLatency
FROM [msdb].[dbo].[log_shipping_secondary_databases] AS SD
     INNER JOIN
     [msdb].[dbo].[log_shipping_monitor_secondary] AS MS
     ON SD.secondary_id = MS.secondary_id
ORDER BY 1;
