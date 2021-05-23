using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server.Interfaces;
using Server.Models;
using Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class AuthService : IUserService
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly IMailService mailService;
        private readonly IMapper mapper;

        public AuthService(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IMailService mailService,
            IConfiguration configuration,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.mailService = mailService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Email confirmation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                };

            var result = await userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));

            if (!result.Succeeded)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Email did not confirm",
                    Errors = result.Errors.Select(e => e.Description)
                };

            return new UserManagerResponse
            {
                Message = "Email confirmed successfully!",
                IsSuccess = true,
            };
        }

        /// <summary>
        /// Sending a password recovery email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<UserManagerResponse> ForgotPasswordAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await userManager.GeneratePasswordResetTokenAsync(user)));

            string url = $"{configuration["AppUrl"]}/ResetPassword?user={user.Id}&token={token}";

            await mailService.SendEmailAsync(email, "Reset Password",
                 "<h1>Follow the instructions to reset your password</h1>" +
                $"<p>To reset your password <a href='{url}'>Click here</a></p>");

            return new UserManagerResponse
            {
                IsSuccess = true,
                Message = "Reset password URL has been sent to the email successfully!"
            };
        }

        /// <summary>
        /// Verifying creditals and returning an access token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<UserManagerResponse> LoginUserAsync(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            var result = false;

            if (user != null)
                result = await userManager.CheckPasswordAsync(user, model.Password);

            if (user == null || result == false)
                return new UserManagerResponse
                {
                    Message = "Invalid email or password",
                    IsSuccess = false,
                };

            var userRoles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, userRole));

            var jwt = new AuthOptions(configuration);

            var key = jwt.GetSymmetricSecurityKey();

            var token = new JwtSecurityToken(
                issuer: jwt.ISSUER,
                audience: jwt.AUDIENCE,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var userViewModel = mapper.Map<UserViewModel>(user);

            return new UserManagerResponse()
            {
                Message = access_token,
                IsSuccess = true,
                ExpireDate = token.ValidTo,
                Model = userViewModel
            };
        }

        /// <summary>
        /// Registration of a new user in the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<UserManagerResponse> RegisterUserAsync(RegisterModel model)
        {
            var user = new User
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.Phone
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return new UserManagerResponse
                {
                    Message = "User did not create",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };

            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            if (!await roleManager.RoleExistsAsync(UserRoles.Administration))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Administration));
            if (!await roleManager.RoleExistsAsync(UserRoles.SysAdmin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.SysAdmin));

            if (await roleManager.RoleExistsAsync(UserRoles.SysAdmin))
            {
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }

            var confirmEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await userManager.GenerateEmailConfirmationTokenAsync(user)));

            string url = $"{configuration["AppUrl"]}/api/auth/confirmemail?userid={user.Id}&token={confirmEmailToken}";

            await mailService.SendEmailAsync(user.Email, "Confirm your email",
                $"<h1>Welcome to Auth Demo</h1><p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");

            return new UserManagerResponse
            {
                Message = "User created successfully!",
                IsSuccess = true,
            };
        }

        /// <summary>
        /// Resetting and setting a new user password
        /// </summary>
        /// <param name="model"></param>
        /// <returns>JSON</returns>
        public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    Message = "Password has been reset successfully!",
                    IsSuccess = true,
                };

            return new UserManagerResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }
    }
}
