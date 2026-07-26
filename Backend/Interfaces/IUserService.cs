using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.Threading.Tasks;

namespace Server.Interfaces
{
    public interface IUserService
    {
        public Task<UserManagerResponse> RegisterUserAsync(RegisterModel model);
        public Task<UserManagerResponse> LoginUserAsync(LoginModel model);
        public Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token);
        public Task<UserManagerResponse> ForgotPasswordAsync(string email);
        public Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model);
    }
}
