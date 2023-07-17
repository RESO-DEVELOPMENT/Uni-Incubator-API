using Application.Helpers;

namespace Application.QueryParams
{
    public class UserQueryParams : PaginationParams
    {
        public string? EmailAddress { get; set; }
    }
}