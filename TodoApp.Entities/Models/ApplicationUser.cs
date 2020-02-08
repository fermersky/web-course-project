using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApp.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual List<Todo> Todos { get; set; }
    }
}
