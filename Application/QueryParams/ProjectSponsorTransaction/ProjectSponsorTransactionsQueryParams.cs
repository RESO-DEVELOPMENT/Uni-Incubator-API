using Application.Domain.Enums.ProjectSponsorTransaction;
using Application.Helpers;

namespace Application.QueryParams.ProjectSponsorTransaction
{
    public class ProjectSponsorTransactionsQueryParams : PaginationParams
    {
        public List<ProjectSponsorTransactionStatus> Status { get; set; } = new List<ProjectSponsorTransactionStatus>();
        public ProjectSponsorTransactionOrderBy OrderBy { get; set; } = ProjectSponsorTransactionOrderBy.DateDesc;
    }
}