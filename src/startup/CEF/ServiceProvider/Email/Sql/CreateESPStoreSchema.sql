--//----------------------------------------------------------------
--// Copyright (c) Microsoft Corporation.  All rights reserved.
--//----------------------------------------------------------------

set ansi_nulls on
set quoted_identifier on
set nocount on
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[ConnectorMetadata]') and type in (N'U'))
begin
    create table [dbo].[ConnectorMetadata]
    (
        [Provider] nvarchar(64) not null,
        [ServiceUri] varchar(256) not null,
        [BatchSize] bigint not null,
        [ReportType] nvarchar(32) not null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([Provider])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[ConnectorCredentials]') and type in (N'U'))
begin
    create table [dbo].[ConnectorCredentials]
    (
        [Provider] nvarchar(64) not null,
        [Id] nvarchar(64) not null,
        [Data] nvarchar(max) not null,
        [Enabled] bit not null default(1),
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([Provider], [Id])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[ConnectorCredentialAssignments]') and type in (N'U'))
begin
    create table [dbo].[ConnectorCredentialAssignments]
    (
        [EngagementAccount] nvarchar(64) not null,
        [Provider] nvarchar(64) not null,
        [Id] nvarchar(64) not null,
        [Enabled] bit not null default(1),
        [Active] bit not null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([EngagementAccount], [Provider], [Id])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[EngagementAccounts]') and type in (N'U'))
begin
    create table [dbo].[EngagementAccounts]
    (
        [EngagementAccount] nvarchar(64) not null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        [SubscriptionId] varchar(36) null,
        [Properties] nvarchar(max) null,
        primary key clustered ([EngagementAccount])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Domains]') and type in (N'U'))
begin
    create table [dbo].[Domains]
    (
        [Domain] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [State] nvarchar(32) not null,
        [Message] nvarchar(256) null,
        [Created] datetime2(7) not null,
        [Modified] datetime2(7) not null,
        primary key clustered ([EngagementAccount], [Domain])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Groups]') and type in (N'U'))
begin
    create table [dbo].[Groups]
    (
        [Name] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [Description] nvarchar(64) null,
        [Created] datetime2(7) not null,
        [Modified] datetime2(7) not null,
        [Properties] nvarchar(max) null,
        primary key clustered ([EngagementAccount], [Name])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[SenderAddresses]') and type in (N'U'))
begin
    create table [dbo].[SenderAddresses]
    (
        [Id] uniqueidentifier not null,
        [Name] nvarchar(128) not null,
        [Domain] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [ForwardAddress] nvarchar(128) not null,
        [Created] datetime2(7) not null,
        [Modified] datetime2(7) not null,
        [Properties] nvarchar(max) null,
        primary key clustered ([EngagementAccount], [Name])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Templates]') and type in (N'U'))
begin
    create table [dbo].[Templates]
    (
        [Name] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [SenderAddressId] uniqueidentifier not null,
        [Domain] nvarchar(64) not null,
        [SenderAlias] nvarchar(128) not null,
        [ReplyAddress] nvarchar(128) not null,
        [Subject] nvarchar(256) not null,
        [MessageBody] nvarchar(max) not null,
        [State] nvarchar(32) not null,
        [StateMessage] nvarchar(256) null,
        [Created] datetime2(7) not null,
        [Modified] datetime2(7) not null,
        [Properties] nvarchar(max) null,
        [EnableUnSubscribe] bit null,
        primary key clustered ([EngagementAccount], [Name])
    )
end
go