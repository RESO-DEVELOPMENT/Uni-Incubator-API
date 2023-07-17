using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Sponsor;

namespace Application.DTOs.Sponsor
{
    public class SponsorCreateDTO
    {
        [Required]
        public string SponsorName { get; set; } = null!;
        [Required]
        public string SponsorDescription { get; set; } = null!;
        [Required]
        public SponsorType Type { get; set; }
        public string? ImageAsBase64 { get; set; } = null!;
    }
}