exec xp_cmdshell 'net use j: \\dd2500\backups yap123 /user:leszek'
exec xp_cmdshell 'net use j: /DELETE'
EXEC master..xp_fixeddrives
EXEC xp_dirtree 'j:\', 10, 1