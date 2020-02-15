using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TodoApp.Entities.Models;

namespace TodoApp.Presentation.ViewModels
{
    public class EditTodoViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Summary { get; set; }

        public string? Hashtag { get; set; }

        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Required]
        public Priority Priority { get; set; }

        public bool IsCompleted { get; set; }
    }
}
