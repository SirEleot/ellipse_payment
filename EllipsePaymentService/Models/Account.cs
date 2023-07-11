using System.Collections.Generic;

namespace EllipsePaymentService.Models
{
    public class Account
    {
        public string Login { get; set; }
        public string SocialclubId { get; set; }
        public string Password { get; set; }
        public float Gocoins { get; set; }
        public List<PaymentInfo> PaymentHistory { get; set; }
    }
}
