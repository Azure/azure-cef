--12/25--
--Create ConnectorMetadata for decouple sp/connector/dispatcher
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
