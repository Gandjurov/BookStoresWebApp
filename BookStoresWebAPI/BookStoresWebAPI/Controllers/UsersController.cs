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
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStoresWebAPI.Controllers
{
    //[Authorize]
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

            UserWithToken userWithToken = new UserWithToken(user);

            if (userWithToken == null)
                return NotFound();

            // sign your token here...
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.EmailAddress)
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials( new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            userWithToken.Token = tokenHandler.WriteToken(token);

            return userWithToken;
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

        private bool UserExists(int id)
        {
            return dbContext.Users.Any(e => e.UserId == id);
        }
    }
}
