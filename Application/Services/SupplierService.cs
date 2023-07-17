using Application.Domain;
using Application.Domain.Enums.Supplier;
using Application.Domain.Models;
using Application.DTOs.Supplier;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Supplier;
using Application.QueryParams.Voucher;
using AutoMapper;

namespace Application.Services
{
  public class SupplierService : ISupplierService
  {
    private readonly IMapper _mapper;
    private readonly UnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SupplierService(IMapper mapper, UnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
      this._httpContextAccessor = httpContextAccessor;
      this._mapper = mapper;
      this._unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateSupplier(SupplierCreateDTO dto)
    {
      var newSupplier = new Supplier();
      _mapper.Map(dto, newSupplier);

      _unitOfWork.SupplierRepository.Add(newSupplier);
      var result = await _unitOfWork.SaveAsync();

      if (!result) throw new BadRequestException("Insert failed, please try again!", ErrorNameValues.SystemError);
      return newSupplier.SupplierId;
    }

    public async Task<List<SupplierDTO>> GetAllSupplier(SupplierQueryParams queryParams)
    {
      var query = _unitOfWork.SupplierRepository.GetQuery();

      if (queryParams.Name != null)
        query = query.Where(x => x.Name.ToLower().Contains(queryParams.Name.ToLower()));

      if (queryParams.Status.Any())
        query = query.Where(x => queryParams.Status.Contains(x.Status));

      query = query
          .OrderBy(x => x.Status)
          .ThenByDescending(x => x.CreatedAt);

      switch (queryParams.OrderBy)
      {
          case SupplierOrderBy.CreatedAtAsc:
              query = query.OrderBy(x => x.CreatedAt);
              break;
          case SupplierOrderBy.CreatedAtDesc:
              query = query.OrderByDescending(x => x.CreatedAt);
              break;
      }

      var suppliers = await PagedList<Supplier>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
      _httpContextAccessor.HttpContext?.Response.AddPaginationHeader(suppliers);

      var mappedSuplliers = _mapper.Map<List<SupplierDTO>>(suppliers);
      return mappedSuplliers;
    }

    public async Task<SupplierDTO> GetSupplier(Guid supplierId)
    {
      var supplier = await _unitOfWork.SupplierRepository.GetByID(supplierId) ??
       throw new NotFoundException("Supplier Not Found");

      var mappedSupplier = _mapper.Map<SupplierDTO>(supplier);
      return mappedSupplier;
    }

    public async Task<bool> UpdateSupplier(SupplierUpdateDTO dto)
    {
      var supplier = await _unitOfWork.SupplierRepository.GetByID(dto.SupplierId) ??
        throw new NotFoundException("Supplier Not Found");

      if (dto.Status == SupplierStatus.Unavailable)
      {
          var vcCount = await _unitOfWork.VoucherRepository.CountBySupplier(supplier.SupplierId);
          if (vcCount > 0)
          {
              throw new BadRequestException("There are vouchers are using this supplier!", ErrorNameValues.SupplierHadVoucher);
          }
      }

      _mapper.Map(dto, supplier);
      _unitOfWork.SupplierRepository.Update(supplier);
      supplier.UpdatedAt = DateTimeHelper.Now();

      var result = await _unitOfWork.SaveAsync();

      return result;
    }
  }
}