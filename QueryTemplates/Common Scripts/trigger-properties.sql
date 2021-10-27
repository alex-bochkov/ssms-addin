SELECT sysobjects.name AS trigger_name,
       USER_NAME(sysobjects.uid) AS trigger_owner,
       s.name AS table_schema,
       OBJECT_NAME(parent_obj) AS table_name,
       OBJECTPROPERTY(id, 'ExecIsUpdateTrigger') AS isupdate,
       OBJECTPROPERTY(id, 'ExecIsDeleteTrigger') AS isdelete,
       OBJECTPROPERTY(id, 'ExecIsInsertTrigger') AS isinsert,
       OBJECTPROPERTY(id, 'ExecIsAfterTrigger') AS isafter,
       OBJECTPROPERTY(id, 'ExecIsInsteadOfTrigger') AS isinsteadof,
       OBJECTPROPERTY(id, 'ExecIsTriggerDisabled') AS [disabled]
FROM sysobjects
     INNER JOIN sysusers	
		ON sysobjects.uid = sysusers.uid
     INNER JOIN sys.tables AS t
		ON sysobjects.parent_obj = t.object_id
     INNER JOIN sys.schemas AS s
		ON t.schema_id = s.schema_id
WHERE sysobjects.type = 'TR'
      --AND sysobjects.name LIKE '%%'
ORDER BY isinsert DESC;

SELECT sys.tables.name,
       sys.triggers.name,
       sys.trigger_events.type,
       sys.trigger_events.type_desc,
       is_first,
       is_last,
       sys.triggers.create_date,
       sys.triggers.modify_date
FROM sys.triggers
     INNER JOIN sys.trigger_events
		ON sys.trigger_events.object_id = sys.triggers.object_id
     INNER JOIN sys.tables
		ON sys.tables.object_id = sys.triggers.parent_id
WHERE 1 = 1 
      -- AND sys.trigger_events.type_desc = 'UPDATE'
      -- AND sys.triggers.name LIKE '%%'
      -- AND is_last = 0
ORDER BY modify_date;
