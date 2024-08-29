using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Timesheet.Data.Models;
using Timesheet.Models;
using System.Net;
using Timesheet.Extension;


namespace Timesheet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, EmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;

        }

        

        [HttpPost("Login")]
        public async Task<IActionResult> LogIn(DtoLogin login)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByNameAsync(login.userName);
                if (user != null)
                {
                    
                    if (!user.IsActive)
                    {
                        return Unauthorized("Your account is deactivated. Please contact the administration.");
                    }

                    if (await _userManager.CheckPasswordAsync(user, login.password))
                    {
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                        var roles = await _userManager.GetRolesAsync(user);
                        var role = roles.FirstOrDefault();

                        foreach (var r in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, r));
                        }

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddHours(1),
                            signingCredentials: signingCredentials
                        );

                        var jwtToken = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            role = role
                        };

                        return Ok(jwtToken);
                    }
                    else
                    {
                        return Unauthorized("Invalid password.");
                    }
                }
                else
                {
                    return Unauthorized("Invalid username.");
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = $"{_configuration["AppUrl"]}/resetpassword?token={Uri.EscapeDataString(token)}&email={model.Email}";

            await _emailSender.SendEmailAsync(
                model.Email,
                "Réinitialisation du mot de passe",
                "ResetPassword",
                callbackUrl: callbackUrl);

            return Ok("Reset password email sent.");
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] DtoResetPassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Les mots de passe ne correspondent pas.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Utilisateur non trouvé.");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok("Le mot de passe a été réinitialisé avec succès.");
        }



    }
}
