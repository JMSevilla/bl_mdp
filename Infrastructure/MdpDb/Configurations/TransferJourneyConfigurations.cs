using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class TransferJourneyConfigurations
{
    public static void ConfigureTransferJourney(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransferJourney>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("TransferJourney")
                .HasKey("Id");
            entity.HasAlternateKey(p => new { p.BusinessGroup, p.ReferenceNumber });
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(p => p.TransferImageId).HasDefaultValue(0).IsRequired();
            entity.Property(p => p.TransferSummaryImageId);
            entity.Property(p => p.CalculationType);
            entity.Property(p => p.GbgId);
            entity.Property(p => p.CaseNumber).HasMaxLength(10);
            entity.Property(p => p.NameOfPlan).HasMaxLength(50);
            entity.Property(p => p.TypeOfPayment).HasMaxLength(50);
            entity.Property(p => p.DateOfPayment).HasColumnType("date");
            entity.Property(x => x.FinancialAdviseDate);
            entity.Property(x => x.TransferVersion).HasMaxLength(250);
            entity.Property(x => x.PensionWiseDate);
            entity.HasMany(p => p.Contacts)
                .WithOne()
                .HasForeignKey("TransferJourneyId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.JourneyBranches)
                .WithOne()
                .HasForeignKey("TransferJourneyId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.SubmissionDate);
            entity.Ignore(x => x.ExpirationDate);

            entity.Navigation(p => p.JourneyBranches).AutoInclude();
        });

        modelBuilder.Entity<TransferJourneyContact>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("TransferJourneyId").IsRequired();
            entity.ToTable("TransferJourneyContact")
                .HasKey("Id");
            entity.Property(p => p.Name).HasMaxLength(50);
            entity.Property(p => p.AdvisorName).HasMaxLength(50);
            entity.Property(p => p.CompanyName).HasMaxLength(50);
            entity.Property(p => p.SchemeName).HasMaxLength(50);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.Type).HasMaxLength(50).IsRequired();

            entity.OwnsOne(p => p.Email, p =>
            {
                p.Property(pp => pp.Address).HasMaxLength(50);
            });
            entity.Navigation(b => b.Email).IsRequired();

            entity.OwnsOne(p => p.Phone, p =>
            {
                p.Property(pp => pp.FullNumber).HasMaxLength(50);
            });
            entity.Navigation(b => b.Phone).IsRequired();

            entity.OwnsOne(p => p.Address, p =>
            {
                p.Property(pp => pp.StreetAddress1).HasMaxLength(50);
                p.Property(pp => pp.StreetAddress2).HasMaxLength(50);
                p.Property(pp => pp.StreetAddress3).HasMaxLength(50);
                p.Property(pp => pp.StreetAddress4).HasMaxLength(50);
                p.Property(pp => pp.StreetAddress5).HasMaxLength(50);
                p.Property(pp => pp.PostCode).HasMaxLength(8);
                p.Property(pp => pp.Country).HasMaxLength(30);
                p.Property(pp => pp.CountryCode).HasMaxLength(3);
            });
            entity.Navigation(b => b.Address).IsRequired();
        });
    }
}
