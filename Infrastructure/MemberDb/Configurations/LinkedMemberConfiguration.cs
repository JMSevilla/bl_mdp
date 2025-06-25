using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class LinkedMemberConfiguration
{
    public static void ConfigureLinkedMembers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LinkedMember>(entity =>
        {
            entity.ToTable("MP_LINKMEMBER");
            entity
                .Property(c => c.LinkedBusinessGroup)
                .HasColumnName("MPLM02X");
            entity
                .Property(c => c.LinkedReferenceNumber)
                .HasColumnName("MPLM03X");
            entity
                .Property(c => c.ReferenceNumber)
                .HasColumnName("REFNO");
            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP");
            entity.HasKey("ReferenceNumber", "BusinessGroup");
        });
    }
}