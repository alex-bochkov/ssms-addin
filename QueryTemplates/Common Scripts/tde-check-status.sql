SELECT 
  DB_NAME(database_id) AS DatabaseName, 
  encryption_state,
  encryption_state_desc = CASE encryption_state
                              WHEN '0'  THEN  'No database encryption key present, no encryption'
                              WHEN '1'  THEN  'Unencrypted'
                              WHEN '2'  THEN  'Encryption in progress'
                              WHEN '3'  THEN  'Encrypted'
                              WHEN '4'  THEN  'Key change in progress'
                              WHEN '5'  THEN  'Decryption in progress'
                              WHEN '6'  THEN  'Protection change in progress (The certificate or asymmetric key that is encrypting the database encryption key is being changed.)'
         ELSE 'No Status'
         END,
  percent_complete,
  encryptor_thumbprint, 
  encryptor_type  
FROM sys.dm_database_encryption_keys
