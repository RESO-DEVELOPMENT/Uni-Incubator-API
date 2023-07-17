using Application.Domain.Enums.Wallet;
using Application.Helpers;
using Application.QueryParams.Transaction;

namespace Application.QueryParams
{
  public class TransactionQueryParams : PaginationParams
  {
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public bool? IsReceived { get; set; }
    public bool? IsSent { get; set; }

    public TransactionOrderBy OrderBy { get; set; } = TransactionOrderBy.CreatedAtAsc;
    public List<TransactionType> Types = new();
  }
}