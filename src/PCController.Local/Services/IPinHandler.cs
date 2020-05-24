using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IPinHandler
    {
        string Pin { get; set; }

        Task InitializeJsAsync();
    }
}
