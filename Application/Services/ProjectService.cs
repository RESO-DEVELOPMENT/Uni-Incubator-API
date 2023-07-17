using Application.DTOs.Project;
using Application.Helpers;
using Application.QueryParams.Project;
using Application.QueryParams.ProjectBonus;
using Application.StateMachines;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Application.Domain;
using Application.Domain.Constants;
using Application.Domain.Enums;
using Application.Domain.Enums.Member;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.Payslip;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectFile;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.SystemFile;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.Persistence.Repositories;
using Microsoft.Data.SqlClient;
using OfficeOpenXml.Packaging.Ionic.Zip;
using Attribute = Application.Domain.Models.Attribute;

namespace Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IMapper _mapper;
        private readonly IQueueService _redisQueueService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly UnitOfWork _unitOfWork;
        private readonly IBoxService _IBoxService;

        public ProjectService(UnitOfWork unitOfWork,
                          IBoxService IBoxService,
                          IMapper mapper,
                          IQueueService redisQueueService,
                          IHttpContextAccessor httpContext)
        {
            _unitOfWork = unitOfWork;
            _IBoxService = IBoxService;
            _mapper = mapper;
            _redisQueueService = redisQueueService;
            this._httpContext = httpContext;
        }

        /// <summary>
        /// Create new project
        /// </summary>
        public async Task<Guid> CreateProject(ProjectCreateDTO dto)
        {
            //var newManager = await _unitOfWork.MemberRepository.GetQuery()
            //  .Where(u => dto.ProjectManagerEmail.Equals(u.EmailAddress))
            //  .FirstOrDefaultAsync();

            var newManager = await _unitOfWork.MemberRepository.GetByEmailWithUser(dto.ProjectManagerEmail) ?? throw new BadRequestException("There is no member with that email!", ErrorNameValues.MemberNotFound);

            if (newManager.MemberStatus != MemberStatus.Available)
                throw new BadRequestException("Thành viên đã bị vô hiệu hoá!", ErrorNameValues.MemberNotAvailable);

            if (newManager.User.RoleId == "ADMIN")
                throw new BadRequestException("Thành viên là admin hệ thống, không thể làm quản lý!", ErrorNameValues.InvalidParameters);

            // var projectMembersEmail = dto.ProjectMembers.Select(pm => pm.MemberEmail);
            // var projectMembers = await _unitOfWork.MemberRepository.GetQuery()
            //         .Where(m => projectMembersEmail.Contains(m.EmailAddress))
            //         .ToListAsync();

            // if (projectMembersEmail.ToHashSet().Count() < projectMembersEmail.Count())
            //   throw new BadRequestException("You have duplicated email in the request!", ErrorNameValues.EmailDuplicated);

            // if (projectMembersEmail.Contains(empManager.EmailAddress))
            //   throw new BadRequestException("You don't need to include mananger's mail to member list!", ErrorNameValues.EmailDuplicated);

            // Check if any email is invalid
            // var badMembers = projectMembersEmail.Where(email => !projectMembers.Select(me => me.EmailAddress).Contains(email)).ToList();
            // if (badMembers.Count() > 0)
            //   throw new NotFoundException(
            //     $"There is no member with the email(s) [{String.Join(",", badMembers)}]",
            //     ErrorNameValues.MemberNotFound);

            var newProject = new Project()
            {
                ProjectStatus = ProjectStatus.Created,
            };

            _mapper.Map(dto, newProject);

            // Add Mananger
            var attrs = new List<Attribute>() {
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.HadEnglishCertificate,
                Value = "True"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.YearsOfExperience,
                Value = "4"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.IsGraduated,
                Value = "True"
              },

              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.LeadershipSkill,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.ProblemSolvingSkill,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.PositiveAttitude,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.TeamworkSkill,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.CommunicationSkill,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.CreativitySkill,
                Value = "10"
              },

              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.SoftSkill,
                Value = "10"
              },
              new Attribute()
              {
                AttributeGroupId = AttributeGroupNameValues.HardSkill,
                Value = "10"
              },
            };

            // Add attrs to Table
            var newPm = new ProjectMember()
            {
                MemberId = newManager.MemberId,
                Member = newManager,
                Status = ProjectMemberStatus.Active,
                Role = ProjectMemberRole.Manager,
                Major = "Leader",
            };

            attrs.ForEach(att =>
            {
                newPm.ProjectMemberAttributes.Add(new ProjectMemberAttribute()
                {
                    Attribute = att
                });
            });


            newProject.ProjectMember.Add(newPm);

            newProject.ProjectWallets.AddRange(new List<ProjectWallet>() {new()
              {
                Wallet = new Wallet()
                {
                  WalletToken = WalletToken.Point,
                  WalletType = WalletType.Cold,
                  ExpiredDate = DateTimeHelper.Now().AddYears(1000),
                  Amount = 0,
                  WalletTag = "main",
                  TargetType = TargetType.Project,
                }
              },new()
              {
                Wallet = new Wallet()
                {
                  WalletToken = WalletToken.Point,
                  WalletType = WalletType.Cold,
                  ExpiredDate = DateTimeHelper.Now().AddYears(1000),
                  Amount = 0,
                  WalletTag = "bonus",
                  TargetType = TargetType.Project,
                }
              }}
            );

            // if (projectMembers.Count > 0)
            // {
            //   projectMembers.ForEach(m =>
            //   {
            //     newProject.ProjectMember.Add(new ProjectMember()
            //     {
            //       MemberId = m.MemberId,
            //       Member = m,
            //       Status = ProjectMemberStatus.Active,
            //       Role = ProjectMemberRole.Member,
            //       Major = dto.ProjectMembers.First(pm => pm.MemberEmail == m.EmailAddress).Major
            //     });
            //   });
            // }

            _unitOfWork.ProjectRepository.Add(newProject);
            try
            {
                var result = await _unitOfWork.SaveAsync();
                if (!result) throw new BadRequestException("Lỗi khi tạo dự án", ErrorNameValues.ServerError);

                var targetList = new Dictionary<string, string>();
                targetList.Add("Project", newProject.ProjectId.ToString());

                await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>()
                {
                    { "MemberId", newManager.MemberId.ToString() },
                    { "Type", NotificationType.ProjectCreate.ToString() },
                    { "TargetId", $"{JsonSerializer.Serialize(targetList)}" },
                    { "Title", $"{newProject.ProjectName}" },
                    { "Content", $"Bạn đã được chọn để làm quản lý cho dự án {newProject.ProjectName}" },
                });

                if (dto.SendEmailToPM)
                    await _redisQueueService.AddToQueue(new QueueTask()
                    {
                        TaskName = TaskName.SendMail,
                        TaskData = new Dictionary<string, string>()
                        {
                            { "ToEmail", newManager.EmailAddress },
                            { "ToName", newManager.FullName },
                            { "Type", "PROJECT_CREATE_NOTIFIY_PM" },
                            { "ProjectId", newProject.ProjectId.ToString() },
                            { "ProjectName", newProject.ProjectName }
                        }
                    });

                return newProject.ProjectId;
            }
            catch (SqlException ex)
            {
                throw new BadRequestException("Tên dư án hoặc tên viết tắt không được trùng!", ErrorNameValues.InvalidParameters);
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Tạo dự án thất bại.", ErrorNameValues.ServerError);
            }
        }

        /// <summary>
        /// Update Project As Admin
        /// </summary>
        public async Task<bool> UpdateProjectAsAdmin(ProjectAdminUpdateDTO dto)
        {
            var project = await _unitOfWork.ProjectRepository
              .GetQuery()
                .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                  .ThenInclude(pm => pm.Member)
              .FirstOrDefaultAsync(d => d.ProjectId == dto.ProjectId);

            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);
            if (project.ProjectStatus != ProjectStatus.Created && project.ProjectStatus != ProjectStatus.Started)
                throw new NotFoundException("Thông tin dự án đã được khoá!", ErrorNameValues.ProjectNotAvailable);
            project.UpdatedAt = DateTimeHelper.Now();
            // Temporary
            var budgetHold = dto.Budget ?? project.Budget;
            _mapper.Map(dto, project);
            project.Budget = budgetHold;

            _unitOfWork.ProjectRepository.Update(project);
            var result = await _unitOfWork.SaveAsync();

            return result;
        }

        public async Task<bool> UpdateProjectAsPM(ProjectPMUpdateDTO dto, string requesterEmail)
        {
            var Member = await _unitOfWork.MemberRepository.GetQuery().Where(e => e.EmailAddress == requesterEmail).FirstOrDefaultAsync();
            if (Member == null) throw new NotFoundException();

            var project = await _unitOfWork.ProjectRepository
            .GetQuery()
              .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                .ThenInclude(pm => pm.Member)
            .Where(p => p.ProjectId == dto.ProjectId!.Value!)
            .FirstOrDefaultAsync();
            if (project == null)
                throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);
            if (project.ProjectStatus != ProjectStatus.Created && project.ProjectStatus != ProjectStatus.Started)
                throw new NotFoundException("Thông tin dự án đã được khoá!", ErrorNameValues.ProjectNotAvailable);

            var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
              .Where(p => p.ProjectId == project.ProjectId
              && p.Role == ProjectMemberRole.Manager
              && p.Status == ProjectMemberStatus.Active)
              .FirstOrDefaultAsync();

            if (projectManager!.MemberId != Member.MemberId)
                throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);

            project.UpdatedAt = DateTimeHelper.Now();
            _mapper.Map(dto, project);

            _unitOfWork.ProjectRepository.Update(project);
            var result = await _unitOfWork.SaveAsync();

            return result;
        }

        public async Task<bool> UpdateProjectStatus(ProjectStatusUpdateDTO dto, string requesterEmail, bool isAdmin = false)
        {
            var Member = await _unitOfWork.MemberRepository.GetQuery().Where(e => e.EmailAddress == requesterEmail).FirstOrDefaultAsync();
            if (Member == null) throw new NotFoundException();

            var project = await _unitOfWork.ProjectRepository
            .GetQuery()
              .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                .ThenInclude(pm => pm.Member)
            .Where(p => p.ProjectId == dto.ProjectId)
            .FirstOrDefaultAsync();
            if (project == null)
                throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            if (!isAdmin)
            {
                var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
                  .Where(p => p.ProjectId == project.ProjectId
                  && p.Role == ProjectMemberRole.Manager
                  && p.Status == ProjectMemberStatus.Active)
                  .FirstOrDefaultAsync();

                if (projectManager == null || projectManager!.MemberId != Member.MemberId)
                    throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }


            if (!isAdmin)
            {
                var allowed = new List<ProjectStatus>() { ProjectStatus.Started };
                if (!allowed.Contains(dto.ProjectStatus))
                    throw new BadRequestException("Bạn chỉ có thể bắt đầu dự án!", ErrorNameValues.InvalidStateChange);
            } else
            {
                var allowed = new List<ProjectStatus>() { ProjectStatus.Started, ProjectStatus.Cancelled };
                if (!allowed.Contains(dto.ProjectStatus))
                    throw new BadRequestException("Bạn chỉ có thể bắt đầu hoặc huỷ dự án!", ErrorNameValues.InvalidStateChange);
            }

            try
            {
                ProjectStateMachine psm = new ProjectStateMachine(project);
                project = psm.TriggerState((ProjectStatus)dto.ProjectStatus);

                if (dto.ProjectStatus == ProjectStatus.Started)
                {
                    project.EndedAt = DateTimeHelper.Now().AddDays(GlobalVar.SystemConfig.ProjectDuration);

                    var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetQuery()
                      .OrderByDescending(s => s.CreatedAt)
                      .Include(p => p.Payslips)
                      .Where(s => s.SalaryCycleStatus < SalaryCycleStatus.Paid)
                      .FirstOrDefaultAsync();

                    if (currentSalaryCycle != null)
                    {
                        var membersInPayslip = currentSalaryCycle.Payslips.Select(x => x.MemberId).ToList();
                        var membersInProject = await _unitOfWork.ProjectMemberRepository.GetQuery()
                            .Include(x => x.Member.MemberLevels.Where(x => x.IsActive)).ThenInclude(x => x.Level)
                            .Where(x => x.ProjectId == project.ProjectId && x.Status == ProjectMemberStatus.Active)
                          .ToListAsync();

                        var memberWithMissingPayslip = membersInProject.Where(m => !membersInPayslip.Contains(m.MemberId)).ToList();
                        if (memberWithMissingPayslip.Count > 0)
                        {
                            memberWithMissingPayslip.ForEach(projectMember =>
                            {
                                var member = projectMember.Member;
                                var level = member.MemberLevels.First().Level;

                                var attrs = new List<PayslipAttribute>() {
                  new PayslipAttribute()
                  {
                    Attribute = new Attribute()
                    {
                      AttributeGroupId = AttributeGroupNameValues.BasePoint,
                      Value = level.BasePoint.ToString()
                    }
                  },
                  new PayslipAttribute()
                  {
                    Attribute = new Attribute()
                    {
                      AttributeGroupId = AttributeGroupNameValues.PointPerHour,
                      Value = level.BasePointPerHour.ToString()
                    }
                  },
                              };

                                var payslipForMember = new Payslip()
                                {
                                    MemberId = member.MemberId,
                                    Status = PayslipStatus.Created,
                                    Note = $"Phiếu lương cho <@SalaryCycle:{currentSalaryCycle.SalaryCycleId}>",
                                    PayslipAttributes = attrs
                                };

                                currentSalaryCycle.Payslips.Add(payslipForMember);
                            });
                        }
                    }
                }
            }
            catch
            {
                throw new BadRequestException("Chuyển trạng thái không hợp lệ!", ErrorNameValues.InvalidStateChange);
            }

            project.UpdatedAt = DateTimeHelper.Now();

            _unitOfWork.ProjectRepository.Update(project);
            var result = await _unitOfWork.SaveAsync();

            // For Notification
            if (result)
            {
                var userIds = project.ProjectMember.Select(pm => pm.Member.MemberId).ToList();
                userIds.ForEach(async (id) =>
                {
                    var targetList = new Dictionary<string, string>();
                    targetList.Add("Project", project.ProjectId.ToString());

                    await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
            {"MemberId", id.ToString()},
            {"Type", NotificationType.ProjectUpdate.ToString()},
            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
            {"Content", $"Dự án {project.ProjectName} đã {ProjectStatusTranslate.Translate(dto.ProjectStatus)}!"},
            {"Title", $"{project.ProjectName}"}
                  });
                });
            }

            return result;
        }

        /// <summary>
        /// Get all project
        /// </summary>
        public async Task<PagedList<Project>> GetAll(ProjectQueryParams queryParams)
        {
            var query = _unitOfWork.ProjectRepository.GetQuery();

            query = query
            .Include(p => p.ProjectFiles)
              .ThenInclude(pf => pf.SystemFile)
            .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
                .ThenInclude(pe => pe.Member);

            // Default to only public project
            if (!queryParams.IncludePrivate)
                query = query.Where(p => p.ProjectVisibility == ProjectVisibility.Public);

            if (queryParams.Status.Count > 0)
                query = query.Where(p => queryParams.Status.Contains(p.ProjectStatus));


            if (queryParams.ProjectName != null)
                query = query.Where(p =>
                  p.ProjectName.ToLower().Contains(queryParams.ProjectName.ToLower()));

            if (queryParams.ManagerEmail != null)
                query = query.Where(p =>
                    p.ProjectMember.Any(pe =>  // Get project with Member is manager
                      pe.Member.EmailAddress == queryParams.ManagerEmail &&
                      pe.Role == ProjectMemberRole.Manager)
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
                query = query.Where(p => p.EndedAt != null && p.EndedAt <= queryParams.EndBefore);

            var projects = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return projects;
        }

        public async Task<List<ProjectWithTotalFundDTO>> GetAllWithTransactions(ProjectMinimalQueryParams queryParams)
        {
            var query = _unitOfWork.ProjectRepository.GetQuery();

            query = query
            // .Include(p => p.ProjectSponsors.Where(ps => ps.Status == ProjectSponsorStatus.Available))
            // .ThenInclude(x => x.ProjectSponsorTransactions)
            .Where(p => p.ProjectSponsors.Where(x => x.ProjectSponsorTransactions.Count() > 0).Count() > 0);


            switch (queryParams.OrderBy)
            {
                case ProjectOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case ProjectOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }
            var projects = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);

            var transactionsForProjects = await _unitOfWork.ProjectSponsorTransactionRepository.GetQuery()
              .Where(pr => projects.Select(p => p.ProjectId).Contains(pr.ProjectSponsor.ProjectId))
              .GroupBy(p => p.ProjectSponsor.ProjectId)
              .ToListAsync();

            Pagination.AddPaginationHeader(_httpContext.HttpContext!.Response, projects);

            var projectsWithFund = new List<ProjectWithTotalFundDTO>();

            projects.ForEach(p =>
            {
                var mappedProject = new ProjectWithTotalFundDTO();
                _mapper.Map(p, mappedProject);

                var trx = transactionsForProjects.Where(id => id.Key == mappedProject.ProjectId).ToList();
                mappedProject.TotalSponsorsed = trx.Sum(x => x.Sum(y => y.Amount));
                projectsWithFund.Add(mappedProject);
            });


            return projectsWithFund;
        }

        /// <summary>
        /// Get all self project (Include your private)
        /// </summary>
        public async Task<PagedList<Project>> GetAllSelf(ProjectSelfQueryParams queryParams, string requesterEmail)
        {
            var query = _unitOfWork.ProjectRepository.GetQuery();
            query = query
              .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
                .ThenInclude(pe => pe.Member)
                  .Include(p => p.ProjectFiles)
                    .ThenInclude(p => p.SystemFile);

            Member? emp = null;
            if (requesterEmail != null)
                emp = await _unitOfWork.MemberRepository.GetQuery()
                  .Where(e => e.EmailAddress == requesterEmail)
                  .FirstOrDefaultAsync();

            // Get public and private by Member
            // query = query.Where(p => p.ProjectVisibility == ProjectVisibility.Public);
            query = query.Where(p => p.ProjectMember.Any(e => e.MemberId == emp!.MemberId && e.Status == ProjectMemberStatus.Active));

            if (queryParams.Status.Count > 0)
                query = query.Where(p => queryParams.Status.Contains(p.ProjectStatus));

            if (queryParams.ProjectName != null)
                query = query.Where(p =>
                  p.ProjectName.ToLower().Contains(queryParams.ProjectName.ToLower()));

            switch (queryParams.OrderBy)
            {
                case ProjectOrderBy.DateAsc:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case ProjectOrderBy.DateDesc:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var projects = await PagedList<Project>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return projects;
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        public async Task<Project?> GetProjectById(Guid projectId, string? requesterEmail = null, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
              .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
                .ThenInclude(pe => pe.Member)
                  .ThenInclude(m => m.MemberLevels.Where(ml => ml.IsActive))
                    .ThenInclude(ml => ml.Level)
                  .Include(p => p.ProjectFiles)
                    .ThenInclude(pf => pf.SystemFile)
                  .Where(p => p.ProjectId == projectId)
                      .FirstOrDefaultAsync();

            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            // If project is private only members who are active in project can view
            if (!isAdmin)
            {
                Member? emp = null;
                if (requesterEmail != null)
                    emp = await _unitOfWork.MemberRepository.GetQuery()
                      .Where(e => e.EmailAddress == requesterEmail)
                      .FirstOrDefaultAsync();


                if (project.ProjectVisibility == ProjectVisibility.Private
                && !project.ProjectMember.Select(pe => pe.MemberId).Contains(emp!.MemberId))
                {
                    throw new NotFoundException("Dự án không tồn tại*!", ErrorNameValues.ProjectIsPrivate);
                }
            }

            return project;
        }

        /// <summary>
        /// Get project wallet
        /// </summary>
        public async Task<List<Wallet>> GetProjectWalletById(Guid projectId, string requesterEmail, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
              .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
              .Include(p => p.ProjectWallets)
                .ThenInclude(p => p.Wallet)
                  .Where(p => p.ProjectId == projectId)
                      .FirstOrDefaultAsync();

            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            // If project is private only members who are active in project can view
            if (!isAdmin)
            {
                Member? member = null;
                if (requesterEmail != null)
                    member = await _unitOfWork.MemberRepository.GetQuery()
                      .Where(e => e.EmailAddress == requesterEmail)
                      .FirstOrDefaultAsync();

                if (member == null || !project.ProjectMember.Select(pe => pe.MemberId).Contains(member!.MemberId))
                    throw new BadRequestException("Bạn không phải thành viên dự án!", ErrorNameValues.NoPermission);
            }

            var wallets = project.ProjectWallets.Select(p => p.Wallet).ToList();
            return wallets;
        }

        public async Task<bool> SendPointToProject(Guid projectId, ProjectSendPointDTO dto)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
              .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
              .Include(p => p.ProjectWallets)
                .ThenInclude(p => p.Wallet)
                  .Where(p => p.ProjectId == projectId)
                      .FirstOrDefaultAsync();

            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);
            if (project.ProjectStatus != ProjectStatus.Created && project.ProjectStatus != ProjectStatus.Started)
                throw new BadRequestException("Dự án không thể được thêm point nữa!", ErrorNameValues.ProjectNotAvailable);

            var wallet = project.ProjectWallets.FirstOrDefault();
            if (wallet == null) throw new BadRequestException("Dự án bị lỗi, xin hạy liên hệ admin!", ErrorNameValues.SystemError);

            var targetToSend = JsonSerializer.Serialize(new List<String>() { "main" });

            await _redisQueueService.AddToQueue(TaskName.SendPoint, new Dictionary<string, string>()
              {
                {"FromId", Guid.Empty.ToString()},
                {"ToId", project.ProjectId.ToString()},
                {"TransactionType", TransactionType.SystemDepositToProject.ToString()},
                {"Amount", dto.Amount.ToString()},
                {"ToTag", targetToSend},
              });

            return true;
        }

        public async Task<PagedList<PayslipItem>> GetBonusFromProject(Guid projectId, ProjectBonusQueryParams queryParams, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
                    .Include(p => p.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
                    .Include(p => p.ProjectWallets)
                      .ThenInclude(p => p.Wallet)
                        .Where(p => p.ProjectId == projectId)
                            .FirstOrDefaultAsync();

            if (project == null) throw new NotFoundException("Dự án không tồn tại!");

            var requestMember = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == requesterEmail)
              .FirstOrDefaultAsync();

            if (!project.ProjectMember.Where(pm => pm.Role == ProjectMemberRole.Manager).Select(pe => pe.MemberId).Contains(requestMember!.MemberId))
                throw new NotFoundException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);

            var query = _unitOfWork.PayslipItemRepository.GetQuery()
            .Where(psi => psi.ProjectId == projectId &&
              psi.Type == PayslipItemType.Bonus);

            if (queryParams.SalaryCycleId != null) query = query.Where(p => queryParams.SalaryCycleId == p.Payslip.SalaryCycleId);

            return await PagedList<PayslipItem>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }

        public async Task<Project?> UploadProjectFinalReport(Guid projectId, IFormFile file)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
                .Include(p => p.ProjectFiles)
                  .ThenInclude(pf => pf.SystemFile)
                    .Where(p => p.ProjectId == projectId)
                      .FirstOrDefaultAsync();

            if (project == null) throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var resultFile = await _IBoxService.UploadProjectFile(file, project, ProjectFileType.FinalReport);
            var curFile = project.ProjectFiles.FirstOrDefault();

            if (curFile == null)
            {
                curFile = new ProjectFile()
                {
                    FileType = ProjectFileType.FinalReport,
                    SystemFile = new SystemFile()
                    {
                        FileId = resultFile.Id,
                        DirectUrl = resultFile.SharedLink.DownloadUrl,
                        Type = SystemFileType.PDF
                    }
                };

                project.ProjectFiles.Add(curFile);
            }
            else
            {
                curFile.SystemFile.UpdatedAt = DateTimeHelper.Now();
            }

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return project;
        }
    }
}