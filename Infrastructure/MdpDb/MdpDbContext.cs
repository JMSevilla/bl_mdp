using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.MdpDb.Configurations;

namespace WTW.MdpService.Infrastructure.MdpDb;
public class MdpDbContext : DbContext
{
    public static readonly ValueConverter<DateTimeOffset?, DateTime?> DateTimeConverter =
        new ValueConverter<DateTimeOffset?, DateTime?>(v => v.HasValue ? v.Value.UtcDateTime : null, v => v);

    public MdpDbContext(DbContextOptions<MdpDbContext> options) : base(options)
    {
    }

    public DbSet<RetirementJourney> RetirementJourneys { get; set; }
    public DbSet<Calculation> Calculations { get; set; }
    public DbSet<ContactConfirmation> ContactConfirmations { get; set; }
    public DbSet<RetirementPostIndexEvent> RetirementPostIndexEvent { get; set; }
    public DbSet<UploadedDocument> UploadedDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureRetirementJourney();
        modelBuilder.ConfigureTransferJourney();
        modelBuilder.ConfigureQuoteSelectionJourney();
        modelBuilder.ConfigureJourneyBranch();
        modelBuilder.ConfigurePostIndexEvent();
        modelBuilder.ConfigureContactConfirmation();
        modelBuilder.ConfigureCalculation();
        modelBuilder.ConfigureTransferCalculation();
        modelBuilder.ConfigureJourneyDocument();
        modelBuilder.ConfigureGenericJourney();

        base.OnModelCreating(modelBuilder);
    }
}