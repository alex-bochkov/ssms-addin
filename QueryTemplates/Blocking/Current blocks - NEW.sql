SELECT  db_name(DTL.[resource_database_id]) AS [Database],
        DTL.[resource_type] AS [Resource Type] ,
        CASE WHEN DTL.[resource_type] IN ( 'DATABASE', 'FILE', 'METADATA' )
             THEN DTL.[resource_type]
             WHEN DTL.[resource_type] = 'OBJECT'
             THEN OBJECT_NAME(DTL.resource_associated_entity_id)
             WHEN DTL.[resource_type] IN ( 'KEY', 'PAGE', 'RID' )
             THEN ( SELECT  OBJECT_NAME([object_id])
                    FROM    sys.partitions
                    WHERE   sys.partitions.[hobt_id] =
                                 DTL.[resource_associated_entity_id]
                  )
             ELSE 'Unidentified'
        END AS [Parent Object] ,
        DTL.[request_mode] AS [Lock Type] ,
        DTL.[request_status] AS [Request Status] ,
        DOWT.[wait_duration_ms] AS [Wait Duration (ms)] ,
        DOWT.[wait_type] AS [Wait Type] ,
        DOWT.[session_id] AS [Blocked Session ID] ,
        DES_Blocked.[login_name] AS [Blocked Login] ,
        SUBSTRING(DEST_Blocked.text, (DER.statement_start_offset / 2) + 1,
                  ( CASE WHEN DER.statement_end_offset = -1 
                         THEN DATALENGTH(DEST_Blocked.text) 
                         ELSE DER.statement_end_offset 
                    END - DER.statement_start_offset ) / 2) 
                                              AS [Blocked Command] , 
        DOWT.[blocking_session_id] AS [Blocking Session ID] ,
        DES_Blocking.[login_name] AS [Blocking Login] ,
        DEST_Blocking.[text] AS [Blocking Command] ,
        DOWT.resource_description AS [Blocking Resource Detail]
FROM    sys.dm_tran_locks DTL
        INNER JOIN sys.dm_os_waiting_tasks DOWT
                    ON DTL.lock_owner_address = DOWT.resource_address
        INNER JOIN sys.[dm_exec_requests] DER
                    ON DOWT.[session_id] = DER.[session_id]
        INNER JOIN sys.dm_exec_sessions DES_Blocked
                    ON DOWT.[session_id] = DES_Blocked.[session_id]
        INNER JOIN sys.dm_exec_sessions DES_Blocking
                    ON DOWT.[blocking_session_id] = DES_Blocking.[session_id]
        INNER JOIN sys.dm_exec_connections DEC
                    ON DOWT.[blocking_session_id] = DEC.[most_recent_session_id]
        CROSS APPLY sys.dm_exec_sql_text(DEC.[most_recent_sql_handle])
                                                         AS DEST_Blocking
        CROSS APPLY sys.dm_exec_sql_text(DER.sql_handle) AS DEST_Blocked