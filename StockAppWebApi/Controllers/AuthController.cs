using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StockAppWebApi.Data;
using StockAppWebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace StockAppWebApi.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ApplicationContext context;

        public AuthController(ApplicationContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public IResult Register(string name, string email, string password)
        {
            if (context.Users.Any(user => user.Email == email)) return Results.Problem("User already exists");

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User(Guid.NewGuid().ToString(), name, email, passwordHash, passwordSalt);

            context.Users.Add(user);

            context.SaveChanges();

            return Results.Ok("User added successfully");
        }

        [HttpPost]
        public IResult Login(string email, string password)
        {
            var user = context.Users.FirstOrDefault(user => user.Email == email);

            if (user is null) return Results.Problem("User not found");
            
            if (!VerifyPasswordHash(password, user.Password, user.PasswordSalt)) return Results.Problem("Wrong credentials");

            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var token = CreateToken(user);

            return Results.Json(new
            {
                user = user,
                token = token
            });
        }

        [Authorize]
        [HttpGet(Name = "Users")]
        public IResult Users() => Results.Json(context.Users.ToList());

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var hashed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return hashed.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("StockAppSecurity"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
