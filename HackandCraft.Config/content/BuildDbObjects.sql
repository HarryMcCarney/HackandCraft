SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[setting](
	[key] [varchar](255) NOT NULL,
	[value] [varchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [api].[global_config]
@p xml = null,
@r xml output
as
begin try

declare @setting xml,
		@global xml
		
		
set @setting = (
				select [key] as "@key",
						value as "@value"
				from setting
				for xml path ('Setting'),type
				)

				
set @global = (
				select @setting as "*"
				for xml path ('Global'), type
				)


select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName",
	@global as "*"
	for xml path('Result')
	)


end try
begin catch
	exec dbo.set_error @p, @r output
end catch


GO


