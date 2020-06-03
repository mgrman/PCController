using System.Reactive.Subjects;

namespace PCController.Common.DataTypes
{
    internal interface IPinProtectedServer : IRemoteServer
    {
        ISubject<string> Pin { get; }
    }
}
