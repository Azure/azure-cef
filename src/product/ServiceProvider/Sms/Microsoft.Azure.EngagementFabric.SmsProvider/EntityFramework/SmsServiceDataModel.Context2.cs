﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.SmsProvider.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SmsServiceDbEntities : DbContext
    {
        public SmsServiceDbEntities()
            : base("name=SmsServiceDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<TemplateEntity> Templates { get; set; }
        public virtual DbSet<SignatureEntity> Signatures { get; set; }
        public virtual DbSet<ConnectorAgentMetadataEntity> ConnectorAgentMetadata { get; set; }
        public virtual DbSet<EngagementAccountEntity> EngagementAccounts { get; set; }
        public virtual DbSet<ConnectorCredentialAssignmentEntity> ConnectorCredentialAssignments { get; set; }
        public virtual DbSet<ConnectorCredentialEntity> ConnectorCredentials { get; set; }
        public virtual DbSet<ConnectorMetadataEntity> ConnectorMetadata { get; set; }
    }
}
