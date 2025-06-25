namespace WTW.MdpService.Domain.Members;
public class CategoryDetail
{
    protected CategoryDetail() { }

    public CategoryDetail(int normalRetirementAge, int? minimumPensionAge)
    {
        NormalRetirementAge = normalRetirementAge;
        MinimumPensionAge = minimumPensionAge;
    }

    public int NormalRetirementAge { get; }
    public int? MinimumPensionAge { get; }
}