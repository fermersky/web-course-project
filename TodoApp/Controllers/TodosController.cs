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

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodosController : Controller
    {
        private readonly TodoRepository todoRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public TodosController(TodoRepository todoRepository, UserManager<ApplicationUser> userManager)
        {
            this.todoRepository = todoRepository;
            this.userManager = userManager;
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

                var signedInUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = await userManager.FindByIdAsync(signedInUserId);

                todo.User = user;

                await todoRepository.AddAsync(todo);

                return RedirectToAction("index", "todos");
            }

            return View();
        }
    }
}