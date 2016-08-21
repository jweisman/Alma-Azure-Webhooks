namespace AlmaWebhookAzure.Models
{
    public class AppSettings
    {
        public FtpSettings FtpSettings { get; set; }
        public DropboxSettings DropboxSettings { get; set; }
    }

    public class FtpSettings
    {
        public string Host { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
    }

    public class DropboxSettings
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }
}