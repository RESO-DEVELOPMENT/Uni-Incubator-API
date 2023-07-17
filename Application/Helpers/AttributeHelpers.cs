using System.ComponentModel.DataAnnotations;
using Application.Domain;

namespace Application.Helpers
{
    public class AttributeHelpers
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class ValidDateAttribute : ValidationAttribute
        {
            public ValidDateAttribute()
            {

            }


            protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
            {
                DateTime date = (DateTime)value!;
                if (date < DateTimeHelper.Now())
                {
                    return new ValidationResult($"Date must be larger than current date! [Expect : > {DateTimeHelper.Now()}, Actual : {date}");
                }

                return ValidationResult.Success!;
            }
        }
    }
}