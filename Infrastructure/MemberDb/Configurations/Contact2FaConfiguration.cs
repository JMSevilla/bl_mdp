using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations
{
    public static class Contact2FaConfiguration
    {
        public static void ConfigureContact2Fa(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactValidation>(entity =>
            {
                entity.ToTable("WW_2FA_CONTACT_VALIDATION");

                entity.Property(x => x.BusinessGroup)
                     .HasColumnName("BGROUP")
                     .HasMaxLength(3)
                     .IsRequired();

                entity.Property(x => x.ReferenceNumber)
                    .HasColumnName("REFNO")
                    .HasMaxLength(7)
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .HasColumnName("USERID")
                    .HasMaxLength(20);

                entity.Property(x => x.Token)
                    .HasColumnName("VALIDATION_TOKEN")
                    .HasMaxLength(6);

                entity.Property(x => x.ContactValid)
                    .HasColumnName("CONTACT_VALID")
                    .HasMaxLength(1);

                entity.Property(p => p.ContactType)
                    .HasColumnName("CONTACT_TYPE")
                    .HasConversion(
                         x => MapContactType(x),
                         x => MapContactTypeToEnum(x))
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(p => p.ContactValidatedAt)
                  .HasColumnName("CONTACT_VALIDATED_DATE")
                  .HasConversion(MemberDbContext.DateTimeConverter);

                entity.Property(p => p.AddressNumber)
                   .HasColumnName("ADDNO")
                   .HasMaxLength(20);

                entity.Property(p => p.ContactPhoneType)
                   .HasColumnName("CONTACT_PHONE_TYPE")
                   .HasMaxLength(15);

                entity.HasOne<Member>()
                .WithMany(x => x.ContactValidations)
                .HasForeignKey("ReferenceNumber", "BusinessGroup");

                entity.HasKey("ReferenceNumber", "BusinessGroup", "ContactType");
            });

            modelBuilder.Entity<ContactCountry>(entity =>
            {
                entity.ToTable("WW_2FA_CONTACT_COUNTRY");

                entity.Property(x => x.BusinessGroup)
                     .HasColumnName("BGROUP")
                     .HasMaxLength(3)
                     .IsRequired();

                entity.Property(x => x.ReferenceNumber)
                    .HasColumnName("REFNO")
                    .HasMaxLength(7)
                    .IsRequired();

                entity.Property(p => p.AddressCode)
                   .HasColumnName("ADDCODE")
                   .HasMaxLength(15)
                   .IsRequired();

                entity.Property(p => p.PhoneType)
                   .HasColumnName("PHONETYPE")
                   .HasMaxLength(15)
                   .IsRequired();

                entity.Property(p => p.AddressNumber)
                   .HasColumnName("ADDNO")
                   .HasMaxLength(20)
                   .IsRequired();

                entity.Property(p => p.Country)
                   .HasColumnName("COUNTRY")
                   .HasMaxLength(15);

                entity.HasKey("ReferenceNumber", "BusinessGroup", "AddressNumber", "AddressCode", "PhoneType");

                entity.HasOne<Member>()
                .WithOne(x => x.ContactCountry)
                .HasForeignKey<ContactCountry>("ReferenceNumber", "BusinessGroup");
            });
        }

        private static MemberContactType MapContactTypeToEnum(string contactType)
        {
            return contactType switch
            {
                "EMAILADR" => MemberContactType.EmailAddress,
                "MOBPHON1" => MemberContactType.MobilePhoneNumber1,
                 _ => MemberContactType.Other
            };
        }

        private static string MapContactType(MemberContactType contactType)
        {
            return contactType switch
            {
                MemberContactType.EmailAddress => "EMAILADR",
                MemberContactType.MobilePhoneNumber1 => "MOBPHON1"
            };
        }
    }
}
