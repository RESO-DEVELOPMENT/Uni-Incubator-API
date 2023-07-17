using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Application.Domain.Enums.MemberVoucher;

namespace Application.DTOs.MemberVoucher
{
  public class MemberVoucherUpdateStatusDTO
  {
    public Guid? MemberVoucherId { get; set; }
    public string? MemberVoucherCode { get; set; }

    [Required]
    public MemberVoucherStatus Status { get; set; }
  }
}