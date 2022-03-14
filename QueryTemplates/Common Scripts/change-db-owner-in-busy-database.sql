-- 1st session
ALTER AUTHORIZATION ON DATABASE::[db] TO [sa]
GO

--2nd session
DECLARE @bloked AS INT = NULL;

SELECT @bloked = blocked
FROM sys.sysprocesses
WHERE spid = <1st session @@SPID>;

IF @bloked > 0
    EXECUTE ('kill ' + @bloked);

WAITFOR DELAY '00:00:00.100';
