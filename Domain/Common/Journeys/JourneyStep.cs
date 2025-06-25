using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace WTW.MdpService.Domain.Common.Journeys;

public class JourneyStep
{
    protected JourneyStep() { }

    private readonly List<CheckboxesList> _checkboxesLists = new();
    private readonly List<JourneyGenericData> _journeyGenericDataList = new();

    private JourneyStep(string currentPageKey, string nextPageKey, int sequenceNumber, DateTimeOffset submitDate, QuestionForm questionForm = null)
    {
        CurrentPageKey = currentPageKey;
        NextPageKey = nextPageKey;
        SequenceNumber = sequenceNumber;
        SubmitDate = submitDate;
        UpdateDate = submitDate;
        QuestionForm = questionForm;
    }

    private JourneyStep(
        string currentPageKey,
        string nextPageKey,
        int sequenceNumber,
        DateTimeOffset submitDate,
        DateTimeOffset? updateDate,
        List<JourneyGenericData> journeyGenericData,
        List<CheckboxesList> checkboxesLists,
        QuestionForm questionForm)
    {
        CurrentPageKey = currentPageKey;
        NextPageKey = nextPageKey;
        SequenceNumber = sequenceNumber;
        SubmitDate = submitDate;
        UpdateDate = updateDate;
        QuestionForm = questionForm;
        _journeyGenericDataList = journeyGenericData;
        _checkboxesLists = checkboxesLists;
    }

    public int SequenceNumber { get; private set; }
    public string CurrentPageKey { get; private set; }
    public string NextPageKey { get; private set; }
    public DateTimeOffset SubmitDate { get; private set; }
    public DateTimeOffset? UpdateDate { get; private set; }
    public bool IsNextPageAsDeadEnd { get; private set; }
    public virtual QuestionForm QuestionForm { get; }
    public virtual IReadOnlyList<JourneyGenericData> JourneyGenericDataList => _journeyGenericDataList;
    public virtual IReadOnlyList<CheckboxesList> CheckboxesLists => _checkboxesLists;

    public static JourneyStep Create(string currentPageKey, string nextPageKey, DateTimeOffset submitDate, QuestionForm questionForm)
    {
        return new(currentPageKey, nextPageKey, 1, submitDate, questionForm);
    }

    public static JourneyStep Create(string currentPageKey, string nextPageKey, DateTimeOffset submitDate)
    {
        return new(currentPageKey, nextPageKey, 1, submitDate);
    }

    public JourneyStep Duplicate()
    {
        return new JourneyStep(CurrentPageKey,
            NextPageKey,
            SequenceNumber,
            SubmitDate,
            UpdateDate,
            JourneyGenericDataList?.Select(x => x.Duplicate()).ToList(),
            CheckboxesLists?.Select(x => x.Duplicate()).ToList(),
            QuestionForm?.Duplicate());
    }

    public void UpdateNextPageKey(string key)
    {
        NextPageKey = key;
    }

    public void UpdateSequenceNumber(int sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
    }

    public void UpdateCurrentPageKey(string key)
    {
        CurrentPageKey = key;
    }

    public void MarkNextPageAsDeadEnd()
    {
        IsNextPageAsDeadEnd = true;
    }

    public void AddCheckboxesList(CheckboxesList checkboxesList)
    {
        var existingCheckboxesList = CheckboxesLists.FirstOrDefault(x => x.CheckboxesListKey == checkboxesList.CheckboxesListKey);
        if (existingCheckboxesList != null)
            _checkboxesLists.Remove(existingCheckboxesList);

        _checkboxesLists.Add(checkboxesList);
    }

    public Option<CheckboxesList> GetCheckboxesListByKey(string checkboxesListKey)
    {
        return CheckboxesLists.SingleOrDefault(x => x.CheckboxesListKey == checkboxesListKey);
    }

    public void UpdateGenericData(string formKey, string dataJson)
    {
        var existingGenericData = JourneyGenericDataList.FirstOrDefault(x => x.FormKey == formKey);

        if (existingGenericData != null)
            _journeyGenericDataList.Remove(existingGenericData);

        _journeyGenericDataList.Add(new JourneyGenericData(dataJson, formKey));
    }

    public Option<JourneyGenericData> GetGenericDataByKey(string formKey)
    {
        return JourneyGenericDataList.SingleOrDefault(x => x.FormKey == formKey);
    }

    public void AppendGenericDataList(IEnumerable<JourneyGenericData> dataList)
    {
        if (dataList == null)
            return;

        _journeyGenericDataList.AddRange(dataList);
    }

    public void RenewUpdateDate(DateTimeOffset updateDate)
    {
        UpdateDate = updateDate;
    }
}