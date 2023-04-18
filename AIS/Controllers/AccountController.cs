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
            try
            {

                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;//Получение текущего менеджера аутентификации
                authenticationManager.SignOut();// Очистка файлов куки и очистка данных авторизованного пользователя
            }
            catch (Exception ex)
            {

                throw ex;
            }

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
        public async Task<ActionResult> Login(LoginViewModel model) //Запрос на аутентификация пользователя
        {
            if (ModelState.IsValid)
            {
                string hashPassword = Models.HashPassword.GetHashPAssword(model.Password);
                var findUsers = dbConnection.Teachers.Where(t => t.Login == model.Login && t.Password == hashPassword).FirstOrDefault(); //Поиск записи в БД по логину и паролю
                if (findUsers != null)
                {
                    this.SignInUser(findUsers, true);
                    return this.RedirectToLocal("/Attestations/Index"); //Перенеаправление авторизованного пользователя на страницу аттестаций
                }
                else { ModelState.AddModelError(string.Empty, "Некорректные логин или пароль."); }
            }
            return this.View(model);
        }



        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff() //Запрос на выход пользователя из системы
        {
            try
            {

                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication; //Получение текущего менеджера аутентификации

                authenticationManager.SignOut(); // Очистка файлов куки и очистка данных авторизованного пользователя
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return this.RedirectToAction("Login", "Account");
        }

        private void SignInUser(Teachers teacher, bool isPersistent) //Авторизация пользователя на основе утверждений
        {

            var claims = new List<Claim>();
            try
            {

                claims.Add(new Claim(ClaimTypes.Name, teacher.LastName +" "+ teacher.FirstName + " " + teacher.Patronymic, ClaimValueTypes.String)); //утверждение полного имени пользователя
                claims.Add(new Claim(ClaimTypes.NameIdentifier, teacher.IdTeachers.ToString(), ClaimValueTypes.String)); // утверждение уникального идентификатора пользователя
                
                if (teacher.Role != null)
                {
                    foreach (var role in teacher.Role) //перебор всех ролей пользователя
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Title, ClaimValueTypes.String)); //утверждение ролей пользователя
                    }
                }
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie); //Установка утверждений и типа аутентификации на основе куки
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;

                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties); // Установка утверждений в менеджер аутентификации
                                                                                                                             // и назначение проверки подлинности данных пользователя
                                                                                                                             // на протяжении нескольких запросов

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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