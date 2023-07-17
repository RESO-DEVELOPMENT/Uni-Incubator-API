using Application.Domain.Enums.Wallet;
using Application.Domain.Models;

namespace Application.Helpers
{
    public static class WalletHelpers
    {
        /// <summary>
        /// Sort <paramref name="wallets"/> by [Hot Wallet First and Near Expired Wallet First] then return sorted wallets
        /// </summary>
        public static List<Wallet> SortWallets(this List<Wallet> wallets)
        {
            return wallets
                .OrderBy(u => u.WalletType)
                .ThenBy(u => u.ExpiredDate)
                .ToList();
        }

        // public static List<Wallet> DeductFromWallet()

        /// <summary>
        /// Calculate total amount of <paramref name="walletToken"/> in <paramref name="wallets"/>
        /// </summary>
        public static double Total(this List<Wallet> wallets, WalletToken walletToken)
        {
            return wallets.Where(w => w.WalletToken == walletToken)
                .Sum(w => w.Amount);
        }
    }
}