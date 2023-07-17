using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.SalaryCycle;

namespace Application.Domain.Models
{
    public class SalaryCycle
    {
        public SalaryCycle()
        {
            Payslips = new List<Payslip>();
            ProjectReports = new List<ProjectReport>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SalaryCycleId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public string Name { get; set; } = null!;

        public SalaryCycleStatus SalaryCycleStatus { get; set; }

        public virtual List<Payslip> Payslips { get; set; }
        public virtual List<ProjectReport> ProjectReports { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}