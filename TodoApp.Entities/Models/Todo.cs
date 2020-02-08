using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApp.Entities.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Hashtag { get; set; }
        public DateTime Deadline { get; set; }
        public Priority Priority { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public enum Priority { Low = 0, Medium = 1, Hight = 2 }
}
