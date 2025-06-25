using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public record MemberPersonalDetailsResponse
{
    public string Title { get; init; }
    public string Gender { get; init; }
    public string Name { get; init; }
    public string DateOfBirth { get; init; }
    public string InsuranceNumber { get; init; }

    public static MemberPersonalDetailsResponse From(PersonalDetails personalDetails, string insuranceNumber)
    {
        return new()
        {
            Title = personalDetails.Title,
            Gender = personalDetails.Gender,
            Name = (personalDetails.Forenames?.Trim() + " " + personalDetails.Surname?.Trim()).Trim(),
            InsuranceNumber = insuranceNumber,
            DateOfBirth = personalDetails.DateOfBirth.HasValue
                ? $"{personalDetails.DateOfBirth.Value.Day} " +
                  $"{personalDetails.DateOfBirth.Value.ToString("MMMM")} " +
                  $"{personalDetails.DateOfBirth.Value.Year}"
                : null
        };
    }
}