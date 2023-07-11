using EllipsePaymentService.Models;
using EllipsePaymentService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Principal;

namespace EllipsePaymentService.Controllers
{
    [Route("/account")]
    public class AccountController: Controller
    {
        private AccountService _accountService;
        public AccountController(AccountService accountService) { 
            _accountService = accountService;
        }

        [HttpPost("account-info")]
        public IActionResult GetAccountInfo()
        {
            string encrypted_account_data;

            if (!HttpContext.Request.Cookies.TryGetValue("ellpise_account", out encrypted_account_data))
                return Ok(new AccountResponse(Errors.Errors.AccountNotAuthorized));

            var account = _accountService.GetAccount(encrypted_account_data);
            return Ok(account);
        }

        [HttpPost("login")]
        public IActionResult Login(string login, string password)
        {
            string encrypted_account_data;
            AccountResponse response;

            if (_accountService.Login(login, password, out response, out encrypted_account_data))
            {
                var cookieOptions = new CookieOptions {
                    Expires = DateTime.Now.AddDays(90),
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    HttpOnly = true
                };
                HttpContext.Response.Cookies.Delete("ellpise_account");
                HttpContext.Response.Cookies.Append("ellpise_account", encrypted_account_data, cookieOptions);
            }

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            string encrypted_account_data;

            if (HttpContext.Request.Cookies.TryGetValue("ellpise_account", out encrypted_account_data))
            {
                var cookieOptions = new CookieOptions
                {
                    Expires =DateTime.Now,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    HttpOnly = true
                };
                HttpContext.Response.Cookies.Delete("ellpise_account");
                HttpContext.Response.Cookies.Append("ellpise_account", "", cookieOptions);
            }

            return Ok(new AccountResponse(Errors.Errors.AccountNotAuthorized));
        }
    }
}
