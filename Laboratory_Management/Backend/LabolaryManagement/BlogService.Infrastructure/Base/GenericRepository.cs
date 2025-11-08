using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Infrastructure.Data;

namespace BlogService.Infrastructure.Base
{
    public class GenericRepository<T> where T : class
    {
        protected readonly DBContext? _context;
        private const int PageSize = 10;

        public GenericRepository(DBContext context)
        {
            _context = context;
        }

        public GenericRepository()
        {
            _context ??= new DBContext();
        }

        public Task<List<T>> GetAllPagedAsync(int pageNumber)
        {
            return Task.Run(() => _context.Set<T>()
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList());
        }

        public Task<List<T>> GetByIdPagesAsync(object id, int pageNumber)
        {
            return Task.Run(() => _context.Set<T>()
                .Where(e => EF.Property<object>(e, "Id") == id)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList());
        }

        public Task<List<T>> GetAllAsync()
        {
            return Task.Run(() => _context.Set<T>().ToList());
        }

        public Task<T?> GetByIdAsync(object id)
        {
            return Task.Run(() => _context.Set<T>().Find(id));
        }

        public Task AddAsync(T entity)
        {
            return Task.Run(() =>
            {
                _context.Set<T>().Add(entity);
                _context.SaveChanges();
                _context.SaveChanges();
            });
        }

        public Task UpdateAsync(T entity)
        {
            return Task.Run(() =>
            {
                _context.Set<T>().Update(entity);
                _context.SaveChanges();
            });
        }

        public Task DeleteAsync(T entity)
        {
            return Task.Run(() =>
            {
                _context.Set<T>().Remove(entity);
                _context.SaveChanges();
            });
        }
    }
}
