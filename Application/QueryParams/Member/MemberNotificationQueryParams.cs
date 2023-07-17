using Application.Helpers;

namespace Application.QueryParams.Member
{
    public class MemberNotificationQueryParams : PaginationParams
    {
        public MemberNotificationOrderBy OrderBy { get; set; } = MemberNotificationOrderBy.DateDesc;
    }
}