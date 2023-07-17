using Application.DTOs;
using Application.DTOs.Wallet;
using Application.Helpers;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    [Authorize]
    public class WalletsController : BaseApiController
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [SwaggerOperation("Test Receive Point")]
        [HttpGet("testAward")]
        public async Task<ResponseDTO<bool>> TestAward()
        {
            var email = User.GetEmail();

            var result = await _walletService.TestAward(email);
            return result.FormatAsResponseDTO(200);
        }

        [Authorize(Roles = "ADMIN")]
        [SwaggerOperation("[ADMIN] Get system wallet's info")]
        [HttpGet("wallet")]
        public async Task<ActionResult<ResponseDTO<WalletsInfoDTO>>> Get()
        {
            var userId = User.GetId();

            var walletInfo = await _walletService.GetSystemWalletInfo();
            return walletInfo.FormatAsResponseDTO(200);
        }

        // [SwaggerOperation("Send token to others")]
        // [SwaggerResponse(200)]
        // [SwaggerResponse(400)]
        // [HttpPost("")]
        // public async Task<ResponseDTO<bool>> SendTokenToOther(WalletSendTokenDTO dto)
        // {
        //     var userId = User.GetId();

        //     var result = await _walletService.SendMoney(userId, dto.UserId, dto.Amount, dto.Token, Models.Enums.Wallet.TransactionType.ProjectReward);
        //     return result.FormatAsResponseDTO<bool>(200);
        // }
    }
}
