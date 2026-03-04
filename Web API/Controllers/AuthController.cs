using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Web_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public AuthController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody] Credential credential)
        {
            //Verify the credentials
            if (credential.Username == "admin" && credential.Password == "password")
            {
                //create security context
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                    new Claim("Department", "HR"),//Custom claim for authorization,
                    new Claim("Admin", "true"),
                    new Claim("Manager", "true"),
                    new Claim("EmploymentDate", "2025-01-01")
                };

                var expireAt = DateTime.UtcNow.AddMinutes(1);



                return Ok(new
                {
                    access_token = CreateToken(claims, expireAt),
                    expires_at = expireAt
                });
            }

            ModelState.AddModelError("Unauthorized", "Invalid username or password");

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
            };

            return Unauthorized(problemDetails);
        }

        private string CreateToken(List<Claim> claims, DateTime expireAt)
        {
            var claimsDic = new Dictionary<string, object>();
            if (claims is not null && claims.Count > 0)
            {
                foreach (var claim in claims)
                {
                    claimsDic.Add(claim.Type, claim.Value);
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["SecretKey"] ?? string.Empty)), 
                    SecurityAlgorithms.HmacSha256Signature),
                Expires = expireAt,
                Claims = claimsDic
            };

            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }

    public class Credential
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
