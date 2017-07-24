SELECT CASE WHEN ses.session_id = @@SPID THEN 'It''s me! ' ELSE '' END + COALESCE (ses.login_name, '???') AS WhosGotTheDAC,
       ses.session_id,
       ses.login_time,
       ses.status,
       ses.original_login_name
FROM sys.endpoints AS en
     INNER JOIN
     sys.dm_exec_sessions AS ses
     ON en.endpoint_id = ses.endpoint_id
WHERE en.name = 'Dedicated Admin Connection';
