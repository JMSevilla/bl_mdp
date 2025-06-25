using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class EmailViewConfigurations
{
    public static void ConfigureEmailView(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailView>(entity =>
        {
            entity.ToTable("ADDRESS_DETAIL");

            entity.Property<string>("ReferenceNumber")
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property<string>("BusinessGroup")
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup");

            entity.OwnsOne(p => p.Email, p =>
            {
                p.Property(pp => pp.Address).HasMaxLength(80).HasColumnName("AR18X");
            });

            entity.Navigation(b => b.Email).IsRequired();

            entity.HasOne<Member>()
                .WithOne(d => d.EmailView)
                .HasForeignKey<EmailView>("ReferenceNumber", "BusinessGroup");
        });
    }
}