SELECT 
	local_database_name,
	CAST(CAST(transferred_size_bytes AS float) / CAST(database_size_bytes AS float) * 100 AS DECIMAL(10, 5)) as TransferredPercent,
	remote_machine_name,
	internal_state_desc,
	DATEADD(second, - DATEDIFF(second, GETDATE(), GETUTCDATE()), start_time_utc) AS SeedingStarted, 
	RIGHT('          ' + FORMAT(transferred_size_bytes / 1024 / 1024 / 1024, 'N0'), 10) AS TransferredSizeGb,
	--db files have empty space inside which doesn't need to be transferred
	RIGHT('          ' + FORMAT((SELECT      SUM(CAST(mf.size  AS BIGINT)) * 8 / 1024 / 1024
			FROM        sys.databases d
			JOIN        sys.master_files mf
			ON          d.database_id = mf.database_id  
			WHERE d.name = fss.local_database_name), 'N0'), 10)
		AS TotalDatabaseSizeGb
FROM sys.dm_hadr_physical_seeding_stats fss
