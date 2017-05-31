SELECT SD.[secondary_database],
       SD.[last_restored_file],
       SD.[last_restored_date],
       MS.last_restored_latency,
       MS.last_copied_date
FROM [msdb].[dbo].[log_shipping_secondary_databases] AS SD
     INNER JOIN
     [msdb].[dbo].[log_shipping_monitor_secondary] AS MS
     ON SD.secondary_id = MS.secondary_id
ORDER BY 1;
