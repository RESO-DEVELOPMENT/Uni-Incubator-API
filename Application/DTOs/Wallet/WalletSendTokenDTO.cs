using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Wallet;

namespace Application.DTOs.Wallet
{
    public class WalletSendTokenDTO
    {
        [Required]
        [Range(0, double.MaxValue)]
        public double Amount { get; set; }
        [Required]
        public WalletToken Token { get; set; }
        [Required]
        public Guid UserId { get; set; }
    }
}