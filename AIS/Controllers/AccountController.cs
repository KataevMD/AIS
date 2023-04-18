using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using AIS.Models;
using System.Collections.Generic;
using System.Data.Entity;
using AIS.Entities;

namespace AIS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private Entities.AISEntities dbConnection = new Entities.AISEntities();

        public AccountController()
        {
        }



        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Attestations/Index");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashPassword = Models.HashPassword.GetHashPAssword(model.Password);
                var findUsers = dbConnection.Teachers.Where(t => t.Login == model.Login && t.Password == hashPassword).FirstOrDefault();
                if (findUsers != null)
                {
                    this.SignInUser(findUsers, false);
                    return this.RedirectToLocal("/Attestations/Index");
                }
                else { ModelState.AddModelError(string.Empty, "Invalid username or password."); }
                //var loginAccount = dbConnection.Teachers
                //return View(model);
            }
            return this.View(model);
        }



        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            try
            {

                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;

                authenticationManager.SignOut();
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return this.RedirectToAction("Login", "Account");
        }

        private void SignInUser(Teachers teacher, bool isPersistent)
        {

            var claims = new List<Claim>();
            try
            {

                claims.Add(new Claim(ClaimTypes.Name, teacher.FirstName));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, teacher.IdTeachers.ToString()));
                if (teacher.Role != null)
                {
                    foreach (var role in teacher.Role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Title, ClaimValueTypes.String));
                    }
                }
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;

                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (_userManager != null)
        //        {
        //            _userManager.Dispose();
        //            _userManager = null;
        //        }

        //        if (_signInManager != null)
        //        {
        //            _signInManager.Dispose();
        //            _signInManager = null;
        //        }
        //    }

        //    base.Dispose(disposing);
        //}

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
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