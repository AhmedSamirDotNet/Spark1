using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Spark.WebApi.Controllers.Admins
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [EnableCors("AllowFrontend")] // Add this line


    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<AdminUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;


        public AdminController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AdminController> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<AdminUser>();
            _logger = logger;
        }

        #region CreateAdmin Api
        [HttpPost("Create")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] AdminUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if username already exists
            var existingUser = await _unitOfWork.AdminUsers
                .GetAsync(u => u.UserName == model.UserName);

            if (existingUser != null)
                return BadRequest(new { message = "Username already exists." });

            // Store the plain password temporarily
            var plainPassword = model.PasswordHash;

            // Hash password using the same method as login verification
            model.PasswordHash = _passwordHasher.HashPassword(model, plainPassword);

            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;

            await _unitOfWork.AdminUsers.AddAsync(model);
            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                message = "Admin created successfully",
                admin = new
                {
                    model.Id,
                    model.UserName,
                    Role = model.Role.ToString(),
                    model.CreatedAt,
                    model.IsActive
                }
            });
        }
        #endregion

        #region Get all admins Api
        [HttpGet("GetAdmins")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetAdmins(CancellationToken cancellationToken)
        {
            try
            {
                var admins = await _unitOfWork.AdminUsers.GetAllAsync(
                    filter: a => a.IsActive,
                    cancellationToken: cancellationToken
                );

                var result = admins.Select(a => new
                {
                    a.Id,
                    a.UserName,
                    a.Role,
                    a.CreatedAt,
                    a.IsActive
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving admin users",
                    details = ex.Message
                });
            }
        }
        #endregion

        #region Delete Api
        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = $"{nameof(UserRole.SuperAdmin)},{nameof(UserRole.Admin)}")] // مين اللي يقدر يمسح
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAdmin(int id, CancellationToken cancellationToken)
        {
            try
            {
                var admin = await _unitOfWork.AdminUsers.GetAsync(
                    filter: a => a.Id == id,
                    cancellationToken: cancellationToken);

                if (admin == null)
                    return NotFound(new { Message = $"Admin with Id {id} not found" });

                await _unitOfWork.AdminUsers.RemoveAsync(admin, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                return Ok(new { Message = "Admin deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin with Id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while deleting admin"
                });
            }
        }
        #endregion

        #region UpdateAdmin Api
        [HttpPatch("Update/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateAdminRequest request)
        {
            var admin = await _unitOfWork.AdminUsers.GetAsync(a => a.Id == id);
            if (admin == null)
                return NotFound(new { message = "Admin not found." });

            if (!string.IsNullOrEmpty(request.UserName))
                admin.UserName = request.UserName;

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                var passwordHasher = new PasswordHasher<AdminUser>();
                admin.PasswordHash = passwordHasher.HashPassword(admin, request.NewPassword);
            }

            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                message = "Admin updated successfully",
                admin = new
                {
                    admin.Id,
                    admin.UserName,
                    Role = admin.Role.ToString(),
                    admin.CreatedAt,
                    admin.IsActive
                }
            });
        }

        // DTO for update request
        public class UpdateAdminRequest
        {
            public string? UserName { get; set; }
            public string? NewPassword { get; set; }
        }
        #endregion


        #region Login
        [EnableCors("AllowLocalhost")]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] AdminUser loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.UserName) || string.IsNullOrEmpty(loginModel.PasswordHash))
                return BadRequest(new { message = "Username and password are required." });

            // جيب اليوزر من الداتابيز
            var admin = await _unitOfWork.AdminUsers
                .GetAsync(u => u.UserName == loginModel.UserName);

            if (admin == null)
                return Unauthorized(new { message = "User not found." });

            if (!admin.IsActive)
                return Unauthorized(new { message = "Account is deactivated." });

            // تحقق من الباسورد
            var result = _passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, loginModel.PasswordHash);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid credentials." });

            // ===== Generate Access Token =====
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, admin.UserName),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, admin.Role.ToString())
        }),
                Expires = DateTime.UtcNow.AddMinutes(30), // access token short lifetime
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // ===== Generate Refresh Token =====
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                AdminUserId = admin.Id
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                message = "Login successful.",
                accessToken,
                refreshToken = refreshToken.Token,
                user = new
                {
                    admin.Id,
                    admin.UserName,
                    Role = admin.Role.ToString(),
                    admin.IsActive,
                    admin.CreatedAt
                }
            });
        }
        #endregion

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }
        }
        public class LogoutRequest
        {
            public string RefreshToken { get; set; }
        }

        #region Logout Api
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Refresh token is required." });

            var token = await _unitOfWork.RefreshTokens.GetValidRefreshTokenAsync(request.RefreshToken);

            if (token == null)
                return NotFound(new { message = "Invalid or expired refresh token." });

            await _unitOfWork.RefreshTokens.RevokeRefreshTokenAsync(request.RefreshToken);

            return Ok(new
            {
                message = "Logout successful. Refresh token revoked.",
                revokedToken = request.RefreshToken
            });
        }

        #endregion

        #region RefreshToken Api  الـ Refresh API وظيفتها إنها تستقبل refreshToken من الفرونت، تتحقق إنه صالح ومرتبط باليوزر، ولو تمام ترجعله accessToken جديد بدل اللي انتهت صلاحيته
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var token = await _unitOfWork.RefreshTokens.GetValidRefreshTokenAsync(request.RefreshToken);

            if (token == null)
                return Unauthorized(new { message = "Invalid refresh token." });

            var admin = token.AdminUser;

            // Generate new Access Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, admin.UserName),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, admin.Role.ToString())
        }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var newAccessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = token.Token
            });
        }
        #endregion

    }
}