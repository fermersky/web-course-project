using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Business.Repositories.Implementations;
using TodoApp.Entities.Models;

namespace TodoApp.Controllers
{
    [Route("api/todos")]
    [ApiController]
    [Authorize]
    public class TodosApiController : ControllerBase
    {
        private readonly TodoRepository repository;
        public string UserId { get => User.FindFirst(ClaimTypes.NameIdentifier).Value; }
        public enum Filter {Day, Week, Month, Year};

        public TodosApiController(TodoRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Todo>>> GetTodos(Filter? filter, string title, string hashtag)
        {
            var result = await repository.GetListByUserIdAsync(UserId);

            if (filter != null)
            {
                var now = DateTime.Now;

                result = filter switch
                {
                    Filter.Day => result.Where(t => t.Deadline.Day == now.Day).ToList(),
                    Filter.Week => result.Where(t => t.Deadline.Day == now.Day || t.Deadline.Day == now.AddDays(7).Day).ToList(),
                    Filter.Month => result.Where(t => t.Deadline.Month == now.Month && t.Deadline.Year == now.Year).ToList(),
                    Filter.Year => result.Where(t => t.Deadline.Year == now.Year).ToList(),
                    _ => result
                };
            }

            if (title != null)
                result = result.Where(t => t.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

            if (hashtag != null)
                result = result.Where(t => t.Hashtag?.Contains(hashtag, StringComparison.OrdinalIgnoreCase) == true).ToList();

            result = result.OrderByDescending(t => t.Priority).ToList();

            return result;
        }
    }
}