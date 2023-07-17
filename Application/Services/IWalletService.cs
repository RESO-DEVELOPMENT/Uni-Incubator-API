using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.Wallet;

namespace Application.Services
{
    public interface IWalletService
    {
        Task<Wallet> CreateWalletForMember(Guid empId, WalletType walletType, WalletToken walletToken, double amount);
        Task<Wallet> CreateWalletForProject(Guid projectId, WalletType walletType, WalletToken walletToken, double amount, string? tag);

        Task<bool> ExpireWallet(Guid walletId);
        Task<Wallet> GetSystemWallet(WalletToken token);
        Task<WalletsInfoDTO> GetSystemWalletInfo();

        Task<WalletsInfoDTO> GetWalletsInfo(string requesterEmail);
        Task<WalletsInfoDTO> GetWalletsInfo(Guid memberId);

        Task<MonthlySendLimitDTO> GetMonthySendLimitForMember(string requesterEmail);
        Task<MonthlySendLimitDTO> GetMonthySendLimitForMember(Guid memberId);

        Task<string> SendTokenFromMemberToMember(string fromEmail, WalletSendPointToOtherDTO dto);

        Task<List<Transaction>> SendToken(Guid fromTargetId, Guid toTargetId,
        double amount,
        WalletToken walletToken, TransactionType type,
        List<String>? fromWalletTags = null, List<String>? toWalletTags = null
        , String? note = null);

        Task<bool> TestAward(string email);
    }
}