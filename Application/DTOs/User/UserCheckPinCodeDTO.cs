using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.User;

namespace Application.DTOs.User
{
    public class UserCheckPinCodeDTO
    {
        public string PinCode { get; set; } = null!;
    }
}