using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password to get JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid request",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Account locked out. Please try again later."
                    });
                }

                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate JWT token
            var token = await _jwtTokenService.GenerateJwtToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour in seconds
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = roles.ToList(),
                    EmployeeId = user.EmployeeId
                }
            };

            _logger.LogInformation("User {Email} logged in successfully via API", request.Email);

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API login");
            return StatusCode(500, new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<UserInfo>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserInfo>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Roles = roles.ToList(),
                EmployeeId = user.EmployeeId
            };

            return Ok(new ApiResponse<UserInfo>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = userInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new ApiResponse<UserInfo>
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Generate new tokens
            var newToken = await _jwtTokenService.GenerateJwtToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = roles.ToList(),
                    EmployeeId = user.EmployeeId
                }
            };

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Unauthorized(new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Invalid token"
            });
        }
    }
}
