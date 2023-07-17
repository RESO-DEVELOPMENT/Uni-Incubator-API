using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MemberVoucher
{
    public class MemberVoucherCreateDTO
    {
        [Required]
        public Guid VoucherId { get; set; }
    }
}