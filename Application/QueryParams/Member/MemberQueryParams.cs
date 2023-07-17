using Application.Helpers;

namespace Application.QueryParams.Member
{
    public class MemberQueryParams : PaginationParams
    {
        public string? EmailAddress { get; set; }
        public string? FullName { get; set; }
        public MemberOrderBy OrderBy { get; set; } = MemberOrderBy.DateDesc;
    }
}