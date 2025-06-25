using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Bereavement;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public class BereavementDbContext : DbContext
{
    public BereavementDbContext(DbContextOptions<BereavementDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BereavementJourney>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("BereavementJourney")
                .HasKey("Id");
            entity.Property(x => x.BusinessGroup).IsRequired();
            entity.Property(x => x.ReferenceNumber).IsRequired();
            entity.HasAlternateKey(p => new { p.BusinessGroup, p.ReferenceNumber });
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(x => x.SubmissionDate);
            entity.Property(x => x.ExpirationDate).IsRequired();
            entity.HasMany(p => p.JourneyBranches)
                .WithOne()
                .HasForeignKey("BereavementJourneyId")
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(p => p.JourneyBranches).AutoInclude();
        });

        modelBuilder.Entity<BereavementContactConfirmation>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("BereavementContactConfirmation").HasKey("Id");

            entity.Property(p => p.BusinessGroup)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(p => p.Token)
                .HasMaxLength(6)
                .IsRequired();

            entity.Property(p => p.Contact)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.FailedConfirmationAttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(p => p.MaximumConfirmationAttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<JourneyBranch>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.Property<long>("BereavementJourneyId");
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
            entity.Ignore(p => p.UpdateDate);
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
        });

        modelBuilder.Ignore<JourneyGenericData>();

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

        base.OnModelCreating(modelBuilder);
    }
}
