using System;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb.Configurations;

public static class RetirementJourneyConfigurations
{
    public static void ConfigureRetirementJourney(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RetirementJourney>(entity =>
        {
            entity.Property<long>("Id").IsRequired();
            entity.ToTable("RetirementJourney")
                .HasKey("Id");
            entity.HasAlternateKey(p => new { p.BusinessGroup, p.ReferenceNumber });
            entity.Property(x => x.CaseNumber);
            entity.Property(x => x.EnteredLtaPercentage);
            entity.Property(p => p.StartDate).IsRequired();
            entity.Property(x => x.SubmissionDate);
            entity.Property(x => x.FinancialAdviseDate);
            entity.Property(x => x.PensionWiseDate);
            entity.Property(x => x.AcknowledgePensionWise);
            entity.Property(x => x.OptOutPensionWise);
            entity.Property(x => x.AcknowledgeFinancialAdvisor);
            entity.Property(x => x.ExpirationDate).IsRequired();
            entity.Property(x => x.SummaryPdf).HasColumnType("bytea");
            entity.HasMany(p => p.JourneyBranches)
                .WithOne()
                .HasForeignKey("RetirementJourneyId")
                .OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(x => x.MemberQuote,
                onb =>
                {
                    onb.Property(x => x.SearchedRetirementDate).IsRequired();
                    onb.Property(x => x.Label).IsRequired();
                    onb.Property(x => x.AnnuityPurchaseAmount);
                    onb.Property(x => x.LumpSumFromDb);
                    onb.Property(x => x.LumpSumFromDc);
                    onb.Property(x => x.SmallPotLumpSum);
                    onb.Property(x => x.TaxFreeUfpls);
                    onb.Property(x => x.TaxableUfpls);
                    onb.Property(x => x.TotalLumpSum);
                    onb.Property(x => x.TotalPension);
                    onb.Property(x => x.TotalSpousePension);
                    onb.Property(x => x.TotalUfpls);
                    onb.Property(x => x.TransferValueOfDc);
                    onb.Property(x => x.MinimumLumpSum);
                    onb.Property(x => x.MaximumLumpSum);
                    onb.Property(x => x.TrivialCommutationLumpSum);
                    onb.Property(x => x.HasAvcs);
                    onb.Property(x => x.LtaPercentage);
                    onb.Property(x => x.EarliestRetirementAge);
                    onb.Property(x => x.NormalRetirementAge);
                    onb.Property(x => x.NormalRetirementDate).IsRequired().HasDefaultValue(DateTimeOffset.MinValue);
                    onb.Property(x => x.DatePensionableServiceCommenced);
                    onb.Property(x => x.DateOfLeaving);
                    onb.Property(x => x.TransferInService);
                    onb.Property(x => x.TotalPensionableService);
                    onb.Property(x => x.FinalPensionableSalary);
                    onb.Property(x => x.CalculationType);
                    onb.Property(x => x.WordingFlags);
                    onb.Property(x => x.StatePensionDeduction);
                    onb.Property(x => x.PensionOptionNumber);
                });

            entity.HasOne(x => x.Calculation)
                .WithOne();

            entity.Navigation(p => p.JourneyBranches).AutoInclude();
            entity.Navigation(p => p.Calculation).AutoInclude();
            entity.Navigation(p => p.MemberQuote).AutoInclude();
        });
    }
}