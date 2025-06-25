using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class MemberConfigurations
{
    public static void ConfigureMember(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("BASIC");
            entity
                .Property(c => c.ReferenceNumber)
                .HasColumnName("REFNO")
                .IsRequired();
            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();
            entity.HasKey(x => new { x.ReferenceNumber, x.BusinessGroup });
            entity
                .Property(c => c.SchemeCode)
                .HasColumnName("SD01X")
                .IsRequired();
            entity
                .Property(c => c.InsuranceNumber)
                .HasColumnName("BD08X");
            entity
                .Property(c => c.Status)
                .HasColumnName("BD19A")
                .IsUnicode(false)
                .HasConversion(
                    // todo: we don't have all the cases to map them back
                    _ => string.Empty,
                    x => MapMemberStatus(x))
                .IsRequired();
            entity
                .Property(c => c.MembershipNumber)
                .HasColumnName("BD03X");
            entity
                .Property(c => c.PayrollNumber)
                .HasColumnName("BD04X");
            entity
                .Property(c => c.EmployerCode)
                .HasColumnName("EM01X");
            entity
                .Property(c => c.LocationCode)
                .HasColumnName("LO01X");
            entity
                .Property(c => c.DateJoinedScheme)
                .HasColumnName("BD15D")
                .IsRequired()
                .HasConversion(MemberDbContext.DateTimeConverter);
            entity
                .Property(c => c.DateJoinedCompany)
                .HasColumnName("BD14D")
                .HasConversion(MemberDbContext.DateTimeConverter);
            entity
               .Property(c => c.DateLeftScheme)
               .HasColumnName("BD18D")
               .HasConversion(MemberDbContext.DateTimeConverter);
            entity
                .Property(c => c.DatePensionableServiceStarted)
                .HasColumnName("BD22D")
                .HasConversion(MemberDbContext.DateTimeConverter);
            entity
                .Property(c => c.StatusCode)
                .HasColumnName("BD19A")
                .IsRequired();
            entity
                .Property(c => c.ComplaintInticator)
                .HasColumnName("WWBD87X")
                .HasMaxLength(1);
            entity
                .Property(c => c.Category)
                .HasColumnName("CA26X")
                .IsRequired();
            entity
                .Property(c => c.RecordsIndicator)
                .HasColumnName("INDICAT");
            entity
                .Property(c => c.MaritalStatus)
                .HasColumnName("BD10A");
            entity.HasOne(d => d.CategoryDetail)
                .WithMany()
                .HasForeignKey(x => new { x.Category, x.BusinessGroup });
            entity.HasMany(d => d.PaperRetirementApplications)
                .WithOne()
                .HasForeignKey("ReferenceNumber", "BusinessGroup");
            entity.HasOne(d => d.Scheme)
                .WithMany()
                .HasForeignKey(x => new { x.SchemeCode, x.BusinessGroup });
            entity.HasMany(d => d.BankAccounts)
                .WithOne()
                .HasForeignKey("ReferenceNumber", "BusinessGroup");
            entity.HasMany(d => d.Beneficiaries)
                .WithOne()
                .HasForeignKey("ReferenceNumber", "BusinessGroup");
            entity.OwnsOne(p => p.PersonalDetails, p =>
            {
                p.Property(pp => pp.Title).HasColumnName("BD07A").HasMaxLength(10);
                p.Property(pp => pp.Gender).HasColumnName("BD09A").HasMaxLength(1);
                p.Property(pp => pp.Forenames).HasColumnName("BD05A").HasMaxLength(32);
                p.Property(pp => pp.Surname).HasColumnName("BD02A").HasMaxLength(20);
                p.Property(pp => pp.DateOfBirth).HasColumnName("BD11D").HasConversion(MemberDbContext.DateTimeConverter);
            });
            entity.Navigation(b => b.PersonalDetails).IsRequired();

            entity.HasMany(d => d.ContactReferences)
               .WithOne();

            entity.HasMany(d => d.EpaEmails)
              .WithOne();

            entity.HasMany(d => d.NotificationSettings)
               .WithOne();

            entity.HasOne(d => d.EmailView)
                .WithOne();

            entity.HasMany(m => m.LinkedMembers)
                .WithOne()
                .HasForeignKey("ReferenceNumber", "BusinessGroup");
        });
    }

    private static MemberStatus MapMemberStatus(string dbStatus)
    {
        return dbStatus switch
        {
            "PJ" => MemberStatus.ReadyToEnroll,
            "AC" => MemberStatus.Active,
            "PP" => MemberStatus.Deferred,
            "PN" => MemberStatus.Pensioner,
            "CB" or "WB" or "DB" => MemberStatus.Dependent,
            _ => MemberStatus.Undefined
        };
    }
}