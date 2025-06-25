using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class CalculationConfigurations
{
    public static void ConfigureCalculation(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Calculation>(entity =>
        {
            entity.ToTable("Calculation");

            entity.Property(p => p.BusinessGroup)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasMaxLength(7)
                .IsRequired();

            entity.HasKey(x => new { x.BusinessGroup, x.ReferenceNumber });
            entity.Property<long?>("RetirementJourneyId");

            entity.HasOne(x => x.RetirementJourney)
                .WithOne(x => x.Calculation)
                .HasForeignKey<Calculation>("RetirementJourneyId");

            entity.Property(p => p.RetirementDatesAgesJson).IsRequired();
            entity.Property(p => p.IsCalculationSuccessful);
            entity.Property(p => p.CalculationStatus);
            entity.Property(p => p.RetirementJson).HasDefaultValue("").IsRequired();
            entity.Property(p => p.RetirementJsonV2).HasDefaultValue("").IsRequired();
            entity.Property(p => p.QuotesJsonV2).HasDefaultValue("").IsRequired();
            entity.Property(p => p.SelectedQuoteName);
            entity.Property(p => p.EffectiveRetirementDate).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt);
            entity.Property(p => p.EnteredLumpSum);
        });
    }
}