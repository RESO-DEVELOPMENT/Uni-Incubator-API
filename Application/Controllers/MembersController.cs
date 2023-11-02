using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs;
using Application.DTOs.Member;
using Application.DTOs.MemberVoucher;
using Application.DTOs.Notification;
using Application.DTOs.Payslip;
using Application.DTOs.Project;
using Application.DTOs.ProjectReportMemberTask;
using Application.DTOs.SalaryCycle;
using Application.DTOs.Transaction;
using Application.DTOs.Wallet;
using Application.Helpers;
using Application.QueryParams;
using Application.QueryParams.Member;
using Application.QueryParams.MemberVoucher;
using Application.QueryParams.Payslip;
using Application.QueryParams.ProjectMemberRequest;
using Application.QueryParams.SalaryCycle;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class MembersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IMemberService _memberService;
        private readonly IWalletService _walletService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IPayslipService _payslipService;
        private readonly ISalaryCycleService _salaryCycleService;
        private readonly IProjectReportMemberTasksService _projectMemberTasksService;
        private readonly ITransactionService _transactionService;

        public MembersController(IMemberService IMemberService,
                              ITransactionService transactionService,
                              IWalletService walletService,
                              IProjectMemberService projectMemberService,
                              IPayslipService payslipService,
                              ISalaryCycleService salaryCycleService,
                              IProjectReportMemberTasksService projectMemberTasksService,
                              IMapper mapper)
        {
            _transactionService = transactionService;
            _walletService = walletService;
            _projectMemberService = projectMemberService;
            _payslipService = payslipService;
            _salaryCycleService = salaryCycleService;
            this._projectMemberTasksService = projectMemberTasksService;
            _memberService = IMemberService;
            _mapper = mapper;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [SwaggerOperation("[ADMIN] Get all Member")]
        public async Task<ResponseDTO<List<MemberDetailedWithRoleDTO>>> GetAll([FromQuery] MemberQueryParams queryParams)
        {
            var userId = User.GetId();
            var members = await _memberService.GetAll(queryParams);

            HttpContext.Response.AddPaginationHeader(members);
            var mappedResult = _mapper.Map<List<MemberDetailedWithRoleDTO>>(members);
            return mappedResult.FormatAsResponseDTO(200);
        }

        // [Authorize(Roles = "ADMIN")]
        // [HttpGet("level-up-")]
        // [SwaggerOperation("[ADMIN] Get all Member Level Up State")]
        // public async Task<ResponseDTO<List<MemberDetailedWithRoleDTO>>> GetAll([FromQuery] MemberQueryParams queryParams)
        // {
        //   var userId = User.GetId();
        //   var members = await _MemberService.GetAll(queryParams);

        //   Pagination.AddPaginationHeader(HttpContext.Response, members);
        //   var mappedResult = _mapper.Map<List<MemberDetailedWithRoleDTO>>(members);
        //   return mappedResult.FormatAsResponseDTO<List<MemberDetailedWithRoleDTO>>(200);
        // }

        [Authorize]
        [HttpGet("{memberId}")]
        [SwaggerOperation("Get member's information")]
        public async Task<ResponseDTO<MemberDetailedWithRoleDTO>> GetMemberByID([FromRoute] Guid memberId)
        {
            var member = await _memberService.GetMemberById(memberId);

            var mappedResult = _mapper.Map<MemberDetailedWithRoleDTO>(member);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{memberId}/projects/count")]
        [SwaggerOperation("Count member's project")]
        public async Task<ResponseDTO<MemberProjectsCountDTO>> GetMemberProjectsCount([FromRoute] Guid memberId)
        {
            var (total, managed) = await _memberService.GetMemberProjectsTotalCount(memberId);
            var result = new MemberProjectsCountDTO() { Total = total, Managed = managed };
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{memberId}/projects")]
        [SwaggerOperation("Get member's project")]
        public async Task<ResponseDTO<List<ProjectWithProjectMemberRoleDTO>>> GetMemberProjects([FromRoute] Guid memberId, [FromQuery] MemberProjectsQueryParams queryParams)
        {
            var projects = await _memberService.GetMemberProjects(queryParams, memberId);

            Response.AddPaginationHeader(projects);
            var mappedResult = _mapper.Map<List<ProjectWithProjectMemberRoleDTO>>(projects);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPatch("me")]
        [SwaggerOperation("Update self Member information")]
        public async Task<ResponseDTO<MemberDTO>> UpdateMember([FromBody] MemberUpdateDTO dto)
        {
            var email = User.GetEmail();
            var result = await _memberService.UpdateMember(email, dto);
            var mappedMember = _mapper.Map<MemberDTO>(result);

            return mappedMember.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("status")]
        [SwaggerOperation("Update Member Status")]
        public async Task<ResponseDTO<bool>> UpdateMemberStatus([FromBody] MemberStatusUpdateDTO dto)
        {
            var result = await _memberService.UpdateMemberStatus(dto);
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation("Get self's information")]
        public async Task<ResponseDTO<MemberDetailedWithRoleDTO>> GetSelf()
        {
            var email = User.GetEmail();
            var user = await _memberService.GetSelf(email);

            var mappedResult = _mapper.Map<MemberDetailedWithRoleDTO>(user);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("Get self transaction")]
        [HttpGet("me/transactions")]
        public async Task<ResponseDTO<List<TransactionDTO>>> GetSelfTransactions([FromQuery] TransactionQueryParams queryParams)
        {
            var trxs = await _transactionService.GetAll(User.GetEmail(), queryParams, TargetType.Member);

            return trxs.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("Get self wallet transaction limit per month!")]
        [HttpGet("me/wallets/limit")]
        public async Task<ResponseDTO<MonthlySendLimitDTO>> GetSelfTransactionLimit()
        {
            var result = await _walletService.GetMonthySendLimitForMember(User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("Send point to other member")]
        [HttpPost("me/wallets/transactions")]
        public async Task<ResponseDTO<string>> SendPointToOther([FromBody] WalletSendPointToOtherDTO dto)
        {
            var result = await _walletService.SendTokenFromMemberToMember(User.GetEmail(), dto);

            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("Get self wallet")]
        [HttpGet("me/wallets")]
        public async Task<ResponseDTO<WalletsInfoDTO>> GetSelfWallet()
        {
            var walletInfo = await _walletService.GetWalletsInfo(User.GetEmail());
            return walletInfo.FormatAsResponseDTO(200);
        }

        [Authorize]
        [SwaggerOperation("Get self done tasks")]
        [HttpGet("me/tasks")]
        public async Task<ResponseDTO<List<ProjectReportMemberTaskForPayslipDTO>>> GetSelfDoneTasks([FromQuery] MemberTasksQueryParams queryParams)
        {
            var tasks = await _projectMemberTasksService.GetMemberTasks(User.GetEmail(), queryParams);
            return tasks.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [SwaggerOperation("[ADMIN] Get member done tasks")]
        [HttpGet("{memberId}/tasks")]
        public async Task<ResponseDTO<List<ProjectReportMemberTaskForPayslipDTO>>> GetMemberDoneTasks([FromRoute] Guid memberId, [FromQuery] MemberTasksQueryParams queryParams)
        {
            var tasks = await _projectMemberTasksService.GetMemberTasks(memberId, queryParams);
            return tasks.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [SwaggerOperation("[ADMIN] Get member's wallet")]
        [HttpGet("{memberId}/wallets")]
        public async Task<ResponseDTO<WalletsInfoDTO>> GetMembersWallet([FromRoute] Guid memberId)
        {
            var walletInfo = await _walletService.GetWalletsInfo(memberId);
            return walletInfo.FormatAsResponseDTO(200);
        }


        [Authorize]
        [HttpGet("me/projects/count")]
        [SwaggerOperation("Count self's project")]
        public async Task<ResponseDTO<MemberProjectsCountDTO>> GetSelfMemberProjectsCount()
        {
            var (total, managed) = await _memberService.GetMemberProjectsTotalCount(User.GetEmail());
            var result = new MemberProjectsCountDTO() { Total = total, Managed = managed };
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/projects")]
        [SwaggerOperation("Get self projects (Include project which you are in and your private one)")]
        public async Task<ResponseDTO<List<ProjectWithProjectMemberRoleDTO>>> GetProjectsSelf([FromQuery] MemberProjectsSelfQueryParams queryParam)
        {
            var email = User.GetEmail();
            var projects = await _memberService.GetSelfMemberProjects(queryParam, email);

            Response.AddPaginationHeader(projects);
            var mappedResult = _mapper.Map<List<ProjectWithProjectMemberRoleDTO>>(projects);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/achievements")]
        [SwaggerOperation("Get self info of what you had done")]
        public async Task<ResponseDTO<MemberAchievementDTO>> GetSelfAchievement()
        {
            var acs = await _memberService.GetMemberAchievement(User.GetEmail());

            return acs.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{memberId}/achievements")]
        [SwaggerOperation("[ADMIN] Get member info of what they had done")]
        public async Task<ResponseDTO<MemberAchievementDTO>> GetMemberAchievement([FromRoute] Guid memberId)
        {
            var acs = await _memberService.GetMemberAchievement(memberId);

            return acs.FormatAsResponseDTO(200);
        }


        [Authorize]
        [HttpGet("me/notifications")]
        [SwaggerOperation("Get self notifications")]
        public async Task<ResponseDTO<List<NotificationDTO>>> GetSelfNotification(
          [FromQuery] MemberNotificationQueryParams queryParams
          )
        {
            var notis = await _memberService.GetSelfNotification(User.GetEmail(), queryParams);

            Response.AddPaginationHeader(notis);
            var mappedResult = _mapper.Map<List<NotificationDTO>>(notis);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/vouchers")]
        [SwaggerOperation("Get self vouchers")]
        public async Task<ResponseDTO<List<MemberVoucherDTO>>> GetSelfVouchers(
          [FromQuery] SelfVoucherQueryParams queryParams
          )
        {
            var voucher = await _memberService.GetAllSelfVoucher(queryParams, User.GetEmail());

            Response.AddPaginationHeader(voucher);
            var mappedResult = _mapper.Map<List<MemberVoucherDTO>>(voucher);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/payslips")]
        [SwaggerOperation("Get self payslip")]
        public async Task<ResponseDTO<List<PayslipDTO>>> GetPayslipSelf([FromQuery] MemberPayslipQueryParams queryParams)
        {
            var email = User.GetEmail();
            var payslips = await _payslipService.GetPayslipsForMember(email, queryParams);

            HttpContext.Response.AddPaginationHeader(payslips);
            var mappedPayslips = _mapper.Map<List<PayslipDTO>>(payslips);

            return mappedPayslips.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("/v2/members/me/payslips")]
        [SwaggerOperation("Get self payslip")]
        public async Task<ResponseDTO<List<PayslipV2DTO>>> GetPayslipSelfV2([FromQuery] MemberPayslipQueryParams queryParams)
        {
            var email = User.GetEmail();
            var payslips = await _payslipService.GetPayslipsForMember(email, queryParams);

            HttpContext.Response.AddPaginationHeader(payslips);
            var mappedPayslips = _mapper.Map<List<PayslipV2DTO>>(payslips);

            return mappedPayslips.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/payslips/info")]
        [SwaggerOperation("Get self payslip's info")]
        public async Task<ResponseDTO<PayslipsTotalDTO>> GetPayslipSelfInfo([FromQuery] MemberPayslipTotalQueryParams queryParams)
        {
            var email = User.GetEmail();
            var psInfo = await _payslipService.GetPayslipsForMemberTotal(email, queryParams);

            return psInfo.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/salaryCycle")]
        [SwaggerOperation("Get salary cycle that you are in!")]
        public async Task<ResponseDTO<List<SalaryCycleDTO>>> GetSalaryCycleSelf([FromQuery] SalaryCycleQueryParams queryParams)
        {
            var projects = await _salaryCycleService.GetAllSalaryCycleOfMember(User.GetEmail(), queryParams);

            HttpContext.Response.AddPaginationHeader(projects);
            var mappedProjects = _mapper.Map<List<SalaryCycleDTO>>(projects);

            return mappedProjects.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/projectMemberRequests")]
        [SwaggerOperation("Get self project's join requests ")]
        public async Task<ResponseDTO<List<PayslipDTO>>> GetSelfProjectMemberRequests([FromQuery] SelfProjectMemberRequestQueryParams queryParams)
        {
            var email = User.GetEmail();
            var projects = await _projectMemberService.GetSelfProjectMemberRequest(queryParams, email);

            HttpContext.Response.AddPaginationHeader(projects);
            var mappedProjects = _mapper.Map<List<PayslipDTO>>(projects);

            return mappedProjects.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{memberId}/payslips")]
        [SwaggerOperation("[ADMIN] Get member's payslip ")]
        public async Task<ResponseDTO<List<PayslipV2DTO>>> GetPayslipForMember([FromRoute] Guid memberId, [FromQuery] MemberPayslipQueryParams queryParams)
        {
            var projects = await _payslipService.GetPayslipsForMember(memberId, queryParams);

            HttpContext.Response.AddPaginationHeader(projects);
            var mappedProjects = _mapper.Map<List<PayslipV2DTO>>(projects);

            return mappedProjects.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{memberId}/payslips/info")]
        [SwaggerOperation("[ADMIN] Get member's payslip's info")]
        public async Task<ResponseDTO<PayslipsTotalDTO>> GetPayslipForMemberInfo([FromRoute] Guid memberId, [FromQuery] MemberPayslipTotalQueryParams queryParams)
        {
            var payslipInfo = await _payslipService.GetPayslipsForMemberTotal(memberId, queryParams);
            return payslipInfo.FormatAsResponseDTO(200);
        }

    }
}
