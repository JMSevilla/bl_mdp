using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class GenericJourneyConfigurations
{
    public static void ConfigureGenericJourney(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenericJourney>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("Journeys")
                .HasKey("Id");

            entity.Property(c => c.ReferenceNumber).HasMaxLength(7).IsRequired();
            entity.Property(c => c.BusinessGroup).HasMaxLength(3).IsRequired();
            entity.Property(c => c.Type).HasMaxLength(100).IsRequired();
            entity.HasAlternateKey(p => new { p.BusinessGroup, p.ReferenceNumber, p.Type });

            entity.Property(c => c.Status).HasMaxLength(100).IsRequired();
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(p => p.IsMarkedForRemoval).HasColumnName("RemoveOnLogin").IsRequired();
            entity.Property(p => p.WordingFlags).HasMaxLength(1500);
            entity.Property(p => p.ExpirationDate);
            entity.Property(p => p.SubmissionDate);
            entity.HasMany(p => p.JourneyBranches)
                .WithOne()
                .HasForeignKey("JourneyId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(p => p.JourneyBranches).AutoInclude();
        });

        modelBuilder.Entity<JourneyGenericData>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("JourneyStepId").IsRequired();
            entity.ToTable("JourneyGenericData").HasKey("Id");
            entity.Property(p => p.FormKey)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(p => p.GenericDataJson)
                .IsRequired();
        });
    }
}