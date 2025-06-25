using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Domain.Common.Journeys;

public abstract class Journey
{
    private readonly List<JourneyBranch> _journeyBranches = new();

    protected Journey() { }

    protected Journey(string businessGroup, string referenceNumber, DateTimeOffset utcNow, string currentPageKey, string nextPageKey)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        StartDate = utcNow;
        _journeyBranches.Add(JourneyBranch.Create(JourneyStep.Create(currentPageKey, nextPageKey, utcNow)));
    }

    protected Journey(string businessGroup, string referenceNumber, DateTimeOffset utcNow, string currentPageKey, string nextPageKey, string questionKey, string answerKey)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        StartDate = utcNow;

        var questionForm = new QuestionForm(questionKey, answerKey);
        var step = JourneyStep.Create(
            currentPageKey,
            nextPageKey,
            utcNow,
            questionForm);
        _journeyBranches.Add(JourneyBranch.Create(step));
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset? SubmissionDate { get; private protected set; }
    public DateTimeOffset ExpirationDate { get; private protected set; }
    public virtual IReadOnlyList<JourneyBranch> JourneyBranches => _journeyBranches;

    public Either<Error, bool> TrySubmitStep(
        string currentPageKey,
        string nextPageKey,
        DateTimeOffset creationDate)
    {
        var step = JourneyStep.Create(
            currentPageKey,
            nextPageKey,
            creationDate);

        return SubmitStep(step);
    }

    public Either<Error, bool> TrySubmitStep(
       string currentPageKey,
       string nextPageKey,
       DateTimeOffset creationDate,
       string questionKey,
       string answerKey)
    {
        var questionForm = new QuestionForm(questionKey, answerKey);
        var step = CreateStep(
            currentPageKey,
            nextPageKey,
            creationDate,
            questionForm);

        return SubmitStep(step);
    }

    public Either<Error, bool> TrySubmitStep(
       string currentPageKey,
       string nextPageKey,
       DateTimeOffset creationDate,
       string questionKey,
       string answerKey,
       string answerValue,
       bool avoidBranching = false)
    {
        var questionForm = new QuestionForm(questionKey, answerKey, answerValue);
        var step = CreateStep(
            currentPageKey,
            nextPageKey,
            creationDate,
            questionForm);

        return SubmitStep(step, avoidBranching);
    }

    public Error? UpdateStep(string switchPageKey, string nextPageKey)
    {
        var step = JourneyBranches.ToSeq().Match(
            () => Option<JourneyStep>.None,
            eb => eb.Single(branch => branch.IsActive).PreviousStep(switchPageKey));

        if (step.IsNone)
            return Error.New("Previous step does not exist in retiremnt journey for given current page key.");

        step.Value().UpdateNextPageKey(nextPageKey);
        return null;
    }

    public Option<string> PreviousStep(string currentPageKey)
    {
        return JourneyBranches.ToSeq().Match(
            () => Option<string>.None,
            eb => eb.Single(branch => branch.IsActive).PreviousStepKey(currentPageKey));
    }

    public Option<QuestionForm> QuestionForm(string currentPageKey)
    {
        return JourneyBranches.ToSeq().Match(
            () => Option<QuestionForm>.None,
            eb => eb.Single(branch => branch.IsActive).QuestionForm(currentPageKey));
    }

    public IEnumerable<QuestionForm> QuestionForms(IEnumerable<string> questionKeys)
    {
        var answers = JourneyBranches.Single(x => x.IsActive).QuestionForms();
        return questionKeys.Any() ? answers.Where(x => questionKeys.Contains(x.QuestionKey)) : answers;
    }

    public void UpdateOldSeqNumbers()
    {
        foreach (var b in JourneyBranches)
        {
            if (b.JourneySteps.Any(s => s.SequenceNumber == 0))
            {
                var failedSteps = b.JourneySteps.Where(s => s.SequenceNumber != 0);
                if (failedSteps.Any())
                    b.RemoveSteps(failedSteps);

                var iterator = 1;
                foreach (var step in b.JourneySteps.OrderBy(ss => ss.SubmitDate))
                {
                    step.UpdateSequenceNumber(iterator);
                    iterator++;
                }
            }
        }
    }

    public string GetRedirectStepPageKey(string pageKey)
    {
        return JourneyBranches
            .Single(x => x.IsActive).JourneySteps
            .SingleOrDefault(x => x.NextPageKey == pageKey)
            .ToOption()
            .Match(
                step => pageKey,
                () =>
                {
                    var activeBranch = _journeyBranches.Single(x => x.IsActive);
                    if (activeBranch.FirstStep().CurrentPageKey == pageKey)
                        return pageKey;

                    return activeBranch.LastStep().NextPageKey;
                });
    }

    public void RemoveStepsStartingWith(string pageKey)
    {
        ActiveBranch().RemoveStepsStartingWith(pageKey);
    }

    public void MarkNextPageAsDeadEnd(string currentPageKey)
    {
        ActiveBranch().MarkNextPageAsDeadEnd(currentPageKey);
    }

    public void RemoveDeadEndSteps()
    {
        JourneyBranches
            .ToList()
            .ForEach(b => b.RemoveDeadEndSteps());
    }

    public void ReplaceAllStepsTo(JourneyStep journeyStep)
    {
        ActiveBranch().ReplaceAllStepsTo(journeyStep);
    }

    public void RemoveInactiveBranches()
    {
        JourneyBranches
            .Where(b => !b.IsActive)
            .ToList()
            .ForEach(x => _journeyBranches.Remove(x));
    }

    public Option<JourneyStep> GetStepByKey(string currentPageKey)
    {
        return ActiveBranch().GetStepByKey(currentPageKey);
    }

    public Option<JourneyStep> GetNextStepFrom(JourneyStep journeyStep)
    {
        return ActiveBranch().GetNextStepFrom(journeyStep);
    }

    public Option<JourneyStep> FindStepFromCurrentPageKeys(IEnumerable<string> currentPageKeys)
    {
        return ActiveBranch().FindStepUsingCurrentPageKeys(currentPageKeys);
    }

    public Option<JourneyStep> FindStepFromNextPageKeys(IEnumerable<string> nextPageKeys)
    {
        return ActiveBranch().FindStepUsingNextPageKeys(nextPageKeys);
    }

    public IEnumerable<CheckboxesList> CheckBoxesLists()
    {
        return ActiveBranch().JourneySteps.SelectMany(x => x.CheckboxesLists);
    }

    public IEnumerable<JourneyGenericData> GetJourneyGenericDataList()
    {
        return ActiveBranch().JourneySteps.Where(x => x.JourneyGenericDataList.Any()).SelectMany(x => x.JourneyGenericDataList);
    }

    public IEnumerable<JourneyStep> GetJourneyStepsWithGenericData()
    {
        return ActiveBranch().JourneySteps.Where(x => x.JourneyGenericDataList.Any());
    }

    public IEnumerable<JourneyStep> GetStepsWithQuestionForms()
    {
        return ActiveBranch().JourneySteps
            .Where(x => x.QuestionForm != null)
            .ToList();
    }

    public IEnumerable<string> GetQuestionFormsWordingFlags()
    {
        return ActiveBranch().JourneySteps
            .Where(x => x.QuestionForm != null)
            .Select(q => $"{q.QuestionForm.QuestionKey}-{q.QuestionForm.AnswerKey}")
            .ToList();
    }

    public IEnumerable<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)> GetStepsWithCheckboxLists()
    {
        return ActiveBranch().JourneySteps
            .Where(s => s.CheckboxesLists.Any())
            .Select(s => (s.CurrentPageKey, s.CheckboxesLists.ToDictionary(l => l.CheckboxesListKey, l => l.Checkboxes.ToList())));
    }

    public JourneyStep GetFirstStep()
    {
        return JourneyBranches.Single(x => x.IsActive).JourneySteps.OrderBy(x => x.SequenceNumber).First();
    }

    private JourneyBranch ActiveBranch() => JourneyBranches.Single(b => b.IsActive);

    private JourneyStep CreateStep(
       string currentPageKey,
       string nextPageKey,
       DateTimeOffset creationDate,
       QuestionForm questionForm)
    {
        return JourneyStep.Create(
             currentPageKey,
             nextPageKey,
             creationDate,
             questionForm);
    }

    private Either<Error, bool> SubmitStep(JourneyStep step, bool avoidBranching = false)
    {
        return JourneyBranches
            .FindSeq(branch =>
                branch.JourneySteps.Any(s => AreStepsEquivalent(step, s, avoidBranching))
                && branch.IsActive)
            .ToOption()
            .Match(
                branch => branch,
                () => SubmitToValidBranch(step))
            .Match<Either<Error, bool>>(branch =>
            {
                _journeyBranches.ForEach(b => b.Deactivate());
                if (TryMergeBranches(step, branch))
                    _journeyBranches.RemoveAll(b => b != branch);
                return branch.Activate();
            },
            error => error);
    }

    private bool TryMergeBranches(JourneyStep step, JourneyBranch activeBranch)
    {
        if (activeBranch.JourneySteps.All(s => s.CurrentPageKey != step.NextPageKey))
            foreach (var branch in _journeyBranches.Where(b => b != activeBranch).OrderByDescending(b => b.SequenceNumber))
            {
                var nextStep = branch.JourneySteps.FirstOrDefault(s => s.CurrentPageKey == step.NextPageKey);
                if (nextStep != null)
                {
                    var stepsToMerge = branch.JourneySteps.Where(s => s.SequenceNumber >= nextStep.SequenceNumber).Select(s => s.Duplicate());
                    var nextSequenceNumber = activeBranch.JourneySteps.Max(b => b.SequenceNumber) + 1;
                    stepsToMerge.OrderBy(s => s.SequenceNumber).ToList().ForEach(s =>
                    {
                        s.UpdateSequenceNumber(nextSequenceNumber);
                        activeBranch.SubmitStep(s);
                        nextSequenceNumber++;
                    });

                    return true;
                }
            }
        return false;
    }

    private Either<Error, JourneyBranch> SubmitToValidBranch(JourneyStep newStep)
    {
        return JourneyBranches
            .ToSeq()
            .Match<Either<Error, JourneyBranch>>(
                () =>
                {
                    var newBranch = JourneyBranch.Create(newStep);
                    _journeyBranches.Add(newBranch);
                    return newBranch;
                },
                eb =>
                {
                    var lastStepBeforeIncomingStepInActiveBranch = _journeyBranches
                        .Single(x => x.IsActive).JourneySteps
                        .SingleOrDefault(st => st.NextPageKey == newStep.CurrentPageKey);

                    if (lastStepBeforeIncomingStepInActiveBranch == null)
                        return Error.New("Invalid \"currentPageKey\"");

                    var lastStepNextPageKeyNotSet = _journeyBranches
                        .Single(x => x.IsActive)
                        .StepWithNextPageKeyNotSet(newStep);

                    var matchingInactiveBranch = AnyMatchWithinInactiveBranches(lastStepBeforeIncomingStepInActiveBranch, newStep);
                    if (matchingInactiveBranch.IsSome)
                        return matchingInactiveBranch.Value();

                    var branchToReturn = eb.Single(x => x.IsActive);
                    var result = branchToReturn.ShouldCreateNewBranch(newStep.CurrentPageKey);

                    if (result.IsLeft)
                        return result.Left();

                    if (result.Right() && lastStepNextPageKeyNotSet.IsNone)
                    {
                        branchToReturn = JourneyBranch.CreateFromBase(branchToReturn, newStep.CurrentPageKey);
                        branchToReturn.UpdateSequenceNumber(_journeyBranches.Max(s => s.SequenceNumber) + 1);
                        _journeyBranches.Add(branchToReturn);
                    }

                    if (lastStepNextPageKeyNotSet.IsSome)
                    {
                        branchToReturn.UpdateNextPageKeyValue(lastStepNextPageKeyNotSet.Value(), newStep);
                        return branchToReturn;
                    }

                    var sequenceNumber = branchToReturn.JourneySteps.Max(s => s.SequenceNumber) + 1;
                    newStep.UpdateSequenceNumber(sequenceNumber);
                    branchToReturn.SubmitStep(newStep);
                    return branchToReturn;
                });
    }

    private Option<JourneyBranch> AnyMatchWithinInactiveBranches(JourneyStep lastStepBeforeIncomingStepInActiveBranch, JourneyStep newStep)
    {
        var validStepsForMatching = JourneyBranches
            .Single(x => x.IsActive).JourneySteps
            .Where(s => s.SequenceNumber <= lastStepBeforeIncomingStepInActiveBranch.SequenceNumber)
            .ToList();
        validStepsForMatching.Add(newStep);

        foreach (var inactiveBranch in _journeyBranches.Where(b => !b.IsActive))
        {
            var isMatch = false;
            var inactiveBranchSteps = inactiveBranch.JourneySteps.OrderBy(x => x.SequenceNumber).ToList();
            var iterator = 0;
            foreach (var step in validStepsForMatching.OrderBy(x => x.SequenceNumber))
            {
                if (inactiveBranchSteps.Count < validStepsForMatching.Count || !AreStepsEquivalent(step, inactiveBranchSteps[iterator]))
                    break;

                if (iterator + 1 == validStepsForMatching.Count)
                    isMatch = true;

                iterator++;
            }

            if (isMatch)
                return inactiveBranch;
        }

        return Option<JourneyBranch>.None;
    }

    private static bool AreStepsEquivalent(JourneyStep newStep, JourneyStep existingStep, bool avoidBranching = false)
    {
        if (avoidBranching && existingStep.CurrentPageKey == newStep.CurrentPageKey && existingStep.NextPageKey == newStep.NextPageKey)
        {
            existingStep?.QuestionForm.Update(newStep.QuestionForm);
            return true;
        }

        return existingStep.CurrentPageKey == newStep.CurrentPageKey &&
            existingStep.NextPageKey == newStep.NextPageKey &&
            newStep.QuestionForm?.AnswerKey == existingStep.QuestionForm?.AnswerKey &&
            newStep.QuestionForm?.QuestionKey == existingStep.QuestionForm?.QuestionKey;
    }
}