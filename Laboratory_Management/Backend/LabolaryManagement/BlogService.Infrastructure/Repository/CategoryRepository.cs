using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Infrastructure.Base;

using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
namespace BlogService.Infrastructure.Repository
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(DBContext context) : base(context) { }
    }
}
