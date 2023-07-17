using Application.Domain.Enums.PayslipItem;
using Application.Helpers;

namespace Application.QueryParams.PayslipItem
{
    public class PayslipItemQueryParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public List<PayslipItemType> Types { get; set; } = new List<PayslipItemType>();

        public Guid? MemberId { get; set; }
        public Guid? SalaryCycleId { get; set; }
    }
}
