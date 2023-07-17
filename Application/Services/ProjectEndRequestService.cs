using Application.Domain;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectEndRequest;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.ProjectEndRequest;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectEndRequest;
using Application.QueryParams.ProjectMemberRequest;
using Application.StateMachines;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ProjectEndRequestService : IProjectEndRequestService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectEndRequestService(UnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<ProjectEndRequestDTO>> GetAllRequestToEnd(ProjectEndRequestQueryParams queryParams, string? requesterEmail = null, bool isAdmin = false)
        {
            var query = _unitOfWork.ProjectEndRequestRepository
                .GetQuery();

            query = query.Include(x => x.Project);

            if (!isAdmin)
            {
                if (requesterEmail == null)
                    throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);
                var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                query = query.Where(p => p.Project.ProjectMember.Any(x => x.MemberId == member.MemberId &&
                                                                          x.Role == ProjectMemberRole.Manager &&
                                                                          x.Status == ProjectMemberStatus.Active));
            }

            if (queryParams.ProjectId != null)
                query = query.Where(x => x.ProjectId == queryParams.ProjectId);

            if (queryParams.Status.Any()) query = query.Where(x => queryParams.Status.Contains(x.Status));
            if (queryParams.PointAction.Any()) query = query.Where(x => queryParams.PointAction.Contains(x.PointAction));

            switch (queryParams.OrderBy)
            {
                case ProjectEndRequestOrderBy.CreatedAtDesc:
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
                case ProjectEndRequestOrderBy.CreatedAtAsc:
                    query = query.OrderBy(x => x.CreatedAt);
                    break;
            }

            var requests = await PagedList<ProjectEndRequest>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);

            _httpContextAccessor.HttpContext?.Response.AddPaginationHeader(requests);
            var mappedRequests = _mapper.Map<List<ProjectEndRequestDTO>>(requests);

            return mappedRequests;
        }

        public async Task<List<ProjectEndRequestDTO>> GetAllRequestToEndFromProject(Guid projectId, ProjectEndRequestQueryParams queryParams, string? requesterEmail = null, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId) ??
                          throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var query = _unitOfWork.ProjectEndRequestRepository.GetQuery();
            query = query.Include(x => x.Project);

            if (!isAdmin)
            {
                if (requesterEmail == null)
                    throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail)
                             ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                var projectMember =
                    await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectId,
                        member.EmailAddress) ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.MemberNotFound);
            }

            query = query.Where(x => x.ProjectId == projectId);

            if (queryParams.Status.Any()) query = query.Where(x => queryParams.Status.Contains(x.Status));
            if (queryParams.PointAction.Any()) query = query.Where(x => queryParams.PointAction.Contains(x.PointAction));


            switch (queryParams.OrderBy)
            {
                case ProjectEndRequestOrderBy.CreatedAtDesc:
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
                case ProjectEndRequestOrderBy.CreatedAtAsc:
                    query = query.OrderBy(x => x.CreatedAt);
                    break;
            }

            var requests = await PagedList<ProjectEndRequest>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);

            _httpContextAccessor.HttpContext?.Response.AddPaginationHeader(requests);
            var mappedRequests = _mapper.Map<List<ProjectEndRequestDTO>>(requests);

            return mappedRequests;
        }

        public async Task<ProjectEndRequestDTO> GetRequestToEnd(Guid requestId, string? requesterEmail = null, bool isAdmin = false)
        {
            var request = await _unitOfWork.ProjectEndRequestRepository.GetQuery()
                .Include(x => x.Project)
                .Where(x => x.RequestId == requestId)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Yêu cầu dừng dự án không tồn tại!", ErrorNameValues.RequestNotFound);

            if (!isAdmin)
            {
                if (requesterEmail == null)
                    throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail)
                             ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);
                var projectMember =
                    await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(request.ProjectId,
                        member.EmailAddress) ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.MemberNotFound);
            }

            var mapped = _mapper.Map<ProjectEndRequestDTO>(request);
            return mapped;

        }

        public async Task<Guid> RequestToEnd(Guid projectId, ProjectEndRequestCreateDTO dto, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId) ??
                          throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            if (project.ProjectStatus != ProjectStatus.Started)
                throw new BadRequestException("Dự án chưa bắt đầu, không thể tạo yêu cầu!", ErrorNameValues.ProjectNotAvailable);

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
            if (currentSalaryCycle != null && (currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Ongoing || currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Locked))
            {
                throw new BadRequestException("Bạn không thể làm điều này khi có kì lương đang diễn ra", ErrorNameValues.SalaryCycleNotAvailable);
            }

            var query = _unitOfWork.ProjectEndRequestRepository.GetQuery()
                .Include(x => x.Project);

            var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail)
                         ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

            var projectMember =
                await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectId,
                    member.EmailAddress) ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.MemberNotFound);

            var currentRequest = await _unitOfWork.ProjectEndRequestRepository.GetQuery()
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.Status == ProjectEndRequestStatus.Created && x.ProjectId == project.ProjectId);

            if (currentRequest != null)
                throw new BadRequestException("Đã có yêu cầu kết thúc dự án trước đó, xin hãy đợi!",
                    ErrorNameValues.RequestDuplicated);

            var request = new ProjectEndRequest()
            {
                ProjectId = project.ProjectId,
                PointAction = dto.PointAction,
                Status = ProjectEndRequestStatus.Created,
                Note = dto.Note,
            };

            _unitOfWork.ProjectEndRequestRepository.Add(request);
            var result = await _unitOfWork.SaveAsync();

            if (!result) throw new BadRequestException("Đã có lỗi khi tạo yêu cầu, vui lòng thử lại!", ErrorNameValues.SystemError);
            return request.RequestId;
        }

        public async Task<bool> ReviewRequestToEnd(ProjectEndRequestReviewDTO dto, string requesterEmail, bool isAdmin = false)
        {
            var currentRequest = await _unitOfWork.ProjectEndRequestRepository.GetQuery()
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.RequestId == dto.RequestId)
                                 ?? throw new NotFoundException("Yêu cầu không tồn tại!", ErrorNameValues.RequestNotFound);

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
            if (currentSalaryCycle != null && (currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Ongoing || currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Locked))
            {
                throw new BadRequestException("Bạn không thể làm điều này khi có kì lương đang diễn ra", ErrorNameValues.SalaryCycleNotAvailable);
            }

            if (!isAdmin)
            {
                var member = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail)
                             ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                var projectMember =
                    await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(currentRequest.ProjectId,
                        member.EmailAddress) ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.MemberNotFound);
            }

            var allowedForPM = new List<ProjectEndRequestStatus>()
                { ProjectEndRequestStatus.Cancelled };
            var disallowed = new List<ProjectEndRequestStatus>() { ProjectEndRequestStatus.Created };
            // var allowedForAdmin = new List<ProjectReportStatus>() { ProjectReportStatus.Accepted, ProjectReportStatus.Rejected };

            // if (isAdmin && !allowedForPM.Contains(dto.Status))
            //   throw new BadRequestException("You can only cancel the report!", ErrorNameValues.InvalidStateChange);
            // else
            if (!isAdmin && !allowedForPM.Contains(dto.Status))
                throw new BadRequestException("Bạn chỉ có thể nộp hoặc huỷ!",
                    ErrorNameValues.InvalidStateChange);

            if (disallowed.Contains(dto.Status))
                throw new BadRequestException("Bạn không thể thực hiện hành động này!",
                    ErrorNameValues.InvalidStateChange);
            try
            {
                var prsm = new ProjectEndRequestStateMachine(currentRequest!);
                prsm.TriggerState(dto.Status);

                if (dto.Status == ProjectEndRequestStatus.Accepted)
                {
                    var curSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
                    if (curSalaryCycle is { SalaryCycleStatus: >= SalaryCycleStatus.Ongoing and <= SalaryCycleStatus.Locked })
                    {
                        throw new BadRequestException("Bạn không thể thực hiện hành động này khi có kì lương đang diễn ra!", ErrorNameValues.SalaryCycleNotAvailable);
                    }

                    var projectWithWallet = await _unitOfWork.ProjectRepository
                      .GetQuery()
                      .Include(x => x.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                      .Include(x => x.ProjectWallets.Where(pw => pw.Wallet.WalletStatus == WalletStatus.Available))
                      .ThenInclude(pw => pw.Wallet)
                      .Where(p => p.ProjectId == currentRequest.ProjectId)
                      .FirstOrDefaultAsync();

                    var systemWallet = await _unitOfWork.WalletRepository.GetSystemWallets();
                    var systemPointWallet = systemWallet.First(x => x.WalletToken == WalletToken.Point);

                    projectWithWallet.ProjectStatus = ProjectStatus.Ended;
                    projectWithWallet.EndedAt = DateTimeHelper.Now();

                    var projectMainWallet = projectWithWallet.ProjectWallets.First(pw => pw.Wallet.WalletTag == "main").Wallet;
                    var projectBonusWallet = projectWithWallet.ProjectWallets.First(pw => pw.Wallet.WalletTag == "bonus").Wallet;

                    if (currentRequest.PointAction == ProjectEndRequestPointAction.PointReturnedToSystem)
                    {
                        if (projectMainWallet.Amount > 0)
                        {
                            var amount = projectMainWallet.Amount;

                            projectMainWallet.Amount = 0;
                            systemPointWallet.Amount += amount;

                            var trx = new Transaction()
                            {
                                FromWalletId = projectMainWallet.WalletId,
                                ToWalletId = systemPointWallet.WalletId,
                                TransactionType = TransactionType.ProjectReturnToSystem,
                                Amount = amount,
                                Note = "Điểm dự án trả về hệ thống",
                                Token = WalletToken.Point,
                                FromAmountAfterTransaction = 0,
                                ToAmountAfterTransaction = systemPointWallet.Amount,
                            };

                            _unitOfWork.TransactionRepository.Add(trx);
                        }

                        if (projectBonusWallet.Amount > 0)
                        {
                            var amount = projectBonusWallet.Amount;

                            projectBonusWallet.Amount = 0;
                            systemPointWallet.Amount += amount;

                            var trx = new Transaction()
                            {
                                FromWalletId = projectBonusWallet.WalletId,
                                ToWalletId = systemPointWallet.WalletId,
                                TransactionType = TransactionType.ProjectReturnToSystem,
                                Amount = amount,
                                Note = "Điểm thưởng dự án trả về hệ thống",
                                Token = WalletToken.Point,
                                FromAmountAfterTransaction = 0,
                                ToAmountAfterTransaction = systemPointWallet.Amount,
                            };

                            _unitOfWork.TransactionRepository.Add(trx);
                        }
                    }
                    else if (currentRequest.PointAction == ProjectEndRequestPointAction.PointSplitToMembers)
                    {
                        currentRequest.Project.ProjectStatus = ProjectStatus.Stopped;
                        //_unitOfWork.ProjectRepository.Update(project);
                        //var memberWithWallets = await _unitOfWork.MemberRepository.GetQuery()
                        //    .Include(x => x.MemberWallets.Where(pw =>
                        //        pw.Wallet.WalletStatus == WalletStatus.Available &&
                        //        pw.Wallet.WalletToken == WalletToken.Point))
                        //        .ThenInclude(pw => pw.Wallet)
                        //    .Where(x => projectWithWallet.ProjectMember.Select(pm => pm.MemberId).Contains(x.MemberId))
                        //    .ToListAsync();

                        //var splitCount = memberWithWallets.Count();

                        //if (projectMainWallet.Amount > 0)
                        //{
                        //    var amount = projectMainWallet.Amount;
                        //    var splitAmount = amount / splitCount;

                        //    memberWithWallets.ForEach(m =>
                        //    {
                        //        var memberWallet = m.MemberWallets.First(x => x.Wallet.WalletType == WalletType.Cold).Wallet;

                        //        memberWallet.Amount += splitAmount;
                        //        projectMainWallet.Amount -= splitAmount;

                        //        var trx = new Transaction()
                        //        {
                        //            FromWalletId = projectMainWallet.WalletId,
                        //            ToWalletId = memberWallet.WalletId,
                        //            TransactionType = TransactionType.ProjectSalary,
                        //            Amount = splitAmount,
                        //            Note = $"Điểm dự án {currentRequest.Project.ProjectName} chia đều cho thành viên khi kết thúc",
                        //            Token = WalletToken.Point,
                        //            FromAmountAfterTransaction = projectMainWallet.Amount,
                        //            ToAmountAfterTransaction = memberWallet.Amount,
                        //        };

                        //        _unitOfWork.TransactionRepository.Add(trx);
                        //    });
                        //}

                        //if (projectBonusWallet.Amount > 0)
                        //{
                        //    var amount = projectBonusWallet.Amount;

                        //    projectBonusWallet.Amount = 0;
                        //    systemPointWallet.Amount += amount;

                        //    var trx = new Transaction()
                        //    {
                        //        FromWalletId = projectBonusWallet.WalletId,
                        //        ToWalletId = systemPointWallet.WalletId,
                        //        TransactionType = TransactionType.ProjectReturnToSystem,
                        //        Amount = amount,
                        //        Note = "Điểm thưởng dự án trả về hệ thống",
                        //        Token = WalletToken.Point,
                        //        FromAmountAfterTransaction = 0,
                        //        ToAmountAfterTransaction = systemPointWallet.Amount,
                        //    };

                        //    _unitOfWork.TransactionRepository.Add(trx);
                        //}
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException("Chuyển trạng thái không hợp lệ!",
                    ErrorNameValues.InvalidStateChange);
            }

            var result = await _unitOfWork.SaveAsync();
            return result;

        }
    }
}