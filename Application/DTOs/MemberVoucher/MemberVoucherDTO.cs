using Application.Domain.Enums.MemberVoucher;
using Application.DTOs.Voucher;

namespace Application.DTOs.MemberVoucher
{
  public class MemberVoucherDTO
  {
    public Guid MemberVoucherId { get; set; }

    public VoucherDTO Voucher { get; set; } = null!;
    public MemberVoucherStatus Status { get; set; }

    public string Code { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
  }
}