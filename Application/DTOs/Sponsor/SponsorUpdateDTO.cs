using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Sponsor;

namespace Application.DTOs.Sponsor
{
    public class SponsorUpdateDTO
    {
        [Required]
        public Guid SponsorId { get; set; }
        public string? SponsorName { get; set; }
        public string? SponsorDescription { get; set; }
        public string? ImageAsBase64 { get; set; }
        public SponsorStatus? SponsorStatus { get; set; }
        public SponsorType? Type { get; set; }
    }
}