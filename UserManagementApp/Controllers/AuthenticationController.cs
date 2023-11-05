using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagementApp.Model;
using UserManagementApp.Model.Authentication.SignUp;

namespace UserManagementApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        

        private readonly ILogger<AuthenticationController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(ILogger<AuthenticationController> logger,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegistration([FromBody] RegisterUser registerUser, string role)
        {
            var userExist = _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist == null)
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
            };

           
            if (await _roleManager.RoleExistsAsync(role)) {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "User Fail Created" }
                       );
                }
                
                // Add Role
                await _userManager.AddToRoleAsync(user, role);

                return StatusCode(StatusCodes.Status201Created,
                       new Response { Status = "Success", Message = "User created successfully" }
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
    }
}