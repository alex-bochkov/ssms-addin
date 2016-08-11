DECLARE @tableHTML VARCHAR(max)
	SET @tableHTML =
		N'<H3>Mismatched AMEX processor</H3>' +
		N'<em>' + CONVERT(NVARCHAR, GETDATE(), 120) + N'</em>' +
		N'<table border="1">' +
		N'<tr> <th>Platform</th> <th>ID</th> <th>Name</th> <th>Ref</th> <th>Amount</th> </tr>' +
		CAST ( ( select 
			  td = [platform], '',
			  td = id, '',
			  td = name, '',
			  td = Ref, '',
			  td = Amount
			from ##ReportMismatchedAmexProcessor
			FOR XML PATH('tr'), TYPE 
		) AS NVARCHAR(MAX) ) +
		N'</table>' ;
		
		
EXEC msdb.dbo.sp_send_dbmail 
		@profile_name = 'DBA',
		@recipients = 'techsupport@yapstone.com',
		@subject = 'Mismatched Amex processor',
		@body = @tableHTML,
		@body_format = 'HTML', 
		@query = @qry,
		 @attach_query_result_as_file = 1,
		 @query_attachment_filename = 'MismatchedAmexProcessor.csv',
		 @query_result_separator = ',',
		 @query_result_width = 32767,
		 @query_result_no_padding = 1;