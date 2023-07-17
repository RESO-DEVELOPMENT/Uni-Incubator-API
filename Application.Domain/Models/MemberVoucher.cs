using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.MemberVoucher;

namespace Application.Domain.Models
{
  public class MemberVoucher
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid MemberVoucherId { get; set; }

    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;

    public Guid VoucherId { get; set; }
    public virtual Voucher Voucher { get; set; } = null!;

    public MemberVoucherStatus Status { get; set; }

    public string Code { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
  }
}