using System.Text.Json;
using Application.Domain.Constants;
using Application.Domain.Enums;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Models;
using Application.DTOs.ProjectReport;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectReport;
using Application.StateMachines;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Attribute = Application.Domain.Models.Attribute;

namespace Application.Services
{
    public class ProjectReportService : IProjectReportService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueueService _queueService;
        private readonly UnitOfWork _unitOfWork;

        public ProjectReportService(UnitOfWork unitOfWork,
                           IMapper mapper,
                           IHttpContextAccessor httpContextAccessor,
                           IQueueService queueService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _queueService = queueService;
        }

        /// <summary>
        /// Get specific report
        /// </summary>
        public async Task<ProjectReportWithTasksDTO> GetProjectReport(Guid reportId, string requesterEmail, bool isAdmin = false)
        {
            var reportExist = await _unitOfWork.ProjectReportRepository.GetByID(reportId) ?? throw new NotFoundException("Báo cáo không tồn tại!", ErrorNameValues.ReportNotFound);

            var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
              .Where(p => p.ProjectId == reportExist!.ProjectId
              && p.Status == ProjectMemberStatus.Active
              && p.MemberId == requesterMember.MemberId)
              .FirstOrDefaultAsync();

            if (!isAdmin)
            {
                if (projectManager == null || projectManager!.MemberId != requesterMember!.MemberId)
                    throw new BadRequestException("Bạn không phải thành viên dự án!", ErrorNameValues.NoPermission);
            }
            var query = _unitOfWork.ProjectReportRepository.GetReportFullQuery(reportId);

            var detailledReport = await query.FirstOrDefaultAsync();

            // Calculate And Convert
            var mappedReport = _mapper.Map<ProjectReportWithTasksDTO>(detailledReport);
            var result = BenefitHelper.EstimateRewardsForReport(detailledReport);

            mappedReport.TaskPointNeeded = result.TotalP2 + result.TotalP3 + result.TotalTaskPoint;
            mappedReport.BonusPointNeeded = result.TotalBonusPoint;

            return mappedReport;
        }

        public async Task<List<ProjectReportWithProjectAndSalaryCycleDTO>> GetProjectsReports(ProjectsReportQueryParams queryParams, string requesterEmail, bool isAdmin = false)
        {
            var query = _unitOfWork.ProjectReportRepository.GetReportsFullQuery();
            query = query
              .Include(pr => pr.Project);

            if (!isAdmin)
            {
                var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
                if (queryParams.ProjectId != null)
                {
                    query = query.Where(q =>
                        q.Project.ProjectMember.Any(pm =>
                            pm.Status == ProjectMemberStatus.Active &&
                            pm.Role == ProjectMemberRole.Manager &&
                            pm.MemberId == requesterMember!.MemberId &&
                            pm.ProjectId == queryParams.ProjectId)
                    );
                }
                else
                {
                    query = query.Where(q =>
                        q.Project.ProjectMember.Any(pm => pm.Status == ProjectMemberStatus.Active &&
                                                          pm.Role == ProjectMemberRole.Manager &&
                                                          pm.MemberId == requesterMember!.MemberId)
                    );
                }
            }
            else
            {
                if (queryParams.ProjectId != null) query = query.Where(r => r.ProjectId == queryParams.ProjectId);
            }

            switch (queryParams.OrderBy)
            {
                case ProjectReportOrderBy.CreatedAtDesc:
                    {
                        query = query.OrderByDescending(x => x.CreatedAt);
                        break;
                    }
                case ProjectReportOrderBy.CreateAtAsc:
                    {
                        query = query.OrderBy(x => x.CreatedAt);
                        break;
                    }
            }

            if (queryParams.SalaryCycleId != null) query = query.Where(r => r.SalaryCycleId == queryParams.SalaryCycleId);
            if (queryParams.Status.Any()) query = query.Where(r => queryParams.Status.Contains(r.Status));

            var reports = await PagedList<ProjectReport>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);

            _httpContextAccessor.HttpContext?.Response.AddPaginationHeader(reports);
            // Calculate And Convert
            var mappedReports = _mapper.Map<List<ProjectReportWithProjectAndSalaryCycleDTO>>(reports);

            mappedReports.ForEach(reportDTO =>
            {
                var report = reports.First(x => x.ReportId == reportDTO.ReportId);
                var result = BenefitHelper.EstimateRewardsForReport(report);

                reportDTO.TaskPointNeeded = result.TotalP2 + result.TotalP3 + result.TotalTaskPoint;
                reportDTO.BonusPointNeeded = result.TotalBonusPoint;
            });

            return mappedReports;
        }

        public async Task<List<ProjectReportWithTasksDTO>> GetProjectReports(Guid projectId,
            ProjectReportQueryParams queryParams, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId) ??
                          throw new NotFoundException("Project not found", ErrorNameValues.ProjectNotFound);

            var member = await _unitOfWork.MemberRepository.GetQuery().FirstOrDefaultAsync(m => m.EmailAddress == requesterEmail);

            var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
              .Where(p => p.ProjectId == projectId
              && p.Status == ProjectMemberStatus.Active)
              .FirstOrDefaultAsync();

            if (projectManager == null || projectManager.MemberId != member!.MemberId)
                throw new BadRequestException("You don't have permission!", ErrorNameValues.NoPermission);

            var query = _unitOfWork.ProjectReportRepository.GetReportsFullQuery();

            query = query
              .Where(r => r.ProjectId == projectId)
              .Include(pr => pr.Project);

            switch (queryParams.OrderBy)
            {
                case ProjectReportOrderBy.CreatedAtDesc:
                    {
                        query = query.OrderByDescending(x => x.CreatedAt);
                        break;
                    }
                case ProjectReportOrderBy.CreateAtAsc:
                    {
                        query = query.OrderBy(x => x.CreatedAt);
                        break;
                    }
            }

            if (queryParams.SalaryCycleId != null) query = query.Where(r => r.SalaryCycleId == queryParams.SalaryCycleId);
            if (queryParams.Status.Any()) query = query.Where(r => queryParams.Status.Contains(r.Status));

            var reports = await PagedList<ProjectReport>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);

            _httpContextAccessor.HttpContext?.Response.AddPaginationHeader(reports);
            var mappedReports = _mapper.Map<List<ProjectReportWithTasksDTO>>(reports);

            mappedReports.ForEach(reportDTO =>
            {
                var report = reports.First(x => x.ReportId == reportDTO.ReportId);
                var result = BenefitHelper.EstimateRewardsForReport(report);

                reportDTO.TaskPointNeeded = result.TotalP2 + result.TotalP3 + result.TotalTaskPoint;
                reportDTO.BonusPointNeeded = result.TotalBonusPoint;
            });

            return mappedReports;
        }

        public async Task<ProjectReportEstimateDTO> EstimateProjectReportReward(Guid projectReportId, string requesterEmail, bool isAdmin = false)
        {
            var reportFull = await _unitOfWork.ProjectReportRepository.GetReportFullQuery(projectReportId).FirstOrDefaultAsync();
            if (reportFull == null) throw new NotFoundException();
            var project = reportFull!.Project;

            if (!isAdmin)
            {
                var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
                    .Where(pm => pm.ProjectId == reportFull.ProjectId &&
                      pm.Status == ProjectMemberStatus.Active &&
                      pm.Role == ProjectMemberRole.Manager)
                    .Include(pm => pm.Member).FirstOrDefaultAsync();

                if (projectManager == null || projectManager.Member.EmailAddress != requesterEmail)
                    throw new NotFoundException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            // Calculatore
            var result = BenefitHelper.EstimateRewardsForReport(reportFull);

            return result;
        }

        public async Task<ProjectReportEstimateDTO> EstimateProjectReportReward(ProjectReportEstimateCreateDTO dto, string requesterEmail, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
              .Where(p => p.ProjectId == dto.ProjectId)
                .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                  .ThenInclude(pm => pm.ProjectMemberAttributes)
                    .ThenInclude(pa => pa.Attribute)
                .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                  .ThenInclude(pm => pm.Member.MemberLevels.Where(ml => ml.IsActive))
                    .ThenInclude(ml => ml.Level)
              .FirstOrDefaultAsync() ?? throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var salaryCycle = await _unitOfWork.SalaryCycleRepository.GetQuery()
              .Where(x => x.SalaryCycleId == dto.SalaryCycleId)
                .Include(sc => sc.Payslips)
                  .ThenInclude(ps => ps.PayslipAttributes)
                    .ThenInclude(ps => ps.Attribute)
                    .FirstOrDefaultAsync();

            if (salaryCycle == null) throw new NotFoundException("Kỳ lương không tồn tại!", ErrorNameValues.SalaryCycleNotFound);

            // Check if PM
            if (!isAdmin)
            {
                var projectManager = await _unitOfWork.ProjectMemberRepository.GetQuery()
                    .Where(pm => pm.ProjectId == project.ProjectId &&
                      pm.Status == ProjectMemberStatus.Active &&
                      pm.Role == ProjectMemberRole.Manager)
                    .Include(pm => pm.Member).FirstOrDefaultAsync();

                if (projectManager == null || projectManager.Member.EmailAddress != requesterEmail)
                    throw new NotFoundException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var dtoMemberEmails = dto.MemberTasks.DistinctBy(m => m.MemberEmail).Select(m => m.MemberEmail);

            var membersDb = await _unitOfWork.MemberRepository.GetQuery()
              .Where(m => dtoMemberEmails.Contains(m.EmailAddress))
              .ToListAsync();

            // Check if any email is invalid
            var badMembers = dtoMemberEmails.Where(email => !membersDb.Select(me => me.EmailAddress).Contains(email)).ToList();
            if (badMembers.Count > 0)
                throw new NotFoundException(
                  $"Thành viên không tồn tại [{string.Join(",", badMembers)}]",
                  ErrorNameValues.MemberNotFound);

            // Check if report have member which is not in team
            var unneededMembers = dtoMemberEmails.Where(email => !project.ProjectMember.Select(me => me.Member.EmailAddress).Contains(email));

            if (unneededMembers.Count() > 0)
                throw new BadRequestException($"Thành viên không nằm trong dự án [{string.Join(",", unneededMembers)}]", ErrorNameValues.DafuqMembers);

            // Setup report to add
            ProjectReportEstimateDTO result = new ProjectReportEstimateDTO();

            membersDb.ForEach((member) =>
            {
                var workHours = 0d;
                var workRealHours = 0d;
                // Setup tasks for each members
                var tasks = dto.MemberTasks.Where(m => m.MemberEmail == member.EmailAddress)
            .ToList();

                var totalTaskPoint = 0d;
                var totalBonusPoint = 0d;

                tasks.ForEach(task =>
                {
                    workHours += task.TaskHour;
                    workRealHours += task.TaskRealHour;
                    totalTaskPoint += task.TaskPoint * (task.TaskEffort / 100);
                    totalBonusPoint += task.TaskBonus;
                });


                var projectMember = project.ProjectMember.First(m => m.MemberId == member.MemberId);

                var memberPayslip = salaryCycle.Payslips.First(x => x.MemberId == projectMember.MemberId);
                var memberAttsInPayslip = memberPayslip.PayslipAttributes.Select(pma => pma.Attribute).ToList();

                var basePointPerHour = double.Parse(memberAttsInPayslip.First(a => a.AttributeGroupId == AttributeGroupNameValues.PointPerHour).Value);
                var basePoint = double.Parse(memberAttsInPayslip.First(a => a.AttributeGroupId == AttributeGroupNameValues.BasePoint).Value);

                var memberAttsInProject = projectMember.ProjectMemberAttributes.Select(pma => pma.Attribute).ToList();

                var softSkill = double.Parse(memberAttsInProject.First(a => a.AttributeGroupId == AttributeGroupNameValues.SoftSkill).Value);
                var hardSkill = double.Parse(memberAttsInProject.First(a => a.AttributeGroupId == AttributeGroupNameValues.HardSkill).Value);

                var (personPoint, performancePoint, xp) = BenefitHelper.CalculateP2_P3_XP(
                    basePointPerHour,
                    softSkill,
                    hardSkill,
                    workHours,
                    workRealHours,
                    tasks.Count);

                // var taskPointSum = tasks.Sum(t => t.TaskPoint);
                // performancePoint += taskPointSum;

                //performancePoint += totalTaskPoint;
                var totalTaskPointForMember = personPoint + performancePoint;
                // var totalTaskPointForMember = basePoint + personPoint + performancePoint + totalBonusPoint;

                result.MemberRewards.Add(new()
                {
                    MemberId = member.MemberId,
                    MemberEmail = member.EmailAddress,
                    P2 = personPoint,
                    P3 = performancePoint,
                    TaskPoint = totalTaskPoint,
                    XP = xp,
                    Bonus = totalBonusPoint
                });

                result.TotalP2 += personPoint;
                result.TotalP3 += performancePoint;
                result.TotalTaskPoint += totalTaskPoint;
                result.TotalBonusPoint += totalBonusPoint;
            });

            return result;
        }

        public async Task<Guid> CreateProjectReport(Guid projectId, ProjectReportCreateDTO dto, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
              .Where(p => p.ProjectId == projectId)
                .Include(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                    .ThenInclude(pm => pm.Member)
              .Include(p => p.ProjectReports.Where(r => r.SalaryCycleId == dto.SalaryCycleId)
                      .OrderByDescending(rp => rp.CreatedAt))
                     .FirstOrDefaultAsync() ?? throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            if (project.ProjectStatus != ProjectStatus.Started && project.ProjectStatus != ProjectStatus.Stopped)
                throw new BadRequestException("Dự án chỉ có thể được báo cáo khi đã bắt đầu hoặc dừng!", ErrorNameValues.ProjectUnavailable);

            // Check if PM
            var requestMember = project.ProjectMember.FirstOrDefault(m => m.Member.EmailAddress == requesterEmail);
            if (requestMember != null && requestMember!.Role != ProjectMemberRole.Manager)
                throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);

            var salaryCycle = await _unitOfWork.SalaryCycleRepository
              .GetQuery()
              .FirstOrDefaultAsync(psc => psc.SalaryCycleId == dto.SalaryCycleId);

            if (salaryCycle == null)
                throw new NotFoundException("Kỳ lương không tồn tại!", ErrorNameValues.SalaryCycleNotFound);

            if (salaryCycle.SalaryCycleStatus != SalaryCycleStatus.Ongoing)
                throw new NotFoundException("Kỳ lương đã bị khoá!", ErrorNameValues.SalaryCycleNotAvailable);

            // Check if there is already an active report
            var activeReportStatuses = new List<ProjectReportStatus>() { ProjectReportStatus.Drafted, ProjectReportStatus.Created };

            if (project.ProjectReports.FirstOrDefault(rp => activeReportStatuses.Contains(rp.Status)) != null)
                throw new BadRequestException("Dự án đang có báo cáo chờ!", ErrorNameValues.RequestDuplicated);

            var acceptedReportStatuses = new List<ProjectReportStatus>() { ProjectReportStatus.Accepted, ProjectReportStatus.Processed };

            if (project.ProjectReports.FirstOrDefault(rp => acceptedReportStatuses.Contains(rp.Status)) != null)
                throw new BadRequestException("Dự án đã được duyệt báo cáo trong kỳ lương này!", ErrorNameValues.RequestSalaryCycleAccepted);

            var report = new ProjectReport()
            {
                ProjectId = projectId,
                Status = ProjectReportStatus.Drafted,
                SalaryCycle = salaryCycle
            };

            _unitOfWork.ProjectReportRepository.Add(report);
            var result = await _unitOfWork.SaveAsync();

            return report.ReportId;
        }

        public async Task<bool> UpdateProjectReport(ProjectReportUpdateDTO dto, string requesterEmail)
        {
            var reportMinimal = await _unitOfWork.ProjectReportRepository.GetQuery()
            .Where(pr => pr.ReportId == dto.ProjectReportId).FirstOrDefaultAsync();

            if (reportMinimal == null) throw new NotFoundException("Report not found!", ErrorNameValues.ReportNotFound);
            if (reportMinimal.Status != ProjectReportStatus.Drafted) throw new BadRequestException("Report is not editable", ErrorNameValues.ReportNotUpdatable);

            var reportFull = await _unitOfWork.ProjectReportRepository.GetReportFullQuery(dto.ProjectReportId).FirstOrDefaultAsync();
            //.Where(pr => pr.ReportId == dto.ProjectReportId)
            //  .Include(pr => pr.ProjectReportMembers)
            //    .ThenInclude(prm => prm.ProjectReportMemberTasks)

            //.Include(pr => pr.Project)
            //      .ThenInclude(p => p.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
            //      .ThenInclude(pm => pm.ProjectMemberAttributes)
            //      .ThenInclude(pma => pma.Attribute)

            //.Include(pr => pr.ProjectReportMembers)
            //    .ThenInclude(x => x.ProjectReportMemberAttributes)
            //    .ThenInclude(x => x.Attribute)

            //.FirstAsync();

            var project = reportFull.Project;

            var projectManager = await _unitOfWork.ProjectMemberRepository
              .TryGetProjectMemberManagerActive(project.ProjectId, requesterEmail);

            if (projectManager == null)
                throw new NotFoundException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);

            var dtoMemberEmails = dto.MemberTasks.Select(m => m.MemberEmail);

            var membersDb = await _unitOfWork.MemberRepository.GetQuery()
              .Where(m => dtoMemberEmails.Contains(m.EmailAddress))
              .ToListAsync();

            // Check if any email is invalid
            var badMembers = dtoMemberEmails.Where(email => !membersDb.Select(me => me.EmailAddress).Contains(email)).ToList();
            if (badMembers.Count > 0)
                throw new NotFoundException(
                  $"Thành viên không tồn tại [{string.Join(",", badMembers)}]",
                  ErrorNameValues.MemberNotFound);

            // Check if report have member which is not in team
            var unneededMembers = dtoMemberEmails.Where(email => !project.ProjectMember.Select(me => me.Member.EmailAddress).Contains(email));

            if (unneededMembers.Any())
                throw new BadRequestException($"Thành viên không có trong dự án [{string.Join(",", unneededMembers)}]", ErrorNameValues.DafuqMembers);

            // Setup report to add
            var membersReport = new List<ProjectReportMember>();
            membersDb.ForEach((member) =>
            {
                // Setup tasks for each members
                var tasksDto = dto.MemberTasks.Where(m => m.MemberEmail == member.EmailAddress).ToList();
                var projectMember = project.ProjectMember.First(x => x.MemberId == member.MemberId);
                var memberTasks = new List<ProjectReportMemberTask>();

                tasksDto.ForEach(task =>
                  {
                      memberTasks.Add(new ProjectReportMemberTask()
                      {
                          TaskName = task.TaskName,
                          TaskDescription = task.TaskDescription,
                          TaskHour = task.TaskHour,
                          TaskRealHour = task.TaskRealHour,
                          TaskPoint = task.TaskPoint,
                          TaskBonus = task.TaskBonus,
                          BonusReason = task.BonusReason,
                          TaskCode = task.TaskCode,
                          TaskEffort = task.TaskEffort
                      });
                  });

                membersReport.Add(new ProjectReportMember()
                {
                    ProjectMemberId = project.ProjectMember.First(pm => pm.MemberId == member.MemberId).ProjectMemberId,
                    ProjectReportMemberTasks = memberTasks,
                    ProjectReportMemberAttributes = new List<ProjectReportMemberAttribute>()
                    {
                        new()
                        {
                            Attribute = new Attribute
                            {
                                AttributeGroupId = AttributeGroupNameValues.SoftSkill,
                                Value = projectMember.ProjectMemberAttributes.First(x => x.Attribute.AttributeGroupId == AttributeGroupNameValues.SoftSkill).Attribute.Value
                            }
                        },
                        new()
                        {
                            Attribute = new Attribute
                            {
                                AttributeGroupId = AttributeGroupNameValues.HardSkill,
                                Value = projectMember.ProjectMemberAttributes.First(x => x.Attribute.AttributeGroupId == AttributeGroupNameValues.HardSkill).Attribute.Value
                            }
                        }
                    }
                });
            });

            // Remove Old Taks
            var oldList = reportFull.ProjectReportMembers.ToList();
            var oldAtts = reportFull.ProjectReportMembers.SelectMany(x => x.ProjectReportMemberAttributes).ToList();
            var oldTaskList = oldList.SelectMany(ol => ol.ProjectReportMemberTasks).ToList();

            _unitOfWork.ProjectReportMemberTaskRepository.Delete(oldTaskList);
            _unitOfWork.ProjectReportMemberAttributeRepository.Delete(oldAtts);
            _unitOfWork.ProjectReportMemberRepository.Delete(oldList);

            // Add New Tasks
            reportFull.ProjectReportMembers.AddRange(membersReport);

            _unitOfWork.ProjectReportRepository.Update(reportFull);
            var result = await _unitOfWork.SaveAsync();

            //var benefitResult = BenefitHelper.EstimateRewardsForReport(reportFull);
            //reportFull
            return result;
        }

        public async Task<bool> UpdateProjectReportStatus(ProjectReportStatusUpdateDTO dto, string requesterEmail, bool isAdmin = false)
        {
            var reportMinimal = await _unitOfWork.ProjectReportRepository
              .GetQuery()
              .Where(r => r.ReportId == dto.ReportId)
              .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Bản báo cáo không tồn tại!", ErrorNameValues.ReportNotFound);

            var sc = await _unitOfWork.SalaryCycleRepository.GetByID(reportMinimal.SalaryCycleId);
            if (sc!.SalaryCycleStatus != SalaryCycleStatus.Ongoing)
                if (dto.Status is not (ProjectReportStatus.Accepted or ProjectReportStatus.Rejected))
                    throw new NotFoundException("Kỳ lương không còn có thể nộp báo cáo!", ErrorNameValues.SalaryCycleNotAvailable);

            if (!isAdmin)
            {
                var pmList = await _unitOfWork.ProjectMemberRepository.GetQuery()
                       .Where(pm => pm.ProjectId == reportMinimal.ProjectId && pm.Status == ProjectMemberStatus.Active)
                         .Include(pm => pm.Member)
                       .ToListAsync();

                // Check if PM
                var requestMember = pmList.FirstOrDefault(m => m.Member.EmailAddress == requesterEmail);
                if (requesterEmail == null || requestMember!.Role != ProjectMemberRole.Manager)
                    throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var report = await _unitOfWork.ProjectReportRepository
              .GetReportFullQuery(dto.ReportId)
              .Include(r => r.Project)
              .FirstAsync();


            try
            {
                var allowedForPM = new List<ProjectReportStatus>()
                    { ProjectReportStatus.Created, ProjectReportStatus.Cancelled };
                var disallowed = new List<ProjectReportStatus>() { ProjectReportStatus.Processed };
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

                var prsm = new ProjectReportStateMachine(report!);
                prsm.TriggerState(dto.Status);
                if (dto.Note != null) report.Note = dto.Note;

                switch (dto.Status)
                {
                    //if (dto.Status is ProjectReportStatus.Accepted or ProjectReportStatus.Created)
                    case ProjectReportStatus.Accepted:
                        {
                            var projectWallets = await _unitOfWork.ProjectWalletRepository.GetQuery()
                                .Include(w => w.Wallet)
                                .Where(w => w.ProjectId == report.ProjectId)
                                .ToListAsync();

                            var pointWallet = projectWallets.First(w => w.Wallet.WalletTag == "main");
                            var bonusWallet = projectWallets.First(w => w.Wallet.WalletTag == "bonus");

                            var estimateResult = BenefitHelper.EstimateRewardsForReport(report);
                            var totalProjectPointNeeded =
                                estimateResult.TotalP2 + estimateResult.TotalP3 + estimateResult.TotalTaskPoint;

                            if (totalProjectPointNeeded > pointWallet.Wallet.Amount)
                            {
                                throw new BadRequestException("Dự án không đủ point để trả cho P2, P3 và điểm task!", ErrorNameValues.InsufficentToken);
                            }

                            if (estimateResult.TotalBonusPoint > bonusWallet.Wallet.Amount)
                            {
                                throw new BadRequestException("Dự án không đủ point thưởng để trả!", ErrorNameValues.InsufficentToken);
                            }

                            break;
                        }
                    default:
                        break;
                }


                // if (dto.Status == ProjectReportStatus.Accepted)
                // {
                // }
            }
            catch (InvalidOperationException)
            {
                throw new BadRequestException("Sai trạng thái!", ErrorNameValues.InvalidStateChange);
            }

            var result = await _unitOfWork.SaveAsync();

            if (result)
            {
                var queueList = new List<QueueTask>();

                Dictionary<string, string>? targetList;
                ProjectMember? pm;
                switch (dto.Status)
                {
                    case ProjectReportStatus.Created:
                        {
                            var admins = await _unitOfWork.MemberRepository.GetActiveAdmins();

                            // Sent Notification
                            targetList = new Dictionary<string, string> {
                            { "Project", report.ProjectId.ToString()},
                            { "ProjectReport", report.ReportId.ToString() }
                        };
                            admins.ForEach(admin =>
                            {
                                queueList.Add(new QueueTask
                                {
                                    TaskName = TaskName.SendNotification,
                                    TaskData = new Dictionary<string, string>() {
                                    {"MemberId", admin.MemberId.ToString()},
                                    {"Type", NotificationType.ProjectPMChange.ToString()},
                                    //{"Type", NotificationType.Test.ToString()},
                                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                                    {"Title", $"Dự án {report.Project.ProjectName} nộp bản báo cáo mới!"},
                                    {"Content", $"Dự án {report.Project.ProjectName} đã nộp bản báo cáo mới đang đợi được duyệt!"},
                                    }
                                });
                            });
                            break;
                        }
                    case ProjectReportStatus.Accepted:
                        pm = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(report.ProjectId);
                        targetList = new Dictionary<string, string> {
                            { "Project", report.ProjectId.ToString()},
                            { "ProjectReport", report.ReportId.ToString() }
                        };

                        queueList.Add(new QueueTask
                        {
                            TaskName = TaskName.SendNotification,
                            TaskData = new Dictionary<string, string>() {
                                    {"MemberId", pm.MemberId.ToString()},
                                    {"Type", NotificationType.ProjectPMChange.ToString()},
                                    //{"Type", NotificationType.Test.ToString()},
                                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                                    {"Title", $"Bản báo cáo cho dự án {report.Project.ProjectName} đã được chấp nhận!"},
                                    {"Content", $"Bản báo cáo cho dự án {report.Project.ProjectName} đã được chấp nhận cho ký lường {report.SalaryCycle.Name}!"},
                                }
                        });
                        break;
                    case ProjectReportStatus.Rejected:
                        pm = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(report.ProjectId);
                        targetList = new Dictionary<string, string> {
                            { "Project", report.ProjectId.ToString()},
                            { "ProjectReport", report.ReportId.ToString() }
                        };

                        queueList.Add(new QueueTask
                        {
                            TaskName = TaskName.SendNotification,
                            TaskData = new Dictionary<string, string>() {
                                {"MemberId", pm.MemberId.ToString()},
                                {"Type", NotificationType.ProjectPMChange.ToString()},
                                //{"Type", NotificationType.Test.ToString()},
                                {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                                {"Title", $"Bản báo cáo cho dự án {report.Project.ProjectName} đã bị từ chối!"},
                                {"Content", $"Bản báo cáo cho dự án {report.Project.ProjectName} đã bị từ chối!"},
                            }
                        });
                        break;
                    case ProjectReportStatus.Processed:
                        break;
                    case ProjectReportStatus.Cancelled:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                queueList.ForEach(async t =>
                {
                    await _queueService.AddToQueue(t);
                });
            }

            return result;
        }

        public async Task<FileStreamResult> GetReportTemplate(ProjectReportTemplateQueryDTO dto, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
                .Include(x => x.ProjectMember.Where(pms => pms.Status == ProjectMemberStatus.Active))
                .ThenInclude(x => x.Member)
                .Where(x => x.ProjectId == dto.ProjectId).FirstOrDefaultAsync()
                ?? throw new BadRequestException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var salaryCycle = await _unitOfWork.SalaryCycleRepository.GetQuery()
                .FirstOrDefaultAsync(x => x.SalaryCycleId == dto.SalaryCycleId)
                ?? throw new BadRequestException("Kỳ lương không tồn tại!", ErrorNameValues.SalaryCycleNotFound);

            var excel = ExcelHelper.GetReportTemplate(project, salaryCycle);

            var stream = new MemoryStream();
            await excel.SaveAsAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            //excel.Stream.Seek(0, SeekOrigin.Begin);

            var fileStream = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = "Report.xlsx"
            };

            return fileStream;
        }

    }
}