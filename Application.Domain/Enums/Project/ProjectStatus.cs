namespace Application.Domain.Enums.Project
{
    public static class ProjectStatusTranslate
    {
        public static string Translate(ProjectStatus status)
        {
            switch (status)
            {
                case ProjectStatus.Created:
                    return "Được tạo";
                case ProjectStatus.Started:
                    return "Bắt đầu";
                case ProjectStatus.Stopped:
                    return "Dừng";
                case ProjectStatus.Ended:
                    return "Kết thúc";
                case ProjectStatus.Cancelled:
                    return "Huỷ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }

    public enum ProjectStatus
    {
        Created = 0,
        Started = 1,
        Ended = 2,
        Cancelled = 3,

        Stopped = 4,
    }
}