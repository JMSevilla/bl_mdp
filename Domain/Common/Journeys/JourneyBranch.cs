using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Domain.Common.Journeys;
public class JourneyBranch
{
    private readonly List<JourneyStep> _journeySteps = new();

    protected JourneyBranch() { }

    public int SequenceNumber { get; private set; }
    public bool IsActive { get; private set; } = true;
    public virtual IReadOnlyList<JourneyStep> JourneySteps => _journeySteps;

    public static JourneyBranch Create(JourneyStep journeyStep)
    {
        var branch = new JourneyBranch();
        branch.SubmitStep(journeyStep);
        branch.SequenceNumber = 1;
        return branch;
    }

    public static JourneyBranch CreateFromBase(JourneyBranch baseBranch, string currentPageKey)
    {
        var isValid = baseBranch.ShouldCreateNewBranch(currentPageKey);

        if (isValid.IsLeft || !isValid.Right())
            throw new InvalidOperationException("Invalid branch: " +
                                                $"{nameof(ShouldCreateNewBranch)}" +
                                                $" must be called before calling {nameof(CreateFromBase)}");

        var branch = new JourneyBranch();
        foreach (var step in baseBranch.JourneySteps.OrderBy(s => s.SequenceNumber))
        {
            if (step.CurrentPageKey == currentPageKey)
                break;

            branch.SubmitStep(step.Duplicate());
        }

        return branch;
    }

    public Either<Error, bool> ShouldCreateNewBranch(string currentPageKey)
    {
        var stepToContinueFrom = JourneySteps.SingleOrDefault(x => x.NextPageKey == currentPageKey);
        if (stepToContinueFrom != null)
            return JourneySteps.Any(x => x.SequenceNumber > stepToContinueFrom.SequenceNumber);

        stepToContinueFrom = JourneySteps.SingleOrDefault(x => x.CurrentPageKey == currentPageKey);
        if (stepToContinueFrom != null)
            return JourneySteps.Any(x => x.SubmitDate == stepToContinueFrom.SubmitDate);

        return Error.New("Invalid \"currentPageKey\"");
    }

    public void SubmitStep(JourneyStep journeyStep)
    {
        if (JourneySteps.All(x => x.NextPageKey != journeyStep.NextPageKey))
            _journeySteps.Add(journeyStep);
    }

    public void UpdateNextPageKeyValue(JourneyStep currentJourneyStep, JourneyStep newStep)
    {
        currentJourneyStep.UpdateNextPageKey(newStep.NextPageKey);
    }

    public Option<JourneyStep> StepWithNextPageKeyNotSet(JourneyStep newStep)
    {
        return JourneySteps.SingleOrDefault(step => step.NextPageKey == "next_page_is_not_set" && step.CurrentPageKey == newStep.CurrentPageKey);
    }

    public Option<JourneyStep> PreviousStep(string currentPageKey)
    {
        return JourneySteps.SingleOrDefault(step => step.NextPageKey == currentPageKey);
    }

    public Option<string> PreviousStepKey(string currentPageKey)
    {
        return JourneySteps.SingleOrDefault(step => step.NextPageKey == currentPageKey)?.CurrentPageKey;
    }

    public bool Activate()
    {
        IsActive = true;
        return IsActive;
    }

    public bool Deactivate()
    {
        IsActive = false;
        return !IsActive;
    }

    public Option<QuestionForm> QuestionForm(string currentPageKey)
    {
        var questionForm = JourneySteps.SingleOrDefault(step => step.CurrentPageKey == currentPageKey)?.QuestionForm;
        if (questionForm == null &&
            LastStep().NextPageKey == currentPageKey)
        {
            return new QuestionForm();
        }

        return questionForm;
    }

    public IEnumerable<QuestionForm> QuestionForms()
    {
        return JourneySteps.Where(x => x.QuestionForm != null).Select(x => x.QuestionForm);
    }

    public JourneyStep LastStep()
    {
        var stepsByDescOrder = JourneySteps.OrderByDescending(s => s.SequenceNumber);
        var lastStep = stepsByDescOrder.First();
        if (lastStep.NextPageKey == "next_page_is_not_set")
            return JourneySteps.OrderByDescending(s => s.SequenceNumber).Skip(1).First();

        return lastStep;
    }

    public JourneyStep FirstStep()
    {
        return JourneySteps.OrderBy(s => s.SubmitDate).First();
    }

    public bool HasLifetimeAllowance()
    {
        return JourneySteps.Any(s => s.CurrentPageKey == "lta_enter_amount");
    }

    public void RemoveStepsStartingWith(string pageKey)
    {
        var step = JourneySteps.SingleOrDefault(s => s.CurrentPageKey == pageKey);

        if (step == null || !_journeySteps.Any(s => s.SequenceNumber < step.SequenceNumber))
            return;

        _journeySteps.RemoveAll(s => s.SequenceNumber >= step.SequenceNumber);
    }

    public void MarkNextPageAsDeadEnd(string pageKey)
    {
        var step = JourneySteps.SingleOrDefault(s => s.CurrentPageKey == pageKey);
        if (step != null)
            step.MarkNextPageAsDeadEnd();
    }

    public void RemoveDeadEndSteps()
    {
        var step = JourneySteps.SingleOrDefault(s => s.IsNextPageAsDeadEnd);

        if (step == null || !_journeySteps.Any(s => s.SequenceNumber < step.SequenceNumber))
            return;

        _journeySteps.RemoveAll(s => s.SequenceNumber >= step.SequenceNumber);
    }

    public void ReplaceAllStepsTo(JourneyStep step)
    {
        JourneySteps.ToList().ForEach(s => _journeySteps.Remove(s));
        step.UpdateSequenceNumber(1);
        _journeySteps.Add(step);
    }

    public void RemoveSteps(IEnumerable<JourneyStep> steps)
    {
        steps.ToList().ForEach(s => _journeySteps.Remove(s));
    }

    public void RemoveStepsExcept(JourneyStep step)
    {
        _journeySteps
            .Where(s => s != step)
            .ToList()
            .ForEach(s => _journeySteps.Remove(s));
    }

    public JourneyStep GetLastStep()
    {
        return JourneySteps.OrderBy(x => x.SequenceNumber).Last();
    }

    public void UpdateSequenceNumber(int sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }

    public Option<JourneyStep> GetStepByKey(string currentPageKey)
    {
        return JourneySteps.SingleOrDefault(s => s.CurrentPageKey == currentPageKey);
    }

    public Option<JourneyStep> GetNextStepFrom(JourneyStep journeyStep)
    {
        return JourneySteps.SingleOrDefault(s => s.CurrentPageKey == journeyStep.NextPageKey);
    }

    public Option<JourneyStep> FindStepUsingCurrentPageKeys(IEnumerable<string> currentPageKeys)
    {
        return JourneySteps.SingleOrDefault(s => currentPageKeys.Contains(s.CurrentPageKey));
    }

    public Option<JourneyStep> FindStepUsingNextPageKeys(IEnumerable<string> currentPageKeys)
    {
        return JourneySteps.SingleOrDefault(s => currentPageKeys.Contains(s.NextPageKey));
    }
}