using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class BankAccountConfigurations
{
    public static void ConfigureBankAccount(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("WW_CM_MEMBER_PAYEE_DETAIL");

            entity.Property<string>(x => x.ReferenceNumber)
                .HasColumnName("REFNO")
                .IsRequired();

            entity.Property<string>(x => x.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasColumnName("SEQNO")
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "SequenceNumber");

            entity.Property(p => p.AccountNumber)
                .HasColumnName("WWMPD06X");

            entity.Property(p => p.Iban)
               .HasColumnName("WWMPD03X");

            entity.Property(p => p.AccountName)
                .HasColumnName("WWMPD08X");

            entity.Property(c => c.EffectiveDate)
                .HasColumnName("WWMPD01D")
                .HasConversion(MemberDbContext.DateTimeConverter);

            entity.HasOne<Member>()
                .WithMany(x => x.BankAccounts)
                .HasForeignKey("ReferenceNumber", "BusinessGroup");

            entity.OwnsOne(p => p.Bank, p =>
            {
                p.Property(pp => pp.SortCode).HasColumnName("WWMPD04X");
                p.Property(pp => pp.Bic).HasColumnName("WWMPD02X");
                p.Property(pp => pp.ClearingCode).HasColumnName("WWMPD05X");
                p.Property(pp => pp.Name).HasColumnName("WWMPD15X");
                p.Property(pp => pp.City).HasColumnName("WWMPD16X");
                p.Property(pp => pp.Country).HasColumnName("WWMPD13X");
                p.Property(pp => pp.CountryCode).HasColumnName("WWMPD18X");
                p.Property(pp => pp.AccountCurrency).HasColumnName("WWMPD09X");
            });
        });
    }
}