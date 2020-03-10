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
using TodoApp.Business.Azure;
using TodoApp.Presentation.Services;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodosController : Controller
    {
        private readonly TodoViewModelService viewModelService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHubContext<TodoHub> hubContext;
        private readonly AzureBlobTodosClient blobClient;
        private readonly TodoRepository todoRepository;

        public TodosController(
            TodoViewModelService viewModelService,
            UserManager<ApplicationUser> userManager,
            AzureBlobTodosClient blobClient,
            TodoRepository todoRepository,
            IHubContext<TodoHub> hubContext
        )
        {
            this.viewModelService = viewModelService;
            this.userManager = userManager;
            this.hubContext = hubContext;
            this.blobClient = blobClient;
            this.todoRepository = todoRepository;
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
                // add todo and bing it to signed in user
                var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await userManager.FindByIdAsync(signedInUserId);

                var todo = await viewModelService.CreateTodoFromModelAsync(model, user);

                await todoRepository.AddAsync(todo);
                await SendUserDateOnTodosUpdateAsync();

                return RedirectToAction("index", "todos");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var todo = await todoRepository.GetByIdAsync(id.Value);

            if (todo == null)
                return NotFound();

            var model = viewModelService.GetEditViewModel(todo);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var toUpdate = await todoRepository.GetByIdAsync(model.Id);
                var todo = viewModelService.UpdateTodoFromModel(model, toUpdate);

                await todoRepository.UpdateAsync(todo);
                await SendUserDateOnTodosUpdateAsync();

                return RedirectToAction("index", "todos");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var todo = await todoRepository.GetByIdAsync(id.Value);

            if (todo == null)
                return NotFound();

            if (todo.FileUrl != null)
                await blobClient.DeleteFileAsync(todo.FileName);

            await todoRepository.DeleteAsync(todo);

            await SendUserDateOnTodosUpdateAsync();
            return RedirectToAction("index", "todos");
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