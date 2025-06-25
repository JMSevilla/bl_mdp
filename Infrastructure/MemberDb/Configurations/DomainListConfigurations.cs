using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class DomainListConfigurations
{
    public static void ConfigureDomainList(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainList>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("DD_DOMAIN_LIST");

            entity.Property(e => e.BusinessGroup)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("BGROUP");

            entity.Property(e => e.TitleOfValidValues)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("DOL_DESCRIPTION");

            entity.Property(e => e.ListOfValidValues)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("DOL_LISTVAL");

            entity.Property(e => e.SequenceNumber)
                .HasPrecision(6)
                .HasColumnName("DOL_SEQ");

            entity.Property(e => e.Domain)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("DOMAIN");
        });
    }
}