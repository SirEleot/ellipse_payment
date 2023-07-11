using System;

namespace EllipsePaymentService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Sum { get; set; }
        public DateTime Date { get; set; }
        public string FystId { get; set; }
        public string SocialclubId { get; set; }
    }
}
