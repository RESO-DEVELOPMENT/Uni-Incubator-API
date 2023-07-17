using Application.Domain.Enums.ProjectFile;
using Application.DTOs.SystemFile;

namespace Application.DTOs.ProjectFile
{
    public class ProjectFileDTO
    {
        public virtual SystemFileDTO File { get; set; } = null!;
        public ProjectFileType FileType { get; set; }
    }
}