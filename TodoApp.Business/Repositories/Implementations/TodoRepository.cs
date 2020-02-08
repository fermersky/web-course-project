using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TodoApp.Business.Repositories.Abstract;
using TodoApp.Entities;
using TodoApp.Entities.Models;

namespace TodoApp.Business.Repositories.Implementations
{
    public class TodoRepository : EntityRepository<Todo>
    {
        public TodoRepository(TodoDbContext context) : base(context)
        {
            this.storage = context.Todos;
        }

        protected override DbSet<Todo> storage { get; set; }

    }
}
