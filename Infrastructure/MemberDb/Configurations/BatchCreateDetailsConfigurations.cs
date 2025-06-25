using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class BatchCreateDetailsConfigurations
{
    public static void ConfigureBatchCreateDetails(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BatchCreateDetails>(entity =>
        {
            entity.ToTable("ww_wf_batch_create_detail".ToUpper());

            entity.Property<string>("ReferenceNumber")
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property<string>("BusinessGroup")
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property<string>("CaseNumber")
                .HasColumnName("CASENO")
                .HasMaxLength(7)
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "CaseNumber");
            entity.Property(x => x.Notes)
                .HasColumnName("NOTES")
                .HasMaxLength(2000);
        });
    }
}
