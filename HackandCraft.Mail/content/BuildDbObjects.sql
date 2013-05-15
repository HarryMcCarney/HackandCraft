SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[fn_promote_message_ref](@xml xml)
RETURNS NVARCHAR(255)
with schemabinding 
AS
BEGIN
DECLARE @ref NVARCHAR(255)

SELECT @ref =  @xml.value('(/Message/@id)[1]', 'NVARCHAR(255)')

RETURN @ref

END 
GO
CREATE TABLE [dbo].[message_status]
(
	[id] [int] IDENTITY(1,1) NOT NULL primary key,
	[name] [varchar](255) NOT NULL
	)
GO
set identity_insert on
insert [dbo].[message_status]
(id, name)
select 1,'FAILED' union
select 2, 'PENDING' union
select 3 'SENT' 
set identity_insert off
go


CREATE TABLE [dbo].[message_queue](
	[id] [int] IDENTITY(1,1) NOT NULL primary key,
	[message] [xml] NOT NULL,
	[retry] [int] NOT NULL,
	[ref]  AS ([dbo].[fn_promote_message_ref]([message])),
	[status_id] [int] NOT NULL foreign key references message_status(id),
	[created] [datetime] NOT NULL default getutcdate()
)
GO

CREATE SCHEMA [mess]
GO


CREATE proc [mess].[dequeue_message]
@p xml = null,
@r xml output
as
begin try
declare @messages varchar(max)


update dbo.message_queue
set @messages = isnull(@messages, '') + cast(message as nvarchar(max)),
retry = retry + 1,
fetched = getutcdate()
where [status_id] in (1,2)
and retry <10
and datediff(hh, isnull(fetched,getutcdate() - 1), getutcdate())> 1


	select @r = (
					select 0 as "@status", 
					object_name(@@procid) as "@procName",
					cast(@messages as xml) as "*"
					for xml path ('Result')
					)
end try
begin catch
	exec dbo.set_error @p, @r output
end catch

GO



create proc [mess].[enqueue_message]
@p xml,
@r xml output
as
begin try

	declare @status_id int
	select @status_id = id from message_status where name ='PENDING'
	insert message_queue
	([message], status_id, retry)
	select @p, @status_id, 0
	
	
		
	select @r = (
					select 0 as "@status", 
					object_name(@@procid) as "@proc_name"
					for xml path ('Result')
				)
	
end try
begin catch
	exec dbo.set_error @p, @r output
end catch 

GO
create  proc [mess].[set_message_status]
@p xml,
@r xml output
as
begin try
declare @status varchar(255),
		@messageid varchar(255),
		@status_id int,
		@result xml
		
	
select @status = @p.value('(MessageStatus/@status)[1]','varchar(255)'),
		@messageid = @p.value('(MessageStatus/Message/@id)[1]','varchar(255)')


select @status_id = id from message_status where @status = [name]
-------------------------------------------------------------------------------------------------------
-------------------------INPUT VALIDATION--------------------------------------------------------------
-------------------------------------------------------------------------------------------------------
	IF @messageid is null 
						
		BEGIN 
		RAISERROR	(N'insuffcient params',
					16, -- Severity.
					1) -- state
		END
	IF @status_id is null 
						
		BEGIN 
		RAISERROR	(N'status not found',
					16, -- Severity.
					1); -- state
		END

-------------------------------------------------------------------------------------------------------
-------------------------END INPUT VALIDATION----------------------------------------------------------
-------------------------------------------------------------------------------------------------------	
		
		update message_queue
		set status_id = @status_id
		where @messageid = ref
		
	SELECT @r = (
					SELECT 0 as status, 
					object_name(@@PROCID) as proc_name
					FOR XML RAW ('Result')
					)

end try
begin catch
	exec dbo.set_error @p, @r output
end catch






GO












