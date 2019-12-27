-- store versions in separate filegroup
ALTER DATABASE [DB] ADD FILEGROUP [PVS]
GO
ALTER DATABASE [DB] ADD FILE ( NAME = N'DB_PVS', FILENAME = N'X:\Databases\DB\DB_PVS.ndf' , SIZE = 8192KB , FILEGROWTH = 65536KB ) TO FILEGROUP [PVS]
GO
ALTER  DATABASE [DB] SET ACCELERATED_DATABASE_RECOVERY = ON (PERSISTENT_VERSION_STORE_FILEGROUP = [PVS]) WITH ROLLBACK IMMEDIATE;

-- cleanup
ALTER  DATABASE [DB] SET ACCELERATED_DATABASE_RECOVERY = OFF WITH ROLLBACK IMMEDIATE;
GO
EXEC sys.sp_persistent_version_cleanup 'DB'
GO

--check storage size
SELECT DB_Name(database_id), persistent_version_store_size_kb, pvs_filegroup_id, *
FROM sys.dm_tran_persistent_version_store_stats
