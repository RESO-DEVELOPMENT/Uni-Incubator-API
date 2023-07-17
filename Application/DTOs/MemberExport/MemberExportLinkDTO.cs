using Application.Domain.Enums.Project;

namespace Application.DTOs.MemberExport
{
    public class MemberExportLinkDTO
    {
        public string Url { get; set; } = null!;
        public string Code { get; set; } = null!;
        public Guid MemberId { get; set; }
    }
}
