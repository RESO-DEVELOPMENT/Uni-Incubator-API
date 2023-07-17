using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.Transaction;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.QueryParams;
using Application.QueryParams.Transaction;
using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Packaging.Ionic.Zip;

namespace Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionService(
            UnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all transaction of target
        /// </summary>
        /// <param name="emaildOrId">target's email</param>
        /// <param name="queryParams">Query Parameters</param>
        /// <param name="targetType">Target's type</param>
        /// <param name="requesterEmail"></param>
        /// <param name="isAdmin"></param>
        /// <returns>List <see cref="Application.Domain.Models.Transaction"/> of user</returns>
        public async Task<List<TransactionDTO>> GetAll(string? emaildOrId, TransactionQueryParams queryParams, TargetType targetType, string? requesterEmail = null, bool isAdmin = false)
        {
            if (queryParams is { IsReceived: not null, IsSent: not null })
                throw new BadRequestException("Please only use one of the parameters [IsReceived, IsSent]", ErrorNameValues.InvalidParameters);

            var requesterMember = requesterEmail == null ? null : await _unitOfWork.MemberRepository.GetByEmail(requesterEmail);
            switch (targetType)
            {
                case TargetType.Member:
                    {
                        var email = emaildOrId;

                        if (!isAdmin && requesterMember != null && requesterMember.EmailAddress != email)
                        {
                            throw new BadRequestException("No permission", ErrorNameValues.NoPermission);
                        }

                        var query = _unitOfWork.TransactionRepository.GetQuery();
                        query = query
                          .Include(t => t.ToWallet)
                            .ThenInclude(w => w.MemberWallet)
                              .ThenInclude(uw => uw!.Member)
                          .Include(t => t.FromWallet)
                            .ThenInclude(w => w.MemberWallet)
                              .ThenInclude(uw => uw!.Member)
                          .Where(t => t.FromWallet!.MemberWallet!.Member.EmailAddress == email
                           || t.ToWallet!.MemberWallet!.Member.EmailAddress == email);

                        if (queryParams.FromDate != null) query = query.Where(q => q.CreatedAt >= queryParams.FromDate);
                        if (queryParams.ToDate != null) query = query.Where(q => q.CreatedAt <= queryParams.ToDate);

                        if (queryParams.Types.Any()) query = query.Where(trx => queryParams.Types.Contains(trx.TransactionType));
                        switch (queryParams.OrderBy)
                        {
                            case TransactionOrderBy.CreatedAtAsc:
                                {
                                    query = query.OrderBy(x => x.CreatedAt);
                                    break;
                                }
                            case TransactionOrderBy.CreatedAtDesc:
                                {
                                    query = query.OrderByDescending(x => x.CreatedAt);
                                    break;
                                }
                        }

                        // Sent and Received

                        if (queryParams.IsReceived is true)
                        {
                            query = query.Where(x =>
                                x.FromWallet.MemberWallet == null ||
                                x.FromWallet.MemberWallet.Member.EmailAddress != email);
                        }

                        if (queryParams.IsSent is true)
                        {
                            query = query.Where(x =>
                                x.FromWallet.MemberWallet != null &&
                                x.FromWallet.MemberWallet.Member.EmailAddress == email);
                        }

                        var trxs = await PagedList<Transaction>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
                        Pagination.AddPaginationHeader(_httpContextAccessor.HttpContext.Response, trxs);

                        var trxsDTO = _mapper.Map<List<TransactionDTO>>(trxs);

                        trxs.ForEach(x =>
                        {
                            if (x.FromWallet.MemberWallet == null || x.FromWallet.MemberWallet.Member.EmailAddress != email)
                            {
                                // Is Received
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.ToAmountAfterTransaction;
                            }
                            else if (x.FromWallet.MemberWallet.Member.EmailAddress == email)
                            {
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).IsReceived = false;
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.FromAmountAfterTransaction;
                            }
                        });

                        return trxsDTO;
                    }
                case TargetType.Project:
                    {
                        if (emaildOrId == null) throw new NotFoundException("Project not found!");
                        var id = Guid.Parse(emaildOrId!);

                        var project = await _unitOfWork.ProjectRepository.GetByID(id) ?? throw new NotFoundException("Project Not Found!", ErrorNameValues.ProjectNotFound);

                        if (!isAdmin)
                        {
                            if (requesterMember == null) throw new BadRequestException("No permission", ErrorNameValues.NoPermission);;
                            var pm = await _unitOfWork.ProjectMemberRepository.TryGetProjectMemberManagerActive(project.ProjectId, requesterEmail) ?? throw new BadRequestException("No permission", ErrorNameValues.NoPermission);
                        }

                        var query = _unitOfWork.TransactionRepository.GetQuery();
                        query = query
                          .Include(t => t.ToWallet)
                            .ThenInclude(w => w.ProjectWallet)
                              .ThenInclude(uw => uw!.Project)
                          .Include(t => t.FromWallet)
                            .ThenInclude(w => w.ProjectWallet)
                              .ThenInclude(uw => uw!.Project)
                          .Where(t => t.FromWallet!.ProjectWallet!.Project.ProjectId == id
                           || t.ToWallet!.ProjectWallet!.Project.ProjectId == id);

                        if (queryParams.FromDate != null) query = query.Where(q => q.CreatedAt >= queryParams.FromDate);
                        if (queryParams.ToDate != null) query = query.Where(q => q.CreatedAt <= queryParams.ToDate);
                        if (queryParams.Types.Any()) query = query.Where(trx => queryParams.Types.Contains(trx.TransactionType));
                        switch (queryParams.OrderBy)
                        {
                            case TransactionOrderBy.CreatedAtAsc:
                                {
                                    query = query.OrderBy(x => x.CreatedAt);
                                    break;
                                }
                            case TransactionOrderBy.CreatedAtDesc:
                                {
                                    query = query.OrderByDescending(x => x.CreatedAt);
                                    break;
                                }
                        }

                        if (queryParams.IsReceived is true)
                        {
                            query = query.Where(x =>
                                x.FromWallet.ProjectWallet == null ||
                                x.FromWallet.ProjectWallet.ProjectId != id);
                        }

                        if (queryParams.IsSent is true)
                        {
                            query = query.Where(x =>
                                x.FromWallet.ProjectWallet != null &&
                                x.FromWallet.ProjectWallet.ProjectId == id);
                        }

                        var trxs = await PagedList<Transaction>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
                        Pagination.AddPaginationHeader(_httpContextAccessor.HttpContext.Response, trxs);

                        var trxsDTO = _mapper.Map<List<TransactionDTO>>(trxs);

                        trxs.ForEach(x =>
                        {
                            if (x.FromWallet.ProjectWallet == null || x.FromWallet.ProjectWallet.ProjectId != id)
                            {
                                // Is Received
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).IsReceived = true;
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.ToAmountAfterTransaction;
                            }
                            else if (x.FromWallet.ProjectWallet.ProjectId == id)
                            {
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).IsReceived = false;
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.FromAmountAfterTransaction;
                            }
                        });

                        return trxsDTO;
                    }
                case TargetType.System:
                    {
                        var query = _unitOfWork.TransactionRepository.GetQuery();
                        query = query
                          .Include(t => t.ToWallet)
                          .Include(t => t.FromWallet)
                          .Where(t => t.FromWallet.IsSystem)
                          .Where(t => t.ToWallet.IsSystem);

                        if (queryParams.FromDate != null) query = query.Where(q => q.CreatedAt >= queryParams.FromDate);
                        if (queryParams.ToDate != null) query = query.Where(q => q.CreatedAt <= queryParams.ToDate);

                        if (queryParams.Types.Any()) query = query.Where(trx => queryParams.Types.Contains(trx.TransactionType));
                        switch (queryParams.OrderBy)
                        {
                            case TransactionOrderBy.CreatedAtAsc:
                                {
                                    query = query.OrderBy(x => x.CreatedAt);
                                    break;
                                }
                            case TransactionOrderBy.CreatedAtDesc:
                                {
                                    query = query.OrderByDescending(x => x.CreatedAt);
                                    break;
                                }
                        }
                        var trxs = await PagedList<Transaction>.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
                        Pagination.AddPaginationHeader(_httpContextAccessor.HttpContext.Response, trxs);

                        var trxsDTO = _mapper.Map<List<TransactionDTO>>(trxs);
                        trxs.ForEach(x =>
                        {
                            if (!x.FromWallet.IsSystem)
                            {
                                // Is Received
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.ToAmountAfterTransaction;
                            }
                            else if (x.FromWallet.IsSystem)
                            {
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).IsReceived = false;
                                trxsDTO.First(tr => tr.TransactionId == x.TransactionId).AmountLeft = x.FromAmountAfterTransaction;
                            }
                        });

                        return trxsDTO;
                    }
                default:
                    return null!;
            }
        }

        public async Task<Wallet?> GetByID(Guid id)
        {
            var wallet = await _unitOfWork.WalletRepository.GetQuery()
                .Where(x => x.WalletId == id)
                    .FirstOrDefaultAsync();

            return wallet;
        }

        public async Task<bool> Insert(Wallet w)
        {
            _unitOfWork.WalletRepository.Add(w);
            return await _unitOfWork.SaveAsync();
        }

        public async Task<bool> Update(Wallet w)
        {
            _unitOfWork.WalletRepository.Update(w);
            return await _unitOfWork.SaveAsync();
        }

        public async Task<bool> SaveAsync()
        {
            return await _unitOfWork.SaveAsync();
        }
    }
}