namespace CityInfo.API.Services
{
    public class CloudMailService : IMailService
    {
        private string _mailTo = string.Empty;
        private string _mailFrom = string.Empty;

        public void Send(string subject, string message)
        {
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"message: {message}");
        }
    
    }
}
