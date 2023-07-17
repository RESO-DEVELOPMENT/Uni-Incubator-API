using Application.Domain.Enums.Project;

namespace Application.Domain.Enums.SalaryCycle
{
    public static class SalaryCycleStatusTranslate
    {
        public static string Translate(SalaryCycleStatus status)
        {
            switch (status)
            {
                case SalaryCycleStatus.Ongoing:
                    return "Đang diễn ra";
                case SalaryCycleStatus.Locked:
                    return "Khoá";
                case SalaryCycleStatus.Paid:
                    return "Kết thúc";
                case SalaryCycleStatus.Cancelled:
                    return "Huỷ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }

    public enum SalaryCycleStatus
    {
        Ongoing,
        Locked,
        Paid,
        Cancelled
    }
}