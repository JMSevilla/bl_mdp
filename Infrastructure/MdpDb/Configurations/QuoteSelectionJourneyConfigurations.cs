using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class QuoteSelectionJourneyConfigurations
{
    public static void ConfigureQuoteSelectionJourney(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuoteSelectionJourney>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("QuoteSelectionJourney")
                .HasKey("Id");
            entity.HasAlternateKey(p => new { p.BusinessGroup, p.ReferenceNumber });
            entity.Property(p => p.StartDate).IsRequired();
            entity.HasMany(p => p.JourneyBranches)
                .WithOne()
                .HasForeignKey("QuoteSelectionJourneyId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(x => x.SubmissionDate);
            entity.Ignore(x => x.ExpirationDate);

            entity.Navigation(p => p.JourneyBranches).AutoInclude();
        });
    }
}