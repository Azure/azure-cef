set ansi_nulls on
set quoted_identifier on
set nocount on
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[Tenants]') and type in (N'U'))
    drop table [dbo].[Tenants]
go

create table [dbo].[Tenants]
(
    [AccountName] [nvarchar](64) not null,
    [Address] [nvarchar](512) null,
    [State] [nvarchar](32) not null,
    [Created] [datetime] not null,
    [LastModified] [datetime] not null,
    [ResourceDescription] [nvarchar](max) not null,
    [SubscriptionId] [nvarchar](36) not null,
    [ResourceGroup] [nvarchar](90) not null,
    [Location] [nvarchar](32) not null,
    [SKU] [nvarchar](32) not null,
    [Tags] [nvarchar](max) not null,
    [Timestamp] [timestamp] not null,
    [ResourceId] [nvarchar](max) not null,
    [IsDisabled] [bit] null,
    primary key clustered ([AccountName])
)
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[Quotas]') and type in (N'U'))
    drop table [dbo].[Quotas]
go

create table [dbo].[Quotas]
(
    [AccountName] [nvarchar](64) not null,
    [QuotaName] [nvarchar](64) not null,
    [Quota] [int] not null,
    [Remaining] [int] not null,
    [CreatedTime] [datetime] not null,
    [LastUpdatedTime] [datetime] not null,
    primary key clustered ([AccountName], [QuotaName])
)
go