using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace EllipsePaymentService.Errors
{
    public enum Errors
    {
        AuthorizationFailed = 401,
        AccountNotFound = 402,
        AccountNotAuthorized = 403
    }

    public static class ErrorHandler
    {
        private static Dictionary<Errors, string> _errorMessages = new Dictionary<Errors, string> {
            { Errors.AuthorizationFailed, "Authorization failed"},
            { Errors.AccountNotFound, "Account not found" } ,
            { Errors.AccountNotAuthorized,"Account not authorized"}
        };

        public static string GetErrorDescription(Errors error)
        {
            if (!_errorMessages.ContainsKey(error))
                return "Unknown error";

            return _errorMessages[error];
        }
    }
}
