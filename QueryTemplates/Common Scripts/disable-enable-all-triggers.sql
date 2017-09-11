WITH AllTables
AS (SELECT DISTINCT parent_id
    FROM sys.triggers
    WHERE is_ms_shipped = 0
          AND is_disabled = 0)
SELECT 'DISABLE TRIGGER ALL ON [' + S.name + '].[' + T2.name + '];',
       'ENABLE TRIGGER ALL ON [' + S.name + '].[' + T2.name + '];'
FROM AllTables AS T
     INNER JOIN
     sys.tables AS T2
     ON T.parent_id = T2.object_id
     INNER JOIN
     sys.schemas AS S
     ON T2.schema_id = S.schema_id;
