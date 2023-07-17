using Application.Domain.Models;
using Application.DTOs.PayslipItems;
using Application.Helpers;
using Application.QueryParams.PayslipItem;

namespace Application.Services
{
    public interface IPayslipItemService
    {
        Task<PagedList<PayslipItem>> GetPayslipItems(PayslipItemQueryParams queryParams, string requesterEmail, bool isAdmin = false);
        Task<PayslipItemsTotalDTO> GetPayslipItemsTotal(PayslipItemTotalQueryParams queryParams, string requesterEmail, bool isAdmin = false);
    }
}
