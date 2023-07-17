namespace Application.Domain.Models
{
    public class QueueTask
    {
        public string TaskName { get; set; } = null!;
        public Dictionary<string, string> TaskData { get; set; } = null!;
    }
}