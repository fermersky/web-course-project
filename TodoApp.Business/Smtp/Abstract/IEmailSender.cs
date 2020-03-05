using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Business.Smtp.Models;

namespace TodoApp.Business.Smtp.Abstract
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
