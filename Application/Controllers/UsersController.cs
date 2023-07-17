using Application.DTOs;
using Application.DTOs.User;
using Application.Helpers;
using Application.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [SwaggerOperation("Login With Email and Password")]
        public async Task<ResponseDTO<UserLoginResultDTO>> Login([FromBody] LoginDTO dto)
        {
            var loginResult = await _userService.Login(dto);
            return loginResult.FormatAsResponseDTO(200);
        }

        [HttpPost("login-google")]
        [SwaggerOperation("Login With Firebase Token")]
        public async Task<ResponseDTO<UserLoginResultDTO>> LoginWithGoogle([FromBody] LoginGoogleDTO dto)
        {
            var loginResult = await _userService.LoginWithGoogle(dto);
            return loginResult.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("fcm-token")]
        [SwaggerOperation("Add FCM Token")]
        public async Task<ResponseDTO<bool>> AddFCMToken([FromBody] FCMTokenAddDTO dto)
        {
            var result = await _userService.AddFCMToken(dto, User.GetId());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpGet("me/pin")]
        [SwaggerOperation("Check if your account have active pin code")]
        public async Task<ResponseDTO<bool>> GetCurrentActivePIN()
        {
            var result = await _userService.GetUserPinCode(User.GetEmail()) != null;
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPost("me/pin-code")]
        [SwaggerOperation("Check your pin")]
        public async Task<ResponseDTO<bool>> CheckPinN([FromBody] UserCheckPinCodeDTO dto)
        {
            var result = await _userService.CheckUserPinCode(dto.PinCode, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("me/pin-code")]
        [SwaggerOperation("Add/Update Your PIN (If didn't have pin don't need to provide old pin)")]
        public async Task<ResponseDTO<bool>> AddOrUpdateYourPIN([FromBody] UserUpdatePinCodeDTO dto)
        {
            var result = await _userService.UpdateUserPinCode(dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [Authorize]
        [HttpPut("me/password")]
        [SwaggerOperation("Change your password")]
        public async Task<ResponseDTO<bool>> ChangePassword([FromBody] UserChangePasswordDTO dto)
        {
            var result = await _userService.ChangePassword(dto, User.GetEmail());
            return result.FormatAsResponseDTO(200);
        }

        [HttpPost("password/request-reset")]
        [SwaggerOperation("Request to reset your password")]
        public async Task<ResponseDTO<bool>> RequestResetPassword([FromBody] UserRequestResetPasswordDTO dto)
        {
            var result = await _userService.RequestChangePassword(dto);
            return result.FormatAsResponseDTO(200);
        }

        [HttpPost("password/reset")]
        [SwaggerOperation("Reset your password")]
        public async Task<ResponseDTO<bool>> ResetPassword([FromBody] UserResetPasswordDTO dto)
        {
            var result = await _userService.ResetPassword(dto);
            return result.FormatAsResponseDTO(200);
        }

        [HttpPost]
        [SwaggerOperation("[ADMIN] [ALL (Temp)] Create new user <As ADMIN (Temp)>")]
        public async Task<ResponseDTO<UserDTO>> Create([FromBody] UserCreateDTO dto)
        {
            var newUser = await _userService.CreateNewUser(dto);
            return newUser.FormatAsResponseDTO(200);
        }

        //[HttpPatch]
        //[SwaggerOperation("[ADMIN] Update user")]
        //public async Task<ResponseDTO<bool>> Update([FromBody] UserUpdateDTO dto)
        //{
        //  var result = await _userService.UpdateUser(dto);
        //  return result.FormatAsResponseDTO(200);
        //}

        //[HttpPatch("bulk")]
        //[SwaggerOperation("[ADMIN] Update user in bulk")]
        //public async Task<ResponseDTO<bool>> UpdateBulk([FromBody] List<UserUpdateDTO> dto)
        //{
        //  var result = await _userService.UpdateUsers(dto);
        //  return result.FormatAsResponseDTO(200);
        //}

        // [Authorize(Roles = "ADMIN")]
        // [HttpGet]
        // [SwaggerOperation("[ADMIN] Get all user")]
        // public async Task<ResponseDTO<List<UserDetailedDTO>>> GetAll([FromQuery] UserQueryParams queryParams)
        // {
        //   var userId = User.GetId();
        //   var users = await _userService.GetAll(queryParams);

        //   Pagination.AddPaginationHeader(HttpContext.Response, users);
        //   var mappedResult = _mapper.Map<List<UserDetailedDTO>>(users);
        //   return mappedResult.FormatAsResponseDTO<List<UserDetailedDTO>>(200);
        // }
    }
}
