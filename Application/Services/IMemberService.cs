using Application.Domain.Models;
using Application.DTOs.Member;
using Application.Helpers;
using Application.QueryParams.Member;
using Application.QueryParams.MemberVoucher;

namespace Application.Services
{
    public interface IMemberService
    {
        Task<PagedList<Member>> GetAll(MemberQueryParams queryParams);
        Task<PagedList<MemberVoucher>> GetAllSelfVoucher(SelfVoucherQueryParams queryParams, string requesterEmail);
        Task<MemberAchievementDTO> GetMemberAchievement(string memberEmail);
        Task<MemberAchievementDTO> GetMemberAchievement(Guid memberId);
        Task<Member?> GetMemberById(Guid memberId);
        Task<PagedList<Project>> GetMemberProjects(MemberProjectsQueryParams queryParams, Guid memberId);
        Task<(int, int)> GetMemberProjectsTotalCount(Guid memberId);
        Task<(int, int)> GetMemberProjectsTotalCount(string email);
        Task<Member?> GetSelf(string email);
        Task<PagedList<Project>> GetSelfMemberProjects(MemberProjectsSelfQueryParams queryParams, string email);
        Task<PagedList<Notification>> GetSelfNotification(string email, MemberNotificationQueryParams queryParams);
        Task<Member> UpdateMember(string email, MemberUpdateDTO dto);
        Task<bool> UpdateMemberStatus(MemberStatusUpdateDTO dto);
    }
}