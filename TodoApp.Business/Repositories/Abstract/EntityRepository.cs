using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Entities;
using TodoApp.Entities.Models;

namespace TodoApp.Business.Repositories.Abstract
{
    public abstract class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly TodoDbContext context;
        protected abstract DbSet<TEntity> storage { get; set; }

        public EntityRepository(TodoDbContext context)
        {
            this.context = context;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await storage.AddAsync(entity);
            await SaveAsync();

            return entity;
        }

        public virtual async Task<List<TEntity>> GetListAsync()
        {
            return await storage.ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(int id)
        {
            return await storage.FindAsync(id);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            storage.Update(entity);
            await SaveAsync();

            return entity;
        }

        public virtual async Task<TEntity> DeleteAsync(TEntity entity)
        {
            storage.Remove(entity);

            await SaveAsync();
            return entity;
        }

        public virtual async Task SaveAsync() => await context.SaveChangesAsync();
    }
}
