using System;

namespace PCController.SignalR.Common
{
    public static class SignalRConfig
    {
        public const string IdHeader = "PCController_ID";
        public const string RelativePath = "/statusHub";
        public const string InvokeCommandMethodName = "InvokeCommand";
        public static readonly Uri RelativeUri = new Uri(RelativePath, UriKind.Relative);
    }
}
