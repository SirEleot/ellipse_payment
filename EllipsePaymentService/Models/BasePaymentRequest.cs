using Microsoft.Extensions.Configuration;

namespace EllipsePaymentService.Models
{
    public class BasePaymentRequest
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string zip_code { get; set; }
        public string ipaddress { get; set; }
        public string email { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string order_desc { get; set; }       

    }
}
