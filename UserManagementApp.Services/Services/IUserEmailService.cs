using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementApp.Services.Models;

namespace UserManagementApp.Services.Services
{
    public interface IUserEmailService
    {
       Task SendEmailAsyc(Message message);    }
}
