using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Voucher
{
  public enum VoucherAction
  {
    Buy
  }

  public class VoucherActionDTO
  {
    public VoucherAction Action { get; set; }
    public string? PinCode { get; set; }
  }
}