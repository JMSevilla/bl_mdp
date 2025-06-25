using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class UserQueryPromptConfiguration
{
    public static void ConfigureUserQueryPrompt(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserQueryPrompt>(entity =>
        {
            entity.ToTable("WW_USER_QUERY_PROMPTS");

            entity
                .Property(c => c.ScoreNumber)
                .HasColumnName("SCRNUM")
                .IsRequired();

            entity
                .Property(c => c.BusinessGroup)
                .HasColumnName("BGROUP")
                .IsRequired();

            entity
                .Property(c => c.Status)
                .HasColumnName("STATUS")
                .HasMaxLength(2);

            entity
                .Property(c => c.Event)
                .HasColumnName("EVENT");

            entity
                .Property(c => c.CaseCode)
                .HasColumnName("CASECODE")
                .IsRequired()
                .HasMaxLength(4);

            entity.Property<string>("CATEGORY");
            entity.Property<string>("SCHEME");
            entity.Property<string>("GROUP_ID");
            entity.Property<string>("SEQNO");
            
            entity.HasKey("BusinessGroup", "CaseCode", "SCHEME", "CATEGORY", "Status", "Event", "GROUP_ID", "SEQNO");
        });
    }
}