select TOP 100 r.destination_database_name, r.restore_date, 
       bs.server_name, m.physical_device_name as backup_device, b.physical_name 
from msdb.dbo.restorehistory r
      inner join msdb.dbo.backupfile b on r.backup_set_id = b.backup_set_id 
      inner join msdb.dbo.backupset bs on b.backup_set_id = bs.backup_set_id
      inner join msdb.dbo.backupmediafamily m on bs.media_set_id = m.media_set_id
WHERE 
      r.destination_database_name = 'RentPayment'
ORDER BY 
r.restore_date DESC