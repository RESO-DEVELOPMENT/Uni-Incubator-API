using Application.Domain;
using Application.Domain.Enums;
using Application.Domain.Enums.MemberVoucher;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.DTOs.MemberVoucher;
using Application.DTOs.User;
using Application.Helpers;
using Application.Persistence.Repositories;
using Application.StateMachines;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class MemberVoucherService : IMemberVoucherService
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IWalletService _walletService;
        private readonly IQueueService _redisQueueService;

        public MemberVoucherService(UnitOfWork unitOfWork,
        IWalletService walletService,
        IQueueService redisQueueService,
                           IMapper mapper,
        IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
            _redisQueueService = redisQueueService;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MemberVoucher> GetMemberVoucherFromCode(string memberVoucherCode)
        {
            var memberVoucher = await _unitOfWork.MemberVoucherRepository.GetVoucherFromCode(memberVoucherCode);
            if (memberVoucher == null)
            {
                throw new NotFoundException("Không có voucher nào của thành viên có mã đó!",
                    ErrorNameValues.MemberVoucherNotFound);
            }

            return memberVoucher;
        }


        public async Task<bool> UpdateMemberVoucherStatus(MemberVoucherUpdateStatusDTO dto, string requesterEmail, bool isAdmin = false)
        {
            var requesterMember = await _unitOfWork.MemberRepository.GetByEmail(requesterEmail) ?? throw new NotFoundException("Thành viên không tồn tại!", ErrorNameValues.MemberNotFound);

            if (dto is { MemberVoucherCode: not null, MemberVoucherId: not null })
                throw new BadRequestException("Bạn chỉ cần cung cấp ID hoặc Code!",
                    ErrorNameValues.TooMuchParams);

            MemberVoucher? memberVoucher = null;

            if (dto.MemberVoucherCode != null)
                memberVoucher = await _unitOfWork.MemberVoucherRepository.GetVoucherFromCode(dto.MemberVoucherCode);

            if (dto.MemberVoucherId != null)
                memberVoucher = await _unitOfWork.MemberVoucherRepository.GetByID(dto.MemberVoucherId.Value);

            if (memberVoucher == null)
            {
                throw new NotFoundException("Không có voucher nào của thành viên có mã đó!",
                    ErrorNameValues.MemberVoucherNotFound);
            }

            if (!isAdmin)
            {
                if (requesterMember.MemberId != memberVoucher.MemberId)
                {
                    throw new NotFoundException("Đây không phải voucher của bạn!",
                        ErrorNameValues.MemberVoucherNotYour);
                };
            }


            if (dto.Status != MemberVoucherStatus.Used)
                throw new BadRequestException("Bạn chỉ có thể cập nhật sang trậng thái đã sử dụng!", ErrorNameValues.InvalidStateChange);
            try
            {
                var stateMachine = new MemberVoucherStateMachine(memberVoucher);
                stateMachine.TriggerState(dto.Status);
            }
            catch
            {
                throw new BadRequestException("Cập nhật sai trạng thái!", ErrorNameValues.InvalidStateChange);
            }

            _unitOfWork.MemberVoucherRepository.Update(memberVoucher);
            var result = await _unitOfWork.SaveAsync();
            return result;
        }


        public async Task<bool> BuyVoucher(Guid voucherId, string requesterEmail, string? pinCode)
        {
            var member = await _unitOfWork.MemberRepository
            .GetQuery()
            .Include(m => m.User)
            .Include(m => m.MemberWallets.Where(mw =>
                  mw.Wallet.WalletToken == WalletToken.Point &&
                  mw.Wallet.WalletStatus != WalletStatus.Unavailable &&
                  mw.Wallet.ExpiredDate > DateTimeHelper.Now()
                ))
                .ThenInclude(mw => mw.Wallet)
            .Where(m => m.EmailAddress == requesterEmail)

                .FirstOrDefaultAsync() ?? throw new NotFoundException(); ;

            var voucher = await _unitOfWork.VoucherRepository.GetQuery().Where(v => v.VoucherId == voucherId).FirstOrDefaultAsync()
                ?? throw new NotFoundException("Voucher không tồn tại!", ErrorNameValues.VoucherNotFound);

            var memberPoint = member.MemberWallets.Select(mw => mw.Wallet).ToList().Total(WalletToken.Point);

            if (memberPoint < voucher.VoucherCost)
                throw new BadRequestException("Bạn không có đủ điểm!", ErrorNameValues.InsufficentToken);

            // Check Pincode
            await _userService.CheckUserPinCode(pinCode, member.EmailAddress);

            await _redisQueueService.AddToQueue(new QueueTask()
            {
                TaskName = TaskName.MemberBuyVoucher,
                TaskData = new Dictionary<string, string>
                {
                  {"MemberId", member.MemberId.ToString() },
                  {"VoucherId", voucher.VoucherId.ToString()},
                }
            });

            return await _unitOfWork.SaveAsync();
        }
    }
}