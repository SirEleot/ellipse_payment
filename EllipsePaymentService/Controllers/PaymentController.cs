using EllipsePaymentService.Models;
using EllipsePaymentService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace EllipsePaymentService.Controllers
{
    [Route("/payment")]
    public class PaymentController: Controller
    {
        private List<PaymentMethod> _methods;
        private SecureService _sequre;
        private AccountService _accountService;
        private string _accountUrl;

        public PaymentController(IConfiguration configuration, SecureService sequre, AccountService accountService)
        {
            _sequre = sequre;
           _methods = new List<PaymentMethod>();
            configuration.GetSection("Integrations").Bind(_methods);
            _accountService = accountService;
            _accountUrl = configuration.GetValue<string>("RedirectToAccountUrl");
        }

        [HttpGet("select-method")]
        async public Task<IActionResult> Index([FromQuery] BasePaymentRequest request)
        {
            List<string> missing_fields;
            FillAdditionalDataToRequest(request);

            if (!IsValidPaymentReqeust(request, out missing_fields))
                return ValidationError(missing_fields);

            return View();          
        }


        [HttpPost("select-method")]
        public async Task<IActionResult> Index([FromQuery] BasePaymentRequest request, [FromForm]string method)
        {
            string encrypted_account_data;
            request.amount = request.amount.Replace(',', '.');
            if (!HttpContext.Request.Cookies.TryGetValue("ellpise_account", out encrypted_account_data))
                return Ok(new AccountResponse(Errors.Errors.AccountNotAuthorized));

            var account = _accountService.GetAccount(encrypted_account_data);

            List<string> missing_fields;

            FillAdditionalDataToRequest(request);
            if (!IsValidPaymentReqeust(request, out missing_fields) || method == null || !_methods.Any(m => m.Name == method))
                return ValidationError(missing_fields);


            try
            {
                var payment_method = _methods.First(m=>m.Name == method);
                var full_request = _accountService.GetFullPaymentRequest(request, account.Account.SocialclubId, payment_method);
                var parsed = PaymentReqeustToDict(full_request);
                var formContent = new FormUrlEncodedContent(parsed);
                using var client = new HttpClient();
                var responce = await client.PostAsync(payment_method.Url, formContent);
                var content = await responce.Content.ReadAsStringAsync();
                Console.WriteLine($"response {content}");
                var result = HttpUtility.ParseQueryString(content);

                if (result.Get("type").Trim() == "async-form-response")
                {
                    return Redirect(result.Get("redirect-url"));
                }
                else
                {
                    var message = new Dictionary<string, string>
                    {
                        { "message", result.Get("error-message") }
                    };
                    return Redirect(Url.Action("Fail", "Payment", message));
                }
            }
            catch (Exception)
            {
                return Redirect(Url.Action("Fail", "Payment"));
            }
        }

        [HttpGet("return")]
        public IActionResult Return()
        {
            return Redirect(_accountUrl);
        }

        [HttpPost("callback"), HttpGet("callback")]
        public IActionResult Callback(string status, string orderid, string client_orderid, string control)
        {
            var method_name = client_orderid.Split('_')[0];
            var method = _methods.First(m => m.Name == method_name);

            if(_sequre.IsSignValid(status, orderid, client_orderid, control, method))
            {
                _accountService.UpdatePaymentStatus(status, orderid, client_orderid);
            }

            return Ok("ok");
        }

        [HttpPost("success"), HttpGet("success")]
        public IActionResult Success(string message)
        {
            return View("Success", message);
        }


        [HttpPost("fail"), HttpGet("fail")]
        public IActionResult Fail(string message)
        {
            return View("Fail", message);
        }

        private bool IsValidPaymentReqeust(BasePaymentRequest request, out List<string> misssing_fields)
        {
            var props = request.GetType().GetProperties();
            misssing_fields = new List<string>();
            foreach (var prop in props)
            {                
                var value = prop.GetValue(request);
                if (value == null)
                    misssing_fields.Add(prop.Name);
            }
            return misssing_fields.Count == 0;
        }

        private IActionResult ValidationError(List<string> fields)
        {
            return Redirect(Url.Action("Fail", "Payment", $"missing required fields: {String.Join(", ", fields.ToArray())}"));
        }

        private Dictionary<string, string> PaymentReqeustToDict(FullPaymentRequest request)
        {
            var payment_json = JsonSerializer.Serialize(request);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(payment_json);
        }

        private void FillAdditionalDataToRequest(BasePaymentRequest request)
        {
            request.ipaddress = HttpContext.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrEmpty(request.currency))
                request.currency = "EUR";

            if (string.IsNullOrEmpty(request.order_desc))
                request.order_desc = "Top up";

        }
    }
}
