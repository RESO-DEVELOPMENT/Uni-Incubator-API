using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Wallet
{
    public class WalletSendPointToOtherDTO
    {
        [Required]
        public Guid ToMemberId { get; set; }
        [Required]
        [Range(1,10000)]
        public double Amount { get; set; }
        public string? PinCode { get; set; }
    }
}
