using Application.Domain.Enums.Voucher;
using Application.DTOs.Supplier;

namespace Application.DTOs.Voucher
{
    public class VoucherDTO
    {
        public Guid VoucherId { get; set; }

        public string VoucherName { get; set; } = null!;
        public string VoucherDescription { get; set; } = null!;

        public double VoucherCost { get; set; }
        public int VoucherAmount { get; set; }

        public VoucherType VoucherType { get; set; }

        public SupplierDTO Supplier { get; set; } = null!;

        public String? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}