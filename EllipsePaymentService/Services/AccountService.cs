using System.Text;
using System.Security.Cryptography;
using System;
using System.Text.Json;
using EllipsePaymentService.Models;
using Microsoft.Extensions.Configuration;

namespace EllipsePaymentService.Services
{
    public class AccountService
    {
        private DbService _dbService;
        private SecureService _secureService;
        private IConfiguration _configuration;
        public AccountService(DbService dbService, SecureService sequreService, IConfiguration configuration) {
            _dbService = dbService;
            _secureService = sequreService;
            _configuration = configuration;
        }

        public AccountResponse GetAccount(string encrypted_account_data)
        {
            var decrypted_account_data = _secureService.DecryptString(encrypted_account_data);
            var accountData = JsonSerializer.Deserialize<AccountData>(decrypted_account_data);
            Account account;

            if (!_dbService.TryGetAccoutn(accountData.Login, out account))
                return new AccountResponse(Errors.Errors.AccountNotFound);

            if(accountData.Password != account.Password)
                return new AccountResponse(Errors.Errors.AuthorizationFailed);

            account.Password = "";
            return new AccountResponse(account);
        }
        
        public void UpdatePaymentStatus(string status, string orderid, string client_orderid)
        {
            var order_id = client_orderid.Split('_')[1];
            var order = _dbService.GetOrderById(order_id);
            if (order.Status == "PROCESSING")
            {
                order.Status = status.ToUpper();
                order.Date = DateTime.Now;
                order.FystId = orderid;
                _dbService.UpdateOrder(order);

                if (order.Status == "APPROVED")
                    _dbService.AddCoins(order.Sum, order.SocialclubId);
            }
        }

        public FullPaymentRequest GetFullPaymentRequest(BasePaymentRequest request, string socialclubId, PaymentMethod payment_method)
        {

            int order_id = _dbService.CreateNewOrder(request, socialclubId);
            var full_reqrust =  new FullPaymentRequest { 
                first_name = request.first_name,
                last_name = request.last_name,
                address1 = request.address1,
                email = request.email,
                amount = request.amount,
                city = request.city,
                currency = request.currency,
                zip_code = request.zip_code,
                phone = request.phone,
                order_desc = request.order_desc,
                country = request.country,
                ipaddress = request.ipaddress                
            };
            full_reqrust.client_orderid = $"{payment_method.Name}_{order_id}";
            full_reqrust.redirect_success_url = _configuration.GetValue<string>("RedirectSuccessUrl");
            full_reqrust.redirect_fail_url = _configuration.GetValue<string>("RedirectFailUrl");
            full_reqrust.server_callback_url = _configuration.GetValue<string>("ServerCallbackUrl");
            full_reqrust.site_url = _configuration.GetValue<string>("Site");
            full_reqrust.control = _secureService.GenerateSign(full_reqrust.client_orderid, full_reqrust.amount, full_reqrust.email, payment_method);
            return full_reqrust;
        }
     
        public bool Login(string login, string password, out AccountResponse response, out string encrypted_account_data)
        {
            Account account;
            encrypted_account_data = null;

            if (!_dbService.TryGetAccoutn(login, out account))
            {
                response = new AccountResponse(Errors.Errors.AccountNotFound);
                return false;
            }

            var hashed_password = _secureService.GetPasswordHash(password);

            if (account.Password != hashed_password)
            {
                response = new AccountResponse(Errors.Errors.AuthorizationFailed);
                return false;
            }
            var account_data = new AccountData
            {
                SocialclubId = account.SocialclubId,
                Login = login,
                Password = account.Password
            };
            var decrypted_account_data = JsonSerializer.Serialize(account_data);
            encrypted_account_data = _secureService.EncryptString(decrypted_account_data);
            account.Password = "";
            response = new AccountResponse(account);
            return true;
        }

    }
}
