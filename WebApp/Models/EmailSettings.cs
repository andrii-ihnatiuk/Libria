namespace Libria.Models
{
    public class EmailSettings
    {
        public int Port { get; set; } = 25;
        public string Server { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}
