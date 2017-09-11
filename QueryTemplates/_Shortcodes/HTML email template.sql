DECLARE @tableHTML VARCHAR(max)
	SET @tableHTML =
		N'<H3> - TITLE - </H3>' +
		N'<em>' + CONVERT(NVARCHAR, GETDATE(), 120) + N'</em>' +
		N'<table border="1">' +
		N'<tr> 
			<th>Platform</th> 
			<th>ID</th> 
			<th>Name</th> 
			<th>Ref</th> 
			<th>Amount</th> 
		</tr>' +
		CAST ( ( select 
			  td = [platform], '',
			  td = id, '',
			  td = name, '',
			  td = Ref, '',
			  td = Amount
			from ##temptable
			FOR XML PATH('tr'), TYPE 
		) AS NVARCHAR(MAX) ) +
		N'</table>' ;
		
		
EXEC msdb.dbo.sp_send_dbmail 
		@profile_name = 'DBA',
		@recipients = 'dba@google.com',
		@subject = 'Email title',
		@body = @tableHTML,
		@body_format = 'HTML';
