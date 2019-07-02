set ansi_nulls on
set quoted_identifier on
set nocount on
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[Engagements]') and type in (N'U'))
    drop table [dbo].[Engagements]
go

create table [dbo].[Engagements]
(
    [EID] uniqueidentifier not null,
    [EngagementAccount] nvarchar(64) not null,
    [EngagementDescription] varbinary(max) not null,
    [Created] datetime2 not null,
    [LastUpdated] datetime2 not null default(getutcdate()),
    [UserName] nvarchar(64) null
    primary key nonclustered ([EngagementAccount], [EID])
)
go

-- Needed for storing in sequence
create clustered index [CIX_Engagements] on [dbo].[Engagements] ([EngagementAccount])
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[Groups]') and type in (N'U'))
    drop table [dbo].[Groups]
go

create table [dbo].[Groups]
(
    [Name] nvarchar(128) not null,
    [EngagementAccount] nvarchar(64) not null,
    [Description] nvarchar(256) null,
    [Created] datetime2 not null,
    [LastUpdated] datetime2 not null default(getutcdate()),
    primary key clustered ([EngagementAccount], [Name])
)
go

if exists (select * from sys.objects where object_id = object_id(N'[dbo].[EngagementGroups]') and type in (N'U'))
    drop table [dbo].[EngagementGroups]
go

create table [dbo].[EngagementGroups]
(
    [EID] uniqueidentifier not null,
    [Name] nvarchar(128) not null,
    [EngagementAccount] nvarchar(64) not null,
    [Created] datetime2 not null,
    primary key nonclustered ([EngagementAccount], [Name], [EID])
)
go

-- Needed for storing in sequence
create clustered index [CIX_EngagementGroups] on [dbo].[EngagementGroups] ([EngagementAccount], [Name])
go

