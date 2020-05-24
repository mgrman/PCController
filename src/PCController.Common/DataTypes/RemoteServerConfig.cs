using System;

namespace PCController.Local
{
    public class RemoteServerConfig
    {
        public string Name { get; set; }

        public string MacAddress { get; set; }

        public Uri Uri { get; set; }

        public string Pin { get; set; }
    }
}
