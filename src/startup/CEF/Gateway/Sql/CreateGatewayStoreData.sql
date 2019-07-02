set nocount on
go

-- Insert mock data for test purpose
insert into [dbo].[Engagements] ([EID], [EngagementAccount], [EngagementDescription], [Created], [LastUpdated])
values ('00000000-0000-0000-0000-000000000001', N'mockaccount', CAST('mockdescription' as varbinary(max)), getutcdate(), getutcdate())
go

insert into [dbo].[Groups] ([Name], [EngagementAccount], [Description], [Created], [LastUpdated])
values (N'mockgroup', N'mockaccount', N'this is a mock group', getutcdate(), getutcdate())
go

insert into [dbo].[EngagementGroups] ([EID], [Name], [EngagementAccount], [Created])
values ('00000000-0000-0000-0000-000000000001', N'mockgroup', N'mockaccount', getutcdate())
go
