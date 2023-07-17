using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Voucher;

namespace Application.DTOs.Voucher
{
    public class VoucherUpdateDTO
    {
        [Required]
        public Guid VoucherId { get; set; }

        public string? VoucherName { get; set; } = null!;
        public string? VoucherDescription { get; set; } = null!;
        public Guid? SupplierId { get; set; }

        [Range(1, 100000)]
        public double? VoucherCost { get; set; }
        [Range(0, 100000)]
        public int? VoucherAmount { get; set; }

        public VoucherType? VoucherType { get; set; }

        public String? ImageAsBase64 { get; set; }
    }
}