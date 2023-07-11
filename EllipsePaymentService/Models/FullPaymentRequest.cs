using Microsoft.Extensions.Configuration;

namespace EllipsePaymentService.Models
{
    public class FullPaymentRequest: BasePaymentRequest
    {
        public string client_orderid { get; set; }
        public string redirect_success_url { get; set; }
        public string redirect_fail_url { get; set; }
        public string server_callback_url { get; set; }
        public string site_url { get; set; }
        public string preferred_language { get; set; } = "en";
        public string control { get; set; }
    }
}
