﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    internal partial class ResourceProviderEntities : DbContext
    {
        public ResourceProviderEntities()
            : base("name=ResourceProviderEntities")
        {
            SubscriptionRegistrations = Set<SubscriptionRegistration>();
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        internal virtual DbSet<SubscriptionRegistration> SubscriptionRegistrations { get; set; }
    }
}
