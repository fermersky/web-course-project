using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Business.Azure;
using TodoApp.Business.Repositories;
using TodoApp.Business.Repositories.Abstract;
using TodoApp.Entities.Models;
using TodoApp.Presentation.ViewModels;

namespace TodoApp.Presentation.Services
{
    public class TodoViewModelService
    {
        private readonly AzureBlobTodosClient blobClient;

        public TodoViewModelService(AzureBlobTodosClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public async Task<Todo> CreateTodoFromModelAsync(CreateTodoViewModel model, ApplicationUser user)
        {
            var todo = new Todo
            {
                Title = model.Title.Trim(),
                Summary = model.Summary.Trim(),
                Deadline = model.Deadline,
                Hashtag = ValidateHashtag(model.Hashtag),
                Priority = model.Priority,
                User = user
            };

            if (model.AttachedFile != null)
            {
                var stream = model.AttachedFile.OpenReadStream();
                string fileName = $"{Guid.NewGuid()}{model.AttachedFile.FileName}";

                var blobInfo = await blobClient.UploadFileAsync(fileName, stream);

                todo.FileUrl = blobInfo.Uri.AbsoluteUri;
                todo.FileName = fileName;
            }

            return todo;
        }

        public Todo UpdateTodoFromModel(EditTodoViewModel model, Todo todo)
        {
            todo.Title = model.Title.Trim();
            todo.Summary = model.Summary.Trim();
            todo.Hashtag = ValidateHashtag(model.Hashtag);
            todo.Deadline = model.Deadline;
            todo.Priority = model.Priority;

            return todo;
        }

        public EditTodoViewModel GetEditViewModel(Todo todo)
        {
            var model = new EditTodoViewModel
            {
                Id = todo.Id,
                Title = todo.Title,
                Hashtag = todo.Hashtag,
                Summary = todo.Summary,
                Deadline = todo.Deadline,
                Priority = todo.Priority,
                IsCompleted = todo.IsCompleted
            };

            return model;
        }

        private string ValidateHashtag(string hashtag)
        {
            if (hashtag == null)
                return hashtag;

            return hashtag.Replace('#', ' ').Replace(" ", string.Empty).ToLower();
        }
    }
}
