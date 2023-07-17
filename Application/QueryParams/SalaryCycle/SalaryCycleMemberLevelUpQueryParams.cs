using Application.Helpers;

namespace Application.QueryParams.SalaryCycle
{
    public class SalaryCycleMemberLevelUpQueryParams : PaginationParams
    {
        public SalaryCycleMemberLevelUpOrderBy? OrderBy { get; set; } = SalaryCycleMemberLevelUpOrderBy.DateDesc;
        public bool IncludeNewAccount { get; set; } = false;
    }
}