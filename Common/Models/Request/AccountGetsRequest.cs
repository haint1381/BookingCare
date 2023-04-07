namespace BookingCare.Common.Models.Request
{
    public class AccountGetsRequest
    {
        public string KeyWord { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
