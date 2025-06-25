using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class BankHolidayConfiguration
{
    public static void ConfigureBankHoliday(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankHoliday>(entity =>
        {
            entity.ToTable("BA_BANKHOLIDAY");

            entity.HasNoKey();

            entity.Property(x => x.Description)
                .HasColumnName("BABH03X")
                .HasMaxLength(80);

            entity.Property(x => x.Date)
               .HasColumnName("BABH02D")
               .IsRequired();
        });
    }
}