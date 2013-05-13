namespace HackandCraft.Payment
{
    public class PaymentStatus
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string redirectUrl { get; set; }
        public string paymentRef { get; set; }
        public string bookingToken { get; set; }
    }
}
