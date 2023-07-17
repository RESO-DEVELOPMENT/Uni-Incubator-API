using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectFile;

namespace Application.Domain.Models
{
    public class ProjectFile
    {
        public ProjectFile()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectFileId { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid SystemFileId { get; set; }
        public virtual SystemFile SystemFile { get; set; } = null!;

        public ProjectFileType FileType { get; set; }
    }
}