using EllipsePaymentService.Errors;

namespace EllipsePaymentService.Models
{
    public class AccountResponse
    {
        public AccountResponse(Errors.Errors error)
        {
            Status = (int)error;
            Description = ErrorHandler.GetErrorDescription(error);
            Status = (int)error;
        }

        public AccountResponse(Account account)
        {
            Account = account;
            Status = 200;
        }
        public int Status { get; set; }
        public string Description { get; set; }
        public Account Account { get; set; }
    }
}
