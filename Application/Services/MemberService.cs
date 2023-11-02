using Application.Domain;
using Application.Domain.Enums;
using Application.Domain.Enums.Member;
using Application.Domain.Enums.MemberFile;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Enums.SystemFile;
using Application.Domain.Enums.User;
using Application.Domain.Models;
using Application.DTOs.Member;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.Member;
using Application.QueryParams.MemberVoucher;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Member = Application.Domain.Models.Member;

namespace Application.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMapper _mapper;
        private readonly IBoxService _IBoxService;
        private readonly UnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IWalletService _walletService;
        private readonly IQueueService _redisQueueService;
        private readonly IFirebaseService _firebaseService;

        public MemberService(UnitOfWork unitOfWork,
                               ITokenService tokenService,
                               IWalletService walletService,
                               IQueueService redisQueueService,
                               IFirebaseService firebaseService,
                               IMapper mapper,
                               IBoxService IBoxService)
        {
            _tokenService = tokenService;
            _walletService = walletService;
            _redisQueueService = redisQueueService;
            _firebaseService = firebaseService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _IBoxService = IBoxService;
        }

        /// <summary>
        /// Get all user
        /// </summary>
        /// <returns>User</returns>
        public async Task<PagedList<Member>> GetAll(MemberQueryParams queryParams)
        {
            var query = _unitOfWork.MemberRepository.GetQuery();
            query = query
              .Include(x => x.MemberLevels.Where(ml => ml.IsActive))
              .ThenInclude(x => x.Level)
              .Include(x => x.User)
              .ThenInclude(x => x.Role);

            if (queryParams.EmailAddress != null) query = query.Where(u => u.EmailAddress.ToLower().Contains(queryParams.EmailAddress.ToLower()));
            if (queryParams.FullName != null) query = query.Where(u => u.FullName.ToLower().Contains(queryParams.FullName.ToLower()));

            var orderedQuery = query
                .OrderBy(x => x.MemberStatus)
                .ThenBy(x => x.User.RoleId);

            switch (queryParams.OrderBy)
            {
                case MemberOrderBy.DateAsc:
                    query = orderedQuery
                        .ThenBy(m => m.CreatedAt);
                    break;
                case MemberOrderBy.DateDesc:
                    query = orderedQuery
                        .ThenByDescending(m => m.CreatedAt);
                    break;
                case MemberOrderBy.LevelAsc:
                    query = orderedQuery
                        .ThenBy(m => m.MemberLevels.First(ml => ml.IsActive).Level.XPNeeded);
                    break;
                case MemberOrderBy.LevelDesc:
                    query = orderedQuery
                        .ThenByDescending(m => m.MemberLevels.First(ml => ml.IsActive).Level.XPNeeded);
                    break;
            }

            var emps = await PagedList<Member>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return emps;
        }


        public async Task<PagedList<MemberVoucher>> GetAllSelfVoucher(SelfVoucherQueryParams queryParams, string requesterEmail)
        {
            var member = await _unitOfWork.MemberRepository
           .GetQuery().Where(m => m.EmailAddress == requesterEmail)
               .FirstOrDefaultAsync();

            if (member == null) throw new NotFoundException();

            var query = _unitOfWork.MemberVoucherRepository.GetQuery();
            query = query.Include(mv => mv.Voucher)
            .ThenInclude(v => v.Supplier)
            .Where(mv => mv.MemberId == member.MemberId);

            if (queryParams.VoucherId != null) query = query.Where(mv => mv.VoucherId == queryParams.VoucherId);
            if (queryParams.Status.Count > 0) query = query.Where(v => queryParams.Status.Contains(v.Status));

            switch (queryParams.OrderBy)
            {
                case MemberVoucherOrderBy.CreatedAtAsc:
                    query = query.OrderBy(v => v.CreatedAt);
                    break;
                case MemberVoucherOrderBy.CreatedAtDesc:
                    query = query.OrderByDescending(v => v.CreatedAt);
                    break;
                default:
                    break;
            }

            return await PagedList<MemberVoucher>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<PagedList<Notification>> GetSelfNotification(string email, MemberNotificationQueryParams queryParams)
        {
            var member = await _unitOfWork.MemberRepository
              .GetQuery()
              .Where(x => x.EmailAddress == email).FirstOrDefaultAsync();

            if (member == null) throw new NotFoundException();

            var query = _unitOfWork.NotificationRepository.GetQuery();
            query = query.Where(m => m.MemberId == member.MemberId);

            switch (queryParams.OrderBy)
            {
                case MemberNotificationOrderBy.DateAsc:
                    query = query.OrderBy(m => m.CreatedAt);
                    break;
                case MemberNotificationOrderBy.DateDesc:
                    query = query.OrderByDescending(m => m.CreatedAt);
                    break;
                default:
                    break;
            }

            var notis = await PagedList<Notification>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return notis;
        }

        /// <summary>
        /// Get self user info
        /// </summary>
        /// <param name="email"></param>
        /// <returns>User</returns>
        public async Task<Member?> GetSelf(string email)
        {
            var emps = await _unitOfWork.MemberRepository
              .GetQuery()
              .Where(x => x.EmailAddress == email)
                  .Include(x => x.MemberLevels.Where(ml => ml.IsActive))
                .ThenInclude(x => x.Level)
                  .Include(x => x.User)
                .ThenInclude(x => x.Role)
              .FirstOrDefaultAsync();
            return emps;
        }

        public async Task<Member?> GetMemberById(Guid memberId)
        {
            var member = await _unitOfWork.MemberRepository
              .GetQuery()
              .Where(x => x.MemberId == memberId)
                .Include(x => x.MemberLevels.Where(ml => ml.IsActive))
                  .ThenInclude(x => x.Level)
                .Include(x => x.User)
                  .ThenInclude(x => x.Role)
              .FirstOrDefaultAsync();

            if (member == null)
                throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);
            return member;
        }

        /// <summary>
        /// Return total joined and total managed
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<(int, int)> GetMemberProjectsTotalCount(Guid memberId)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(memberId)
                         ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var totalJoined = await _unitOfWork.ProjectRepository
                .GetQuery()
                .Where(x => x.ProjectMember.Any(p => p.MemberId == memberId &&
                     p.Status == ProjectMemberStatus.Active))
                .CountAsync();

            var totalManaged = await _unitOfWork.ProjectRepository
                .GetQuery()
                .Where(x => x.ProjectMember.Any(p => p.MemberId == memberId &&
                                                     p.Status == ProjectMemberStatus.Active &&
                                                     p.Role == ProjectMemberRole.Manager))
                                                    .CountAsync();

            return (totalJoined, totalManaged);
        }

        public async Task<(int, int)> GetMemberProjectsTotalCount(string email)
        {
            var member = await _unitOfWork.MemberRepository
             .GetQuery()
             .Where(x => x.EmailAddress == email).FirstOrDefaultAsync();

            if (member == null) throw new NotFoundException("There is no member with that id", ErrorNameValues.MemberNotFound);
            return await GetMemberProjectsTotalCount(member.MemberId);
        }

        public async Task<PagedList<Project>> GetMemberProjects(MemberProjectsQueryParams queryParams, Guid memberId)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(memberId) ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.ProjectRepository
                .GetQuery()
                .Include(x => x.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active &&
                                                          pm.MemberId == member.MemberId))
                .ThenInclude(x => x.Member)
                .Where(x => x.ProjectMember.Any(pm =>
                    pm.MemberId == memberId && pm.Status == ProjectMemberStatus.Active));

            if (queryParams.Status.Count > 0)
                query = query.Where(p => queryParams.Status.Contains(p.ProjectStatus));

            if (queryParams.ProjectName != null)
                query = query.Where(p =>
                  p.ProjectName.ToLower().Contains(queryParams.ProjectName.ToLower()));

            switch (queryParams.OrderBy)
            {
                case MemberProjectsOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case MemberProjectsOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.StartAfter != null)
                query = query.Where(p => p.StartedAt >= queryParams.StartAfter);

            if (queryParams.EndBefore != null)
                query = query.Where(p => p.EndedAt <= queryParams.EndBefore);

            var project = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return project;
        }

        public async Task<PagedList<Project>> GetSelfMemberProjects(MemberProjectsSelfQueryParams queryParams, string email)
        {
            var member = await _unitOfWork.MemberRepository.GetByEmail(email) ?? throw new NotFoundException("There is no member with that id", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.ProjectRepository
                .GetQuery();

            query = _unitOfWork.ProjectRepository
                .GetQuery();

            switch (queryParams.IsManager)
            {
                case null or false:
                    query = query.Include(x => x.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active && pm.MemberId == member.MemberId))
                        .ThenInclude(x => x.Member)
                        .Where(x => x.ProjectMember.Any(pm => pm.MemberId == member.MemberId && pm.Status == ProjectMemberStatus.Active));
                    break;
                case true:
                    query = query.Include(x => x.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active &&
                                                                           pm.MemberId == member.MemberId &&
                                                                           pm.Role == ProjectMemberRole.Manager))
                        .ThenInclude(x => x.Member)
                        .Where(x => x.ProjectMember.Any(pm => pm.MemberId == member.MemberId &&
                                                              pm.Status == ProjectMemberStatus.Active &&
                                                              pm.Role == ProjectMemberRole.Manager));
                    break;
            }

            if (queryParams.Status.Count > 0)
                query = query.Where(p => queryParams.Status.Contains(p.ProjectStatus));

            if (queryParams.ProjectName != null)
                query = query.Where(p =>
                  p.ProjectName.ToLower().Contains(queryParams.ProjectName.ToLower()));

            switch (queryParams.OrderBy)
            {
                case MemberProjectsOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case MemberProjectsOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            if (queryParams.StartAfter != null)
                query = query.Where(p => p.StartedAt >= queryParams.StartAfter);

            if (queryParams.EndBefore != null)
                query = query.Where(p => p.EndedAt <= queryParams.EndBefore);

            var project = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return project;
        }

        /// <summary>
        /// Update Member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="dto"></param>
        /// <returns>bool if update success</returns>
        public async Task<Member> UpdateMember(string email, MemberUpdateDTO dto)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(m => m.EmailAddress == email)
              .Include(m => m.MemberFiles)
              .ThenInclude(mf => mf.SystemFile)
              .FirstOrDefaultAsync();

            if (member == null) throw new NotFoundException();
            _mapper.Map(dto, member);

            // if (dto.ImageAsBase64 != null && dto.ImageAsFile != null)
            //   throw new BadRequestException("Please use only one image parameter to upload the image!");

            // if (dto.ImageAsFile != null)
            // {
            //   var file = dto.ImageAsFile;

            //   var newImageUrl = await _IBoxService.UploadImage(file);
            //   user.ImageUrl = newImageUrl.SharedLink.DownloadUrl;
            // }

            if (dto.ImageAsBase64 != null)
            {
                dto.ImageAsBase64 = dto.ImageAsBase64
                  .Replace("data:image/png;base64,", "")
                  .Replace("data:image/svg+xml;base64,", "")
                  .Replace("data:image/jpeg;base64,", "");

                var resultFile = await _IBoxService.UploadProfileImage(dto.ImageAsBase64, member);
                var curFile = member.MemberFiles.FirstOrDefault(f => f.FileType == MemberFileType.ProfileImage);

                if (curFile == null)
                {
                    curFile = new MemberFile()
                    {
                        FileType = MemberFileType.ProfileImage,
                        SystemFile = new SystemFile()
                        {
                            FileId = resultFile.Id,
                            DirectUrl = resultFile.SharedLink.DownloadUrl,
                            Type = SystemFileType.JPEG
                        }
                    };

                    member.MemberFiles.Add(curFile);
                }
                else
                {
                    curFile.SystemFile.UpdatedAt = DateTimeHelper.Now();
                }

                member.ImageUrl = resultFile.SharedLink.DownloadUrl;
            }
            member.UpdatedAt = DateTimeHelper.Now();

            await Update(member);
            return member;
        }

        public async Task<bool> UpdateMemberStatus(MemberStatusUpdateDTO dto)
        {
            var member = await _unitOfWork.MemberRepository.GetByIDWithUser(dto.MemberId);

            if (member == null) throw new NotFoundException();

            if (member.MemberStatus == MemberStatus.Disabled)
                throw new BadRequestException("Người dùng đã bị vô hiệu hoá và không thể được phục hồi!", ErrorNameValues.MemberNotAvailable);

            if (dto.Status == MemberStatus.Available)
            {
                member.MemberStatus = MemberStatus.Available;
                member.User.UserStatus = UserStatus.Available;
            }
            else if (dto.Status == MemberStatus.Unavailable)
            {
                member.MemberStatus = MemberStatus.Unavailable;
                member.User.UserStatus = UserStatus.Unavailable;
            }
            else if (dto.Status == MemberStatus.Disabled)
            {
                member.MemberStatus = MemberStatus.Disabled;
                member.User.UserStatus = UserStatus.Disabled;
            }

            member.UpdatedAt = DateTimeHelper.Now();

            var result = await Update(member);

            if (result)
            {
                await _redisQueueService.AddToQueue(new QueueTask
                {
                    TaskName = TaskName.CheckDisabledMemberWallet,
                    TaskData = new Dictionary<string, string>() { }
                });
            }

            return result;
        }

        public async Task<MemberAchievementDTO> GetMemberAchievement(string memberEmail)
        {
            var members = await _unitOfWork.MemberRepository
              .GetQuery().Where(m => m.EmailAddress == memberEmail)
              .Include(m => m.ProjectMembers)
                .ThenInclude(m => m.ProjectMemberReports.Where(prm => prm.ProjectReport.Status == ProjectReportStatus.Processed))
                  .ThenInclude(m => m.ProjectReportMemberTasks)
              .FirstOrDefaultAsync();

            if (members == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            MemberAchievementDTO dto = new MemberAchievementDTO
            {
                TotalWorkHours = (int)members!.ProjectMembers
                    .Sum(x => x.ProjectMemberReports
                        .Sum(x => x.ProjectReportMemberTasks
                            .Sum(p => p.TaskHour))),
                TotalTaskDone = members!.ProjectMembers
                    .Sum(x => x.ProjectMemberReports
                        .Sum(x => x.ProjectReportMemberTasks.Count()))
            };

            return dto;
        }

        public async Task<MemberAchievementDTO> GetMemberAchievement(Guid memberId)
        {
            var members = await _unitOfWork.MemberRepository
              .GetQuery().Where(m => m.MemberId == memberId)
              .Include(m => m.ProjectMembers)
                .ThenInclude(m => m.ProjectMemberReports.Where(prm => prm.ProjectReport.Status == ProjectReportStatus.Processed))
                  .ThenInclude(m => m.ProjectReportMemberTasks)
              .FirstOrDefaultAsync();

            if (members == null) throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            MemberAchievementDTO dto = new MemberAchievementDTO();
            dto.TotalWorkHours = (int)members!.ProjectMembers
              .Sum(x => x.ProjectMemberReports
                .Sum(x => x.ProjectReportMemberTasks
                .Sum(p => p.TaskHour)));
            dto.TotalTaskDone = members!.ProjectMembers
              .Sum(x => x.ProjectMemberReports
                .Sum(x => x.ProjectReportMemberTasks.Count()));

            return dto;
        }


        private async Task<List<Member>> GetAll()
        {
            var emps = await _unitOfWork.MemberRepository.GetQuery()
              .ToListAsync();
            return emps;
        }

        private async Task<Member?> GetByID(Guid id)
        {
            var emp = await _unitOfWork.MemberRepository.GetQuery()
                .Where(x => x.MemberId == id)
                    .FirstOrDefaultAsync();

            return emp;
        }

        private async Task<Member?> GetByEmail(string email)
        {
            var query = _unitOfWork.MemberRepository.GetQuery().Where(x => x.EmailAddress == email);

            var emp = await query.FirstOrDefaultAsync();

            return emp;
        }

        private async Task<bool> Insert(Member e)
        {
            _unitOfWork.MemberRepository.Add(e);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<bool> Update(Member e)
        {
            _unitOfWork.MemberRepository.Update(e);
            return await _unitOfWork.SaveAsync();
        }
    }
}