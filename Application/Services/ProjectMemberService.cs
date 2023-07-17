using Application.Domain.Constants;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectMemberRequest;
using Application.Domain.Models;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams.ProjectMemberRequest;
using Application.StateMachines;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using Application.Domain.Enums;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.Payslip;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.Wallet;
using Attribute = Application.Domain.Models.Attribute;
using Application.Domain.Enums.Project;

namespace Application.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IQueueService _queueService;

        public ProjectMemberService(UnitOfWork unitOfWork,
                            IQueueService queueService,
                           IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _queueService = queueService;
            _mapper = mapper;
        }

        /// <summary>
        /// Request to join project
        /// </summary>
        public async Task<PagedList<ProjectMemberRequest>> GetProjectMemberRequests(Guid projectId, ProjectMemberRequestQueryParams queryParams, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId)
                          ?? throw new NotFoundException("There is no project with that ID!", ErrorNameValues.ProjectNotFound);

            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == requesterEmail)
              .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

            var pm = await _unitOfWork.ProjectMemberRepository
              .GetQuery()
              .Where(pe => pe.ProjectId == project.ProjectId &&
                pe.Status == ProjectMemberStatus.Active &&
                pe.MemberId == member.MemberId &&
                pe.Role == ProjectMemberRole.Manager)
              .FirstOrDefaultAsync();

            if (pm == null) throw new BadRequestException("Bạn không có quyền!", ErrorNameValues.NoPermission);

            var query = _unitOfWork.ProjectMemberRequestRepository.GetQuery();
            query = query.Where(pe => pe.ProjectId == projectId)
              .Include(emp => emp.Member.MemberLevels.Where(ml => ml.IsActive))
              .ThenInclude(x => x.Level);

            if (queryParams.MemberEmail != null) query = query.Where(per => per.Member.EmailAddress.ToLower().Contains(queryParams.MemberEmail.ToLower()));

            if (queryParams.Status.Count > 0)
            {
                query = query.Where(per => queryParams.Status.Contains(per.Status));
            }

            var pers = await PagedList<ProjectMemberRequest>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return pers;
        }


        /// <summary>
        /// Request to join project
        /// </summary>
        public async Task<PagedList<ProjectMemberRequest>> GetSelfProjectMemberRequest(SelfProjectMemberRequestQueryParams queryParams, string requesterEmail)
        {

            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == requesterEmail)
              .FirstOrDefaultAsync()
                         ?? throw new NotFoundException("Người dừng không tồn tại!", ErrorNameValues.MemberNotFound);

            var query = _unitOfWork.ProjectMemberRequestRepository.GetQuery();
            query = query.Where(pe => pe.MemberId == member.MemberId)
                  .Include(emp => emp.Member.MemberLevels.Where(ml => ml.IsActive))
                .ThenInclude(x => x.Level);

            if (queryParams.ProjectId != null) query = query.Where(pr => pr.ProjectId == queryParams.ProjectId);

            if (queryParams.Status.Count > 0)
            {
                query = query.Where(per => queryParams.Status.Contains(per.Status));
            }

            var pers = await PagedList<ProjectMemberRequest>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
            return pers;

        }

        /// <summary>
        /// Request to join project
        /// </summary>
        public async Task<Guid> RequestToJoin(Guid projectId, ProjectMemberRequestCreateDTO dto, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetByID(projectId)
                ?? throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            if (project.ProjectStatus != ProjectStatus.Created && project.ProjectStatus != ProjectStatus.Started)
                throw new BadRequestException("Dự án không thể tham gia được nữa!", ErrorNameValues.ProjectUnavailable);

            var requesterMember = await _unitOfWork.MemberRepository.GetByEmailWithUser(requesterEmail)
                ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

            if (requesterMember.User.RoleId == "ADMIN")
                throw new BadRequestException("Bạn là quản trị viên, không thể tham gia dự án!", ErrorNameValues.InvalidParameters);

            var pe = await _unitOfWork.ProjectMemberRepository
              .GetQuery()
              .Where(pe => pe.ProjectId == project.ProjectId && pe.Status == ProjectMemberStatus.Active)
              .ToListAsync();

            if (pe.Select(e => e.MemberId).Contains(requesterMember.MemberId))
                throw new BadRequestException("Bạn đã ở bện trong dự án rồi!", ErrorNameValues.AlreadyInProject);

            var per = await _unitOfWork.ProjectMemberRequestRepository.GetQuery()
              .Where(pe => pe.MemberId == requesterMember.MemberId
                  && pe.ProjectId == projectId
                  && pe.Status == ProjectMemberRequestStatus.Created)
              .FirstOrDefaultAsync();

            if (per != null)
                throw new BadRequestException("Bạn đã yêu cầu tham gia trước đó, vui lòng đợi!", ErrorNameValues.RequestDuplicated);

            var newProjectMemberRequest = new ProjectMemberRequest()
            {
                ProjectId = projectId,
                MemberId = requesterMember.MemberId,
                Major = dto.Major,
                Note = dto.Note,
                Status = ProjectMemberRequestStatus.Created
            };

            var result = await Insert(newProjectMemberRequest);
            if (!result) throw new BadRequestException("Có lỗi, vui lòng thử lại", ErrorNameValues.ServerError);

            var projectManager = pe.First(x => x.Role == ProjectMemberRole.Manager);

            // Sent Notification
            var targetList = new Dictionary<string, string> {
                { "Project", project.ProjectId.ToString() },
                { "ProjectMemberRequest", newProjectMemberRequest.RequestId.ToString() }
            };

            await _queueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                {"MemberId", projectManager.MemberId.ToString()},
                {"Type", NotificationType.ProjectMemberRequestPending.ToString()},
                //{"Type", NotificationType.Test.ToString()},
                {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                {"Title", $"Dự án {project.ProjectName} có một yêu cầu tham gia mới!"},
                {"Content", $"Dự án {project.ProjectName} có một yêu cầu tham gia mới từ {requesterMember.EmailAddress}!"},
            });

            return newProjectMemberRequest.RequestId;
        }

        public async Task<bool> ChangeProjectManager(Guid projectId, ProjectMemberChangePMDTO dto, string requesterEmail)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
                              .Include(x => x.ProjectMember.Where(pm => pm.Status == ProjectMemberStatus.Active))
                              .FirstOrDefaultAsync(x => x.ProjectId == projectId)
                          ?? throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            var newManager = await _unitOfWork.MemberRepository.GetByEmail(dto.Email)
                ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var newManagerInProject = project.ProjectMember.FirstOrDefault(x => x.MemberId == newManager.MemberId)
                ?? throw new NotFoundException("Thành viên không tồn tại trong dự án!", ErrorNameValues.MemberNotFound);

            if (newManagerInProject.Role == ProjectMemberRole.Manager)
                throw new BadRequestException("Thành viên hiện tại đã là quản lý của dự án rồi!",
                    ErrorNameValues.InvalidParameters);

            var currentPM = project.ProjectMember.First(x => x.Role == ProjectMemberRole.Manager);

            currentPM.Role = ProjectMemberRole.Member;
            newManagerInProject.Role = ProjectMemberRole.Manager;

            _unitOfWork.ProjectMemberRepository.Update(currentPM);
            _unitOfWork.ProjectMemberRepository.Update(newManagerInProject);

            var result = await _unitOfWork.SaveAsync();
            if (result)
            {
                // Sent Notification
                var targetList = new Dictionary<string, string> {
                    { "Project", project.ProjectId.ToString() }
                };

                await _queueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                    {"MemberId", newManagerInProject.MemberId.ToString()},
                    {"Type", NotificationType.ProjectPMChange.ToString()},
                    //{"Type", NotificationType.Test.ToString()},
                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                     {"Title", $"Bạn đã được lên làm quản lý dự án cho {project.ProjectName}"},
                    {"Content", $"Bạn đã được lên làm quản lý dự án cho {project.ProjectName}"},
                });
            }

            return result;
        }

        public async Task<bool> ReviewRequestToJoin(Guid projectEmpRequestId, ProjectMemberRequestReviewDTO dto, string requesterEmail)
        {
            var pmr = await _unitOfWork.ProjectMemberRequestRepository.GetQuery()
              .Where(per => per.RequestId == projectEmpRequestId)
              .Include(per => per.Project)
              .ThenInclude(pr => pr.ProjectMember.Where(pe => pe.Status == ProjectMemberStatus.Active))
              .FirstOrDefaultAsync();

            if (pmr == null) throw new NotFoundException("Yêu cầu không tồn tại!", ErrorNameValues.RequestNotFound);
            if (pmr.Status != ProjectMemberRequestStatus.Created) throw new BadRequestException("Yêu cầu không còn có thể được phê duyệt!", ErrorNameValues.RequestNotAvailableToBeUpdate);

            var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail)
                ?? throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

            var isOwn = pmr.MemberId == requesterMember.MemberId;

            if (!isOwn) // If is not your
            {
                var pe = await _unitOfWork.ProjectMemberRepository
                  .GetQuery()
                  .Include(x => x.Member.MemberLevels.Where(memberLevel => memberLevel.IsActive))
                  .ThenInclude(x => x.Level)
                  .Where(pe => pe.ProjectId == pmr!.ProjectId
                    && pe.Status == ProjectMemberStatus.Active
                    && pe.MemberId == requesterMember.MemberId
                    && pe.Role == ProjectMemberRole.Manager)
                  .FirstOrDefaultAsync();

                if (pe == null) throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var isAcceptMember = false;
            var isRejectMember = false;

            try
            {
                var allowedForOwn = new List<ProjectMemberRequestStatus>() { ProjectMemberRequestStatus.Cancelled };
                var allowedForPM = new List<ProjectMemberRequestStatus>() { ProjectMemberRequestStatus.Accepted, ProjectMemberRequestStatus.Rejected };

                if (isOwn && !allowedForOwn.Contains(dto.Status))
                    throw new BadRequestException("Bạn chỉ có thể huỷ yêu cầu!", ErrorNameValues.InvalidStateChange);
                if (!isOwn && !allowedForPM.Contains(dto.Status))
                    throw new BadRequestException("Bạn chỉ có thể phê duyệt yêu cầu!", ErrorNameValues.InvalidStateChange);


                var pmrsm = new ProjectMemberRequestStateMachine(pmr!);
                pmrsm.TriggerState(dto.Status);

                switch (dto.Status)
                {
                    case ProjectMemberRequestStatus.Accepted:
                        {
                            var pm = new ProjectMember()
                            {
                                MemberId = pmr.MemberId,
                                Major = pmr.Major,
                                Role = ProjectMemberRole.Member,
                                Status = ProjectMemberStatus.Active,
                            };

                            var hardSkill = 0;
                            if (dto.Graduated == true) hardSkill += 2;
                            if (dto.HaveEnghlishCert == true) hardSkill += 2;
                            if (dto.YearOfExp == 0)
                            {
                                hardSkill += 2;
                            }
                            else if (dto.YearOfExp > 0 && dto.YearOfExp < 1)
                            {
                                hardSkill += 4;
                            }
                            else if (dto.YearOfExp >= 1)
                            {
                                hardSkill += 6;
                            }


                            var attrs = new List<Attribute>()
                        {
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.IsGraduated,
                                Value = dto.Graduated.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.YearsOfExperience,
                                Value = dto.YearOfExp.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.HadEnglishCertificate,
                                Value = dto.HaveEnghlishCert.ToString()!
                            },

                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.LeadershipSkill,
                                Value = dto.LeadershipSkill.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.ProblemSolvingSkill,
                                Value = dto.ProblemSolvingSkill.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.PositiveAttitude,
                                Value = dto.PositiveAttitude.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.TeamworkSkill,
                                Value = dto.TeamworkSkill.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.CommunicationSkill,
                                Value = dto.CommnicationSkill.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.CreativitySkill,
                                Value = dto.CreativitySkill.ToString()!
                            },

                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.SoftSkill,
                                Value = hardSkill.ToString()!
                            },
                            new()
                            {
                                AttributeGroupId = AttributeGroupNameValues.HardSkill,
                                Value = ((dto.LeadershipSkill + dto.CommnicationSkill + dto.ProblemSolvingSkill +
                                          dto.PositiveAttitude + dto.TeamworkSkill + dto.CreativitySkill) / 6)
                                    .ToString()!
                            },
                        };

                            // Add attrs to Table
                            attrs.ForEach(att =>
                            {
                                pm.ProjectMemberAttributes.Add(new ProjectMemberAttribute()
                                {
                                    Attribute = att
                                });
                            });

                            pmr.Status = ProjectMemberRequestStatus.Accepted;
                            pmr.Project.ProjectMember.Add(pm);

                            // Check if salary cycle is started and member didn't have any payslip
                            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetQuery()
                                .FirstOrDefaultAsync(x => x.SalaryCycleStatus <= SalaryCycleStatus.Locked);

                            if (currentSalaryCycle != null)
                            {
                                var level = requesterMember.MemberLevels.First().Level;

                                var payslipAttrs = new List<PayslipAttribute>()
                            {
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
                                    MemberId = requesterMember.MemberId,
                                    Status = PayslipStatus.Created,
                                    Note = $"Phiếu lương cho <@SalaryCycle:{currentSalaryCycle.SalaryCycleId}>",
                                    PayslipAttributes = payslipAttrs
                                };

                                var p1 = new PayslipItem()
                                {
                                    Type = PayslipItemType.P1,
                                    Token = WalletToken.Point,
                                    Amount = level.BasePoint,
                                    Note = $"P1 từ hệ thống",
                                };

                                payslipForMember.PayslipItems.Add(p1);

                                currentSalaryCycle.Payslips.Add(payslipForMember);
                                _unitOfWork.SalaryCycleRepository.Update(currentSalaryCycle);
                            }
                            isAcceptMember = true;
                            break;
                        }
                    case ProjectMemberRequestStatus.Rejected:
                        {
                            isRejectMember = true;
                            break;
                        }
                }
            }
            catch (InvalidOperationException)
            {
                throw new BadRequestException("Chuyển trạng thái không hợp lệ!", ErrorNameValues.InvalidStateChange);
            }

            _unitOfWork.ProjectMemberRequestRepository.Update(pmr);
            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Lỗi hệ thống, vui lòng thử lại!", ErrorNameValues.ServerError);

            if (isAcceptMember)
            {
                var targetList = new Dictionary<String, String> {
                    { "Project", pmr.ProjectId.ToString() },
                    { "ProjectMemberRequest", pmr.RequestId.ToString() }
                };

                await _queueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                  {"MemberId", pmr.MemberId.ToString()},
                  {"Type", NotificationType.ProjectMemberRequestAccepted.ToString()},
                  //{"Type", NotificationType.Test.ToString()},
                  {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                  {"Title", $"Bạn đã được chấp thuận vào dự án"},
                  {"Content", $"Bạn đã được chấp thuận vào dự án {pmr.Project.ProjectName}"},
                });
            }

            if (isRejectMember)
            {
                var targetList = new Dictionary<String, String> {
                    { "Project", pmr.ProjectId.ToString() },
                    { "ProjectMemberRequest", pmr.RequestId.ToString() }
                };

                await _queueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                    {"MemberId", pmr.MemberId.ToString()},
                    {"Type", NotificationType.ProjectMemberRequestRejected.ToString()},
                    //{"Type", NotificationType.Test.ToString()},
                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                    {"Title", $"Bạn đã bị từ chối vào dự án"},
                    {"Content", $"Bạn đã bị từ chối vào dự án {pmr.Project.ProjectName}"},
                });
            }

            return result;
        }

        public async Task<bool> UpdateProjectMember(ProjectMemberUpdateDTO dto, string requesterEmail)
        {
            var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
                .Include(x => x.ProjectMemberAttributes).ThenInclude(x => x.Attribute)
              .Where(x => x.ProjectMemberId == dto.ProjectMemberId && x.Status == ProjectMemberStatus.Active)
              .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Thành viên không tồn tại trong dự án!", ErrorNameValues.ProjectMemberNotFound);

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
            if (currentSalaryCycle != null && (currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Ongoing || currentSalaryCycle.SalaryCycleStatus == SalaryCycleStatus.Locked))
            {
                throw new BadRequestException("Bạn không thể làm điều này khi có kì lương đang diễn ra", ErrorNameValues.SalaryCycleNotAvailable);
            }

            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == requesterEmail)
              .FirstOrDefaultAsync() ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var pe = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectMember.ProjectId, requesterEmail)
                ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);


            var hardSkill = 0;
            if (dto.Graduated == true) hardSkill += 2;
            if (dto.HaveEnghlishCert == true) hardSkill += 2;
            if (dto.YearOfExp == 0)
            {
                hardSkill += 2;
            }
            else if (dto.YearOfExp > 0 && dto.YearOfExp < 1)
            {
                hardSkill += 4;
            }
            else if (dto.YearOfExp >= 1)
            {
                hardSkill += 6;
            }

            var attrs = new List<Attribute>() {
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.IsGraduated,
                Value = dto.Graduated.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.YearsOfExperience,
                Value = dto.YearOfExp.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.HadEnglishCertificate,
                Value = dto.HaveEnghlishCert.ToString()!
              },

              new()
              {
                AttributeGroupId = AttributeGroupNameValues.LeadershipSkill,
                Value = dto.LeadershipSkill.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.ProblemSolvingSkill,
                Value = dto.ProblemSolvingSkill.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.PositiveAttitude,
                Value = dto.PositiveAttitude.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.TeamworkSkill,
                Value = dto.TeamworkSkill.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.CommunicationSkill,
                Value = dto.CommnicationSkill.ToString()!
              },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.CreativitySkill,
                Value = dto.CreativitySkill.ToString()!
              },

              new()
              {
                AttributeGroupId = AttributeGroupNameValues.SoftSkill,
                Value = hardSkill.ToString()!
            },
              new()
              {
                AttributeGroupId = AttributeGroupNameValues.HardSkill,
                Value = ((dto.LeadershipSkill + dto.CommnicationSkill + dto.ProblemSolvingSkill + dto.PositiveAttitude + dto.TeamworkSkill + dto.CreativitySkill)/6).ToString()!
              },
            };

            // Remove old attrs
            var oldMemberAttrs = projectMember.ProjectMemberAttributes.ToList();
            var oldAttrs = oldMemberAttrs.Select(x => x.Attribute).ToList();

            _unitOfWork.ProjectMemberAttributeRepository.Delete(oldMemberAttrs);
            _unitOfWork.AttributeRepository.Delete(oldAttrs);

            // Add new attributes
            attrs.ForEach(att =>
            {
                projectMember.ProjectMemberAttributes.Add(new ProjectMemberAttribute()
                {
                    Attribute = att
                });
            });

            projectMember.UpdatedAt = DateTime.Now;
            _unitOfWork.ProjectMemberRepository.Update(projectMember);

            var result = await _unitOfWork.SaveAsync();
            return result;
        }

        public async Task<List<ProjectMember>> GetProjectMembersDetailed(Guid projectId, string? requesterEmail = null, bool isAdmin = false)
        {
            var project = await _unitOfWork.ProjectRepository.GetQuery()
                  .Where(p => p.ProjectId == projectId)
                      .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("Dự án không tồn tại!", ErrorNameValues.ProjectNotFound);

            if (!isAdmin)
            {
                // Only Member Who Is In Project
                if (requesterEmail == null)
                    throw new NotFoundException("Người dùng không tồn tại!", ErrorNameValues.MemberNotFound);

                var member = await _unitOfWork.MemberRepository.GetQuery()
                        .Where(e => e.EmailAddress == requesterEmail)
                        .FirstOrDefaultAsync()
                   ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

                var pm = await _unitOfWork.ProjectMemberRepository.GetQuery().Where(
                    x => x.ProjectId == project.ProjectId &&
                         x.MemberId == member.MemberId &&
                         x.Status == ProjectMemberStatus.Active)
                    .FirstOrDefaultAsync() ??
                    throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var projectMembers = await _unitOfWork.ProjectMemberRepository.GetQuery()
                .Where(pe => pe.ProjectId == projectId && pe.Status == ProjectMemberStatus.Active)
                .Include(emp => emp.Member.MemberLevels.Where(ml => ml.IsActive))
                .ThenInclude(x => x.Level)
                .Include(pm => pm.ProjectMemberAttributes).ThenInclude(pma => pma.Attribute)
                .ToListAsync();

            return projectMembers;
        }

        public async Task<ProjectMember> GetProjectMemberDetailed(Guid projectMemberId, string? requesterEmail = null, bool isAdmin = false)
        {
            var projectMember = await _unitOfWork.ProjectMemberRepository.GetQuery()
                  .Where(p => p.ProjectMemberId == projectMemberId && p.Status == ProjectMemberStatus.Active)
                      .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("Thành viên không tồn tại trong dự án!", ErrorNameValues.MemberNotFound);

            if (!isAdmin)
            {
                // Only Member Who Is In Project
                if (requesterEmail == null)
                    throw new NotFoundException("Tài khoản không tồn tại!", ErrorNameValues.MemberNotFound);

                var member = await _unitOfWork.MemberRepository.GetQuery()
                        .Where(e => e.EmailAddress == requesterEmail)
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

                var pm = await _unitOfWork.ProjectMemberRepository.GetQuery().Where(
                    x => x.ProjectId == projectMember.ProjectId &&
                         x.MemberId == member.MemberId &&
                         x.Status == ProjectMemberStatus.Active)
                    .FirstOrDefaultAsync()
                    ?? throw new BadRequestException("Bạn không phải quản lý dự án!", ErrorNameValues.NoPermission);
            }

            var projectMemberDetailed = await _unitOfWork.ProjectMemberRepository.GetQuery()
                .Include(emp => emp.Member.MemberLevels.Where(ml => ml.IsActive))
                .ThenInclude(x => x.Level)
                .Include(pm => pm.ProjectMemberAttributes).ThenInclude(pma => pma.Attribute)
                .Where(pe => pe.ProjectMemberId == projectMember.ProjectMemberId && pe.Status == ProjectMemberStatus.Active)
                .FirstAsync();

            return projectMemberDetailed;
        }

        private async Task<List<ProjectMemberRequest>> GetAll()
        {
            var pers = await _unitOfWork.ProjectMemberRequestRepository.GetQuery()
              .ToListAsync();
            return pers;
        }

        private async Task<ProjectMemberRequest?> GetByID(Guid projectId, Guid MemberId)
        {
            var per = await _unitOfWork.ProjectMemberRequestRepository.GetQuery()
                .Where(x => x.ProjectId == projectId && x.MemberId == MemberId)
                    .FirstOrDefaultAsync();

            return per;
        }

        private async Task<bool> Insert(ProjectMemberRequest e)
        {
            _unitOfWork.ProjectMemberRequestRepository.Add(e);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<bool> Update(ProjectMemberRequest e)
        {
            _unitOfWork.ProjectMemberRequestRepository.Update(e);
            return await _unitOfWork.SaveAsync();
        }

        public async Task<bool> UpdateProjectMemberStatus(ProjectMemberUpdateStatusDTO dto, string requesterEmail)
        {
            var projectMemberToEdit = await _unitOfWork.ProjectMemberRepository.GetQuery()
            .Where(pe => pe.ProjectMemberId == dto.ProjectMemberId)
              .Include(pe => pe.Member)
              .Include(pm => pm.Project)
              .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            var requesterMember = await _unitOfWork.MemberRepository.GetQuery()
                .Where(e => e.EmailAddress == requesterEmail)
                .FirstOrDefaultAsync();

            var requesterProjectMember =
                _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(projectMemberToEdit.ProjectId,
                    requesterEmail)
                ?? throw new BadRequestException("Bạn không phải quản lý dự án", ErrorNameValues.NoPermission);

            try
            {
                var pmsm = new ProjectMemberStateMachine(projectMemberToEdit);
                pmsm.TriggerState(dto.Status);
            }
            catch
            {
                throw new BadRequestException("Cập nhật sai trạng thái!", ErrorNameValues.InvalidStateChange);
            }

            _unitOfWork.ProjectMemberRepository.Update(projectMemberToEdit);
            var result = await _unitOfWork.SaveAsync();

            return result;
        }
    }
}