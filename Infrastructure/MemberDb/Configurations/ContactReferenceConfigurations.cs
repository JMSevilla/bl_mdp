using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class ContactReferenceConfigurations
{
    public static void ConfigureContactReference(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContactReference>(entity =>
        {
            entity.ToTable("PS_ADDRESS");

            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(p => p.SequenceNumber)
                .HasColumnName("ADSEQNO")
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "SequenceNumber");

            entity.Property(p => p.SchemeMemberIndicator)
               .HasMaxLength(1)
               .HasColumnName("SCHMEM")
               .IsUnicode(false)
               .IsRequired();

            entity.Property(p => p.AddressCode)
                .HasMaxLength(8)
                .HasColumnName("ADDCODE")
                .IsUnicode(false)
                .IsRequired();

            entity.Property<long>("AddressNumber")
               .HasColumnName("ADDNO")
               .IsRequired();

            entity.Property<long?>("AuthorizationNumber")
               .HasColumnName("AUTHNO");


            entity.Property(p => p.Status)
               .HasColumnName("ASTATUS")
               .IsUnicode(false)
               .HasMaxLength(8);

            entity.Property(p => p.UseThisAddressForPayslips)
              .HasColumnName("PSLIP")
              .HasConversion(
                x => x.HasValue
                    ? x.Value ? "Y" : "N"
                    : null,
                y => y != null
                    ? y == "Y" ? true : false
                    : null)
              .HasMaxLength(1);

            entity.Property(p => p.StartDate)
               .HasColumnName("STARTD")
               .HasConversion(MemberDbContext.DateTimeConverter)
               .IsRequired();

            entity.Property(p => p.EndDate)
               .HasColumnName("ENDD")
               .HasConversion(MemberDbContext.DateTimeConverter);

            entity.HasOne<Member>()
                .WithMany(p => p.ContactReferences)
                .HasForeignKey("ReferenceNumber", "BusinessGroup")
                .IsRequired();

            entity.HasOne(x => x.Contact)
               .WithOne()
               .HasForeignKey<ContactReference>("BusinessGroup", "AddressNumber");

            entity.HasOne(x => x.Authorization)
               .WithOne()
               .HasForeignKey<ContactReference>("BusinessGroup", "AuthorizationNumber");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("PS_ADDRESSDETAILS");

            entity.Property(p => p.AddressNumber)
               .HasColumnName("ADDNO")
               .IsRequired();

            entity.Property(p => p.BusinessGroup)
                 .HasColumnName("BGROUP")
                 .HasMaxLength(3)
                 .IsRequired();

            entity.OwnsOne(p => p.Email, p =>
            {
                p.Property(pp => pp.Address).HasMaxLength(50).HasColumnName("EMAILADR");
            });
            entity.Navigation(b => b.Email).IsRequired();

            entity.OwnsOne(p => p.MobilePhone, p =>
            {
                p.Property(pp => pp.FullNumber)
                    .HasMaxLength(20)
                    .HasColumnName("MOBPHON1");
            });
            entity.Navigation(b => b.MobilePhone).IsRequired();

            entity.HasKey("BusinessGroup", "AddressNumber");

            entity.OwnsOne(p => p.Address, p =>
            {
                p.Property(pp => pp.StreetAddress1).HasMaxLength(50).HasColumnName("ADDRESS1");
                p.Property(pp => pp.StreetAddress2).HasMaxLength(50).HasColumnName("ADDRESS2");
                p.Property(pp => pp.StreetAddress3).HasMaxLength(50).HasColumnName("ADDRESS3");
                p.Property(pp => pp.StreetAddress4).HasMaxLength(50).HasColumnName("ADDRESS4");
                p.Property(pp => pp.StreetAddress5).HasMaxLength(50).HasColumnName("ADDRESS5");
                p.Property(pp => pp.PostCode).HasMaxLength(8).HasColumnName("POSTCODE");
                p.Property(pp => pp.Country).HasMaxLength(30).HasColumnName("COUNTRY");
                p.Property(pp => pp.CountryCode).HasMaxLength(3).HasColumnName("ISOCODE");
            });
            entity.Navigation(b => b.Address).IsRequired();

            entity.OwnsOne(p => p.Data, p =>
            {
                p.Property(pp => pp.Telephone).HasMaxLength(30).HasColumnName("TELEPHON");
                p.Property(pp => pp.Fax).HasMaxLength(30).HasColumnName("FAX");
                p.Property(pp => pp.OrganizationName).HasMaxLength(80).HasColumnName("ORGNAME");
                p.Property(pp => pp.WorkMail).HasMaxLength(50).HasColumnName("WORKMAIL");
                p.Property(pp => pp.HomeMail).HasMaxLength(50).HasColumnName("HOMEMAIL");
                p.Property(pp => pp.WorkPhone).HasMaxLength(30).HasColumnName("WORKPHON");
                p.Property(pp => pp.HomePhone).HasMaxLength(30).HasColumnName("HOMEPHON");
                p.Property(pp => pp.MobilePhone2).HasMaxLength(30).HasColumnName("MOBPHON2");
                p.Property(pp => pp.NonUkPostCode).HasMaxLength(30).HasColumnName("NONUKPOS");
            });
            entity.Navigation(b => b.Data).IsRequired();
        });
    }
}