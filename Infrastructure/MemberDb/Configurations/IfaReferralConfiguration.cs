using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class IfaReferralConfiguration
{
    public static void ConfigureIfaReferral(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IfaReferral>(entity =>
        {
            entity.ToTable("IFA_REFERRAL");
            
            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();
            
            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.CalculationType)
                .HasColumnName("CALCTYPE")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasKey(ifa => new
            {
                ifa.BusinessGroup,
                ifa.ReferenceNumber,
            });

            entity.Property(p => p.ReferralInitiatedOn)
                .HasColumnName("REFERRAL_DATE")
                .HasConversion(MemberDbContext.DateTimeConverter)
                .IsRequired();

            entity.Property(p => p.ReferralResult)
                .HasColumnName("REFERRAL_RESULT")
                .HasMaxLength(50);
        });

        modelBuilder.Entity<IfaConfiguration>(entity =>
        {
            entity.ToTable("IFA_CONFIGURATION");

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.IfaName)
                .HasColumnName("IFA_NAME")
                .IsRequired();

            entity.Property(p => p.CalculationType)
                .HasColumnName("CALCTYPE")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasKey(ifa => new
            {
                ifa.BusinessGroup,
                ifa.IfaName,
                ifa.CalculationType
            });

            entity.Property(p => p.IfaEmail)
                .HasColumnName("IFA_EMAIL")
                .HasMaxLength(50);
        });
    }
}
