using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Domain.Common.Journeys;

public class GenericJourney : Journey
{
    protected GenericJourney() { }

    public GenericJourney(string businessGroup,
        string referenceNumber,
        string type,
        string currentPageKey,
        string nextPageKey,
        bool isMarkedForRemoval,
        string status,
        DateTimeOffset utcNow,
        DateTimeOffset? expirationDate = null)
        : base(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey)
    {
        Type = type;
        Status = status ?? "Started";
        IsMarkedForRemoval = isMarkedForRemoval;
        ExpirationDate = expirationDate;
    }

    public string Type { get; }
    public string Status { get; private set; }
    public string WordingFlags { get; private set; }
    public bool IsMarkedForRemoval { get; }
    public new DateTimeOffset? ExpirationDate { get; }

    public Either<Error, bool> TrySubmitStep(
        string currentPageKey,
        string nextPageKey,
        string status,
        DateTimeOffset creationDate)
    {
        if (!string.IsNullOrWhiteSpace(status))
            Status = status;

        return TrySubmitStep(currentPageKey, nextPageKey, creationDate);
    }

    public JourneyGenericData GetGenericDataByFormKey(string formKey)
    {
        return GetJourneyGenericDataList()
            .Where(x => x.FormKey == formKey)
            .FirstOrDefault();
    }

    public void SubmitJourney(DateTimeOffset utcNow)
    {
        SubmissionDate = utcNow;
        Status = "Submitted";
    }

    public IEnumerable<GenericJourneyStageStatus> GetStageStatus(IEnumerable<GenericJourneyStage> stages)
    {
        return stages.Select(stage => FindStepFromCurrentPageKeys(stage.Page.StageStartSteps)
              .Match<GenericJourneyStageStatus>(stageStartStep =>
              {
                  DateTime? stageCompletionDate = (stageStartStep.UpdateDate ?? stageStartStep.SubmitDate).DateTime;
                  Option<JourneyStep> nextStep = stageStartStep;

                  do
                  {
                      if (nextStep.Value().SubmitDate.DateTime > stageCompletionDate)
                          stageCompletionDate = (nextStep.Value().UpdateDate ?? nextStep.Value().SubmitDate).DateTime;

                      if (stage.Page.StageEndSteps.Contains(nextStep.Value().NextPageKey))
                          return new GenericJourneyStageStatus(stage.Stage, stageCompletionDate, false, stageStartStep.CurrentPageKey);

                      nextStep = GetNextStepFrom(nextStep.Value());

                  } while (nextStep.IsSome);

                  return new GenericJourneyStageStatus(stage.Stage, null, true);
              },
              () =>
              {
                  var isStepInEndSteps = FindStepFromNextPageKeys(stage.Page.StageStartSteps);
                  if (isStepInEndSteps.IsSome)
                      return new GenericJourneyStageStatus(stage.Stage, null, true);
                  return new GenericJourneyStageStatus(stage.Stage, null, false);
              })).ToList();
    }

    public void SetExpiredStatus()
    {
        Status = "expired";
    }

    public bool IsExpired(DateTimeOffset utcNow)
    {
        return ExpirationDate <= utcNow;
    }

    public IEnumerable<string> GetWordingFlags()
    {
        if (string.IsNullOrWhiteSpace(WordingFlags))
            return Enumerable.Empty<string>();

        return WordingFlags.Split(';').Where(x => !string.IsNullOrWhiteSpace(x));
    }

    public void SetWordingFlags(IEnumerable<string> wordingFlags)
    {
        if (wordingFlags != null && wordingFlags.Any())
            WordingFlags = string.Join(';', wordingFlags);
    }

    public void RenewStepUpdatedDate(string currentPageKey, string nextPageKey, DateTimeOffset utcNow)
    {
        JourneyBranches
            .Single(x => x.IsActive)
            .JourneySteps.SingleOrDefault(x => x.CurrentPageKey == currentPageKey && x.NextPageKey == nextPageKey)
            .ToOption()
            .IfSome(x => x.RenewUpdateDate(utcNow));
    }
}