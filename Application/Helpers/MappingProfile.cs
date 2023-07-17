using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.AttributeGroup;
using Application.DTOs.Level;
using Application.DTOs.Member;
using Application.DTOs.MemberLevel;
using Application.DTOs.MemberVoucher;
using Application.DTOs.Notification;
using Application.DTOs.Payslip;
using Application.DTOs.PayslipItem;
using Application.DTOs.Project;
using Application.DTOs.ProjectEndRequest;
using Application.DTOs.ProjectFile;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectMemberRequest;
using Application.DTOs.ProjectReport;
using Application.DTOs.ProjectReportMemberTask;
using Application.DTOs.ProjectSponsor;
using Application.DTOs.ProjectSponsorTransaction;
using Application.DTOs.ProjetMilestone;
using Application.DTOs.Role;
using Application.DTOs.SalaryCycle;
using Application.DTOs.Sponsor;
using Application.DTOs.Supplier;
using Application.DTOs.SystemFile;
using Application.DTOs.Transaction;
using Application.DTOs.User;
using Application.DTOs.Voucher;
using Application.DTOs.Wallet;
using AutoMapper;

namespace Application.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MemberUpdateDTO, Member>()
             .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));
            CreateMap<Member, MemberDTO>();
            CreateMap<Member, MemberWithLevelDTO>()
              .ForMember(src => src.MemberLevels, opt => opt.MapFrom(u => u.MemberLevels.First()));
            // .ForMember(src => src.Role, opt => opt.MapFrom(u => u.Role.RoleId));
            CreateMap<Member, MemberDetailedWithRoleDTO>()
              .ForMember(src => src.MemberLevels, opt => opt.MapFrom(u => u.MemberLevels.First()))

            .ForMember(src => src.Role, opt => opt.MapFrom(x => x.User.Role));
            // .ForMember(src => src.Role, opt => opt.MapFrom(u => u.Role.RoleId));
            CreateMap<Member, MemberProjectsDTO>()
              .ForMember(src => src.TotalProjects,
                opt => opt.MapFrom(m => m.ProjectMembers.Select(pm => pm.Project).ToHashSet().Count()))
              .ForMember(src => src.TotalManagedProjects,
                 opt => opt.MapFrom(m =>
                  m.ProjectMembers
                  .Where(pm =>
                        pm.Status == ProjectMemberStatus.Active &&
                        pm.Role == ProjectMemberRole.Manager)
                  .Select(pm => pm.Project)
                  .ToList()
                  .Count()))
              .ForMember(src => src.Projects, opt => opt.MapFrom<MemberProjectsResolver>());

            CreateMap<MemberLevel, MemberLevelWithMemberDTO>();
            CreateMap<MemberLevel, MemberLevelDTO>();

            CreateMap<UserCreateDTO, User>();
            CreateMap<UserCreateDTO, Member>();
            CreateMap<Role, RoleDTO>();

            CreateMap<User, UserDTO>();
            CreateMap<User, UserDTO>()
              .ForMember(src => src.Role, opt => opt.MapFrom(u => u.Role.RoleId));
            CreateMap<UserUpdateDTO, User>()
             .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));


            CreateMap<Wallet, WalletDTO>();
            CreateMap<Transaction, TransactionDTO>();

            // Project
            CreateMap<Project, ProjectDTO>();
            CreateMap<Project, ProjectWithFilesDTO>();
            CreateMap<Project, MemberProjectsDTO_ProjectCompactDTO>();
            CreateMap<Project, ProjectDetailDTO>()
              .ForMember(src => src.Members, opt => opt.MapFrom(u => u.ProjectMember));
            CreateMap<Project, ProjectDetailWithMemberLevelDTO>()
              .ForMember(src => src.Members, opt => opt.MapFrom(u => u.ProjectMember));
            CreateMap<Project, ProjectWithProjectMemberRoleDTO>()
                .ForMember(p => p.Role, src => src.MapFrom(p => p.ProjectMember.First().Role));

            CreateMap<ProjectCreateDTO, Project>()
              .ForMember(opt => opt.ProjectLongDescription, src => src.MapFrom(src => src.ProjectDescription))
              .ForMember(opt => opt.ProjectShortDescription, src => src.MapFrom(src => src.ProjectDescription!.SubstringWithDots(0, 100)));

            CreateMap<ProjectPMUpdateDTO, Project>()
              .ForMember(opt => opt.ProjectLongDescription, src => src.MapFrom(src => src.ProjectDescription))
              .ForMember(opt => opt.ProjectShortDescription, src => src.MapFrom(src => src.ProjectDescription!.SubstringWithDots(0, 100)))
              .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));

            CreateMap<ProjectAdminUpdateDTO, Project>()
              .ForMember(opt => opt.ProjectLongDescription, src => src.MapFrom(src => src.ProjectDescription))
              .ForMember(opt => opt.ProjectShortDescription, src => src.MapFrom(src => src.ProjectDescription!.SubstringWithDots(0, 100)))
              .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));

            // ProjectMember
            CreateMap<ProjectMember, ProjectMemberDTO>()
              .ForMember(src => src.Role, opt => opt.MapFrom(pe => pe.Role))
              .ForMember(src => src.Member, opt => opt.MapFrom(pe => pe.Member));
            CreateMap<ProjectMember, ProjectMemberWithLevelDTO>()
              .ForMember(src => src.Role, opt => opt.MapFrom(pe => pe.Role))
              .ForMember(src => src.Member, opt => opt.MapFrom(pe => pe.Member));
            CreateMap<ProjectMember, ProjectMemberDetailedDTO>()
              .ForMember(src => src.Role, opt => opt.MapFrom(pe => pe.Role))
              .ForMember(src => src.Member, opt => opt.MapFrom(pe => pe.Member))
              .ForMember(src => src.Attributes, opt => opt.MapFrom<ProjectMemberDetailedResolver>());


            CreateMap<ProjectMemberRequestCreateDTO, ProjectMemberRequest>();
            CreateMap<ProjectMemberRequest, ProjectMemberRequestDTO>();

            CreateMap<AttributeGroup, AttributeGroupDTO>();

            CreateMap<SystemFile, SystemFileDTO>();
            CreateMap<ProjectFile, ProjectFileDTO>()
            .ForMember(opt => opt.File, src => src.MapFrom(src => src.SystemFile));

            CreateMap<ProjectReport, ProjectReportWithProjectAndSalaryCycleDTO>()
                .ForMember(src => src.Project, opt => opt.MapFrom(pr => pr.Project))
                .ForMember(src => src.SalaryCycle, opt => opt.MapFrom(pr => pr.SalaryCycle));
              //.ForMember(src => src.TaskPointNeeded,
              //    opt => opt.MapFrom((src, dest) =>
              //    {
              //        var result = BenefitHelper.EstimateRewardsForReport(src);
              //        return result.TotalP2 + result.TotalP3 + result.TotalTaskPoint;
              //    }))
              //.ForMember(src => src.BonusPointNeeded,
              //    opt => opt.MapFrom((src, dest) =>
              //    {
              //        var result = BenefitHelper.EstimateRewardsForReport(src);
              //        return result.TotalBonusPoint;
              //    }));

              CreateMap<ProjectReport, ProjectReportWithTasksDTO>()
                  .ForMember(src => src.Tasks, opt => opt.MapFrom<ProjectReportDetailResolver>());
              //.ForMember(src => src.TaskPointNeeded,
              //    opt => opt.MapFrom((src, dest) =>
              //    {
              //        var result = BenefitHelper.EstimateRewardsForReport(src);
              //        return result.TotalP2 + result.TotalP3 + result.TotalTaskPoint;
              //    }))
              //.ForMember(src => src.BonusPointNeeded,
              //    opt => opt.MapFrom((src, dest) =>
              //    {
              //        var result = BenefitHelper.EstimateRewardsForReport(src);
              //        return result.TotalBonusPoint;
              //    }));

            CreateMap<ProjectReportMemberTask, ProjectReportMemberTaskDTO>();
            CreateMap<ProjectReportMemberTask, ProjectReportMemberTaskForPayslipDTO>()
            .ForMember(x => x.Project, opt => opt.MapFrom(src => src.ProjectReportMember.ProjectMember.Project));

            CreateMap<ProjectReportMemberTaskCreateDTO, ProjectReportMemberTask>();

            CreateMap<Sponsor, SponsorDTO>();
            CreateMap<Sponsor, SponsorDetailedDTO>()
              .ForMember(sp => sp.TotalPoint, src =>
                src.MapFrom(sp => sp.ProjectSponsors.Sum(ps =>
                  ps.ProjectSponsorTransactions.Sum(pst => pst.Amount))))
              .ForMember(sp => sp.TotalProjects, src =>
                src.MapFrom(sp => sp.ProjectSponsors.Select(ps => ps.Project).DistinctBy(p => p.ProjectId).Count()));

            CreateMap<SponsorCreateDTO, Sponsor>();
            CreateMap<SponsorUpdateDTO, Sponsor>()
             .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));

            CreateMap<ProjectSponsor, ProjectSponsorDTO>()
              .ForMember(src => src.Sponsor, opt => opt.MapFrom(pe => pe.Sponsor))
              .ForMember(src => src.LastDepositDate, opt =>
              {
                  opt.PreCondition(ps => ps.ProjectSponsorTransactions.Any());
                  opt.MapFrom(ps => ps.ProjectSponsorTransactions.First().CreatedAt);
              })
              .ForMember(src => src.LastDepositAmount, opt =>
              {
                  opt.PreCondition(ps => ps.ProjectSponsorTransactions.Any());
                  opt.MapFrom(ps => ps.ProjectSponsorTransactions.First().Amount);
              });

            CreateMap<ProjectSponsor, ProjectSponsorDetailedDTO>()
              .ForMember(src => src.Sponsor, opt => opt.MapFrom(pe => pe.Sponsor))
              .ForMember(src => src.LastDepositDate, opt =>
              {
                  opt.PreCondition(ps => ps.ProjectSponsorTransactions.Any());
                  opt.MapFrom(ps => ps.ProjectSponsorTransactions.First().CreatedAt);
              })
              .ForMember(src => src.LastDepositAmount, opt =>
              {
                  opt.PreCondition(ps => ps.ProjectSponsorTransactions.Any());
                  opt.MapFrom(ps => ps.ProjectSponsorTransactions.First().Amount);
              }); ;

            CreateMap<ProjectSponsorCreateDTO, ProjectSponsor>();
            CreateMap<ProjectSponsorUpdateDTO, ProjectSponsor>()
             .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));

            CreateMap<Level, LevelDTO>();
            CreateMap<LevelUpdateDTO, Level>()
              .ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null)); ;
            CreateMap<LevelCreateDTO, Level>();

            CreateMap<ProjectMilestone, ProjectMilestoneDTO>();
            CreateMap<ProjectMilestoneCreateDTO, ProjectMilestone>();

            CreateMap<Payslip, PayslipDTO>()
              .ForMember(opt => opt.Items, src => src.MapFrom(p => p.PayslipItems))
              .ForMember(opt => opt.TotalP1, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P1).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalP2, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P2).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalP3, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P3).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalXP, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.XP).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalBonus, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.Bonus).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.Attributes, src => src.MapFrom<PayslipDetailedResolver>());

            CreateMap<Payslip, PayslipV2DTO>()
              .ForMember(opt => opt.Items, src => src.MapFrom(p => p.PayslipItems))
              .ForMember(opt => opt.TotalP1, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P1).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalP2, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P2).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalP3, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.P3).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalXP, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.XP).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.TotalBonus, src => src.MapFrom(p => p.PayslipItems.Where(psi => psi.Type == PayslipItemType.Bonus).Sum(psi => psi.Amount)))
              .ForMember(opt => opt.Attributes, src => src.MapFrom<PayslipDetailedResolverV2>());

            CreateMap<PayslipItem, PayslipItemDTO>()
                .ForMember(opt => opt.Attributes, src => src.MapFrom<PayslipItemsDetailedResolver>());
            CreateMap<PayslipItem, PayslipItemV2DTO>()
                .ForMember(opt => opt.Attributes, src => src.MapFrom<PayslipItemsDetailedResolverV2>());

            CreateMap<SalaryCycle, SalaryCycleDTO>()
              .ForMember(dst => dst.Status, src => src.MapFrom(s => s.SalaryCycleStatus));
            CreateMap<SalaryCycle, SalaryCycleWithPayslipDTO>()
              .ForMember(dst => dst.Status, src => src.MapFrom(s => s.SalaryCycleStatus))
              .ForMember(dst => dst.TotalPoint,
                src => src.MapFrom(s => s.Payslips.Sum(ps =>
                  ps.PayslipItems.Where(psi =>
                  (psi.Type >= PayslipItemType.P1 && psi.Type <= PayslipItemType.P3) || psi.Type == PayslipItemType.Bonus)
                    .Sum(psi => psi.Amount)
                    )));

            CreateMap<Voucher, VoucherDTO>();
            CreateMap<VoucherCreateDTO, Voucher>();
            CreateMap<VoucherUpdateDTO, Voucher>()
                .ForAllMembers(
                    opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));

            CreateMap<Voucher, VoucherDTO>();
            CreateMap<Notification, NotificationDTO>();
            CreateMap<MemberVoucher, MemberVoucherDTO>();
            CreateMap<ProjectSponsorTransaction, ProjectSponsorTransactionDTO>();
            CreateMap<Project, ProjectWithTotalFundDTO>();

            CreateMap<Supplier, SupplierDTO>();
            CreateMap<SupplierCreateDTO, Supplier>();
            CreateMap<SupplierUpdateDTO, Supplier>();

            CreateMap<ProjectEndRequest, ProjectEndRequestDTO>();

        }
    }

    public class ProjectMemberDetailedResolver : IValueResolver<ProjectMember, ProjectMemberDetailedDTO, Dictionary<string, string>>
    {
        public Dictionary<string, string> Resolve(ProjectMember source, ProjectMemberDetailedDTO destination, Dictionary<string, string> destMember, ResolutionContext context)
        {
            Dictionary<string, string> newDict = new Dictionary<string, string>();
            var atts = source.ProjectMemberAttributes.Select(pma => pma.Attribute)
              .DistinctBy(a => a.AttributeGroupId).ToList();
            atts.ForEach(att =>
            {
                newDict.Add(att.AttributeGroupId, att.Value);
            });

            return newDict;
        }
    }

    public class PayslipDetailedResolver : IValueResolver<Payslip, PayslipDTO, Dictionary<string, string>>
    {
        public Dictionary<string, string> Resolve(Payslip source, PayslipDTO destination, Dictionary<string, string> destMember, ResolutionContext context)
        {
            Dictionary<string, string> newDict = new Dictionary<string, string>();
            var atts = source.PayslipAttributes.Select(pma => pma.Attribute)
            .DistinctBy(a => a.AttributeGroupId).ToList();

            atts.ForEach(att =>
            {
                newDict.Add(att.AttributeGroupId, att.Value);
            });

            return newDict;
        }
    }

    public class PayslipDetailedResolverV2 : IValueResolver<Payslip, PayslipV2DTO, Dictionary<string, string>>
    {
        public Dictionary<string, string> Resolve(Payslip source, PayslipV2DTO destination, Dictionary<string, string> destMember, ResolutionContext context)
        {
            Dictionary<string, string> newDict = new Dictionary<string, string>();
            var atts = source.PayslipAttributes.Select(pma => pma.Attribute)
            .DistinctBy(a => a.AttributeGroupId).ToList();

            atts.ForEach(att =>
            {
                newDict.Add(att.AttributeGroupId, att.Value);
            });

            return newDict;
        }
    }

    public class PayslipItemsDetailedResolver : IValueResolver<PayslipItem, PayslipItemDTO, Dictionary<string, string>>
    {
        public Dictionary<string, string> Resolve(PayslipItem source, PayslipItemDTO destination, Dictionary<string, string> destMember, ResolutionContext context)
        {
            Dictionary<string, string> newDict = new Dictionary<string, string>();
            var atts = source.PayslipItemAttributes.Select(pma => pma.Attribute)
                .DistinctBy(a => a.AttributeGroupId).ToList();

            atts.ForEach(att =>
            {
                newDict.Add(att.AttributeGroupId, att.Value);
            });

            return newDict;
        }
    }

    public class PayslipItemsDetailedResolverV2 : IValueResolver<PayslipItem, PayslipItemV2DTO, Dictionary<string, string>>
    {
        public Dictionary<string, string> Resolve(PayslipItem source, PayslipItemV2DTO destination, Dictionary<string, string> destMember, ResolutionContext context)
        {
            Dictionary<string, string> newDict = new Dictionary<string, string>();
            var atts = source.PayslipItemAttributes.Select(pma => pma.Attribute)
                .DistinctBy(a => a.AttributeGroupId).ToList();

            atts.ForEach(att =>
            {
                newDict.Add(att.AttributeGroupId, att.Value);
            });

            return newDict;
        }
    }


    public class MemberProjectsResolver : IValueResolver<Member, MemberProjectsDTO, List<MemberProjectsDTO_ProjectCompactDTO>>
    {
        public List<MemberProjectsDTO_ProjectCompactDTO> Resolve(Member source, MemberProjectsDTO destination, List<MemberProjectsDTO_ProjectCompactDTO> destMember, ResolutionContext context)
        {
            var newList = new List<MemberProjectsDTO_ProjectCompactDTO>();
            foreach (var project in source.ProjectMembers.Where(pm => pm.Status == ProjectMemberStatus.Active).Select(p => p.Project).ToHashSet())
            {
                var pDto = context.Mapper.Map<MemberProjectsDTO_ProjectCompactDTO>(project);
                pDto.Role = source.ProjectMembers.First(p => p.ProjectId == project.ProjectId).Role;
                newList.Add(pDto);
            }
            return newList;
        }
    }

    //public class TransactionDTOIsReceivedResolver : IValueResolver<Transaction, TransactionDTO, bool>
    //{
    //    public bool Resolve(Transaction source, TransactionDTO destination, bool destMember, ResolutionContext context)
    //    {
    //        if (source.TransactionType == TransactionType.MemberToMember)
    //        {
    //            if (source.FromWallet.MemberWallet.MemberId == )
    //        }
    //    }
    //}

    public class ProjectReportDetailResolver : IValueResolver<ProjectReport, ProjectReportWithTasksDTO, List<ProjectReportTaskDTO>>
    {
        public List<ProjectReportTaskDTO> Resolve(ProjectReport source, ProjectReportWithTasksDTO destination, List<ProjectReportTaskDTO> destMember, ResolutionContext context)
        {
            var tasksList = new List<ProjectReportTaskDTO>();
            source.ProjectReportMembers.ForEach(prm => prm.ProjectReportMemberTasks.ForEach(task =>
            {
                tasksList.Add(new ProjectReportTaskDTO
                {
                    ProjectReportMemberTaskId = task.ProjectReportMemberTaskId,
                    //Member = context.Mapper.Map<Member, MemberDTO>(prm.ProjectMember.Member),
                    MemberEmail = prm.ProjectMember.Member.EmailAddress,
                    TaskDescription = task.TaskDescription,
                    TaskHour = task.TaskHour,
                    TaskName = task.TaskName,
                    TaskRealHour = task.TaskRealHour,
                    TaskPoint = task.TaskPoint,
                    TaskBonus = task.TaskBonus,
                    BonusReason = task.BonusReason,
                    TaskCode = task.TaskCode,
                    TaskEffort = task.TaskEffort
                });
            }));
            return tasksList;
        }
    }

}