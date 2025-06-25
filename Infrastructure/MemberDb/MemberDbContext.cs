using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Domain.Members.Beneficiaries;
using WTW.MdpService.Infrastructure.MemberDb.Configurations;

namespace WTW.MdpService.Infrastructure.MemberDb;
public class MemberDbContext : DbContext
{
    public static readonly ValueConverter<DateTimeOffset?, DateTime?> DateTimeConverter =
        new ValueConverter<DateTimeOffset?, DateTime?>(v => v.HasValue ? v.Value.UtcDateTime : null, v => v);

    public MemberDbContext(DbContextOptions<MemberDbContext> options) : base(options)
    { }

    public DbSet<Member> Members { get; set; }
    public DbSet<ObjectStatus> ObjectStatuses { get; set; }
    public DbSet<UserQueryPrompt> UserQueryPrompts { get; set; }
    public DbSet<EventRti> EventRtis { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<IdvHeader> IdvHeaders { get; set; }
    public DbSet<IdvDetail> IdvDetails { get; set; }
    public DbSet<PaperRetirementApplication> Cases { get; set; }
    public DbSet<PensionWise> PensionWises { get; set; }
    public DbSet<IfaReferral> IfaReferrals { get; set; }
    public DbSet<IfaReferralHistory> IfaReferralHistories { get; set; }
    public DbSet<Beneficiary> Beneficiaries { get; set; }
    public DbSet<CalculationHistory> CalculationHistories { get; set; }
    public DbSet<IfaConfiguration> IfaConfigurations { get; set; }
    public DbSet<DomainList> DomainLists { get; set; }
    public DbSet<ContactCentreRule> ContactCentreRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureMember();
        modelBuilder.ConfigurePaperRetirementApplication();
        modelBuilder.ConfigureScheme();
        modelBuilder.ConfigureCategoryDetail();
        modelBuilder.ConfigureBankAccount();
        modelBuilder.ConfigureContactReference();
        modelBuilder.ConfigureContact2Fa();
        modelBuilder.ConfigureAuthorization();
        modelBuilder.ConfigureObjectStatus();
        modelBuilder.ConfigureUserQueryPrompt();
        modelBuilder.ConfigureEventRti();
        modelBuilder.ConfigureIdvHeader();
        modelBuilder.ConfigureIdvDetail();
        modelBuilder.ConfigureDocument();
        modelBuilder.ConfigureBatchCreateDetails();
        modelBuilder.ConfigureNotificationSetting();
        modelBuilder.ConfigureEpaEmail();
        modelBuilder.ConfigureEmailView();
        modelBuilder.ConfigureRetirementTimelines();
        modelBuilder.ConfigureBankHoliday();
        modelBuilder.ConfigureLinkedMembers();
        modelBuilder.ConfigurePensionWise();
        modelBuilder.ConfigureIfaReferral();
        modelBuilder.ConfigureIfaReferralHistory();
        modelBuilder.ConfigureBeneficiaries();
        modelBuilder.ConfigureSdDomainList();
        modelBuilder.ConfigureDependants();
        modelBuilder.ConfigureCalculationHistory();
        modelBuilder.ConfigureDomainList();
        modelBuilder.ConfigureContactCentreRule();

        ConfigureStringPropertiesAsNonUnicode(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureStringPropertiesAsNonUnicode(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                    property.SetIsUnicode(false);
            }
        }
    }
}