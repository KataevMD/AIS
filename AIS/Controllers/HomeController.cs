using AIS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System.Diagnostics;
using Microsoft.AspNet.Identity;

namespace AIS.Models
{
    public class HomeController : Controller //Контроллер для домашней страницы, которая впоследсвтии будет отображаться после авторизации
    {
        private readonly Entities.AISEntities db = new Entities.AISEntities();
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Администратор")]
        [Authorize(Roles = "Преподаватель")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetTemplateImportAttestation()
        {
            var idCurentUser = Int32.Parse(User.Identity.GetUserId());
            var curentUser = db.Teachers.Find(idCurentUser);

            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Workbook excelDoc = null;

            Worksheet worksheetDiscipline = null;
            Worksheet worksheetGroups = null;
            Worksheet worksheetTypeAttestation = null;
            try
            {
                excelApp = new Microsoft.Office.Interop.Excel.Application();
                object missing = Type.Missing;
                string fileName = HttpContext.Server.MapPath("~/MainTemplates/Шаблон для импорта криетриев.xlsx"); //Путь к шаблону ведомости
                excelDoc = excelApp.Workbooks.Open(fileName); //открываем шаблон ведомости

                var listDiscipline = curentUser.DisciplineTeachers.Where(dt => dt.IdTeacher == idCurentUser).ToList();

                //var listDiscipline = db.Discipline.Where(d => d.DisciplineTeachers.).ToList();
                var listGroups = db.Group.Where(g => g.IdSpeciality == curentUser.IdSpeciality).ToList();
                var listTypeAttestation = db.TypeAttestation.ToList();

                worksheetDiscipline = excelDoc.Worksheets[2];
                worksheetGroups = excelDoc.Worksheets[3];
                worksheetTypeAttestation = excelDoc.Worksheets[4];

                for (int i = 1; i < 300; i++)
                {
                    worksheetDiscipline.Cells[i + 1, 1].Value = "";
                    worksheetDiscipline.Cells[i + 1, 2].Value = "";
                }

                for (int i = 1; i < 300; i++)
                {
                    worksheetGroups.Cells[i + 1, 1].Value = "";
                    worksheetGroups.Cells[i + 1, 2].Value = "";
                }

                for (int i = 1; i < 300; i++)
                {
                    worksheetTypeAttestation.Cells[i + 1, 1].Value = "";
                    worksheetTypeAttestation.Cells[i + 1, 2].Value = "";
                }

                for (int i = 1; i <= listDiscipline.Count; i++)
                {
                    worksheetDiscipline.Cells[i + 1, 1].Value = i;
                    worksheetDiscipline.Cells[i + 1, 2].Value = listDiscipline[i - 1].Discipline.Title;
                }

                for (int i = 1; i <= listGroups.Count; i++)
                {
                    worksheetGroups.Cells[i + 1, 1].Value = i;
                    worksheetGroups.Cells[i + 1, 2].Value = listGroups[i - 1].Title;
                }

                for (int i = 1; i <= listTypeAttestation.Count; i++)
                {
                    worksheetTypeAttestation.Cells[i + 1, 1].Value = i;
                    worksheetTypeAttestation.Cells[i + 1, 2].Value = listTypeAttestation[i - 1].Title;
                }

                
                worksheetDiscipline = null;
                worksheetGroups = null;
                worksheetTypeAttestation = null;
                
                excelDoc.Save();
                excelDoc.Close(false); //закрытие активного документа
                excelApp.Quit();
                excelDoc = null;
                excelApp = null;
                

                GC.Collect();
                CloseProcess();
            }
            catch (Exception ex)
            {
                worksheetDiscipline = null;
                worksheetGroups = null;
                worksheetTypeAttestation = null;

                excelDoc.Save();
                excelDoc.Close(false); //закрытие активного документа
                excelApp.Quit();
                excelDoc = null;
                excelApp = null;


                GC.Collect();
                CloseProcess();
                Console.WriteLine(ex.Message);
            }

            string path = HttpContext.Server.MapPath("~/MainTemplates/Шаблон для импорта криетриев.xlsx"); //путь до сохраненной ранее ведомости
            string fileType = "application/vnd.ms-excel";
            // Имя файла - необязательно. Это то имя файла, которое будет задано скачиваемому файлу
            string file_name = "Шаблон для импорта криетриев.xlsx";
            
            return File(path, fileType, file_name); //отправка на клиент файла ведомости
           
        }

        public void CloseProcess()
        {
            Process[] List;
            List = Process.GetProcessesByName("EXCEL");
            foreach (Process proc in List)
            {
                proc.Kill();
            }

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}