namespace BookSearchBot.Model
{
    
    public class StatusRequest
    {
        public StatusRequest(string status)
        {
            Status = status;
        }
        public StatusRequest()
        {

        }
        public string Status { get; set; }

        public static string OK { get; } = "OK";
        public static string FAIL { get; } = "FAIL";
    }
}
