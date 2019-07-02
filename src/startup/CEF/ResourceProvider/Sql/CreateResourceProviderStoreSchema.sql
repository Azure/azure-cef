set ansi_nulls on
set quoted_identifier on
set nocount on
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[SubscriptionRegistrations]') and type in (N'U'))
    drop table [dbo].[SubscriptionRegistrations]
go

create table [dbo].[SubscriptionRegistrations]
(
    [SubscriptionId] [nvarchar](36) not null,
    [State] [nvarchar](32) not null,
    [TenantId] [nvarchar](36) not null,
    [RegistrationDate] [datetime2](7) not null,
    [LocationPlacementId] [nvarchar](max) not null,
    [QuotaId] [nvarchar](max) not null,
    [RegisteredFeatures] [nvarchar](max) not null,
    primary key clustered ([SubscriptionId])
)
go