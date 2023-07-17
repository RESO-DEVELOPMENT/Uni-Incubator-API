using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Helpers;

namespace Application.QueryParams.ProjectBonus
{
  public class ProjectBonusQueryParams : PaginationParams
  {
    public Guid? SalaryCycleId { get; set; }
    public ProjectBonusOrderBy OrderBy { get; set; } = ProjectBonusOrderBy.DateDesc;
  }
}