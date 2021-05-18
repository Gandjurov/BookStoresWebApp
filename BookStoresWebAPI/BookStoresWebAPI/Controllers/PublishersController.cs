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
        public async Task<ActionResult<Publisher>> GetPublisherDetails(int id)
        {
            // Eager Loading
            //var publisher = dbContext.Publishers
            //                         .Include(pub => pub.Books)
            //                            .ThenInclude(book => book.Sales)
            //                         .Include(pub => pub.Users)
            //                         .Where(pub => pub.PubId == id)
            //                         .FirstOrDefault();

            //Explicit Loading
            var publisher = await dbContext.Publishers.SingleAsync(pub => pub.PubId == id);

            dbContext.Entry(publisher)
                     .Collection(pub => pub.Users)
                     .Load();

            dbContext.Entry(publisher)
                     .Collection(pub => pub.Books)
                     .Query()
                     .Include(book => book.Sales)
                     .Load();

            var user = await dbContext.Users.SingleAsync(usr => usr.UserId == 1);
            dbContext.Entry(user)
                     .Reference(usr => usr.Role)
                     .Load();

            if (publisher == null)
                return NotFound();

            return publisher;
        }

        // Post: api/Publishers/5
        [HttpGet("PostPublisherDetails")]
        public ActionResult<Publisher> PostPublisherDetails()
        {
            var publisher = new Publisher();
            publisher.PublisherName = "Harper & Brothers";
            publisher.City = "New York City";
            publisher.State = "NY";
            publisher.Country = "USA";

            Book book1 = new Book();
            book1.Title = "Good Night moon - 1";
            book1.PublishedDate = DateTime.Now;

            Book book2 = new Book();
            book2.Title = "Good Night moon - 2";
            book2.PublishedDate = DateTime.Now;

            Sale sale1 = new Sale();
            sale1.Quantity = 2;
            sale1.StoreId = "8042";
            sale1.OrderNum = "XYZ";
            sale1.PayTerms = "Net 30";
            sale1.OrderDate = DateTime.Now;

            Sale sale2 = new Sale();
            sale2.Quantity = 2;
            sale2.StoreId = "7131";
            sale2.OrderNum = "QA879.1";
            sale2.PayTerms = "Net 30";
            sale2.OrderDate = DateTime.Now;

            book1.Sales.Add(sale1);
            book2.Sales.Add(sale2);

            publisher.Books.Add(book1);
            publisher.Books.Add(book2);

            dbContext.Publishers.Add(publisher);
            dbContext.SaveChanges();

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
