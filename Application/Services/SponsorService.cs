using Application.Domain;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectSponsor;
using Application.Domain.Enums.ProjectSponsorTransaction;
using Application.Domain.Enums.SponsorFile;
using Application.Domain.Enums.SystemFile;
using Application.Domain.Models;
using Application.DTOs.Sponsor;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams;
using Application.QueryParams.Project;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly IMapper _mapper;
        private readonly IBoxService _boxService;
        private readonly UnitOfWork _unitOfWork;

        public SponsorService(UnitOfWork unitOfWork,
                           IMapper mapper,
                           IBoxService boxService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _boxService = boxService;
        }

        /// <summary>
        /// Get all sponsors
        /// </summary>
        public async Task<PagedList<Sponsor>> GetAllSponsors(SponsorQueryParams queryParams)
        {
            var query = _unitOfWork.SponsorRepository.GetQuery();
            query = query.Include(sp => sp.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
                  .ThenInclude(ps => ps.ProjectSponsorTransactions.Where(pst => pst.Status == ProjectSponsorTransactionStatus.Paid))
                .Include(sp => sp.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
                  .ThenInclude(ps => ps.Project);

            if (queryParams.SponsorName != null) query = query.Where(s => s.SponsorName.ToLower().Contains(queryParams.SponsorName.ToLower()));
            if (queryParams.Status.Count() > 0) query = query.Where(s => queryParams.Status.Contains(s.SponsorStatus));

            var sponsors = await PagedList<Sponsor>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return sponsors;
        }

        public async Task<Sponsor> GetSponsorById(Guid sponsorId)
        {
            var sponsor = await _unitOfWork.SponsorRepository
              .GetQuery()
                .Where(sp => sp.SponsorId == sponsorId)
                .Include(sp => sp.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
                  .ThenInclude(ps => ps.ProjectSponsorTransactions.Where(pst => pst.Status == ProjectSponsorTransactionStatus.Paid))
                .Include(sp => sp.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
                  .ThenInclude(ps => ps.Project)
              .FirstOrDefaultAsync() ?? throw new NotFoundException("Nhà tài trợ không tồn tại!", ErrorNameValues.SponsorNotFound);

            return sponsor;
        }

        public async Task<PagedList<Project>> GetSponsorJoinedProjects(ProjectForSponsorQueryParams queryParams, Guid sponsorId)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetByID(sponsorId);
            if (sponsor == null) throw new NotFoundException("Sponsor not found!", ErrorNameValues.SponsorNotFound);

            var query = _unitOfWork.ProjectRepository.GetQuery()
              .Include(p => p.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
              .Where(p => p.ProjectSponsors.Select(ps => ps.SponsorId).Contains(sponsor.SponsorId));

            if (queryParams.Status.Count > 0)
                query = query.Where(p => queryParams.Status.Contains(p.ProjectStatus));

            if (queryParams.ProjectName != null)
                query = query.Where(p =>
                  p.ProjectName.ToLower().Contains(queryParams.ProjectName.ToLower()));

            if (queryParams.ManagerEmail != null)
                query = query.Where(p =>
                    p.ProjectMember.FirstOrDefault(pe =>  // Get project with Member is manager
                      pe.Member.EmailAddress == queryParams.ManagerEmail &&
                      pe.Role == ProjectMemberRole.Manager) != null
                    );

            switch (queryParams.OrderBy)
            {
                case ProjectOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case ProjectOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.BudgetMin != null)
                query = query.Where(p => p.Budget >= (double)queryParams.BudgetMin);

            if (queryParams.BudgetMax != null)
                query = query.Where(p => p.Budget <= (double)queryParams.BudgetMax);

            if (queryParams.StartAfter != null)
                query = query.Where(p => p.StartedAt >= queryParams.StartAfter);

            if (queryParams.EndBefore != null)
                query = query.Where(p => p.EndedAt <= queryParams.EndBefore);

            var projects = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return projects;
        }

        public async Task<Guid> CreateSponsor(SponsorCreateDTO dto)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetQuery()
            .Where(s => s.SponsorName.ToLower() == dto.SponsorName.ToLower())
            .FirstOrDefaultAsync();

            if (sponsor != null)
                throw new BadRequestException("Trùng tên với nhà tài trợ khác!", ErrorNameValues.SponsorNameExisted);

            sponsor = new Sponsor();
            _mapper.Map(dto, sponsor);

            _unitOfWork.SponsorRepository.Add(sponsor);

            if (dto.ImageAsBase64 != null)
            {
                dto.ImageAsBase64 = dto.ImageAsBase64
                  .Replace("data:image/png;base64,", "")
                  .Replace("data:image/svg+xml;base64,", "")
                  .Replace("data:image/jpeg;base64,", "");

                var resultFile = await _boxService.UploadSponsorImage(dto.ImageAsBase64, sponsor);
                var curFile = sponsor.SponsorFiles.FirstOrDefault(f => f.FileType == SponsorFileType.SponsorImage);

                if (curFile == null)
                {
                    curFile = new SponsorFile()
                    {
                        FileType = SponsorFileType.SponsorImage,
                        SystemFile = new SystemFile()
                        {
                            FileId = resultFile.Id,
                            DirectUrl = resultFile.SharedLink.DownloadUrl,
                            Type = SystemFileType.JPEG
                        }
                    };

                    sponsor.SponsorFiles.Add(curFile);
                }
                else
                {
                    curFile.SystemFile.UpdatedAt = DateTimeHelper.Now();
                }

                sponsor.ImageUrl = resultFile.SharedLink.DownloadUrl;
            }

            var result = await _unitOfWork.SaveAsync();

            if (!result) throw new BadRequestException("Lỗi hệ thống, vui lòng thử lại!", ErrorNameValues.SystemError);
            return sponsor.SponsorId;
        }

        public async Task<bool> UpdateSponsor(SponsorUpdateDTO dto)
        {
            var sponsor = await _unitOfWork.SponsorRepository.GetQuery()
            .Where(s => s.SponsorId == dto.SponsorId)
            .Include(s => s.SponsorFiles)
            .ThenInclude(sf => sf.SystemFile)
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Nhà tài trợ không tồn tại!", ErrorNameValues.SponsorNotFound);

            _mapper.Map(dto, sponsor);

            if (dto.ImageAsBase64 != null)
            {
                dto.ImageAsBase64 = dto.ImageAsBase64
                  .Replace("data:image/png;base64,", "")
                  .Replace("data:image/svg+xml;base64,", "")
                  .Replace("data:image/jpeg;base64,", "");

                var resultFile = await _boxService.UploadSponsorImage(dto.ImageAsBase64, sponsor);
                var curFile = sponsor.SponsorFiles.FirstOrDefault(f => f.FileType == SponsorFileType.SponsorImage);

                if (curFile == null)
                {
                    curFile = new SponsorFile()
                    {
                        FileType = SponsorFileType.SponsorImage,
                        SystemFile = new SystemFile()
                        {
                            FileId = resultFile.Id,
                            DirectUrl = resultFile.SharedLink.DownloadUrl,
                            Type = SystemFileType.JPEG
                        }
                    };

                    sponsor.SponsorFiles.Add(curFile);
                }
                else
                {
                    curFile.SystemFile.UpdatedAt = DateTimeHelper.Now();
                }

                sponsor.ImageUrl = resultFile.SharedLink.DownloadUrl;
            }

            _unitOfWork.SponsorRepository.Update(sponsor);
            var result = await _unitOfWork.SaveAsync();

            if (!result) throw new BadRequestException("Some error happen, try again!", ErrorNameValues.SystemError);
            return result;
        }
    }
}