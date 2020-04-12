using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Services
{
    public interface IControllerService
    {
        void Shutdown();

        void Sleep();

        void Lock();
    }
}