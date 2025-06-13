namespace JP_APIService.Models
{
    public class RateLimitingOptionsModel
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
        public int QueueLimit { get; set; }
    }
}
