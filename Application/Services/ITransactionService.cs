using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.Transaction;
using Application.Helpers;
using Application.QueryParams;

namespace Application.Services
{
  public interface ITransactionService
  {
    Task<List<TransactionDTO>> GetAll(string? emaildOrId, TransactionQueryParams queryParams, TargetType targetType, String? requesterEmail = null, bool isAdmin = false);
    Task<Wallet?> GetByID(Guid id);
    Task<bool> Insert(Wallet w);
    Task<bool> SaveAsync();
    Task<bool> Update(Wallet w);
  }
}