using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Server.Interfaces;
using Server.Models;
using Server.Services;
using System;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IConfiguration configuration;
        private readonly IMailService mailService;

        public AuthController(
            IUserService userService,
            IConfiguration configuration,
            IMailService mailService)
        {
            this.userService = userService;
            this.configuration = configuration;
            this.mailService = mailService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Some properties are not valid");

            var result = await userService.RegisterUserAsync(model);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result); ;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Some properties are not valid");

            var result = await userService.LoginUserAsync(model);

            if (result.IsSuccess)
            {
                await mailService.SendEmailAsync(model.Email, "New login", "<h1>Hey!, new login to your account noticed</h1><p>New login to your account at " + DateTime.Now + "</p>");
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("Forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (string.IsNullOrEmpty(model.Email))
                return NotFound();

            var result = await userService.ForgotPasswordAsync(model.Email);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("Reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Some properties are not valid");

            var result = await userService.ResetPasswordAsync(model);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new
                {
                    message = "Invalid params"
                });
            var result = await userService.ConfirmEmailAsync(userId, token);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Redirect($"{configuration["AppURL"]}/ConfirmEmail.html");
        }
    }
}
