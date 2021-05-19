using BookStoresWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BookStoresWebAPI.Controllers
{
    [Authorize]
    public class UsersController : ApiController
    {
        private readonly BookStoresDBContext dbContext;
        private readonly JWTSettings jwtSettings;

        public UsersController(BookStoresDBContext context, IOptions<JWTSettings> jwtSettings)
        {
            dbContext = context;
            this.jwtSettings = jwtSettings.Value;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await dbContext.Users.ToListAsync();
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await dbContext.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpGet("Login")]
        public async Task<ActionResult<UserWithToken>> Login([FromBody] User user)
        {
            user = await dbContext.Users.Where(u => u.EmailAddress == user.EmailAddress
                                                && u.Password == user.Password).FirstOrDefaultAsync();

            UserWithToken userWithToken = null;

            if (user == null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await dbContext.SaveChangesAsync();

                userWithToken = new UserWithToken(user);
                userWithToken.RefreshToken = refreshToken.Token;
            }

            if (userWithToken == null)
                return NotFound();

            // sign your token here...
            userWithToken.AccessToken = GenerateAccessToken(user.UserId);

            return userWithToken;
        }

        [HttpGet("RefreshToken")]
        public async Task<ActionResult<UserWithToken>> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            User user = GetUserFromAccessToken(refreshRequest.AccessToken);

            if (user != null && ValidateRefreshToken(user, refreshRequest.RefreshToken))
            {
                UserWithToken userWithToken = new UserWithToken(user);
                userWithToken.AccessToken = GenerateAccessToken(user.UserId);

                return userWithToken;
            }
            return null;
        }

        [HttpGet("GetUser")]
        public async Task<ActionResult<User>> GetUser()
        {
            string emailAddress = HttpContext.User.Identity.Name;

            var user = await dbContext.Users
                                      .Where(user => user.EmailAddress == emailAddress)
                                      .FirstOrDefaultAsync();
            user.Password = null;

            if (user == null)
                return NotFound();

            return user;
        }

        // POST api/<UsersController>
        public async Task<ActionResult<User>> PostUser(User user)
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.PubId }, user);
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User user)
        {
            if (id != user.PubId)
                return BadRequest();

            dbContext.Entry(user).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // -------------- Private Methods -------------- 
        private bool ValidateRefreshToken(User user, string refreshToken)
        {
            RefreshToken refreshTokenUser = dbContext.RefreshTokens
                                                     .Where(rt => rt.Token == refreshToken)
                                                     .OrderByDescending(rt => rt.ExpiryDate)
                                                     .FirstOrDefault();

            if (refreshTokenUser != null && 
                refreshTokenUser.UserId == user.UserId &&
                refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }                            

            return false;
        }
        private User GetUserFromAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var userId = principle.FindFirst(ClaimTypes.Name)?.Value;

                return dbContext.Users.Where(usr => usr.UserId == Convert.ToInt32(User)).FirstOrDefault();
            }

            return null;
        }
        private string GenerateAccessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, Convert.ToString(userId))
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }

            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(6);

            return refreshToken;
        }
        private bool UserExists(int id)
        {
            return dbContext.Users.Any(e => e.UserId == id);
        }
    }
}
