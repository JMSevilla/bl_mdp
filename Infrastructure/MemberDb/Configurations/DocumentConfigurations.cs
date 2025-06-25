using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class DocumentConfigurations
{
    public static void ConfigureDocument(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("WW_ECOMMS_METADATA");

            entity.Property(p => p.Id)
                .HasColumnName("ID")
                .IsRequired();

            entity.Property(p => p.ReferenceNumber)
                .HasColumnName("REFNO")
                .HasMaxLength(7)
                .IsRequired();

            entity.Property(p => p.BusinessGroup)
                .HasColumnName("BGROUP")
                .HasMaxLength(3)
                .IsRequired();

            entity.HasKey("ReferenceNumber", "BusinessGroup", "Id");

            entity.Property(p => p.Type)
               .HasColumnName("DOCLABEL")
               .HasMaxLength(60)
               .IsRequired();

            entity.Property(p => p.Name)
               .HasColumnName("DOCTYPE")
               .HasMaxLength(60)
               .IsRequired();

            entity.Property(p => p.TypeId)
              .HasColumnName("DOCID")
              .HasMaxLength(20);

            entity.Property(p => p.Schema)
              .HasColumnName("SCHMEM")
              .HasMaxLength(1)
              .IsRequired();

            entity.Property(p => p.FileName)
              .HasColumnName("DOCNAME")
              .HasMaxLength(40)
              .IsRequired();

            entity.Property(p => p.Date)
              .HasColumnName("DOCTS")
              .HasConversion(MemberDbContext.DateTimeConverter)
              .IsRequired();

            entity.Property(p => p.LastReadDate)
              .HasColumnName("DOCVISITED")
              .HasConversion(MemberDbContext.DateTimeConverter);

            entity.Property(p => p.ImageId)
              .HasColumnName("IMAGEID")
              .IsRequired();
        });
    }
}