using Application.Domain.Models;
using Application.DTOs.User;
using Application.Helpers;
using Application.QueryParams;

namespace Application.Services
{
    public interface IUserService
    {
        Task<bool> AddFCMToken(FCMTokenAddDTO dto, Guid userId);
        Task<UserDTO> CreateNewUser(UserCreateDTO dto);
        Task<PagedList<User>> GetAll(UserQueryParams queryParams);
        Task<User?> GetSelf(Guid userId);
        Task<UserLoginResultDTO> Login(LoginDTO dto);
        Task<UserLoginResultDTO> LoginWithGoogle(LoginGoogleDTO dto);

        Task<bool> UpdateUserPinCode(UserUpdatePinCodeDTO dto, string requesterEmail);
        Task<bool> CheckUserPinCode(string? pincode, string requesterEmail);
        Task<string?> GetUserPinCode(string requesterEmail);

        Task<bool> ChangePassword(UserChangePasswordDTO dto, string requesterEmail);
        Task<bool> RequestChangePassword(UserRequestResetPasswordDTO dto);
        Task<bool> ResetPassword(UserResetPasswordDTO dto);

        Task<bool> UpdateUser(UserUpdateDTO dto);
        Task<bool> UpdateUsers(List<UserUpdateDTO> dto);
    }
}