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

        public async Task<IActionResult> Index()
        {
            var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var model = await this.todoRepository.GetListByUserIdAsync(signedInUserId);

            return View(model);
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
                    Title = model.Title,
                    Summary = model.Summary,
                    Deadline = model.Deadline,
                    Hashtag = model.Hashtag,
                    Priority = model.Priority,
                };

                // add todo and bing it to signed in user
                var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                todo.User = await userManager.FindByIdAsync(signedInUserId);
                await todoRepository.AddAsync(todo);

                // serialize todo to json
                var json = JsonSerializer.Serialize(model);

                // notify clients with a same username via websockets
                await hubContext.Clients.Group(todo.User.UserName)
                    .SendAsync("TodoCreated", json);

                return RedirectToAction("index", "todos");
            }

            return View();
        }
    }
}