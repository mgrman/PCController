namespace PCController
{

    public class EmailConfig
    {
        public IMAPConfig Imap { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class IMAPConfig
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public bool? UseSsl { get; set; }
    }
}