using Application.Domain.Models;
using Application.DTOs.MemberVoucher;

namespace Application.Services
{
  public interface IMemberVoucherService
  {
    Task<bool> BuyVoucher(Guid voucherId, string requesterEmail, string? pinCode);
    Task<MemberVoucher> GetMemberVoucherFromCode(string memberVoucherCode);
    Task<bool> UpdateMemberVoucherStatus(MemberVoucherUpdateStatusDTO dto, string requesterEmail, bool isAdmin = false);
  }
}