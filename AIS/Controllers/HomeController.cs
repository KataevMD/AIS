﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AIS.Models
{
    public class HomeController : Controller //Контроллер для домашней страницы, которая впоследсвтии будет отображаться после авторизации
    {
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Администратор")]
        [Authorize(Roles = "Преподаватель")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetTemplateImportAttestation()
        {
           

            return View();
        }

        public ActionResult GetGuideUsers()
        {


            return View();
        }
    }
}