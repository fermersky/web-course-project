using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Entities.Models;

namespace TodoApp.Business.TodosSignalR
{
    public class TodoHub : Hub
    {
        public void Notify(Todo todo)
        {
            string name = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;
        }

        public override async Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;
            await Groups.AddToGroupAsync(Context.ConnectionId, name);

            await base.OnConnectedAsync();
        }
    }
}
