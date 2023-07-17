using Application.Helpers;

namespace Application.QueryParams.Member
{
  public class MemberTasksQueryParams : PaginationParams
  {
    public Guid? SalaryCycleId { get; set; }
  }
}