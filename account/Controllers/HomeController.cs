﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using account.BusinessLogic;
using account.ViewModels;
using account.ViewModel;

namespace account.Controllers
{
    public class HomeController : Controller
    {
        const string EMAIL_CONFIRMATION = "EmailConfirmation";
        const string PASSWORD_RESET = "ResetPassword";
        void CreateTokenProvider(UserManager<IdentityUser> manager, string tokenType)
        {
            var provider = new DpapiDataProtectionProvider("CompSci Elites");
            manager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser>(
            provider.Create(tokenType));
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]            
        public ActionResult Index(Login login)
        {
            // UserStore and UserManager manages data retreival.
            UserStore<IdentityUser> userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> manager = new UserManager<IdentityUser>(userStore);
            //IdentityUser identityUser = manager.Find(login.UserName,
            //                                                 login.Password);

            if (ModelState.IsValid)
            {
                if (ValidLogin(login))
                {
                    IAuthenticationManager authenticationManager
                                           = HttpContext.GetOwinContext().Authentication;
                    authenticationManager
                   .SignOut(DefaultAuthenticationTypes.ExternalCookie);

                    var identity = new ClaimsIdentity(new[] {
                                            new Claim(ClaimTypes.Name, login.UserName),
                                        },
                                        DefaultAuthenticationTypes.ApplicationCookie,
                                        ClaimTypes.Name, ClaimTypes.Role);
                    // SignIn() accepts ClaimsIdentity and issues logged in cookie. 
                    authenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, identity);
                    return RedirectToAction("SecureArea", "Home");
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisteredUser newUser)
        {
            var userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> manager = new UserManager<IdentityUser>(userStore)
            {
                UserLockoutEnabledByDefault = true,
                DefaultAccountLockoutTimeSpan = new TimeSpan(0, 10, 0),
                MaxFailedAccessAttemptsBeforeLockout = 3
            };

            var identityUser = new IdentityUser()
            {
                UserName = newUser.UserName,
                Email = newUser.Email
            };
            IdentityResult result = manager.Create(identityUser, newUser.Password);

            if (result.Succeeded)
            {
                CreateTokenProvider(manager, EMAIL_CONFIRMATION);

                var code = manager.GenerateEmailConfirmationToken(identityUser.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { userId = identityUser.Id, code = code }, protocol: Request.Url.Scheme);

                MailHelper mailer = new MailHelper("anabellchan@gmail.com", "Account Confirmation", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">Confirm Registration</a>");

                bool response = mailer.EmailFromArvixe();
                const string SUCCESS = "Please check your email to confirm registration.";
                const string FAIL = "Failure sending email.";

                if (response)
                    ViewBag.Response = SUCCESS;
                else
                    ViewBag.Response = FAIL;

                ViewBag.Display = "style='Display:none'";
                //string email = "Please confirm your account by clicking this link: <a href=\""
                //                + callbackUrl + "\">Confirm Registration</a>";
                //ViewBag.FakeConfirmation = email;

            }
            return View();
        }
        [Authorize]
        public ActionResult SecureArea()
        {
            ViewBag.Logout = false;
            return View();
        }

        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }
        bool ValidLogin(Login login)
        {
            UserStore<IdentityUser> userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> userManager = new UserManager<IdentityUser>(userStore)
            {
                UserLockoutEnabledByDefault = true,
                DefaultAccountLockoutTimeSpan = new TimeSpan(0, 10, 0),
                MaxFailedAccessAttemptsBeforeLockout = 3
            };
            var user = userManager.FindByName(login.UserName);

            if (user == null)
                return false;

            // User is locked out.
            if (userManager.SupportsUserLockout && userManager.IsLockedOut(user.Id))
                return false;

            // Validated user was locked out but now can be reset.
            if (userManager.CheckPassword(user, login.Password) && userManager.IsEmailConfirmed(user.Id))
            {
                if (userManager.SupportsUserLockout
                 && userManager.GetAccessFailedCount(user.Id) > 0)
                {
                    userManager.ResetAccessFailedCount(user.Id);
                }
            }
            // Login is invalid so increment failed attempts.
            else
            {
                bool lockoutEnabled = userManager.GetLockoutEnabled(user.Id);
                if (userManager.SupportsUserLockout && userManager.GetLockoutEnabled(user.Id))
                {
                    userManager.AccessFailed(user.Id);
                    return false;
                }
            }
            return true;
        }

        public ActionResult ConfirmEmail(string userID, string code)
        {
            var userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> manager = new UserManager<IdentityUser>(userStore);
            var user = manager.FindById(userID);
            CreateTokenProvider(manager, EMAIL_CONFIRMATION);
            try
            {
                IdentityResult result = manager.ConfirmEmail(userID, code);
                if (result.Succeeded)
                    ViewBag.Message = "You are now registered!";
            }
            catch
            {
                ViewBag.Message = "Validation attempt failed!";
            }
            return View();
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            ViewBag.Display = "";
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> manager = new UserManager<IdentityUser>(userStore);
            var user = manager.FindByEmail(email);
            CreateTokenProvider(manager, PASSWORD_RESET);

            if (user != null)
            {
                var code = manager.GeneratePasswordResetToken(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Home", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                
                MailHelper mailer = new MailHelper("anabellchan@gmail.com", "Reset Password", "Please reset yor password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");

                bool response = mailer.EmailFromArvixe();
                const string SUCCESS = "Please check your email to reset password.";
                const string FAIL = "Failure sending email.";


                if (response)
                    ViewBag.Response = SUCCESS;
                else
                    ViewBag.Response = FAIL;

                ViewBag.Display = "style='Display:none'";
                return View();
            }
            else
            {
                // do not reveal that user does not exist. 
                return RedirectToAction("ForgotPassword");  
            } 
        }

        [HttpGet]
        public ActionResult ResetPassword(string userID, string code)
        {
            ViewBag.PasswordToken = code;
            ViewBag.UserID = userID;
            ViewBag.Result = "";
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(string password, string confirmPassword,
                                          string passwordToken, string userID)
        {


            var userStore = new UserStore<IdentityUser>();
            UserManager<IdentityUser> manager = new UserManager<IdentityUser>(userStore);
            var user = manager.FindById(userID);
            CreateTokenProvider(manager, PASSWORD_RESET);

            IdentityResult result = manager.ResetPassword(userID, passwordToken, password);
            if (result.Succeeded)
                ViewBag.Result = "The password has been reset.";
            else
                ViewBag.Result = "The password has not been reset.";
            return View();
        }




    }
}

