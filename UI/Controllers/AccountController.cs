using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MySql.Data.MySqlClient;
using UI.Models;

namespace UI.Controllers
{
    [Authorize]
    [HandleError]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        UrlUtil url = new UrlUtil();

        #region custom helpers


        [AllowAnonymous]
        public ActionResult UserProfileByUserNumber(int UserNumber)
        {
            if (UserNumber > 0)
            {
                DBUtil objDBUtil = new DBUtil(1);
                ApplicationUser AppUser = UserManager.Users.Where(n => n.UserNumber == UserNumber).ToList()[0];

                return View("UserProfile", AppUser);
            }

            TempData["StatusMsg"] = "An error occured while displaying user profile";
            TempData["ErrorPrevention"] = "Make sure that you provided valid user details";
            return View("Error");
        }

        [Authorize]
        public ActionResult Update(ApplicationUser AppUser)
        {

            if (AppUser.UserName != null && AppUser.Email != null)
            {
                if (Regex.IsMatch(AppUser.Email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                {

                    DBUtil objDBUtil = new DBUtil(1);
                    MySqlCommand cmd = new MySqlCommand("spUpdateUser");
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("pEmail", AppUser.Email);
                    cmd.Parameters.AddWithValue("pUserName", AppUser.UserName.AddHandleToUserName());
                    cmd.Parameters.AddWithValue("pDOB", AppUser.DOB);
                    cmd.Parameters.AddWithValue("pLocation", AppUser.Location);
                    cmd.Parameters.AddWithValue("pDisplayStatus", AppUser.DisplayStatus);
                    cmd.Parameters.AddWithValue("pWebSite", AppUser.Website);
                    cmd.Parameters.AddWithValue("pUserNumber", AppUser.UserNumber);

                    DataSet dsUpdateAppUser = objDBUtil.FillDataSet(cmd);
                    AppUser = AppUser.GenModel4mDS(dsUpdateAppUser);
                    return RedirectPermanent(this.GetRequestReferrer());

                }
            }

            TempData["StatusMsg"] = "An error occured while updating user profile";
            TempData["ErrorPrevention"] = "Make sure that you provided valid user details";
            return View("Error");

        }

        [AllowAnonymous]
        public ActionResult UserProfileByUserName(string UserName)
        {
            User.Identity.GetUserByUserName(UserName);
            ApplicationUser AppUser = UserManager.Users.Where(u => u.UserName == UserName).ToList()[0];

            return View("UserProfile", AppUser);
        }

        [AllowAnonymous]
        public ActionResult League()
        {
            ApplicationUser AppUser = new ApplicationUser();
            AppUser.AppUsers = UserManager.Users.ToList();

            TempData["Title"] = "League";

            return View(AppUser);
        }

        #endregion

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //make sure user is not already logged in
            if (Request.IsAuthenticated)
            {
                TempData["UserControlMsg"] = string.Format("You are already logged in as {0}", User.Identity.Name);
                return url.RedirectToLocal(this, null);
            }

            //So that the user can be referred back to where they were when they click logon
            if (string.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null)
                returnUrl = Server.UrlEncode(Request.UrlReferrer.PathAndQuery);

            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.returnUrl = returnUrl;
            }

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true

            //Bug fight tip: keep EF 6.1.1, http://blog.hadafsoft.com/post/2015/01/08/Entity-Framework-612-bug-when-integrating-with-MySql-NET-Connector.aspx
            model.UserName = model.UserName.AddHandleToUserName();
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    TempData["UserControlMsg"] = string.Format(("Welcome back {0}!"), model.UserName);
                    
                    if (TempData["UrlReferrer"] != null)
                    {
                        return Redirect(TempData["UrlReferrer"].ToString());
                    }

                    return url.RedirectToLocal(this, returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToActionPermanent("SendCode", new { returnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, returnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return url.RedirectToLocal(this, model.returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = new ApplicationUser { UserName = model.UserName.AddHandleToUserName(), Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // TODO: For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToActionPermanent("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.UserName);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // TODO: For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToActionPermanent("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToActionPermanent("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToActionPermanent("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { returnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, returnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToActionPermanent("VerifyCode", new { Provider = model.SelectedProvider, returnUrl = model.returnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToActionPermanent("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return url.RedirectToLocal(this, returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToActionPermanent("SendCode", new { returnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.returnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToActionPermanent("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return url.RedirectToLocal(this, returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.returnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            Session.Abandon();
            return RedirectToActionPermanent("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }

}


#region Commented

//MySqlCommand cmd = new MySqlCommand("spGetUserByNumber");
//cmd.CommandType = CommandType.StoredProcedure;
//cmd.Parameters.AddWithValue("pUserNumber", UserNumber);
//DataSet dsUsers = objDBUtil.FillDataSet(cmd);

//if (objDBUtil.Rows_Exists(dsUsers))
//{
//    appusers = new List<ApplicationUser>();
//    DataTable dtUsers = dsUsers.Tables[0];

//    for (int i = 0; i < dtUsers.Rows.Count; i++)
//    {

//        ApplicationUser item = new ApplicationUser();
//        item.UserName = dtUsers.Rows[i]["UserName"].ToString();
//        item.UserNumber = int.Parse(dtUsers.Rows[i]["UserNumber"].ToString());
//        item.ProfileUrl = dtUsers.Rows[i]["ProfileUrl"].ToString();
//        item.FName = dtUsers.Rows[i]["FName"].ToString();
//        item.MName = dtUsers.Rows[i]["MName"].ToString();
//        item.LName = dtUsers.Rows[i]["LName"].ToString();
//        item.DOB = Convert.ToDateTime(dtUsers.Rows[i]["DOB"].ToString());
//        item.InterestedFields = dtUsers.Rows[i]["InterestedFields"].ToString();
//        item.Title = dtUsers.Rows[i]["Title"].ToString();
//        item.MarksScore = int.Parse(dtUsers.Rows[i]["MarksScore"].ToString());
//        item.SchoolIds = int.Parse(dtUsers.Rows[i]["SchoolIds"].ToString());
//        item.BadgeCount = int.Parse(dtUsers.Rows[i]["BadgeCount"].ToString());
//        item.TrophyCount = int.Parse(dtUsers.Rows[i]["TrophyCount"].ToString());
//        item.CertsCount = int.Parse(dtUsers.Rows[i]["CertsCount"].ToString());
//        item.Location = dtUsers.Rows[i]["Location"].ToString();
//        item.Address = dtUsers.Rows[i]["Address"].ToString();
//        item.ClassmateCount = int.Parse(dtUsers.Rows[i]["ClassmateCount"].ToString());
//        item.Bio = dtUsers.Rows[i]["Bio"].ToString();
//        item.BookIds = dtUsers.Rows[i]["BookIds"].ToString();
//        item.InterestsIds = dtUsers.Rows[i]["InterestsIds"].ToString();
//        item.TestimonialsIds = dtUsers.Rows[i]["TestimonialsIds"].ToString();
//        item.CertIds = dtUsers.Rows[i]["CertIds"].ToString();
//        item.AwardIds = dtUsers.Rows[i]["AwardIds"].ToString();
//        item.EventIds = dtUsers.Rows[i]["EventIds"].ToString();
//        item.TeamIds = dtUsers.Rows[i]["TeamIds"].ToString();
//        item.Joined = Convert.ToDateTime(dtUsers.Rows[i]["Joined"].ToString());
//        item.UserAcStatus = int.Parse(dtUsers.Rows[i]["UserAcStatus"].ToString());
//        item.DisplayStatus = dtUsers.Rows[i]["DisplayStatus"].ToString();
//        //TODO: display user profile data

//        appusers.Add(item);
//    }

//}

#endregion

