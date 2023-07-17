using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Voucher;

namespace Application.DTOs.Voucher
{
  public class VoucherCreateDTO
  {
    [Required]
    public string VoucherName { get; set; } = null!;
    [Required]
    public string VoucherDescription { get; set; } = null!;
    [Required]
    public Guid SupplierId { get; set; }
    [Required]
    [Range(1, 100000)]
    public double VoucherCost { get; set; }
    [Required]
    [Range(0, 1000)]
    public int VoucherAmount { get; set; }

    [Required]
    public VoucherType VoucherType { get; set; }

    public string? ImageAsBase64 { get; set; }
  }
}