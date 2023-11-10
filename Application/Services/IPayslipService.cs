using Application.Domain.Models;
using Application.DTOs.Payslip;
using Application.Helpers;
using Application.QueryParams.Payslip;

namespace Application.Services
{
    public interface IPayslipService
    {
        Task<PagedList<Payslip>> GetPayslipsForMember(string memberEmail, MemberPayslipQueryParams queryParams);
        Task<PayslipsTotalDTO> GetPayslipsForMemberTotal(string memberEmail, MemberPayslipTotalQueryParams queryParams);

        Task<PagedList<Payslip>> GetPayslipsForMember(Guid memberId, MemberPayslipQueryParams queryParams);
        Task<PayslipsTotalDTO> GetPayslipsForMemberTotal(Guid memberId, MemberPayslipTotalQueryParams queryParams);

        Task<PayslipsTotalDTO> GetPayslipsTotalForProject(Guid projectId, ProjectPayslipTotalQueryParams queryParams, String requesterEmail, bool isAdmin = false);
        Task<PagedList<Payslip>> GetPayslipsForProject(Guid projectId, ProjectPayslipQueryParams queryParams, String requesterEmail, bool isAdmin = false);

        Task<PayslipsTotalDTO> GetPayslipsTotal(PayslipTotalQueryParams queryParams);
        Task<PagedList<Payslip>> GetPayslips(PayslipQueryParams queryParams);

        Task<Payslip> GetPayslipsById(Guid payslipId);


        Task<Payslip> UpdatePayslipStatus(Guid payslipId);
    }
}