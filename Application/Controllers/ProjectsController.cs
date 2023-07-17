using Application.Domain.Enums.Wallet;
using Application.DTOs;
using Application.DTOs.Payslip;
using Application.DTOs.PayslipItem;
using Application.DTOs.Project;
using Application.DTOs.Transaction;
using Application.DTOs.Wallet;
using Application.Helpers;
using Application.QueryParams;
using Application.QueryParams.Payslip;
using Application.QueryParams.Project;
using Application.QueryParams.ProjectBonus;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class ProjectsController : BaseApiController
    {
        private readonly ITransactionService _transactionService;
        private readonly IPayslipService _payslipService;
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectsController(IProjectService projectService,
            ITransactionService transactionService,
                                  IPayslipService payslipService,
                                  IMapper mapper)
        {
            _projectService = projectService;
            _transactionService = transactionService;
            _payslipService = payslipService;
            _mapper = mapper;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [SwaggerOperation("[ADMIN] Create new project")]
        public async Task<ResponseDTO<Guid>> CreateProject([FromBody] ProjectCreateDTO dto)
        {
            var newProjectId = await _projectService.CreateProject(dto);

            return newProjectId.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("admin")]
        [SwaggerOperation("[Admin] Update project")]
        public async Task<ResponseDTO<bool>> AdminUpdateProject([FromBody] ProjectAdminUpdateDTO dto)
        {
            var result = await _projectService.UpdateProjectAsAdmin(dto);
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPatch("pm")]
        [SwaggerOperation("[PM] Update project")]
        public async Task<ResponseDTO<bool>> PMUpdateProject([FromBody] ProjectPMUpdateDTO dto)
        {
            var result = await _projectService.UpdateProjectAsPM(dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("status")]
        [SwaggerOperation("[ADMIN/PM] Change project status")]
        public async Task<ResponseDTO<bool>> UpdateProjectStatus([FromBody] ProjectStatusUpdateDTO dto)
        {
            var result = await _projectService.UpdateProjectStatus(dto, User.GetEmail(), User.IsAdmin());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet]
        [SwaggerOperation("Get all projects (Admin can get private)")]
        public async Task<ResponseDTO<List<ProjectDetailDTO>>> GetProjects([FromQuery] ProjectQueryParams queryParams)
        {
            if (!User.IsAdmin()) queryParams.IncludePrivate = false;

            var projects = await _projectService.GetAll(queryParams);

            HttpContext.Response.AddPaginationHeader(projects);
            var mappedProjects = _mapper.Map<List<ProjectDetailDTO>>(projects);

            return mappedProjects.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}")]
        [SwaggerOperation("Get project by id")]
        public async Task<ResponseDTO<ProjectDetailWithMemberLevelDTO>> GetProjectById(Guid projectId)
        {
            var project = await _projectService.GetProjectById(projectId, User.GetEmail(), User.IsAdmin());

            var mappedProject = _mapper.Map<ProjectDetailWithMemberLevelDTO>(project);
            return mappedProject.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/wallet")]
        [SwaggerOperation("Get project's wallet")]
        public async Task<ResponseDTO<List<WalletDTO>>> GetProjectWalletById(Guid projectId)
        {
            var project = await _projectService.GetProjectWalletById(projectId, User.GetEmail(), User.IsAdmin());

            var mappedProject = _mapper.Map<List<WalletDTO>>(project);
            return mappedProject.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/transactions")]
        [SwaggerOperation("Get project's transactions")]
        public async Task<ResponseDTO<List<TransactionDTO>>> GetProjectTransactions([FromRoute] Guid projectId, [FromQuery] TransactionQueryParams queryParams)
        {
            var trxs = await _transactionService.GetAll(projectId.ToString(), queryParams, TargetType.Project, User.GetEmail(), User.IsAdmin());

            return trxs.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("{projectId}/wallet")]
        [SwaggerOperation("[Admin} Send point to project from system (Able when salary cycle is task editing or project bonus)")]
        public async Task<ResponseDTO<bool>> SendPointToProject(Guid projectId, ProjectSendPointDTO dto)
        {
            var email = User.GetEmail();
            var project = await _projectService.SendPointToProject(projectId, dto);

            var mappedProject = _mapper.Map<bool>(project);
            return mappedProject.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/bonus")]
        [SwaggerOperation("[PM] Get bonus payslip item from project")]
        public async Task<ResponseDTO<List<PayslipItemDTO>>> GetBonusPayslipItem(Guid projectId, [FromQuery] ProjectBonusQueryParams queryParams)
        {
            var email = User.GetEmail();
            var pss = await _projectService.GetBonusFromProject(projectId, queryParams, User.GetEmail());

            Pagination.AddPaginationHeader(Response, pss);
            var mappedPls = _mapper.Map<List<PayslipItemDTO>>(pss);
            return mappedPls.FormatAsResponseDTO(200);
        }

        // [Authorize]
        // [HttpPost("{projectId}/bonus")]
        // [SwaggerOperation("[PM] Create bonus payslip item for member")]
        // public async Task<ResponseDTO<bool>> CreateBonusPayslipItem(Guid projectId, ProjectBonusCreateDTO dto)
        // {
        //   var email = User.GetEmail();
        //   var project = await _projectService.CreateBonusForProject(projectId, dto, User.GetEmail());

        //   var mappedProject = _mapper.Map<bool>(project);
        //   return mappedProject.FormatAsResponseDTO(200);
        // }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("{projectId}/files/final-report")]
        [SwaggerOperation("[ADMIN] Update project final report file ")]
        public async Task<ResponseDTO<ProjectWithFilesDTO>> UpdateProjectFinalReport([FromRoute] Guid projectId,
        IFormFile file)
        {
            var result = await _projectService.UploadProjectFinalReport(projectId, file);

            var mapped = _mapper.Map<ProjectWithFilesDTO>(result);
            return mapped.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/payslips")]
        [SwaggerOperation("[Admin] [PM] Get project's payslips")]
        public async Task<ResponseDTO<List<PayslipDTO>>> GetPayslips([FromRoute] Guid projectId,
        [FromQuery] ProjectPayslipQueryParams queryParam)
        {
            var payslips = await _payslipService.GetPayslipsForProject(projectId, queryParam, User.GetEmail(), User.IsAdmin());

            Response.AddPaginationHeader(payslips);
            var mappedResult = _mapper.Map<List<PayslipDTO>>(payslips);
            return mappedResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("{projectId}/payslips/total")]
        [SwaggerOperation("[Admin] [PM] Get project's payslip total")]
        public async Task<ResponseDTO<PayslipsTotalDTO>> GetPayslipsInfo([FromRoute] Guid projectId,
        [FromQuery] ProjectPayslipTotalQueryParams queryParam)
        {
            var payslipsInfo = await _payslipService.GetPayslipsTotalForProject(projectId, queryParam, User.GetEmail(), User.IsAdmin());

            return payslipsInfo.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("withTotalSponsoredStatus")]
        [SwaggerOperation("[Admin] Get project with funds")]
        public async Task<ResponseDTO<List<ProjectWithTotalFundDTO>>> GetProjectWithTotalFunds([FromQuery] ProjectMinimalQueryParams queryParam)
        {
            var payslipsInfo = await _projectService.GetAllWithTransactions(queryParam);

            return payslipsInfo.FormatAsResponseDTO(200);
        }
    }
}
