namespace FreshSpotRewardsWebApp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public partial class LoyayContext : DbContext
    {
        public LoyayContext() : base("name=LoyayContext")
        { }

        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<LoyaltyDetailRewardOptIn> LoyaltyDetailRewardOptIns { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Card>()
                .Property(e => e.M_IDNUM)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Card>()
                .Property(e => e.EntityID)
                .IsOptional();

            modelBuilder.Entity<Card>()
                .Property(e => e.EntityCategoryID)
                .IsOptional();

            modelBuilder.Entity<Card>()
                .Property(e => e.ProgramID)
                .IsOptional();

            modelBuilder.Entity<Card>()
                .Property(e => e.MerchantID)
                .IsOptional();

            modelBuilder.Entity<Card>()
                .Property(e => e.LocationID)
                .IsOptional();

            modelBuilder.Entity<Card>()
                .Property(e => e.CH_STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<Card>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Card>()
                .Property(e => e.CH_MPHONE)
                .IsUnicode(false);

            modelBuilder.Entity<Card>()
                .Property(e => e.VerificationCode)
                .HasColumnName("RESERVED1");

            modelBuilder.Entity<Card>()
                .Property(e => e.SkuGroupIds)
                .HasColumnName("RESERVED2");

            modelBuilder.Entity<Card>()
                .Property(e => e.AccountNumber);

            modelBuilder.Entity<LoyaltyDetailRewardOptIn>()
                .Property(e => e.LoyaltyDetailRewardSKUGroupID);

            modelBuilder.Entity<LoyaltyDetailRewardOptIn>()
                .Property(e => e.EntityCategoryID);

            modelBuilder.Entity<LoyaltyDetailRewardOptIn>()
                .Property(e => e.MobilePhone);

            modelBuilder.Entity<LoyaltyDetailRewardOptIn>()
                .Property(e => e.AddDate);

            modelBuilder.Entity<LoyaltyDetailRewardOptIn>()
                .Property(e => e.ID);


        }
    }
}
