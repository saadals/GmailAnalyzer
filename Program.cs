using GmailAnalyzer;

namespace Analyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var gmailApi = new GmailApi("CLIENT_ID", "CLIENT_SECRET");      //Fake credentials
            
            gmailApi.SendEmail(
                "myEmail@gmail.com", 
                "OtherEmail@gmail.com", 
                "Please Give report",
                "Hello Other, Please send it, I need it Thanks");
            
            gmailApi.DeleteMessageBySender("@fakeDomain.com");
            
            gmailApi.DeleteMessageBySubject("Overdue taxes");
        }
    }
}