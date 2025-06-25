using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class PaperRetirementApplicationConfiguration
{
    public static void ConfigurePaperRetirementApplication(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaperRetirementApplication>(entity =>
        {
            entity.ToTable("CW_CASE_LIST");

            entity.Property<string>("ReferenceNumber")
                .HasColumnName("REFNO")
                .IsRequired();

            entity.Property<string>("BusinessGroup")
                .HasColumnName("BGROUP")
                .IsRequired();

            entity
                .Property(c => c.Code)
                .HasColumnName("CL11X");

            entity
                .Property(c => c.CaseCode)
                .HasColumnName("CASECODE")
                .HasMaxLength(4)
                .IsRequired();

            entity
                .Property(c => c.EventType)
                .HasColumnName("EVTYPE")
                .HasMaxLength(2);

            entity
                .Property(c => c.Status)
                .HasColumnName("CL10X")
                .HasMaxLength(2);

            entity
              .Property(c => c.CaseReceivedDate)
              .HasColumnName("CL02D")
              .HasConversion(MemberDbContext.DateTimeConverter);
            
            entity
              .Property(c => c.CaseCompletionDate)
              .HasColumnName("CL22D")
              .HasConversion(MemberDbContext.DateTimeConverter);

            entity
                .Property(c => c.CaseNumber)
                .HasColumnName("CASENO")
                .HasMaxLength(7);

            entity.HasOne(x => x.BatchCreateDeatils)
               .WithOne()
               .HasForeignKey<PaperRetirementApplication>("ReferenceNumber", "BusinessGroup", "CaseNumber");

            entity.HasKey("ReferenceNumber", "BusinessGroup", "CaseNumber");
        });
    }

}