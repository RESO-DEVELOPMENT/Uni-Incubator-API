using Application.Domain.Enums.Sponsor;
using Application.Helpers;

namespace Application.QueryParams
{
    public class SponsorQueryParams : PaginationParams
    {
        public string? SponsorName { get; set; }
        public List<SponsorStatus> Status { get; set; } = new List<SponsorStatus>();
    }
}