using BookStoresWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStoresWebAPI.Controllers
{
    public class SalesController : ApiController
    {
        private readonly BookStoresDBContext dbContext;

        public SalesController(BookStoresDBContext context)
        {
            dbContext = context;
        }

        // GET: Sales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            return await dbContext.Sales.ToListAsync();
        }

        // GET: Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await dbContext.Sales.FindAsync(id);

            if (sale == null)
                return NotFound();

            return sale;
        }

        // PUT: Sales/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> PutSale(int id, Sale sale)
        {
            if (id != sale.SaleId)
                return BadRequest();

            dbContext.Entry(sale).State = EntityState.Modified;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<Sale>> PostSale(Sale sale)
        {
            dbContext.Sales.Add(sale);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction("GetSale", new { id = sale.SaleId }, sale);
        }

        // DELETE: Sales/5
        [HttpDelete("[action]/{id}")]
        public async Task<ActionResult<Sale>> DeleteSale(int id)
        {
            var sale = await dbContext.Sales.FindAsync(id);
            if (sale == null)
                return NotFound();

            dbContext.Sales.Remove(sale);
            await dbContext.SaveChangesAsync();

            return sale;
        }

        private bool SaleExists(int id)
        {
            return dbContext.Sales.Any(e => e.SaleId == id);
        }
    }
}
