using Azure.Core;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MuseumAPI.Data;
using MuseumAPI.Model;
using MuseumAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MuseumAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILoginService _LoginService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ILoginService LoginService, ICustomerService customerService,AppDbContext context,IConfiguration configuration)
        {
            _LoginService = LoginService;
          _customerService = customerService;
            _context = context;
            _configuration = configuration;
            //_logger = logger;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var user = _LoginService.Login(loginRequest);

                if (user == null)
                    return Unauthorized("Invalid username or password");

                return Ok(user);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Exception during login for user: {Username}", request.Username);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCustomerDto dto)
        {
            var result = await _customerService.RegisterCustomerAsync(dto);

            if (!result)
                return BadRequest("Email already exists");

            return Ok("Customer registered successfully");
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            // 1. Validate Input
            if (request == null || string.IsNullOrEmpty(request.IdToken))
                return BadRequest("IdToken is required");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                // 2. Validate with Google
                // Make sure 'request.IdToken' does NOT contain the word "Bearer"
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:WebClientId"] }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                // This is where your current error is caught
                return Unauthorized(new { message = "Invalid JWT Format", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Google Token Validation Failed", detail = ex.Message });
            }

            // 3. User Logic
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == payload.Email);

            if (customer == null)
            {
                customer = new Customer
                {
                    Custm_name = payload.Name,
                    Email = payload.Email,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // 4. Generate your own App JWT
            var jwtToken = GenerateJwtToken(customer);

            return Ok(new
            {
                token = jwtToken,
                customer = new { customer.CustmId, customer.Custm_name, customer.Email }
            });
        }

        // =========================
        // JWT TOKEN
        // =========================
        private string GenerateJwtToken(Customer user)
        {
            // 1. Get Secret Key from appsettings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Define Claims (Matched to your Customer Model)
            var claims = new[]
            {
        // Changed user.Id to user.CustmId
        new Claim(JwtRegisteredClaimNames.Sub, user.CustmId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        // Changed user.Name to user.Custm_name
        new Claim("name", user.Custm_name)
    };

            // 3. Create Token Descriptor
            // If "Jwt:DurationInMinutes" isn't in your JSON, it defaults to 60 minutes
            var duration = _configuration["Jwt:DurationInMinutes"] != null
                           ? Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])
                           : 60;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(duration),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            // 4. Serialize Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }
    }


}
