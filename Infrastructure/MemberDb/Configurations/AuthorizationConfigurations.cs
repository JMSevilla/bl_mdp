using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class AuthorizationConfigurations
{
    public static void ConfigureAuthorization(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Members.Authorization>(entity =>
        {
            entity.ToTable("PS_AUTH");

            entity.Property(p => p.ReferenceNumber)
               .HasColumnName("REFNO")
               .HasMaxLength(7);

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.AuthorizationNumber)
               .HasColumnName("AUTHNO")
               .IsRequired();

            entity.HasKey("BusinessGroup", "AuthorizationNumber");

            entity.Property(p => p.AuthorizationCode)
               .HasColumnName("AUTHCODE")
               .HasMaxLength(8)
               .IsRequired();

            entity.Property(p => p.SchemeMemberIndicator)
               .HasColumnName("SCHMEM")
               .HasMaxLength(1);

            entity.Property(p => p.UserWhoCarriedOutActivity)
              .HasColumnName("COBY")
              .HasMaxLength(15)
              .IsRequired();

            entity.Property(p => p.UserWhoAuthorisedActivity)
              .HasColumnName("AUTHBY")
              .HasMaxLength(15);

            entity.Property(p => p.AcitivityAuthorizedDate)
                .HasColumnName("AUTHD")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(p => p.AcitivityCarriedOutDate)
                .HasColumnName("COD")
                .HasConversion(MemberDbContext.DateTimeConverter)
                .IsRequired();

            entity.Property(p => p.AcitivityProcessedDate)
                .HasColumnName("PROCD")
                .HasConversion(MemberDbContext.DateTimeConverter);
        });
    }
}