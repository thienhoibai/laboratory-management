using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;
using BlogService.Infrastructure.Data;

namespace BlogService.Infrastructure.Repository
{
    public class TagRepository : GenericRepository<Models.Tag>
    {
        public TagRepository(DBContext context) : base(context)
        {
        }
        public TagRepository() : base()
        {
        }
    }
}
