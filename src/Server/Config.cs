namespace PCController
{
    public class Config
    {
        public string Pin { get; set; }
        public EmailConfig Email { get; set; }

    }

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