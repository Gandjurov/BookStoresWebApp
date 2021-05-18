using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoresWebAPI.Models;

namespace BookStoresWebAPI.Controllers
{
    public class PublishersController : ApiController
    {
        private readonly BookStoresDBContext dbContext;

        public PublishersController(BookStoresDBContext context)
        {
            dbContext = context;
        }

        // GET: api/Publishers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Publisher>>> GetPublishers()
        {
            return await dbContext.Publishers.ToListAsync();
        }

        // GET: api/Publishers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Publisher>> GetPublisher(int id)
        {
            var publisher = await dbContext.Publishers.FindAsync(id);

            if (publisher == null)
                return NotFound();

            return publisher;
        }

        // GET: api/Publishers/5
        [HttpGet("GetPublisherDetails/{id}")]
        public ActionResult<Publisher> GetPublisherDetails(int id)
        {
            var publisher = dbContext.Publishers
                                     .Include(pub => pub.Books)
                                        .ThenInclude(book => book.Sales)
                                     .Include(pub => pub.Users)
                                     .Where(pub => pub.PubId == id)
                                     .FirstOrDefault();

            if (publisher == null)
                return NotFound();

            return publisher;
        }

        // PUT: api/Publishers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublisher(int id, Publisher publisher)
        {
            if (id != publisher.PubId)
                return BadRequest();

            dbContext.Entry(publisher).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublisherExists(id))
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

        // POST: api/Publishers
        [HttpPost]
        public async Task<ActionResult<Publisher>> PostPublisher(Publisher publisher)
        {
            dbContext.Publishers.Add(publisher);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetPublisher", new { id = publisher.PubId }, publisher);
        }

        // DELETE: api/Publishers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var publisher = await dbContext.Publishers.FindAsync(id);
            if (publisher == null)
                return NotFound();

            dbContext.Publishers.Remove(publisher);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool PublisherExists(int id)
        {
            return dbContext.Publishers.Any(e => e.PubId == id);
        }
    }
}
