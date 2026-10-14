using OnlineBakingSystem.Database;
using OnlineBakingSystem.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using OnlineBankingSystem.RequestsResponses;

namespace OnlineBakingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _loginManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<JWTOptions> _jwtOptions;

        public UsersController(UserManager<User> userManager, SignInManager<User> loginManager, RoleManager<IdentityRole> roleManager, IOptions<JWTOptions> jwtOptions)
        {
            _userManager = userManager;
            _loginManager = loginManager;
            _roleManager = roleManager;
            _jwtOptions = jwtOptions;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest user)
        {
            if (user is null)
            {
                return BadRequest("Invalid user data");
            }

            var newUser = new User
            {
                UserName = user.UserName,
                Email = user.Email,
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
            var roleToAssign = string.IsNullOrEmpty(user.Role) ? "user" : user.Role;
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { success = false, error = $"Role not found: {roleToAssign}" });
            }
            var roleAssignmentResult = await _userManager.AddToRoleAsync(newUser, roleToAssign);
            if (!roleAssignmentResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, errors = roleAssignmentResult.Errors.Select(e => e.Description) });
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser(DeleteUserRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName))
            {
                return BadRequest("Username is required");
            }

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user is null)
            {
                return NotFound($"User not found: {request.UserName}");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = $"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                });
            }

            return Ok(new DeleteUserResponse
            {
                Success = true,
                Message = "User deleted successfully",
                UserName = request.UserName
            });
        }

        [HttpPatch("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdateUserPasswordRequest userRequest) 
        { 
            var callerIdentity = User.Identity;
            if(callerIdentity is null || !callerIdentity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            if(callerIdentity.Name != userRequest.UserName && !User.IsInRole("Admin"))
            {
                return Forbid("You are not authorized to change this user's password");
            }

            var currentUser = await _userManager.FindByNameAsync(userRequest.UserName);
            if(currentUser is null)
            {
                return NotFound($"User not found: {userRequest.UserName}");
            }

            var result = await _userManager.ChangePasswordAsync(currentUser, userRequest.Password, userRequest.NewPassword);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
            return Ok(new UpdateUserPasswordResponse
            {
                Success = true,
                Message = "Password updated successfully",
                UserName = userRequest.UserName
            });
        }

        [Authorize(Roles = "Admin, Clerk")]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Username is required");
            }
             ;
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return NotFound($"User not found: {userName}");
            }
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { user.UserName, user.Email, Roles = roles });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new { user.UserName, user.Email, Roles = roles });
            }
            return Ok(userList);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest loginRequest)
        {
            if (loginRequest is null)
            {
                return BadRequest("Invalid user data");
            }

            var existingUser = await _userManager.FindByNameAsync(loginRequest.UserName);
            if(existingUser is null)
            {
                return NotFound($"User not found: {loginRequest.UserName}");
            }

            var loginResult = await _loginManager.PasswordSignInAsync(loginRequest.UserName, loginRequest.Password, false, false);
            if (loginResult.IsLockedOut)
            {
                return Forbid("User account is locked out due to multiple failed login attempts");
            }
            if (!loginResult.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            var roles = await _userManager.GetRolesAsync(existingUser);
            var signingKey = new SymmetricSecurityKey(UTF8Encoding.UTF8.GetBytes(_jwtOptions.Value.SecretKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, existingUser.Id),
                new(JwtRegisteredClaimNames.Email, existingUser.NormalizedEmail!),
                ..roles.Select(role => new Claim(ClaimTypes.Role, role))
            ];

            var tokenDesccriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpirationMinutes),
                SigningCredentials = signingCredentials,
                Issuer = _jwtOptions.Value.Issuer,
                Audience = _jwtOptions.Value.Audience
            };

            var tokenHandler = new JsonWebTokenHandler();
            string accessToken = tokenHandler.CreateToken(tokenDesccriptor);

            return Ok(new LoginUserResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                UserName = existingUser.UserName
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutUserRequest logoutRequest)
        {
            var logoutUser = await _userManager.FindByNameAsync(logoutRequest.UserName);
            if (logoutUser is null)
            {
                return NotFound($"User not found: {logoutRequest.UserName}");
            }
            if (User.Identity?.Name != logoutUser.UserName && !User.IsInRole("Admin"))
            {
                return Forbid("You are not authorized to log out this user");
            }
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized("User is not authenticated");
            }
            await _userManager.UpdateSecurityStampAsync(logoutUser);
            await _loginManager.SignOutAsync();

            return Ok(new LogoutUserResponse
            {
                Success = true,
                Message = "Logout successful",
                UserName = logoutRequest.UserName
            });
        }

    }
}
