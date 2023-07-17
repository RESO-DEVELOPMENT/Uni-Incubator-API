using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Application.Domain;
using Application.Domain.Enums;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.Wallet;
using Application.Helpers;
using Application.Persistence.Repositories;
using AutoMapper;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using MimeKit.Encodings;
using org.mariuszgromada.math.mxparser;

namespace Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IMapper _mapper;
        private readonly IQueueService _redisQueueService;
        private readonly IUserService _userService;
        private readonly UnitOfWork _unitOfWork;

        public WalletService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            IQueueService redisQueueService,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _redisQueueService = redisQueueService;
            _userService = userService;
        }

        public async Task<bool> TestAward(string email)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery().Where(e => e.EmailAddress == email).FirstOrDefaultAsync();
            var result = await SendToken(Guid.Empty, member!.MemberId, 100, WalletToken.Point, TransactionType.NewAccount);
            return result.Any();
            // await CreateWalletForMember(Member.MemberId, WalletType.Hot, WalletToken.Point, 250); ;
        }

        public async Task<string> SendTokenFromMemberToMember(string fromEmail, WalletSendPointToOtherDTO dto)
        {
            var fromMember = await _unitOfWork.MemberRepository.GetByEmail(fromEmail)
                ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            var toMember = await _unitOfWork.MemberRepository.GetByID(dto.ToMemberId)
                ?? throw new NotFoundException("Destination member not found!", ErrorNameValues.MemberNotFound);

            if (fromMember.MemberId == toMember.MemberId)
                throw new BadRequestException("You can't send point to yourself!", ErrorNameValues.InvalidParameters);

            var fromWallets = await GetAllWalletByTargetId(fromMember.MemberId, TargetType.Member, WalletToken.Point, false);
            if (fromWallets.Total(WalletToken.Point) < dto.Amount)
                throw new BadRequestException("You don't have enough token!", ErrorNameValues.InsufficentToken);

            var limit = await GetMonthySendLimitForMember(fromMember.EmailAddress);
            if (limit.PointLeft - dto.Amount < 0)
            {
                throw new BadRequestException("The amount exceed your monthly allowance!", ErrorNameValues.ExceedLimit);
            }

            // Check Pin Code
            await _userService.CheckUserPinCode(dto.PinCode, fromMember.EmailAddress);

            await _redisQueueService.AddToQueue(TaskName.SendPoint, new Dictionary<string, string>()
            {
                {"FromId", fromMember.MemberId.ToString()},
                {"ToId", toMember.MemberId.ToString()},
                {"Amount", dto.Amount.ToString()},
                {"TransactionType", TransactionType.MemberToMember.ToString()},
                {"Note", $"Point từ {fromMember.EmailAddress} đến {toMember.EmailAddress}"},
                {"SendNotification", "True"}
            });

            return "Transfer request had been sent, please wait...";
        }

        public async Task<MonthlySendLimitDTO> GetMonthySendLimitForMember(string requesterEmail)
        {
            var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            return await GetMonthySendLimitForMember(requesterMember.MemberId);
        }

        public async Task<MonthlySendLimitDTO> GetMonthySendLimitForMember(Guid memberId)
        {
            var requesterMember = await _unitOfWork.MemberRepository.GetQuery()
                .Include(x => x.MemberLevels.Where(x => x.IsActive))
                .ThenInclude(ml => ml.Level)
                .FirstOrDefaultAsync(x => x.MemberId == memberId) ?? throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

            var transactionInMonth = await _unitOfWork.TransactionRepository.GetQuery()
                .Where(x =>
                    x.FromWallet.MemberWallet.MemberId == requesterMember.MemberId &&
                    x.TransactionType == TransactionType.MemberToMember)
                .SumAsync(x => x.Amount);

            var limitEq = GlobalVar.SystemConfig.MaxMemberSendPerMonthEquation
                .Replace("{levelBasePoint}", requesterMember.MemberLevels.First().Level.BasePoint.ToString());

            var eq = new Expression(limitEq);
            var limit = eq.calculate();

            return new MonthlySendLimitDTO()
            {
                MaxPointPerMonth = limit,
                PointLeft = limit - transactionInMonth
            };
        }

        public async Task<List<Transaction>> SendToken(
        Guid fromTargetId,
        Guid toTargetId,
        double amount,
        WalletToken walletToken,
        TransactionType type,
        List<string>? fromWalletTags = null, List<string>? toWalletTags = null,
        string? note = null)
        {
            // Check for targets
            var targetTypes = TransactionHelper.GetTargetTypesFromTransactionType(type)!;

            List<Wallet> fromTargetWallet = new List<Wallet>();
            List<Wallet> toTargetWallet = new List<Wallet>();

            var fromTargetType = targetTypes.Value.Item1;
            var toTargetType = targetTypes.Value.Item2;

            // Fix TargetId for System
            if (fromTargetType == TargetType.System) fromTargetId = Guid.Empty;
            if (toTargetType == TargetType.System) toTargetId = Guid.Empty;

            // Extract Wallets WIth Correct Token
            var fromWallets = await GetAllWalletByTargetId(fromTargetId, fromTargetType, walletToken, false, tags: fromWalletTags);

            var toWallets = await GetAllWalletByTargetId(toTargetId, toTargetType, walletToken, false, tags: toWalletTags);

            // Check if all wallets have enought amount of token
            var currentAmount = fromWallets.Total(walletToken);
            var requiredAmount = amount;
            if (currentAmount < requiredAmount) throw new BadRequestException("Not enough token!", ErrorNameValues.InsufficentToken);

            fromWallets = fromWallets.SortWallets(); // Sort wallet to ready to deduct token
            var destWallet = toWallets
             .Where(w => w.WalletType == WalletType.Cold)
             .OrderBy(w => w.ExpiredDate).FirstOrDefault();

            if (destWallet == null)
            {
                throw new BadRequestException("Destination wallet not found!", ErrorNameValues.SystemError);
            }

            var transactions = new List<Transaction>();

            //var fromAllWallets = await GetAllWalletByTargetId(fromTargetId, fromTargetType, walletToken, false);
            //var toAllWallets = await GetAllWalletByTargetId(toTargetId, toTargetType, walletToken, false);

            //var fromLog = WalletHelpers.Total(fromAllWallets, WalletToken.Point);
            //var toLog = WalletHelpers.Total(toAllWallets, WalletToken.Point);

            foreach (var wallet in fromWallets)  // Deduct until requiredAmount is 0
            {
                var walletAmount = wallet.Amount;
                var deductAmount = 0d;

                if (walletAmount <= requiredAmount)
                { // If wallet have less or equal to requiredAmount
                    deductAmount = walletAmount;
                    wallet.Amount = 0;

                    if (wallet.WalletType == WalletType.Hot) // If this is hot wallet, disable it
                        wallet.WalletStatus = WalletStatus.Unavailable;

                    requiredAmount -= walletAmount;
                }
                else if (walletAmount > requiredAmount)
                {  // If wallet has enough token, deduct only the needed amount
                    deductAmount = requiredAmount;

                    wallet.Amount -= requiredAmount;
                    requiredAmount -= walletAmount;
                }
                var tran = new Transaction()
                {
                    FromWalletId = wallet.WalletId,
                    ToWalletId = destWallet.WalletId,
                    TransactionType = type,
                    Token = walletToken,
                    Amount = deductAmount,
                    Note = note,

                    FromAmountAfterTransaction = wallet.Amount,
                    ToAmountAfterTransaction = destWallet.Amount + deductAmount
                };

                transactions.Add(tran);
                wallet.TransactionsFrom.Add(tran);
                // Deduct complete
                if (requiredAmount <= 0) break;
            }

            // Add token to to target's cold wallet
            destWallet.Amount += amount;

            _unitOfWork.WalletRepository.Update(fromWallets);
            if (toWallets.Count() == 0)
            {
                _unitOfWork.WalletRepository.Add(destWallet);
            }
            else
            {
                _unitOfWork.WalletRepository.Update(toWallets);
            }

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Fail!");

            return transactions;
        }

        public async Task<WalletsInfoDTO> GetWalletsInfo(String requesterEmail)
        {
            var member = await _unitOfWork.MemberRepository.GetQuery()
              .Where(e => e.EmailAddress == requesterEmail)
              .FirstOrDefaultAsync();

            if (member == null) throw new BadRequestException("There is no Member with that email!", ErrorNameValues.MemberNotFound);

            return await GetWalletsInfo(member.MemberId);
        }

        public async Task<WalletsInfoDTO> GetWalletsInfo(Guid memberId)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(memberId);

            if (member == null) throw new BadRequestException("There is no Member with that id!", ErrorNameValues.MemberNotFound);

            var wallets = await GetAllWalletByTargetId(member.MemberId, TargetType.Member);
            wallets = wallets.SortWallets();

            var mappedWallets = _mapper.Map<List<WalletDTO>>(wallets);

            WalletsInfoDTO walletInfoDTO = new WalletsInfoDTO();

            walletInfoDTO.Wallets.AddRange(mappedWallets);
            walletInfoDTO.TotalXP = wallets.Total(WalletToken.XP);
            walletInfoDTO.TotalPoint = wallets.Total(WalletToken.Point);

            return walletInfoDTO;
        }

        /// <summary>
        /// Get system's wallet info
        /// </summary>
        /// <returns><see cref="WalletsInfoDTO"/>  of system</returns>
        /// <exception cref="BadRequestException"/>
        public async Task<WalletsInfoDTO> GetSystemWalletInfo()
        {
            var wallets = await GetAllWalletByTargetId(Guid.Empty, TargetType.System);
            wallets = wallets.SortWallets();

            var mappedWallets = _mapper.Map<List<WalletDTO>>(wallets);

            WalletsInfoDTO walletInfoDTO = new WalletsInfoDTO();

            walletInfoDTO.Wallets.AddRange(mappedWallets);
            walletInfoDTO.TotalXP = wallets.Total(WalletToken.XP);
            walletInfoDTO.TotalPoint = wallets.Total(WalletToken.Point);

            return walletInfoDTO;
        }

        /// <summary>
        /// Get all wallets from target (Default to Get All and Available)
        /// </summary>
        /// <param name="targetId">Target's Id</param>
        /// <param name="targetType">Target's Type</param>
        /// <param name="walletToken">Token to get (Get all if null)</param>
        /// <param name="withExpired">Get expired wallet also</param>
        /// <param name="walletType"></param>
        /// <param name="tags"></param>
        /// <returns>List <see cref="Application.Domain.Models.Wallet"/> of user</returns>
        public async Task<List<Wallet>> GetAllWalletByTargetId(Guid targetId,
        TargetType targetType,
        WalletToken? walletToken = null,
        bool withExpired = false,
        WalletType? walletType = null,
        List<String>? tags = null)
        {
            var query = _unitOfWork.WalletRepository.GetQuery();

            // Parse Query With Correct Type
            switch (targetType)
            {
                case TargetType.Project:
                    {
                        var project = await _unitOfWork.ProjectRepository.GetByID(targetId) ??
                            throw new NotFoundException("Project not found!", ErrorNameValues.ProjectNotFound);

                        query = query
                     .Include(w => w.ProjectWallet)
                     .ThenInclude(uw => uw!.Project)
                     .Where(w => w.ProjectWallet!.ProjectId == targetId);
                        break;
                    }
                case TargetType.Member:
                    {
                        var member = await _unitOfWork.MemberRepository.GetByID(targetId) ??
                                      throw new NotFoundException("Member not found!", ErrorNameValues.MemberNotFound);

                        query = query
                     .Include(w => w.MemberWallet)
                     .ThenInclude(uw => uw!.Member)
                     .Where(w => w.MemberWallet!.MemberId == targetId);
                        break;
                    }
                case TargetType.System:
                    {
                        query = query
                                 .Where(w => w.IsSystem);
                        break;
                    }
            }

            if (walletToken != null) query = query.Where(w => w.WalletToken == walletToken);
            if (walletType != null) query = query.Where(w => w.WalletType == walletType);
            if (!withExpired) query = query.Where(w => w.WalletStatus == WalletStatus.Available);
            if (tags != null) query = query.Where(w => tags.Contains(w.WalletTag!));

            List<Wallet> wallets = await query.ToListAsync();
            return wallets;
        }

        public async Task<Wallet> CreateWalletForMember(Guid empId, WalletType walletType, WalletToken walletToken, double amount)
        {
            var member = await _unitOfWork.MemberRepository.GetByID(empId);
            if (member == null) throw new NotFoundException("There is no Member with that id!", ErrorNameValues.MemberNotFound);

            var wallet = new Wallet()
            {
                WalletToken = walletToken,
                WalletType = walletType,
                Amount = amount,
                ExpiredDate = DateTimeHelper.Now().AddDays(30),
                MemberWallet = new MemberWallet() { MemberId = member.MemberId }
            };

            _unitOfWork.WalletRepository.Add(wallet);

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Save error!", ErrorNameValues.SystemError);
            return wallet;
        }

        public async Task<Wallet> CreateWalletForProject(Guid projectId, WalletType walletType, WalletToken walletToken, double amount, String? tag)
        {
            var proj = await _unitOfWork.ProjectRepository.GetByID(projectId);
            if (proj == null) throw new NotFoundException("There is no Project with that id!", ErrorNameValues.ProjectNotFound);

            var wallet = new Wallet()
            {
                WalletToken = walletToken,
                WalletType = walletType,
                Amount = amount,
                ExpiredDate = DateTimeHelper.Now().AddDays(30),
                ProjectWallet = new ProjectWallet() { ProjectId = proj.ProjectId }
            };

            _unitOfWork.WalletRepository.Add(wallet);

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new BadRequestException("Save error!", ErrorNameValues.SystemError);
            return wallet;
        }

        public async Task<bool> ExpireWallet(Guid walletId)
        {
            var query = _unitOfWork.WalletRepository.GetQuery();
            var systemWallet = await query.FirstOrDefaultAsync(w =>
            w.IsSystem == true &&
            w.WalletToken == WalletToken.Point);

            var wallet = await query.FirstOrDefaultAsync(w => w.WalletId == walletId);

            var trx = new Transaction()
            {
                FromWallet = wallet!,
                ToWallet = systemWallet!,
                TransactionType = TransactionType.WalletExpire,
                Note = "Wallet Expired",
                Amount = wallet!.Amount,
                Token = WalletToken.Point,

                FromAmountAfterTransaction = wallet.Amount,
                ToAmountAfterTransaction = systemWallet.Amount + wallet.Amount
            };

            wallet!.WalletStatus = WalletStatus.Unavailable;
            wallet!.TransactionsTo.Add(trx);

            systemWallet!.Amount += wallet.Amount;

            _unitOfWork.WalletRepository.Update(wallet);

            return await _unitOfWork.SaveAsync();
        }


        public async Task<Wallet> GetSystemWallet(WalletToken token)
        {
            return await _unitOfWork.WalletRepository.GetQuery().Where(w => w.IsSystem && w.WalletToken == token).FirstAsync();
        }

        private async Task<Wallet?> GetByID(Guid id)
        {
            var wallet = await _unitOfWork.WalletRepository.GetQuery()
                .Where(x => x.WalletId == id)
                    .FirstOrDefaultAsync();

            return wallet;
        }

        private async Task<bool> Insert(Wallet w)
        {
            _unitOfWork.WalletRepository.Add(w);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<bool> Update(Wallet w)
        {
            _unitOfWork.WalletRepository.Update(w);
            return await _unitOfWork.SaveAsync();
        }

        private async Task<bool> SaveAsync()
        {
            return await _unitOfWork.SaveAsync();
        }
    }
}