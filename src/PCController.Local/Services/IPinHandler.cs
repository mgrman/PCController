using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IPinHandler
    {
        string PIN { get; set; }

        Task InitializeJSAsync();
    }
}