using EllipsePaymentService.Errors;

namespace EllipsePaymentService.Models
{
    public class Error
    {
        public Error(Errors.Errors error)
        {
            Code = (int)error;
            Description = ErrorHandler.GetErrorDescription(error);
        }
        public int Code { get; set; }
        public string Description { get; set; }
    }
}
