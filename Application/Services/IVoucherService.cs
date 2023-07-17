using Application.Domain.Models;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.QueryParams.Voucher;

namespace Application.Services
{
  public interface IVoucherService
  {
    Task<Voucher> CreateNewVoucher(VoucherCreateDTO dto);
    Task<PagedList<Voucher>> GetAllVoucher(VoucherQueryParams queryParams);
    Task<VoucherDTO> GetVoucherById(Guid voucherId);
    Task<Voucher> UpdateVoucher(VoucherUpdateDTO dto);
  }
}