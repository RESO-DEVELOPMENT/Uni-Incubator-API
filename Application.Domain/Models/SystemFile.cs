using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.SystemFile;

namespace Application.Domain.Models
{
  public class SystemFile
  {
    public SystemFile()
    {
      ProjectFiles = new List<ProjectFile>();
      MemberFiles = new List<MemberFile>();
      SponsorFiles = new List<SponsorFile>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SystemFileId { get; set; }

    public string FileId { get; set; } = null!;
    public string DirectUrl { get; set; } = null!;
    public SystemFileType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    public DateTime? UpdatedAt { get; set; }

    public virtual List<ProjectFile>? ProjectFiles { get; set; }
    public virtual List<MemberFile>? MemberFiles { get; set; }
    public virtual List<SponsorFile>? SponsorFiles { get; set; }
    public virtual List<VoucherFile>? VoucherFiles { get; set; }
  }
}