-- beginning of hour 
SELECT DATEADD(hour, DATEDIFF(hour, 0, GETDATE()), 0)
-- beginning of day
SELECT DATEADD(day, DATEDIFF(day, 0, GETDATE()), 0)
-- yeasterday 
SELECT DATEADD(day, DATEDIFF(day, 0, GETDATE()), -1)
-- beginning of the month
SELECT DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)
-- enf of the month
SELECT DATEADD(month, DATEDIFF(month, 0, GETDATE()), -1)
-- start of next month:
SELECT DATEADD(month, DATEDIFF(month, 0, GETDATE()), 31)
-- adjust to nearest 4 hours interval: 00:00, 04:00, 08:00, 12:00, 16:00, 20:00
SELECT DATEADD(hour, ROUND(DATEDIFF(hour, 0, GETDATE()) / 4, 0, 1) * 4, 0)
