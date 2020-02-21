using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
        public bool IsCompleted { get; set; }

        public string FileUrl { get; set; }
        public string FileName { get; set; }
    }

    public enum Priority { Low = 0, Medium = 1, Hight = 2 }
}
