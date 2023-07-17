using Application.Domain.Enums.Voucher;
using Application.Helpers;

namespace Application.QueryParams.Voucher
{
  public class VoucherQueryParams : PaginationParams
  {
    public string? Name { get; set; }
    public Guid? SupplierId { get; set; }
    public VoucherOrderBy? OrderBy { get; set; } = VoucherOrderBy.CreatedAtDesc;
    public List<VoucherStatus> Status { get; set; } = new();
    public List<VoucherType> Type { get; set; } = new();

  }
}