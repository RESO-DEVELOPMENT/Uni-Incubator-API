using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectMemberRequest;

namespace Application.DTOs.ProjectMemberRequest
{
  public class RequiredIfAccepted : ValidationAttribute
  {
    RequiredAttribute _innerAttribute = new RequiredAttribute();
    public string _dependentProperty { get; set; }

    public RequiredIfAccepted(string dependentProperty)
    {
      _dependentProperty = dependentProperty;
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
      var field = validationContext.ObjectType.GetProperty(_dependentProperty);
      if (field != null)
      {
        var dependentValue = field.GetValue(validationContext.ObjectInstance, null);
        var parsedDependentValue = (ProjectMemberRequestStatus?)dependentValue;

        if (dependentValue == null && parsedDependentValue == null || parsedDependentValue.Equals(ProjectMemberRequestStatus.Accepted))
        {
          if (!_innerAttribute.IsValid(value))
          {
            string name = validationContext.DisplayName;
            string specificErrorMessage = ErrorMessage!;
            if (specificErrorMessage!.Length < 1)
              specificErrorMessage = $"{name} is required.";

            return new ValidationResult(specificErrorMessage, new[] { validationContext.MemberName! });
          }
        }
        return ValidationResult.Success!;
      }
      else
      {
        return new ValidationResult(FormatErrorMessage(_dependentProperty));
      }
    }
  }

  public class ProjectMemberRequestReviewDTO
  {
    [Required]
    public Guid RequestId { get; set; }
    [Required]
    public ProjectMemberRequestStatus Status { get; set; }

    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    public bool? Graduated { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(0, 10)]
    public double? YearOfExp { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    public bool? HaveEnghlishCert { get; set; }

    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? LeadershipSkill { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? CreativitySkill { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? ProblemSolvingSkill { get; set; }

    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? PositiveAttitude { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? TeamworkSkill { get; set; }
    [RequiredIfAccepted("Status", ErrorMessage = "Please provide points if you accept the report")]
    [Range(1, 10)]
    public double? CommnicationSkill { get; set; }
  }
}