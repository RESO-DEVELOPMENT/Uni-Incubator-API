using Application.Domain;
using Application.Domain.Enums.Supplier;
using Application.Domain.Enums.SystemFile;
using Application.Domain.Enums.Voucher;
using Application.Domain.Enums.VoucherFile;
using Application.Domain.Models;
using Application.DTOs.Voucher;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Voucher;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IBoxService _IBoxService;

        public VoucherService(UnitOfWork unitOfWork,
                              IBoxService IBoxService,
                              IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            this._IBoxService = IBoxService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all user
        /// </summary>
        /// <returns>User</returns>
        public async Task<PagedList<Voucher>> GetAllVoucher(VoucherQueryParams queryParams)
        {
            var query = _unitOfWork.VoucherRepository.GetQuery();
            query = query
              .Include(x => x.Supplier)
              .Where(v => v.Status == VoucherStatus.Created);

            if (queryParams.Name != null) query = query.Where(v => v.VoucherName.ToLower().Contains(queryParams.Name.ToLower()));
            if (queryParams.SupplierId != null) query = query.Where(v => v.SupplierId == queryParams.SupplierId);
            if (queryParams.Status.Count > 0) query = query.Where(v => queryParams.Status.Contains(v.Status));
            if (queryParams.Type.Any()) query = query.Where(v => queryParams.Type.Contains(v.VoucherType));

            switch (queryParams.OrderBy)
            {
                case VoucherOrderBy.CreatedAtAsc:
                    query = query.OrderBy(v => v.CreatedAt);
                    break;
                case VoucherOrderBy.CreatedAtDesc:
                    query = query.OrderByDescending(v => v.CreatedAt);
                    break;
                default:
                    break;
            }

            return await PagedList<Voucher>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<VoucherDTO> GetVoucherById(Guid voucherId)
        {
            var voucher = await _unitOfWork.VoucherRepository.GetQuery()
            .Include(x => x.Supplier)
            .Where(x => x.VoucherId == voucherId)
            .FirstOrDefaultAsync() ??
              throw new NotFoundException("Voucher not found!", ErrorNameValues.VoucherNotFound);

            var dto = _mapper.Map<VoucherDTO>(voucher);
            return dto;
        }

        public async Task<Voucher> CreateNewVoucher(VoucherCreateDTO dto)
        {
            var newVoucher = new Voucher();

            _mapper.Map(dto, newVoucher);
            _unitOfWork.VoucherRepository.Add(newVoucher);

            var supplier = await _unitOfWork.SupplierRepository.GetQuery().FirstOrDefaultAsync(x => x.SupplierId == dto.SupplierId && x.Status == SupplierStatus.Available) ?? throw new BadRequestException("Supplier not found!", ErrorNameValues.SupplierNotFound);

            if (dto.ImageAsBase64 != null)
            {
                dto.ImageAsBase64 = dto.ImageAsBase64
                  .Replace("data:image/png;base64,", "")
                  .Replace("data:image/svg+xml;base64,", "")
                  .Replace("data:image/jpeg;base64,", "");

                var resultFile = await _IBoxService.UploadVoucherImage(dto.ImageAsBase64, newVoucher);
                var curFile = new VoucherFile()
                {
                    FileType = VoucherFileType.VoucherImage,
                    SystemFile = new SystemFile()
                    {
                        FileId = resultFile.Id,
                        DirectUrl = resultFile.SharedLink.DownloadUrl,
                        Type = SystemFileType.JPEG
                    }
                };

                newVoucher.VoucherFiles.Add(curFile);
                newVoucher.ImageUrl = resultFile.SharedLink.DownloadUrl;
            }

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);

            return newVoucher;
        }

        public async Task<Voucher> UpdateVoucher(VoucherUpdateDTO dto)
        {
            var voucher = await _unitOfWork.VoucherRepository.GetQuery()
            .Include(vc => vc.VoucherFiles).ThenInclude(vc => vc.SystemFile)
              .Where(v => dto.VoucherId == v.VoucherId)
              .FirstOrDefaultAsync();

            if (voucher == null)
                throw new NotFoundException($"Voucher not found!", ErrorNameValues.VoucherNotFound);

            if (dto.SupplierId != null)
            {
                var supplier = await _unitOfWork.SupplierRepository.GetQuery().FirstOrDefaultAsync(x => x.SupplierId == dto.SupplierId.Value && x.Status == SupplierStatus.Available) ?? throw new BadRequestException("Supplier not found!", ErrorNameValues.SupplierNotFound);
            }

            if (dto.ImageAsBase64 != null)
            {
                dto.ImageAsBase64 = dto.ImageAsBase64
                  .Replace("data:image/png;base64,", "")
                  .Replace("data:image/svg+xml;base64,", "")
                  .Replace("data:image/jpeg;base64,", "");

                var resultFile = await _IBoxService.UploadVoucherImage(dto.ImageAsBase64, voucher);
                var curFile = voucher.VoucherFiles.FirstOrDefault(f => f.FileType == VoucherFileType.VoucherImage);

                if (curFile == null)
                {
                    curFile = new VoucherFile()
                    {
                        FileType = VoucherFileType.VoucherImage,
                        SystemFile = new SystemFile()
                        {
                            FileId = resultFile.Id,
                            DirectUrl = resultFile.SharedLink.DownloadUrl,
                            Type = SystemFileType.JPEG
                        }
                    };

                    voucher.VoucherFiles.Add(curFile);
                }
                else
                {
                    curFile.SystemFile.UpdatedAt = DateTimeHelper.Now();
                }

                voucher.ImageUrl = resultFile.SharedLink.DownloadUrl;
            }

            dto.VoucherCost ??= voucher.VoucherCost;
            dto.VoucherAmount ??= voucher.VoucherAmount;
            var supplierId = dto.SupplierId ?? voucher.SupplierId;

            _mapper.Map(dto, voucher);
            voucher.SupplierId = supplierId;

            _unitOfWork.VoucherRepository.Update(voucher);

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);

            return voucher;
        }
    }
}