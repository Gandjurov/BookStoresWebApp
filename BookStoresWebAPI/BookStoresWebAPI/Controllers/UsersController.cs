using BookStoresWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStoresWebAPI.Controllers
{
    [Authorize]
    public class UsersController : ApiController
    {

        private readonly BookStoresDBContext dbContext;

        public UsersController(BookStoresDBContext context)
        {
            dbContext = context;
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
