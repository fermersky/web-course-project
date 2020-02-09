﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Entities;

namespace TodoApp.Business.Repositories.Abstract
{
    public abstract class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly TodoDbContext context;
        protected abstract DbSet<TEntity> storage { get; set; }

        public EntityRepository(TodoDbContext context)
        {
            this.context = context;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await storage.AddAsync(entity);
            await Save();

            return entity;
        }

        public async Task<List<TEntity>> GetListAsync()
        {
            return await storage.ToListAsync();
        }

        public async Task Save() => await context.SaveChangesAsync();


    }
}