using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IPinHandler
    {
        Task InitializeJSAsync();
        string PIN { get; set; }
    }
}