namespace Application.DTOs.SystemFile
{
    public class SystemFileDTO
    {
        public string DirectUrl { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}