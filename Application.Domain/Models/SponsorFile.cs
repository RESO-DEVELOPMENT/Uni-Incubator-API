using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.SponsorFile;

namespace Application.Domain.Models
{
    public class SponsorFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SponsorFileId { get; set; }

        public Guid SponsorId { get; set; }
        public virtual Sponsor Sponsor { get; set; } = null!;

        public Guid SystemFileId { get; set; }
        public virtual SystemFile SystemFile { get; set; } = null!;

        public SponsorFileType FileType { get; set; }
    }
}