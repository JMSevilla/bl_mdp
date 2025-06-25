using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class ContactConfirmationConfigurations
{
    public static void ConfigureContactConfirmation(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContactConfirmation>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("ContactConfirmation").HasKey("Id");

            entity.Property(p => p.BusinessGroup)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.Token)
                .HasMaxLength(6)
                .IsRequired();

            entity.Property(p => p.Contact)
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property(p => p.FailedConfirmationAttemptCount)
                .HasDefaultValue(0)
                .IsRequired();
            
            entity.Property(p => p.MaximumConfirmationAttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(p => p.ContactType)
                .HasConversion(
                    v => v.ToString(),
                    v => (ContactType)Enum.Parse(typeof(ContactType), v))
                .HasMaxLength(25)
                .IsRequired();

            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.ExpiresAt).IsRequired();
        });
    }
}