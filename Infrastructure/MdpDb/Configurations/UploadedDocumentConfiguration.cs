using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class UploadedDocumentConfiguration
{
    public static void ConfigureJourneyDocument(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UploadedDocument>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("UploadedDocument").HasKey("Id");
            entity
                .Property(c => c.ReferenceNumber)
                .HasColumnName("REFNO")
                .IsRequired();
            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();
            entity.Property(p => p.Uuid)
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(p => p.Tags)
                .HasMaxLength(1000);
            entity.Property(p => p.FileName)
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(p => p.DocumentSource)
                .HasConversion(
                    v => v.ToString(),
                    v => (DocumentSource)Enum.Parse(typeof(DocumentSource), v))
               .HasMaxLength(100);
            entity.Property(p => p.IsEpaOnly);
            entity.Property(p => p.IsEdoc);
            entity.Property(p => p.JourneyType)
            .HasColumnName("Type")
            .IsRequired();
            entity.Property(p => p.DocumentType)
                .IsRequired(false);
        });
    }
}