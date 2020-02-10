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
        public override async Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;
            await Groups.AddToGroupAsync(Context.ConnectionId, name);

            await base.OnConnectedAsync();
        }
    }
}
