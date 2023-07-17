namespace Application.DTOs
{
    public class ResponseDTO<K>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public K Message { get; set; } = default!;
        public string? ErrorName { get; set; }
        public string? ErrorMsg { get; set; }
    }
}