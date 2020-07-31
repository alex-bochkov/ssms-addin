SELECT t.name AS TableName,
       c.name AS ColumnName,
       k.name AS KeyName,
       c.encryption_type_desc,
       c.encryption_algorithm_name
FROM sys.columns AS c
     INNER JOIN
     sys.column_encryption_keys AS k
     ON c.column_encryption_key_id = k.column_encryption_key_id
     INNER JOIN
     sys.tables AS t
     ON c.object_id = t.object_id
WHERE encryption_type IS NOT NULL;
