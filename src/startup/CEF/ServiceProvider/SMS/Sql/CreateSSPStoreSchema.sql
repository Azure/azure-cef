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
        [InboundMessageType] nvarchar(32) not null,
        [SingleReportForLongMessage] bit not null,
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
        [ChannelType] nvarchar(32) not null,
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
        [ChannelType] nvarchar(32) not null,
        [Enabled] bit not null default(1),
        [Active] bit not null,
        [ExtendedCode] varchar(12) null,
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
        [Settings] nvarchar(max) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        [SubscriptionId] varchar(36) null,
        [Provider] nvarchar(64) null,
        primary key clustered ([EngagementAccount])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Signatures]') and type in (N'U'))
begin
    create table [dbo].[Signatures]
    (
        [Signature] nvarchar(64) not null,
        [ChannelType] nvarchar(32) not null,
        [EngagementAccount] nvarchar(64) not null,
        [ExtendedCode] varchar(12) null,
        [State] nvarchar(32) not null,
        [Message] nvarchar(256) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([EngagementAccount], [Signature])
    )
end
go

-- Needed for querying all signatures by account
create nonclustered index [CIX_Signatures_EngagementAccount] on [dbo].[Signatures] ([EngagementAccount])
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Templates]') and type in (N'U'))
begin
    create table [dbo].[Templates]
    (
        [Name] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [Signature] nvarchar(64) not null,
        [Category] nvarchar(64) not null,
        [Body] nvarchar(max) not null,
        [State] nvarchar(32) not null,
        [Message] nvarchar(256) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([EngagementAccount], [Name])
    )
end
go

-- Needed for querying all templates by account and signature
create nonclustered index [CIX_Templates_EngagementAccount_Signature] on [dbo].[Templates] ([EngagementAccount], [Signature])
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[ConnectorAgentMetadata]') and type in (N'U'))
begin
    create table [dbo].[ConnectorAgentMetadata]
    (
        [Provider] nvarchar(64) not null,
        [Id] nvarchar(64) not null,
        [LastMessageSendTime] datetime2 null,
        [LastReportUpdateTime] datetime2 null,
        [PendingReceive] bigint null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([Provider], [Id])
    )
end
go--//----------------------------------------------------------------
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
        [InboundMessageType] nvarchar(32) not null,
        [SingleReportForLongMessage] bit not null,
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
        [ChannelType] nvarchar(32) not null,
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
        [ChannelType] nvarchar(32) not null,
        [Enabled] bit not null default(1),
        [Active] bit not null,
        [ExtendedCode] varchar(12) null,
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
        [Settings] nvarchar(max) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        [SubscriptionId] varchar(36) null,
        [Provider] nvarchar(64) null,
        primary key clustered ([EngagementAccount])
    )
end
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Signatures]') and type in (N'U'))
begin
    create table [dbo].[Signatures]
    (
        [Signature] nvarchar(64) not null,
        [ChannelType] nvarchar(32) not null,
        [EngagementAccount] nvarchar(64) not null,
        [ExtendedCode] varchar(12) null,
        [State] nvarchar(32) not null,
        [Message] nvarchar(256) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([EngagementAccount], [Signature])
    )
end
go

-- Needed for querying all signatures by account
create nonclustered index [CIX_Signatures_EngagementAccount] on [dbo].[Signatures] ([EngagementAccount])
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[Templates]') and type in (N'U'))
begin
    create table [dbo].[Templates]
    (
        [Name] nvarchar(64) not null,
        [EngagementAccount] nvarchar(64) not null,
        [Signature] nvarchar(64) not null,
        [Category] nvarchar(64) not null,
        [Body] nvarchar(max) not null,
        [State] nvarchar(32) not null,
        [Message] nvarchar(256) null,
        [Created] datetime2 not null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([EngagementAccount], [Name])
    )
end
go

-- Needed for querying all templates by account and signature
create nonclustered index [CIX_Templates_EngagementAccount_Signature] on [dbo].[Templates] ([EngagementAccount], [Signature])
go

if not exists (select * from sys.objects where object_id = object_id(N'[dbo].[ConnectorAgentMetadata]') and type in (N'U'))
begin
    create table [dbo].[ConnectorAgentMetadata]
    (
        [Provider] nvarchar(64) not null,
        [Id] nvarchar(64) not null,
        [LastMessageSendTime] datetime2 null,
        [LastReportUpdateTime] datetime2 null,
        [PendingReceive] bigint null,
        [Modified] datetime2 not null default(getutcdate()),
        primary key clustered ([Provider], [Id])
    )
end
go