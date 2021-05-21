using BookStoresWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoresWebAPI.Controllers
{
    public class AuthorsController : ApiController
    {
        private BookStoresDBContext dbContext;
        public AuthorsController(BookStoresDBContext context)
        {
            this.dbContext = context;
        }

        // GET: Authors
        [HttpGet("GetAuthors")]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            return await dbContext.Authors.ToListAsync();
        }

        [HttpGet("GetAuthorsCount")]
        public async Task<ActionResult<ItemCount>> GetAuthorsCount()
        {
            ItemCount itemCount = new ItemCount();

            itemCount.Count = dbContext.Authors.Count();
            return await Task.FromResult(itemCount);
        }

        // GET: Authors
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthorsByPage(int pageSize, int pageNumber)
        {
            List<Author> AuthorList = await dbContext.Authors.ToListAsync();
            AuthorList = AuthorList.Skip(pageNumber * pageSize).Take(pageSize).ToList();

            return await Task.FromResult(AuthorList);
        }

        // GET: Authors/5
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await dbContext.Authors.FindAsync(id);

            if (author == null)
                return NotFound();

            return author;
        }

        // PUT: Authors/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("UpdateAuthor/{id}")]
        public async Task<IActionResult> PutAuthor(int id, Author author)
        {
            if (id != author.AuthorId)
                return BadRequest();

            dbContext.Entry(author).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: Authors
        [HttpPost("CreateAuthor")]
        public async Task<ActionResult<Author>> PostAuthor(Author author)
        {
            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetAuthor", new { id = author.AuthorId }, author);
        }

        // DELETE: Authors/5
        [HttpDelete("DeleteAuthor/{id}")]
        public async Task<ActionResult<Author>> DeleteAuthor(int id)
        {
            var author = await dbContext.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            dbContext.Authors.Remove(author);
            await dbContext.SaveChangesAsync();

            return author;
        }

        private bool AuthorExists(int id)
        {
            return dbContext.Authors.Any(e => e.AuthorId == id);
        }
    }
}
