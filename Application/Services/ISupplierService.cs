using Application.Domain.Models;
using Application.DTOs.Supplier;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.QueryParams.Supplier;
using Application.QueryParams.Voucher;

namespace Application.Services
{
  public interface ISupplierService
  {
    Task<Guid> CreateSupplier(SupplierCreateDTO dto);
    Task<List<SupplierDTO>> GetAllSupplier(SupplierQueryParams queryParams);
    Task<SupplierDTO> GetSupplier(Guid supplierId);
    Task<bool> UpdateSupplier(SupplierUpdateDTO dto);
  }
}