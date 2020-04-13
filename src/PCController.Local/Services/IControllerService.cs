using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IControllerService
    {
        void InvokeCommand(Command command);
    }
}