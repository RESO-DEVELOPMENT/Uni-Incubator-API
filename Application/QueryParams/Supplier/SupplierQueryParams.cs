using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Domain.Enums.Supplier;
using Application.Helpers;

namespace Application.QueryParams.Supplier
{
  public class SupplierQueryParams : PaginationParams
  {
    public string? Name { get; set; }
    public List<SupplierStatus> Status { get; set; } = new();
    public SupplierOrderBy OrderBy { get; set; } = SupplierOrderBy.CreatedAtDesc;
  }
}