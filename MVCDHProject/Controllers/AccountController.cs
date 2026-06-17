using MailKit.Net.Smtp;
//using System.Net.Mail;
using System.Text;
using Krakenar.Core.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MVCDHProject.Models;
using NuGet.Common;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace MVCDHProject.Controllers
{
    public class AccountController : Controller
    {
        #region Fields and Constructor
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        #endregion

        #region Register Code
        [HttpGet]
        public ViewResult Register()
        {

            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                //IdentityUser represents a new user with a given set of attributes 
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = userModel.Name,
                    Email = userModel.Email,
                    PhoneNumber = userModel.Mobile
                };
                //Creates a new user and returns a result which tells about success or failure
                var result = await userManager.CreateAsync(identityUser, userModel.Password);
                if (result.Succeeded)
                {
                    //Implementing logic for sending a mail to confirm the Email
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                    var confirmationUrlLink = Url.Action("ConfirmEmail", "Account", new { UserId = identityUser.Id, Token = token }, Request.Scheme);
                    //Call Send MAil Method
                    SendMail(identityUser, confirmationUrlLink, "Email Confirmation Link");
                    TempData["Title"] = "Email Confirmation Link";
                    TempData["Message"] = "A confirm email link has been sent to registered mail,click on it to confirm.";
                    return View("DisplayMessages");
                }
                else
                {
                    foreach (var Error in result.Errors)
                    {
                        //Displaying error details to the user
                        ModelState.AddModelError("", Error.Description);
                    }

                }
            }
            return View(userModel);
        }
        #endregion

        #region Sending Emails & Confirm Mail
        public void SendMail(IdentityUser identityUser, string requestLink, string subject)
        {
            StringBuilder mailBody = new StringBuilder();
            mailBody.Append("Hello " + identityUser.UserName + "<br/><br/>");
            if (subject == "Email Confirmation Link")
            {
                mailBody.Append("Click on the link below to confirm your email");
            }
            else if (subject == "Change Password Link")
            {
                mailBody.Append("Click on the link below to reset your password");
            }

            mailBody.Append("<br/>");
            mailBody.Append(requestLink);

            mailBody.Append("<br /><br />");
            mailBody.Append("Regards");

            mailBody.Append("<br /><br />");
            mailBody.Append("Customer Support.");

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = mailBody.ToString();

            MailboxAddress fromAddress = new MailboxAddress("Customer Support", "gangreddiwartushar4@gmail.com");
            MailboxAddress toAddress = new MailboxAddress(identityUser.UserName, identityUser.Email);

            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(fromAddress);
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = bodyBuilder.ToMessageBody();

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 465, true);
            //Create App Password using Gmail from Chrome and use here
            smtpClient.Authenticate("gangreddiwartushar4@gmail.com", "wnxe btit nqvu kczx");
            smtpClient.Send(mailMessage);
        }

        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
            if(userId != null && token != null)
            {
                var User = await userManager.FindByIdAsync(userId);
                if(User!=null)
                {
                    var result = await userManager.ConfirmEmailAsync(User, token);
                    if(result.Succeeded)
                    {
                        TempData["Title"] = "Email Confirmation Success";
                        TempData["Message"] = "Email confirmstion id completed.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        StringBuilder Errors = new StringBuilder();
                        foreach(var Error in result.Errors)
                        {
                            Errors.Append(Error.Description + ".");
                        }
                        TempData["Title"] = "Confirmation Email Failure";
                        TempData["Message"] = Errors.ToString();
                        return View("DisplayMessages");
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User Id";
                    TempData["Message"] = "User Id which is present in confirmation email link is in-valid";
                    return View("DisplayMessages");
                }
            }
            else
            {
                TempData["Title"] = "Invalid Email Confirmation Link.";
                TempData["Message"] = "Email confirmation link is invalid,either missing user id or Confirmation Token";
                return View("DisplayMessages");
            }
        }

        #endregion

        #region Login Code
        [HttpGet]
        public ViewResult Login()
        {
            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
           if(ModelState.IsValid)
            {
                //Code to check whether Email is confirmed or not
                var user = await userManager.FindByNameAsync(loginModel.Name);
                if(user!=null && (await userManager.CheckPasswordAsync(user,loginModel.Password)) && user.EmailConfirmed == false)
                {
                    ModelState.AddModelError("", "Your email is not confirmed.");
                    return View(loginModel);
                }

                var result = await signInManager.PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false);
                if(result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return LocalRedirect(loginModel.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View(loginModel);
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion

        #region Forgot Password & Reset Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var User = await userManager.FindByEmailAsync(model.Name);
                if (User != null && await userManager.IsEmailConfirmedAsync(User))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(User);
                    var confirmationUrlLink = Url.Action("ChangePassword", "Account", new { UserId = User.Id, Token = token },Request.Scheme);
                    SendMail(User, confirmationUrlLink, "Change Password Link");
                    TempData["Title"] = "Change Password MAil Generation Failed";
                    TempData["Message"] = "Either the Username you have been sent to your mail,click on it and change password";
                    return View("DisplayMessages");
                }
                else
                {
                    TempData["Title"] = "Change Password Mail Generation Failed";
                    TempData["Message"] = "Either the Username you have entered is in-vailed or your email is not confirmed.";
                    return View("DisplayMessages");
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var User = await userManager.FindByIdAsync(model.UserId);
                if(User!=null)
                {
                    var result = await userManager.ResetPasswordAsync(User, model.Token, model.Password);
                    if(result.Succeeded)
                    {
                        TempData["Title"] = "Reset PAssword Success";
                        TempData["Message"] = "Your passord has been reset succeed.";
                        return View("DisplayMessages");
                    }
                    else
                    {
                        foreach(var Error in result.Errors)
                        {
                            ModelState.AddModelError("", Error.Description);
                        }
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User";
                    TempData["Message"] = "No user exists with the given UserId.";
                    return View("DisplayMessages");
                }
            }
            return View(model);
        }
        #endregion

        #region External Login (Facebook & Google)
        public IActionResult ExternalLogin(string returnUrl,string Provider)
        {
            var url = Url.Action("CallBack", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(Provider, url);
            return new ChallengeResult(Provider, properties);
        }
        public async Task<IActionResult> CallBack(string returnUrl)
        {
            if(string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "~/";
            }
            //LoginViewModel model = new LoginViewModel();
            var info = await signInManager.GetExternalLoginInfoAsync();
            if(info==null)
            {
                ModelState.AddModelError("", "Error loading external login information.");
                return View("Login");
            }
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if(signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if(email!=null)
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if(user==null)
                    {
                        user = new IdentityUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone),
                        };
                        var identityResult = await userManager.CreateAsync(user);
                    }
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, false);
                    return LocalRedirect(returnUrl);
                }
                TempData["Title"] = "Error";
                TempData["Message"] = "Email claim not received from third party provided.";
                return RedirectToAction("DisplayMessages");
            }
        }
        #endregion
    }
}
