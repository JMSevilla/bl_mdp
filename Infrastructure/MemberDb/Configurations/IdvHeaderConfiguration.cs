using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class IdvHeaderConfiguration
{
    public static void ConfigureIdvHeader(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdvHeader>(entity =>
        {
            entity.ToTable("WW_IDV_HEADER");

            entity
                .Property(c => c.ReferenceNumber)
                .HasColumnName("REFNO")
                .IsRequired();

            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();

            entity
                .Property(c => c.SchemeMember)
                .HasColumnName("SCHMEM")
                .HasMaxLength(1)
                .IsRequired();

            entity
                .Property(c => c.Date)
                .HasColumnName("SCREENING_DATE")
                .IsRequired();

            entity
                .Property(c => c.Type)
                .HasColumnName("TYPE")
                .HasMaxLength(1)
                .IsRequired();

            entity
                .Property(c => c.CaseNumber)
                .HasColumnName("CASENO")
                .IsRequired();

            entity
                .Property(c => c.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity
                .Property(c => c.Status)
                .HasColumnName("STATUS")
                .IsRequired();

            entity
                .Property(c => c.AddressLine1)
                .HasColumnName("WWIDH01X")
                .HasMaxLength(50);

            entity
                .Property(c => c.AddressLine2)
                .HasColumnName("WWIDH02X")
                .HasMaxLength(50);

            entity
                .Property(c => c.AddressLine3)
                .HasColumnName("WWIDH03X")
                .HasMaxLength(50);

            entity
                .Property(c => c.AddressLine4)
                .HasColumnName("WWIDH04X")
                .HasMaxLength(50);

            entity
                .Property(c => c.AddressCity)
                .HasColumnName("WWIDH05X")
                .HasMaxLength(50);

            entity
                .Property(c => c.AddressPostCode)
                .HasColumnName("WWIDH06X")
                .HasMaxLength(8)
                .IsRequired();

            entity
                .Property(c => c.IssuingCountryCode)
                .HasColumnName("WWIDH07X")
                .HasMaxLength(8)
                .IsRequired();

            entity
                .Property(c => c.Phone)
                .HasColumnName("WWIDH08X")
                .HasMaxLength(8);

            entity
                .Property(c => c.Email)
                .HasColumnName("WWIDH09X")
                .HasMaxLength(8);

            entity
                .Property<int>("DetailId")
                .HasColumnName("DETAIL_ID");

            entity.HasOne(x => x.Detail)
                .WithMany()
                .HasForeignKey("DetailId");

            entity.HasKey(x => new
            {
                x.ReferenceNumber,
                x.BusinessGroup,
                x.SchemeMember,
                x.SequenceNumber
            });
        });
    }

}