using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MemberDb.Configurations;

public static class ContactCentreRuleConfigurations
{
    public static void ConfigureContactCentreRule(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContactCentreRule>(entity =>
        {
            entity.HasKey(e => new { e.BusinessGroup, e.Scheme, e.MemberStatus })
                .HasName("CONTACT_CENTRE_RULES_PK");

            entity.ToTable("CONTACT_CENTRE_RULES");

            entity.Property(e => e.BusinessGroup)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("BGROUP");

            entity.Property(e => e.Scheme)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("SCHEME");

            entity.Property(e => e.MemberStatus)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("MEMBER_STATUS");

            entity.Property(e => e.Ddi)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DDI");

            entity.Property(e => e.EmergencyRedirectPage)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("EMERGENCYREDIRECTPAGE");

            entity.Property(e => e.HolidayRedirectPage)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("HOLIDAYREDIRECTPAGE");

            entity.Property(e => e.RedirectPage)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("OOHREDIRECTPAGE");

            entity.Property(e => e.RequestSurvey)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("REQUESTSURVEY");

            entity.Property(e => e.RequestTranscript)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("REQUESTTRANSCRIPT");

            entity.Property(e => e.UserIdList)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("USERIDLIST");

            entity.Property(e => e.WebChatUrl)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("WC_URL");

            entity.Property(e => e.WebchatFlag)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("WEBCHAT");
        });
    }
}