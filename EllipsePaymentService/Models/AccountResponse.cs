using EllipsePaymentService.Errors;

namespace EllipsePaymentService.Models
{
    public class AccountResponse
    {
        public AccountResponse(Errors.Errors error)
        {
            Error = new Error(error);
            Status = (int)error;
        }

        public AccountResponse(Account account)
        {
            Account = account;
            Status = 200;
        }
        public int Status { get; set; }
        public Error Error { get; set; }
        public Account Account { get; set; }
    }
}
