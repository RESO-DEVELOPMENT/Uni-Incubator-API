using Application.Domain.Enums.Sponsor;

namespace Application.DTOs.Sponsor
{
    public class SponsorDTO
    {
        public Guid SponsorId { get; set; }
        public string SponsorName { get; set; } = null!;
        public string SponsorDescription { get; set; } = null!;
        public string? ImageUrl { get; set; } = null!;
        public SponsorStatus SponsorStatus { get; set; }
        public SponsorType Type { get; set; }
    }
}