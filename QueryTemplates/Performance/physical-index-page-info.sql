/*
  Get stats around physical index pages
*/
CREATE TABLE #TempDBCC_IND
(
    PageFID           SMALLINT,
    PagePID           INT,
    IAMFID            SMALLINT,
    IAMPID            INT,
    ObjectID          INT,
    IndexID           SMALLINT,
    PartitionNumber   SMALLINT,
    PartitionID       BIGINT,
    iam_chain_type    VARCHAR(100),
    PageType          TINYINT,
    IndexLevel        TINYINT,
    NextPageFID       SMALLINT,
    NextPagePID       INT,
    PrevPageFID       SMALLINT,
    PrevPagePID       INT
)

INSERT INTO #TempDBCC_IND
EXEC('DBCC IND(''DatabaseName'', ''TableName'', 1) WITH NO_INFOMSGS');

SELECT PageFID,
       COUNT(*) AS NumberOfPages
FROM #TempDBCC_IND
WHERE PartitionNumber = 1
GROUP BY PageFID
ORDER BY 2;
