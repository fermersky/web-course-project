using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Business.Repositories.Abstract;
using TodoApp.Entities;
using TodoApp.Entities.Models;

namespace TodoApp.Business.Repositories.Implementations
{
    public class TodoRepository : EntityRepository<Todo>
    {
        protected override DbSet<Todo> storage { get; set; }

        public TodoRepository(TodoDbContext context) : base(context)
        {
            this.storage = context.Todos;
        }

        public async Task<List<Todo>> GetListByUserIdAsync(string userId)
        {
            var result = storage.Where(todo => todo.User.Id == userId);
            return await result.ToListAsync();
        }
    }
}
