namespace Glaz.Server.Data.AppSettings
{
    public sealed class EmailSenderOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public EmailCredentials Credentials { get; set; }
        public bool EnableSsl { get; set; }
        public string From { get; set; }
    }
}