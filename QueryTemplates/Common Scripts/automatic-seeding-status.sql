SELECT 
	local_database_name,
	CAST(CAST(transferred_size_bytes AS float) / CAST(database_size_bytes AS float) * 100 AS DECIMAL(10, 5)) as TransferredPercent,
	remote_machine_name,
	internal_state_desc,
	DATEADD(second, - DATEDIFF(second, GETDATE(), GETUTCDATE()), start_time_utc) AS SeedingStarted
FROM sys.dm_hadr_physical_seeding_stats;
