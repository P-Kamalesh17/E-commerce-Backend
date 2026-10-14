using DAL.Data;
using DAL.DTO.Authentication;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        ////Refresh Token
        //private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationController(UserManager<User> userManager,
                RoleManager<IdentityRole> roleManager,
                AppDbContext context,
                IConfiguration configuration
                //TokenValidationParameters tokenValidationParameters
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            //_tokenValidationParameters = tokenValidationParameters;
        }
        private static List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password is required.");
                return errors;
            }

            if (password.Length < 8)
                errors.Add("Password must be at least 8 characters long.");

            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter.");

            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least one number.");

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                errors.Add("Password must contain at least one special character.");

            return errors;
        }

        [HttpPost("register-user")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }
            var passwordErrors = ValidatePassword(payload.Password);

            if (passwordErrors.Any())
            {
                return BadRequest(new
                {
                    Message = "Password validation failed.",
                    Errors = passwordErrors
                });
            }

            var userExists = await _userManager.FindByEmailAsync(payload.Email);

            if (userExists != null)
            {
                return BadRequest($"User {payload.Email} already exists");
            }

            var newUser = new User
            {
                FullName = payload.FullName,
                Email = payload.Email,
                UserName = payload.Email,
                PhoneNumber = payload.PhoneNumber,
                Address = payload.Address,
                CreatedAt = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, payload.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "User creation failed.",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            // Assign the user to the "User" role
            await _userManager.AddToRoleAsync(newUser, UserRoles.User);

            return Created(nameof(Register), $"User {payload.Email} created successfully");
        }

        [HttpPost("login-user")]
        [AllowAnonymous]

        public async Task<IActionResult> Login([FromBody] LoginVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }
            var user = await _userManager.FindByEmailAsync(payload.Email);

                if(user != null && await _userManager.CheckPasswordAsync(user, payload.Password))
            {
                var token = await GenerateJwtTokenAsync(user,"");
                return Ok(token);
            }
            else
            {
                return Unauthorized("Invalid credentials");
            }
        }

        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken([FromBody]TokenRequestVM payload)
        //{
        //    try
        //    {
        //        var result = await VerityAndGenerateTokenAsync(payload);

        //        if(result == null)
        //        {
        //            return BadRequest("Invalid tokens");
        //        }
        //        return Ok(result);
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //private async Task<AuthResultVM> VerityAndGenerateTokenAsync(TokenRequestVM payload)
        //{
        //    var jwtTokenHandler = new JwtSecurityTokenHandler();

        //    //Check JWT token format
        //    var tokenInVerification = jwtTokenHandler.ValidateToken(payload.Token, _tokenValidationParameters, out var validatedToken);

        //    //Check encryption algorithm
        //    if(validatedToken is JwtSecurityToken jwtSecurityToken)
        //    {
        //        var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
        //            StringComparison.InvariantCultureIgnoreCase);

        //        if (result == false)
        //        {
        //            return null;
        //        }
        //    }

        //    // Validate expiration date
        //    var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

        //    var expiryDate = UnixTimeStampToDateTimeInUTC(utcExpiryDate);

        //    if(expiryDate > DateTime.UtcNow)
        //    {
        //        throw new Exception("Token has not yet expired");
        //    }

        //    //Refresh token exists in db
        //    var dbRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync<RefreshToken>(x => x.Token == payload.RefreshToken);

        //    if(dbRefreshToken == null)
        //    {
        //        throw new Exception("Refresh token does not exist");
        //    }

        //    // Validate id

        //    var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        //    if(dbRefreshToken.JwtId != jti)
        //    {
        //        throw new Exception("Token does not match");
        //    }

        //    //Refresh token is expired

        //    if (dbRefreshToken.DateExpire <= DateTime.UtcNow)
        //    {
        //        throw new Exception("Refresh token has expired, please re-login");
        //    }
        //    // Refresh token is revoked
        //    if (dbRefreshToken.IsRevoked)
        //    {
        //        throw new Exception("Refresh token has been revoked, please re-login");
        //    }

        //    //Generate new token (with existing refresh token)

        //    var dbUser = await _userManager.FindByIdAsync(dbRefreshToken.UserId);

        //    var newToken = await GenerateJwtTokenAsync(dbUser,payload.RefreshToken);

        //    return newToken;

        //}

        //private DateTime UnixTimeStampToDateTimeInUTC(long utcTimeStamp)
        //{
        //    var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //    dateTimeVal = dateTimeVal.AddSeconds(utcTimeStamp);
        //    return dateTimeVal;
        //}

        private async Task<AuthResultVM> GenerateJwtTokenAsync(User user, string existingrefreshToken)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            // Add user roles as claims

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            //
            var authSigninKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddDays(7),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            //var refreshToken = new RefreshToken();

            //if (string.IsNullOrEmpty(existingrefreshToken))
            //{
            //    refreshToken = new RefreshToken()
            //    {
            //        JwtId = token.Id,
            //        IsRevoked = false,
            //        UserId = user.Id,
            //        DateAdded = DateTime.UtcNow,
            //        DateExpire = DateTime.UtcNow.AddMonths(6),
            //        Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
            //    };
            //    await _context.RefreshTokens.AddAsync(refreshToken);
            //    await _context.SaveChangesAsync();
            //}
            var response = new AuthResultVM()
            {
                Token = jwtToken,
                //RefreshToken = (string.IsNullOrEmpty(existingrefreshToken) ? refreshToken.Token : existingrefreshToken),
                RefreshToken = string.Empty,
                ExpiresAt = token.ValidTo
            };
            return response;
        }

    }
}
