using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Voucher;
using Application.Domain.Enums.VoucherFile;

namespace Application.Domain.Models
{
  public class Voucher
  {
    public Voucher()
    {
      MemberVouchers = new List<MemberVoucher>();
      VoucherFiles = new List<VoucherFile>();
    }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid VoucherId { get; set; }

    public string VoucherName { get; set; } = null!;
    public string VoucherDescription { get; set; } = null!;

    public VoucherStatus Status { get; set; }
    public double VoucherCost { get; set; }
    public int VoucherAmount { get; set; }
    public VoucherType VoucherType { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTimeHelper.Now();

    public virtual Supplier Supplier { get; set; }
    public virtual Guid SupplierId { get; set; }

    public virtual List<MemberVoucher> MemberVouchers { get; set; }
    public virtual List<VoucherFile> VoucherFiles { get; set; }
  }
}