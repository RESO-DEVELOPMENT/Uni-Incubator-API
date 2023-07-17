namespace Application.DTOs.Wallet
{
    public class WalletsInfoDTO
    {
        public double TotalXP { get; set; } = 0;
        public double TotalPoint { get; set; } = 0;

        public List<WalletDTO> Wallets { get; set; } = new List<WalletDTO>();
    }
}