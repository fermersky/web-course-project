using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Business.Repositories.Implementations;
using TodoApp.Entities.Models;
using TodoApp.Presentation.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TodoApp.Business.TodosSignalR;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodosController : Controller
    {
        private readonly TodoRepository todoRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHubContext<TodoHub> hubContext;

        public TodosController(TodoRepository todoRepository, UserManager<ApplicationUser> userManager, IHubContext<TodoHub> hubContext)
        {
            this.todoRepository = todoRepository;
            this.userManager = userManager;
            this.hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var todo = new Todo
                {
                    Title = model.Title.Trim(),
                    Summary = model.Summary.Trim(),
                    Deadline = model.Deadline,
                    Hashtag = model.Hashtag.Trim().ToLower(),
                    Priority = model.Priority,
                };

                // add todo and bing it to signed in user
                var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                todo.User = await userManager.FindByIdAsync(signedInUserId);
                await todoRepository.AddAsync(todo);

                await SendUserDateOnTodosUpdateAsync();

                return RedirectToAction("index", "todos");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTodoStatus(int id)
        {
            var todo = await todoRepository.ChangeTodoStatusAsync(id);

            if (todo == null)
                return NotFound();

            await SendUserDateOnTodosUpdateAsync();

            return Ok($"Status of todo with id = {id} was successfully changed");
        }

        private async Task SendUserDateOnTodosUpdateAsync()
        {
            var json = await GetTodosForUserAsJson();
            var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await userManager.FindByIdAsync(signedInUserId);

            // notify clients with a same username via websockets
            await hubContext.Clients.Group(user.UserName)
                .SendAsync("TodosUpdated", json);
        }

        private async Task<string> GetTodosForUserAsJson()
        {
            var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await todoRepository.GetListByUserIdAsync(signedInUserId);
            var todos = result.OrderByDescending(t => t.Priority).ToList();

            return JsonSerializer.Serialize(todos, options: new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}