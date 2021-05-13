using BookStoresWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoresWebAPI.Controllers
{
    public class AuthorController : ApiController
    {
        private BookStoresDBContext context;
        public AuthorController(BookStoresDBContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IEnumerable<Author> Get()
        {
            return context.Authors.ToList();
        }
    }
}
