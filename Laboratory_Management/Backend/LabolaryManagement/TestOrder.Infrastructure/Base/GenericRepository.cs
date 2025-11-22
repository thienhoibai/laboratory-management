using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Data;

namespace TestOrder.Infrastructure.Base
{
    public class GenericRepository<T> where T : class
    {
        protected readonly TestOrderDBContext? _context;
        private const int PageSize = 10;

        public GenericRepository(TestOrderDBContext context)
        {
            _context = context;
        }

        public GenericRepository()
        {
            _context ??= new TestOrderDBContext();
        }

        public async Task<List<T>> GetAllPagedAsync(int pageNumber,int pageSize)
        {
            return await _context.Set<T>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

<<<<<<<<< Temporary merge branch 1
        public Task<List<T>> GetByIdPagesAsync(object id, int pageNumber)
        {
            return Task.Run(() => _context.Set<T>()
=========
        public async Task<List<T>> GetByIdPagesAsync(object id, int pageNumber)
        {
            return await _context.Set<T>()
>>>>>>>>> Temporary merge branch 2
                .Where(e => EF.Property<object>(e, "Id") == id)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

<<<<<<<<< Temporary merge branch 1
        public Task<T?> GetByIdAsync(object id)
=========
        public async Task<T?> GetByIdAsync(object id)
>>>>>>>>> Temporary merge branch 2
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public Task AddAsync(T entity)
        {
            return Task.Run(() =>
            {
                _context.Set<T>().Add(entity);
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
