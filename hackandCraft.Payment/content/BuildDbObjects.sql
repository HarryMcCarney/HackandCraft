----------------------------------------------------------------------------------------------------------------
---------------------------TABLES-------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[subscription_method]
(
[name] [varchar] (25) COLLATE Latin1_General_CI_AS NOT NULL
)
go
CREATE TABLE [dbo].[subscription_level]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[name] [varchar] (25) COLLATE Latin1_General_CI_AS NOT NULL,
[year_price] [int] NULL
)
go
CREATE TABLE [dbo].[subscription_interval]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[name] [varchar] (25) COLLATE Latin1_General_CI_AS NOT NULL,
[yearly_intervals] [int] NULL,
[grace_days] [int] NULL,
[price] [int] NULL,
[active] [bit] NOT NULL CONSTRAINT [DF__subscript__activ__4924D839] DEFAULT ((1))
)
GO
CREATE TABLE [dbo].[subscription_period]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL,
[months] [int] NOT NULL,
[monthly_price] [int] NOT NULL,
[lockin] [bit] NOT NULL CONSTRAINT [DF__subscript__locki__795DFB40] DEFAULT ((0)),
[total_price] AS ([monthly_price]*[months])
)
GO
CREATE TABLE [dbo].[subscription]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[user_id] [int] NOT NULL,
[subscription_interval_id] [int] NOT NULL,
[created] [datetime] NOT NULL CONSTRAINT [DF__subscript__creat__27C3E46E] DEFAULT (getutcdate()),
[cvc] [varchar] (5) COLLATE Latin1_General_CI_AS NULL,
[cancelled] [datetime] NULL,
[subscription_method_id] [varchar] (25) COLLATE Latin1_General_CI_AS NULL,
[subscription_period_id] [int] NULL
)
go

CREATE TABLE [dbo].[installment]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[subscription_id] [int] NOT NULL,
[due_date] [datetime] NOT NULL,
[amount] [int] NOT NULL,
[created] [datetime] NOT NULL CONSTRAINT [DF__installme__creat__2F650636] DEFAULT (getutcdate()),
[queue_ref] [char] (36) COLLATE Latin1_General_CI_AS NULL,
[free] [bit] NOT NULL CONSTRAINT [DF__installmen__free__4F9CCB9E] DEFAULT ((0))
)
GO
CREATE TABLE [dbo].[payment]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[ref] [char] (36) COLLATE Latin1_General_CI_AS NOT NULL unique,
[payment_method_id] [int] NULL,
[amount] [int] NOT NULL,
[created] [datetime] NOT NULL CONSTRAINT [DF__payment__created__3D5E1FD2] DEFAULT (getutcdate()),
[installment_id] [int] NOT NULL foreign key references installment(id)
)
GO
CREATE TABLE [dbo].[payment_notice]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[payment_id] [int] NOT NULL foreign key references payment(id),
[notice_type_id] [int] NOT NULL,
[notice_status] [bit] NOT NULL,
[adyen_transaction_id] [bigint] NOT NULL,
[reason] [nvarchar] (255) COLLATE Latin1_General_CI_AS NULL,
[created] [datetime] NOT NULL
)
GO

CREATE TABLE [dbo].[payment_method]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[name] [varchar] (25) COLLATE Latin1_General_CI_AS NOT NULL
)
GO
CREATE TABLE [dbo].[notice_type]
(
[id] [int] NOT NULL IDENTITY(1, 1) primary key,
[name] [varchar] (255) COLLATE Latin1_General_CI_AS NOT NULL
)
go
CREATE TABLE [dbo].[payment_queue]
(
[ref] [uniqueidentifier] NOT NULL primary key,
[fetch_date] [datetime] NULL,
[failed_date] [datetime] NULL,
[succeed_date] [datetime] NULL,
[created] [datetime] NOT NULL CONSTRAINT [DF__payment_q__creat__47FBA9D6] DEFAULT (getutcdate()),
[message] [xml] NULL,
[note] [nvarchar] (4000) COLLATE Latin1_General_CI_AS NULL
)
GO
----------------------------------------------------------------------------------------------------------------
---------------------------VIEWS--------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------
	CREATE  view [dbo].[v_valid_payment] 
	with schemabinding
	as
	select distinct s.id subscription_id, i.id as installment_id, p.id payment_id, p.amount as amount,  p.created, p.ref, pn1.adyen_transaction_id
	from dbo.subscription s
	join dbo.installment i
		on s.id = i.subscription_id
	left join dbo.payment p
		on p.installment_id = i.id
	left join(dbo.payment_notice pn1
			join dbo.notice_type nt1
				on pn1.notice_type_id = nt1.id
				)
		ON p.id = pn1.payment_id
		AND nt1.name = N'AUTHORISATION'
		AND pn1.notice_status = 1
	LEFT JOIN (dbo.payment_notice pn2
			join dbo.notice_type nt2
				on pn2.notice_type_id = nt2.id
				)
		ON p.id = pn2.payment_id
		AND  (nt2.name IN (N'REFUND', N'CHARGEBACK', N'CANCELLATION', N'REFUSED', N'CANCEL_OR_REFUND','NOTIFICATION_OF_CHARGEBACK') --( was 2,4,5,7,8)--rejected payments this cant handle reverse charge backs
		OR (nt2.name = N'AUTHORISATION' AND pn2.notice_status = 0))
	WHERE (pn2.id IS NULL
	AND pn1.id IS NOT NULL)
GO

CREATE view [dbo].[v_failed_payment]
as
select payment_id, payment_status
from (
	select distinct p.id payment_id, nt2.name payment_status, rank() over(partition by p.id order by pn2.created desc) r
	from dbo.subscription s
	join dbo.installment i
		on s.id = i.subscription_id
	join dbo.payment p
		on p.installment_id = i.id
	join (dbo.payment_notice pn2
			join dbo.notice_type nt2
				on pn2.notice_type_id = nt2.id
				)
		ON p.id = pn2.payment_id
		AND  (nt2.name IN (N'REFUND', N'CHARGEBACK', N'CANCELLATION', N'REFUSED', N'CANCEL_OR_REFUND','NOTIFICATION_OF_CHARGEBACK') --( was 2,4,5,7,8)--rejected payments this cant handle reverse charge backs
		OR (nt2.name = N'AUTHORISATION' AND pn2.notice_status = 0))
	)x
where r = 1
GO
----------------------------------------------------------------------------------------------------------------
---------------------------SCHEMA-------------------------------------------------------------------------------
create schema job
go

----------------------------------------------------------------------------------------------------------------
---------------------------PROCS--------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------
create proc api.user_payment_from_ref
@p xml,
@r xml output
as
begin try

	declare @ref char(36),
			@payment xml 

	select	@ref = @p.value('(Payment/@paymentRef)[1]','char(36)')

	set @payment =
	(
	select @ref as "@paymentRef",
			amount as "@amount"
	from payment 
	where ref = @ref
	for xml path ('Payment'), type
	)

	select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName",
	@payment
	for xml path('Result')
	)

end try
begin catch
	exec dbo.set_error @p, @r output
end catch
GO


CREATE proc [job].[payment_create_with_installment]
@p xml,
@r xml output
as
begin try

	declare @token char(36),
			@interval varchar(255),
			@amount int,
			@year_price int,
			@method varchar(255),
			@method_id int,
			@payment_ref char(36), 
			@payment xml, 
			@email varchar(255),
			@user_id int,
			@installment_id int,
			@passed_installment_id int



	select @token = @p.value('(Payment/@userToken)[1]','char(36)'),
			@method = @p.value ('(Payment/@method)[1]', 'varchar(25)'),
			@passed_installment_id = @p.value ('(Payment/@installmentId)[1]', 'int')



-------------------------------------------------------------------------------------------------------
-------------------------input validation--------------------------------------------------------------
-------------------------------------------------------------------------------------------------------
	if   @token is null or @passed_installment_id is null
		begin 
		raiserror	(N'insufficient input params',
					16, -- severity.
					1) -- state
		end
-------------------------------------------------------------------------------------------------------
-------------------------end input validation----------------------------------------------------------
-------------------------------------------------------------------------------------------------------

	/*
	1. find the relevant installment.
	this should be the first one which isnt yet paid
	2.create a payment row linked to this installment
	*/

	select @user_id = id from [user] where token = @token 

	select top 1 @installment_id = i.installment_id, @amount = i.amount, @email = email
	from v_installment i
	join [user] u
		on u.id = i.[user_id]
	where  i.status not in  ('CANCELLED') -- a user can only have one non-cancelled suscription
	and i.[user_id] = @user_id
	and i.paid = 0
	order by subscription_created desc, due_date asc



	if @passed_installment_id != @installment_id
	begin 
	raiserror	(N'installment id of payment is already paid or doesnt match next due',
				16, -- severity.
				1) -- state
	end
	
	select @method_id =id from payment_method where name = @method 
	set @payment_ref = newid()
	insert payment 
	(ref, payment_method_id, amount, created, installment_id)
	select @payment_ref, @method_id, @amount, getutcdate(), @installment_id

	set @payment = (
						select	@payment_ref as "@paymentRef",
								@email as "@shopperEmail",
								case when @amount = 0 then 1 else @amount end as "@amount",
								'EUR' as "@currency",
								@token as "@shopperRef"
						for xml path ('Payment'), type
					)
								
	select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName",
	@payment as "*"
	for xml path('Result')
	)





end try
begin catch
		exec dbo.set_error @p, @r output	
end catch


go
CREATE proc [job].[get_payment_queue]
@p xml = null,
@r xml output
as
begin try

		declare @payment_xml nvarchar(max)
		
		
		update  top (10) c
		set @payment_xml = isnull(@payment_xml, '') + cast([message] as nvarchar(max)),
		fetch_date = getutcdate()
		from dbo.payment_queue c
		where (fetch_date is null or 
		(fetch_date < dateadd(hh,-24,getutcdate())  and failed_date is not null and succeed_date is null))

		select @r = (
		select 0 as "@status", object_name(@@procid) as "@procName",
		cast(@payment_xml as xml) as "Payments"
		for xml path ('Result')
		)
	
end try
begin catch
	exec dbo.set_error @p, @r output
end catch
GO

CREATE proc [job].[set_payment_queue]
@p xml,
@r xml output
as
begin try

		declare @queue_ref nvarchar(255),
				@status bit,
				@note nvarchar(255)
		
				
		select 
		@queue_ref = @p.value('(PaymentStatus/@queueRef)[1]', 'nvarchar(255)'),
		@status = @p.value('(PaymentStatus/@success)[1]', 'bit'),
		@note = @p.value('(PaymentStatus/@message)[1]', 'nvarchar(4000)')
-------------------------------------------------------------------------------------------------------
-------------------------input validation--------------------------------------------------------------
-------------------------------------------------------------------------------------------------------
	if  @queue_ref is null
		or @status is null
		begin 
		raiserror	('insufficient input params',
					16, -- severity.
					1)with log -- state
		end
		

-------------------------------------------------------------------------------------------------------
-------------------------end input validation----------------------------------------------------------
-------------------------------------------------------------------------------------------------------

		update payment_queue
		set failed_date = case when @status = 0 then getutcdate() else failed_date end,
			succeed_date = case when @status = 1 then getutcdate() else succeed_date end,
			note = @note
		where @queue_ref = ref
		
	select @r = (
	select 0 as "@status", object_name(@@procid) as "@procName" 
	for xml path ('Result')
	)

end try 
begin catch
	exec dbo.set_error @p, @r output
end catch

GO


CREATE proc [job].[queue_subscription_payments]
@p xml = null,
@r xml output
as


declare @payments table (ref char(36) not null,installment_id int, shopper_ref char(36) not null, amount int not null, currency char(3) not null, cvc varchar(5) , [type] varchar(255))
declare @rowcount int

insert @payments
(ref,installment_id, shopper_Ref, amount, currency, cvc,  [type])
select newid() , 
		i.installment_id,
		u.token ,
		i.amount ,
		'EUR',
		cvc,
		s.subscription_method_id
from [user] u
join v_installment i
	on i.[user_id] = u.id
join subscription s
	on s.id = i.subscription_id
join subscription_period sp
	on sp.id = s.subscription_period_id
where i.due_date <= getutcdate()
and i.queue_ref is null
and i.paid = 0
and i.status != 'PENDING' 
and (i.status != 'CANCELLED' or sp.lockin = 1)
and s.created + 1 < getutcdate()--DIRTY. so that very first payment doesnt go into queue before it cn be marked as free or paid.

if exists( select 1 from @payments)
begin

	begin tran
	insert payment_queue
	(ref, [message])
	select p.ref, 
	(
	select	ref as "@queueRef",
			shopper_ref as "@shopperRef",
			amount as "@amount",
			cvc as "@cvc",
			currency as "@currency",
			[type] as "@type",
			installment_id as "@installmentId"
	from @payments p2
	where p.ref= p2.ref
	for xml path ('Payment'), type
	)
	from @payments p
	
	update i
	set queue_ref = p.ref
	from installment i
	join @payments p
		on p.installment_id = i.id
	set @rowcount = @@ROWCOUNT

	select @rowcount
	select * from @payments
	if @@error <> 0 or @rowcount = 0
	rollback tran
	else
	commit tran

end

	select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName"
	for xml path('Result')
	)



GO

CREATE proc [api].[payment_create]
@p xml,
@r xml output
as
begin try

	declare @token char(36),
			@interval varchar(255),
			@amount int,
			@year_price int,
			@method varchar(255),
			@method_id int,
			@payment_ref char(36), 
			@payment xml, 
			@email varchar(255),
			@user_id int,
			@installment_id int



	select @token = @p.value('(Payment/@userToken)[1]','char(36)'),
			@method = @p.value ('(Payment/@method)[1]', 'varchar(25)')
			



-------------------------------------------------------------------------------------------------------
-------------------------input validation--------------------------------------------------------------
-------------------------------------------------------------------------------------------------------
	if   @token is null 
		begin 
		raiserror	(N'insufficient input params',
					16, -- severity.
					1) -- state
		end
-------------------------------------------------------------------------------------------------------
-------------------------end input validation----------------------------------------------------------
-------------------------------------------------------------------------------------------------------

	/*
	1. find the relevant installment.
	this should be the first one which isnt yet paid
	2.create a payment row linked to this installment
	*/

	select @user_id = id from [user] where token = @token 

	select top 1 @installment_id = i.installment_id, @amount = i.amount, @email = email
	from v_installment i
	join [user] u
		on u.id = i.[user_id]
	where i.status != 'CANCELLED' -- a user can only have one non-cancelled suscription
	and i.[user_id] = @user_id
	and i.paid = 0
	order by subscription_created desc, due_date asc
	
	select @method_id =id from payment_method where name = @method 
	set @payment_ref = newid()
	insert payment 
	(ref, payment_method_id, amount, created, installment_id)
	select @payment_ref, @method_id, @amount, getutcdate(), @installment_id

	set @payment = (
						select	@payment_ref as "@paymentRef",
								@email as "@shopperEmail",
								case when @amount = 0 then 1 else @amount end as "@amount",
								'EUR' as "@currency",
								@token as "@shopperRef"
						for xml path ('Payment'), type
					)
								
	select @r = (
	select 0 as "@status", 
	object_name(@@procid) as "@procName",
	@payment as "*"
	for xml path('Result')
	)





end try
begin catch
		exec dbo.set_error @p, @r output	
end catch

GO
create proc [api].[user_from_payment]
@p xml,
@r xml output
as 
begin try

	declare @token char(36),
			@user_id int

	select @token = @p.value('(Payment/@paymentRef)[1]','char(36)')
-------------------------------------------------------------------------------------------------------
-------------------------INPUT VALIDATION--------------------------------------------------------------
-------------------------------------------------------------------------------------------------------

	If @token is null
		BEGIN 
		RAISERROR	(N'No token',
					16, -- Severity.
					1)WITH LOG; -- state
		END
-------------------------------------------------------------------------------------------------------
-------------------------END INPUT VALIDATION----------------------------------------------------------
-------------------------------------------------------------------------------------------------------

		select @user_id = s.[user_id]
		from payment p
		join installment i
			on i.id = p.installment_id
		join subscription s
			on s.id = i.subscription_id
		 where p.ref = @token
	

			select @r = (
					select 0 as "@status", 
					object_name(@@procid) as "@procName", 
					dbo.fn_get_user(@user_id, null) as "*"
					for xml path ('Result')
				)



end try
begin catch
	exec dbo.set_error @p, @r output
end catch
GO
GO
----------------------------------------------------------------------------------------------------------------
---------------------------DATA---------------------------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------

	
SET IDENTITY_INSERT [dbo].[notice_type] ON
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (1, 'AUTHORISATION')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (2, 'CANCEL_OR_REFUND')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (3, 'CANCELLATION')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (4, 'CAPTURE')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (5, 'CHARGEBACK')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (6, 'NOTIFICATION_OF_CHARGEBACK')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (7, 'REFUND')
INSERT INTO [dbo].[notice_type] ([id], [name]) VALUES (8, 'REFUSED')
SET IDENTITY_INSERT [dbo].[notice_type] OFF


SET IDENTITY_INSERT [dbo].[payment_method] ON
INSERT INTO [dbo].[payment_method] ([id], [name]) VALUES (1, 'AMEX')
INSERT INTO [dbo].[payment_method] ([id], [name]) VALUES (2, 'MC')
INSERT INTO [dbo].[payment_method] ([id], [name]) VALUES (3, 'PAYPAL')
INSERT INTO [dbo].[payment_method] ([id], [name]) VALUES (4, 'VISA')
INSERT INTO [dbo].[payment_method] ([id], [name]) VALUES (5, 'ELV')
SET IDENTITY_INSERT [dbo].[payment_method] OFF


SET IDENTITY_INSERT [dbo].[subscription_interval] ON
INSERT INTO [dbo].[subscription_interval] ([id], [name], [yearly_intervals], [grace_days], [price], [active]) VALUES (1, 'MINUTLY', 525949, 10, 1051898, 0)
INSERT INTO [dbo].[subscription_interval] ([id], [name], [yearly_intervals], [grace_days], [price], [active]) VALUES (2, 'MONTHLY', 12, 10, 15480, 1)
INSERT INTO [dbo].[subscription_interval] ([id], [name], [yearly_intervals], [grace_days], [price], [active]) VALUES (3, 'YEARLY', 1, 1, 12000, 0)
INSERT INTO [dbo].[subscription_interval] ([id], [name], [yearly_intervals], [grace_days], [price], [active]) VALUES (4, 'HALF_YEARLY', 2, 15, 12000, 0)
INSERT INTO [dbo].[subscription_interval] ([id], [name], [yearly_intervals], [grace_days], [price], [active]) VALUES (5, 'QUARTERLY', 4, 10, 14000, 0)
SET IDENTITY_INSERT [dbo].[subscription_interval] OFF

SET IDENTITY_INSERT [dbo].[subscription_level] ON
INSERT INTO [dbo].[subscription_level] ([id], [name], [year_price]) VALUES (1, 'BRONZE', 10000)
INSERT INTO [dbo].[subscription_level] ([id], [name], [year_price]) VALUES (2, 'GOLD', 20000)
INSERT INTO [dbo].[subscription_level] ([id], [name], [year_price]) VALUES (3, 'SILVER', 15000)
SET IDENTITY_INSERT [dbo].[subscription_level] OFF


SET IDENTITY_INSERT [dbo].[subscription_period] ON
INSERT INTO [dbo].[subscription_period] ([id], [name], [months], [monthly_price], [lockin]) VALUES (1, '3MONTHS', 3, 1190, 1)
INSERT INTO [dbo].[subscription_period] ([id], [name], [months], [monthly_price], [lockin]) VALUES (2, '6MONTHS', 6, 990, 1)
INSERT INTO [dbo].[subscription_period] ([id], [name], [months], [monthly_price], [lockin]) VALUES (3, 'MONTHLY', 12, 1290, 0)
SET IDENTITY_INSERT [dbo].[subscription_period] OFF
