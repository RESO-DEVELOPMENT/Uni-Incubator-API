using Application.Helpers;

namespace Application.QueryParams.Project
{
  public class ProjectMinimalQueryParams : PaginationParams
  {
    public ProjectOrderBy OrderBy { get; set; } = ProjectOrderBy.DateDesc;
  }
}