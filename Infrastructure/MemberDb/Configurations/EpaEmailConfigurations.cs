using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class EpaEmailConfigurations
{
    public static void ConfigureEpaEmail(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EpaEmail>(entity =>
        {
            entity.ToTable("BASIC_EPA_EMAILADDRESS");

            entity.Property<string>("ReferenceNumber")
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property<string>("BusinessGroup")
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "SequenceNumber");

            entity.OwnsOne(p => p.Email, p =>
            {
                p.Property(pp => pp.Address).HasMaxLength(80).HasColumnName("EMAIL");
            });

            entity.Property(p => p.EffectiveDate)
               .HasColumnName("EFFDATE")
               .HasConversion(MemberDbContext.DateTimeConverter)
               .IsRequired();

            entity.Property(p => p.CreaetedAt)
               .HasColumnName("CREATED")
               .HasConversion(MemberDbContext.DateTimeConverter);

            entity.HasOne<Member>()
                .WithMany(p => p.EpaEmails)
                .HasForeignKey("ReferenceNumber", "BusinessGroup")
                .IsRequired();
        });
    }
}