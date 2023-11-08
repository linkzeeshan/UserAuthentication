using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using Org.BouncyCastle.Bcpg.Sig;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagementApp.Model;
using UserManagementApp.Model.Authentication.Login;
using UserManagementApp.Model.Authentication.SignUp;
using UserManagementApp.Services.Models;
using UserManagementApp.Services.Services;

namespace UserManagementApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        

        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserEmailService _emailService;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(ILogger<AuthenticationController> logger,UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration,
            IUserEmailService emailService, SignInManager<IdentityUser> signinManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _signinManager = signinManager;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegistration([FromBody] RegisterUser registerUser, string role)
        {
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = "User Already Exist" }
                    );
            }
            //Add user in database
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.UserName,
                TwoFactorEnabled = true, //I have Enabled two factor authentication It will verify user on login leve
            };

           
            if (await _roleManager.RoleExistsAsync(role)) {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = $"User Fail Created: {result.Errors.FirstOrDefault().Description}" }
                       );
                }
                
                // Add Role
                await _userManager.AddToRoleAsync(user, role);

                //Add token to verify email
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, user.Email);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email });
                var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink!);

                await _emailService.SendEmailAsyc(message);

                return StatusCode(StatusCodes.Status201Created,
                       new Response { Status = "Success", Message = $"User created & Email sent to {user.Email} successfully" }
                       );
               
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response { Status = "Error", Message = "User Fail Created" }
                  );
            }
            // Add Role

        }
        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            var message = new Message(new string[] { "linkzeeshan.ayyub@gmail.com" }, "Test", "<h1>I am testing email</h1>");
            await _emailService.SendEmailAsyc(message);
            return StatusCode(StatusCodes.Status201Created,
                       new Response { Status = "Success", Message = "Email has been sent successfully" }
                       );

        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
                var user = await _userManager.FindByEmailAsync(email);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                       new Response { Status = "Success", Message = "Email is confirmed successfully" }
                       );
                }
                
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                 new Response { Status = "Error", Message = "User Does" }
                 );
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> login(LoginModel login)
        {
            //chekcing user
            var user = await _userManager.FindByNameAsync(login.UserName);
            //checking password
            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                //checking claim
                var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                //checking and add role in the list
                var userRoles = await _userManager.GetRolesAsync(user);
                //generate token with claim
                  foreach(var role in userRoles)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role , role) );
                }
                //Implementing  two factor authentication in case of Enabled two factor
                if(user.TwoFactorEnabled)
                {
                    //sending OTP to user email for varification
                    //Add token to verify email
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    //var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email });
                    var message = new Message(new string[] { user.Email! }, "OTP Confirmation", token);
                    return StatusCode(StatusCodes.Status200OK,
                       new Response { Status = "Success", Message = $"We have sent to OTP on your Email {user.Email} successfully" }
                       );
                }

                //return token as expected

                  var jwtToken = GetToken(authClaim);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo
                });
            }

            return Unauthorized();
        }
        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> loginWithOTP(string code, string username)
        { 
            var user = await _userManager.FindByNameAsync(username);
            var signin = await _signinManager.TwoFactorSignInAsync("Email", code, false, false);
            if (signin.Succeeded)
            {
                if(user != null)
                {
                    //checking claim
                    var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                    //checking and add role in the list
                    var userRoles = await _userManager.GetRolesAsync(user);
                    //generate token with claim
                    foreach (var role in userRoles)
                    {
                        authClaim.Add(new Claim(ClaimTypes.Role, role));
                    }
                    //return token as expected

                    var jwtToken = GetToken(authClaim);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        expiration = jwtToken.ValidTo
                    });
                }

            }
            return StatusCode(StatusCodes.Status404NotFound,
                       new Response { Status = "Success", Message = $"Invalid Code" }
                       );
        }
            private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Issuer"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}