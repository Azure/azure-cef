set ansi_nulls on
set quoted_identifier on
set nocount on
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[OtpCodes]') and type in (N'U'))
    drop table [dbo].OtpCodes
go

create table [dbo].OtpCodes
(
    [PhoneNumber] [nvarchar](32) NOT NULL,
	[EngagementAccount] [nvarchar](64) NOT NULL,
	[Code] [nvarchar](32) NOT NULL,
	[CreatedTime] [datetime2](7) NOT NULL,
	[ExpiredTime] [datetime2](7) NOT NULL,
    primary key nonclustered ([EngagementAccount],[PhoneNumber])
);
create nonclustered index [idx_otpcodes_engagementaccount] on [dbo].[otpcodes]([engagementaccount] asc);
go

