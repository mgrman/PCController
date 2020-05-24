using System.Reactive.Subjects;

namespace PCController.Common.DataTypes
{
    public interface IPinProtectedServer : IRemoteServer
    {
        ISubject<string> Pin { get; }
    }
}
