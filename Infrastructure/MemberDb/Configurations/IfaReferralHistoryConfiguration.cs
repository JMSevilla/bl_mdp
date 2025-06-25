using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class IfaReferralHistoryConfiguration
{
    public static void ConfigureIfaReferralHistory(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IfaReferralHistory>(entity =>
        {
            entity.ToTable("IFA_REFERRAL_HISTORY");
            
            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();
            
            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();
            
            entity.HasKey(history => new
            {
                history.BusinessGroup,
                history.ReferenceNumber,
                history.SequenceNumber
            });
         
            entity.Property(p => p.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity.Property(p => p.ReferralInitiatedOn)
                .HasColumnName("REFERRAL_DATE")
                .HasConversion(MemberDbContext.DateTimeConverter)
                .IsRequired();

            entity.Property(p => p.ReferralStatusDate)
                .HasColumnName("REFERRAL_STATUS_DATE")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(p => p.ReferralStatus)
                .HasColumnName("REFERRAL_STATUS");

            entity.Property(p => p.ReferralResult)
                .HasColumnName("REFERRAL_RESULT")
                .HasMaxLength(50);
        });
    }
}
