using Application.Helpers;

namespace Application.QueryParams.SalaryCycle
{
    public class SalaryCycleQueryParams : PaginationParams
    {
        public DateTime? StartedAt { get; set; }
        public DateTime? StartedBefore { get; set; }

        public DateTime? EndedAfter { get; set; }
        public DateTime? EndedBefore { get; set; }
        
        public SalaryCycleOrderBy? OrderBy { get; set; } = SalaryCycleOrderBy.DateDesc;
    }
}