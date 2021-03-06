


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING OFF
GO


CREATE TABLE [dbo].[errors](
	[de_id] [bigint] IDENTITY(1,1) NOT NULL,
	[de_error_number] [int] NULL,
	[de_error_severity] [int] NULL,
	[de_error_state] [int] NULL,
	[de_error_procedure] [varchar](100) NULL,
	[de_error_params] [xml] NULL,
	[de_error_line] [int] NULL,
	[de_error_message] [varchar](max) NULL,
	[de_error_date] [datetime] NULL,
	[de_login] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[de_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
 
GO

SET ANSI_PADDING OFF
GO




SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[set_error]
@params xml = null,
@result xml OUTPUT
AS
BEGIN

DECLARE @errmsg   NVARCHAR(2048),
		@severity TINYINT,
		@state    TINYINT,
		@errno    INT,
		@proc     SYSNAME,
		@lineno   INT,
		@returned_error_message NVARCHAR(4000),
		@error_id INT
		
		SELECT 
		@errmsg = ISNULL(error_message(),''), 
		@severity = ISNULL(error_severity(),''),   
		@state  = ISNULL(error_state(),''), 
		@errno = ISNULL(error_number(),''),
		@proc   = ISNULL(error_procedure(),''),
		@lineno = ISNULL(error_line(),'')
		
		SELECT @returned_error_message = '*** ' + quotename(@proc) + 
			', ' + ltrim(str(@lineno)) + '. Errno ' + 
			ltrim(str(@errno)) + ': ' + @errmsg
	


	INSERT dbo.errors
	(
	de_error_number, 
	de_error_severity, 
	de_error_state, 
	de_error_procedure, 
	de_error_params, 
	de_error_line, 
	de_error_message, 
	de_error_date, 
	de_login
	)
	SELECT
    @errno, 
	@severity,
    @state, 
    @proc,
    @params, 
    @lineno,
    @errmsg,
    GETUTCDATE(),
	system_user
	SET @error_id = SCOPE_IDENTITY()

	/*
	if db_name() != 'BurnPlus_dev'
	
	EXEC msdb.dbo.sp_send_dbmail
	@profile_name = 'BurnPlus',
	@recipients = 'errors@hackandcraft.com',
	@body = @errmsg,
	@subject = 'DB ERROR - BurnPlus' ;
	*/
	
	SELECT @result = (
				SELECT @error_id as "@status", 
				@returned_error_message as "@errorMessage",
				object_name(@@PROCID) as "@procName"
				FOR XML PAth ('Result') , type
				)

		
END



GO


GO
/****** Object:  StoredProcedure [api].[admin_collector_get]    Script Date: 11/05/2013 21:40:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc dbo.example_proc
@p xml,
@r xml output
as
begin try


	select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName"
for xml path('Result')
	)

end try
begin catch
	exec dbo.set_error @p,  @r output
end catch

