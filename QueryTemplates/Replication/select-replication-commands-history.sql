USE distribution;
 
DECLARE @PublisherDB SYSNAME
    ,@PublisherDBID INT
    ,@SeqNo NCHAR(22)
    ,@CommandID INT
 
SET @PublisherDB = N'myPublisher'
SET @SeqNo = N'0x000921630000191D000C00000001'
SET @CommandID = 1
 
SELECT @PublisherDBID = id
FROM dbo.MSpublisher_databases
WHERE publisher_db = @PublisherDB
 
SELECT *
FROM MSarticles
WHERE article LIKE '%articleName%'
 
CREATE TABLE #browsereplcmds (
    xact_seqno VARBINARY(16) NULL
    ,originator_srvname SYSNAME NULL
    ,originator_db SYSNAME NULL
    ,article_id INT NULL
    ,type INT NULL
    ,partial_command BIT NULL
    ,hashkey INT NULL
    ,originator_publication_id INT NULL
    ,originator_db_version INT NULL
    ,originator_lsn VARBINARY(16) NULL
    ,command NVARCHAR(1024) NULL
    ,command_id INT
    )
 
INSERT INTO #browsereplcmds
EXEC sp_browsereplcmds @xact_seqno_START = @SeqNo
    ,@publisher_database_id = @PublisherDBID;
 
SELECT *
FROM #browsereplcmds
 
DROP TABLE #browsereplcmds
