using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class JourneyBranchConfigurations
{
    public static void ConfigureJourneyBranch(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JourneyBranch>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long?>("RetirementJourneyId");
            entity.Property<long?>("TransferJourneyId");
            entity.Property<long?>("QuoteSelectionJourneyId");
            entity.Property<long?>("JourneyId");
            entity.ToTable("JourneyBranch").HasKey("Id");
            entity.HasMany(p => p.JourneySteps)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JourneyStep>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("JourneyBranchId").IsRequired();
            entity.ToTable("JourneyStep").HasKey("Id");
            entity.Property(p => p.CurrentPageKey)
                .HasMaxLength(25)
                .IsRequired();
            entity.Property(p => p.NextPageKey)
                .HasMaxLength(25)
                .IsRequired();
            entity.Property(p => p.SubmitDate).IsRequired();
            entity.Property(p => p.UpdateDate);
            entity.Property(p => p.IsNextPageAsDeadEnd)
                .IsRequired()
                .HasDefaultValue(false);
            entity.HasOne<JourneyBranch>()
                .WithMany(j => j.JourneySteps)
                .HasForeignKey("JourneyBranchId");
            entity.HasOne(p => p.QuestionForm)
                .WithOne()
                .HasForeignKey<QuestionForm>("JourneyStepId")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.CheckboxesLists)
               .WithOne()
               .HasForeignKey("JourneyStepId")
               .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(d => d.JourneyGenericDataList)
               .WithOne()
               .HasForeignKey("JourneyStepId")
               .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionForm>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("JourneyStepId").IsRequired();
            entity.ToTable("QuestionForm").HasKey("Id");
            entity.Property(p => p.QuestionKey)
                .HasMaxLength(25)
                .IsRequired();
            entity.Property(p => p.AnswerKey)
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(p => p.AnswerValue)
                .HasMaxLength(1000);
            entity.HasOne<JourneyStep>()
                .WithOne(p => p.QuestionForm);
        });

        modelBuilder.Entity<CheckboxesList>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("JourneyStepId").IsRequired();
            entity.ToTable("CheckboxesList").HasKey("Id");
            entity.Property(p => p.CheckboxesListKey)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasMany(p => p.Checkboxes)
               .WithOne()
               .HasForeignKey("CheckboxesListId")
               .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Checkbox>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("CheckboxesListId").IsRequired();
            entity.ToTable("Checkbox").HasKey("Id");
            entity.Property(p => p.Key)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(p => p.AnswerValue).IsRequired();
        });
    }
}