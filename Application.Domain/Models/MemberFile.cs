using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.MemberFile;

namespace Application.Domain.Models
{
    public class MemberFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MemberFileId { get; set; }

        public Guid MemberId { get; set; }
        public virtual Member Member { get; set; } = null!;

        public Guid SystemFileId { get; set; }
        public virtual SystemFile SystemFile { get; set; } = null!;

        public MemberFileType FileType { get; set; }
    }
}