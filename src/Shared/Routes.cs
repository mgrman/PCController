namespace PCController.Shared
{
    public static class Routes
    {
        public const string CommandWord = "command";
        public const string CommandPlaceholder = "{" + CommandWord + "}";
        public const string StatusRoute = "api/status";
        public const string CheckPinRout = "api/pin";
        public const string CommandRoute = "api/controller/" + CommandPlaceholder;
        public const string PinHeader = "pin";

        public const string MacAddressRoute = "api/mac";
    }
}