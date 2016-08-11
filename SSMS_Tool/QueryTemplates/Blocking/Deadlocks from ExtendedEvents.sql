Use Master
SELECT
       xed.value('@timestamp', 'datetime') as Creation_Date,
       xed.query('.') AS Extend_Event
FROM
(
       SELECT CAST([target_data] AS XML) AS Target_Data
       FROM sys.dm_xe_session_targets AS xt
       INNER JOIN sys.dm_xe_sessions AS xs
       ON xs.address = xt.event_session_address
       WHERE xs.name = N'system_health'
       AND xt.target_name = N'ring_buffer'
) AS XML_Data
CROSS APPLY Target_Data.nodes('RingBufferTarget/event[@name="xml_deadlock_report"]') AS XEventData(xed)
ORDER BY Creation_Date DESC