using AIS.Entities;
using AIS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AIS.Controllers
{
    // GET: Attestations
    [Authorize(Roles = "Преподаватель,Администратор")]
    public class HoldingAttestationsController : Controller
    {

        public class DataGetTrue // Класс для хранения информации об принятых и непринятых критериях для тсудента за текущую аттестацию
        {
            public bool Accepted { get; set; }
            [Required]
            public decimal Point { get; set; }
        }

        public class Responce
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private AISEntities db = new AISEntities();
        public static Attestation holdingAttestations;
        private static List<Criteria> criterias;
        private static List<Student> studentList;
        private static decimal maxPointDiscipline;
        private static Student student;

        // GET: HoldingAttestations
        public ActionResult Index(int? id) //Запрос на получение страницы с оцениванием аттестации
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            holdingAttestations = db.Attestation.Find(id); //Получение текущей аттестации
            studentList = db.Student.Where(s => s.IdGroup == holdingAttestations.IdGroup && !db.Vedomosti.Select(v => v.IdStudent).Contains(s.IdStudent)).ToList(); // Получение списка студентов группы
                                                                                                                                                                    // проходящую текущую аттестацию

            criterias = db.Criteria.Where(c => c.IdAttestation == holdingAttestations.IdAttestation).ToList(); // Получение списка криетриев по аттестации

            decimal countPoint = 0;
            foreach (var crt in criterias) // Расчет общего количества баллов за все критерии дисциплины
            {
                countPoint = countPoint + Convert.ToDecimal(crt.NumberOfPionts);
            }

            maxPointDiscipline = countPoint;

            AttestationStudentViewModel asvm = new AttestationStudentViewModel { Attestations = holdingAttestations, Students = studentList, Criterias = criterias }; // Объект для передачи данных в представление

            return View(asvm);
        }

        [HttpPost]
        public ActionResult GetStudent(int? idStudent) // Запрос на выбор студента для оценивания
        {
            //student = null;
            student = db.Student.Find(idStudent);
            if (student != null)
                return PartialView(student);
            return null;
        }

        /// <summary>
        /// Сохранение результатов студента 
        /// </summary>
        /// <param name="dataGets">Принимается из представления HoldingAttestation. Хранит информацию об принятых и непринятых криетриях и количестве допушеных ошибок.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveResult(DataGetTrue[] dataGets) //Запрос на сохранение результатов студента за аттестацию по текущей дисциплине
        {
            Responce responce = new Responce();
            List<StudentResult> studentResults = new List<StudentResult>();

            if (student == null) //Если студент не был выбран, то на клиент отправялется ответ с информацией о невыбранном студенте
            {
                responce.name = "StudentNotFound";
                responce.id = 0;
                return Json(responce, JsonRequestBehavior.AllowGet);
            }
            else //Иначе если студент был выбран
            {

                int i = 0;

                foreach (var data in dataGets) //Перебор полученных данных о принятых и непринятых критериях
                {
                    if (!data.Accepted) // Если критерий не принят, то переход к следующему криетрию
                    {
                        i++;
                        continue;
                    }
                    else
                    {
                        Criteria criteria = criterias[i];
                        StudentResult studentResult = new StudentResult();

                        studentResult.IdStudent = student.IdStudent;
                        studentResult.IdCriteria = criteria.IdCriteria;

                        decimal maxPoint = Convert.ToDecimal(criteria.NumberOfPionts);
                        decimal pointForCriteria = 0;
                        decimal withdrawPerc = 0;
                        decimal removePoint;


                        //Проверка на возможность конвертировать Проценты снятия и Баллы снятия за ошибки
                        if ((Decimal.TryParse(criteria.WithdrawPercent.Trim(), out withdrawPerc) && decimal.TryParse(criteria.RemoveAPoint.Trim(), out removePoint)))
                        {
                            if (withdrawPerc == 0 && removePoint == 0) //Если процент снятия и балл снятия равны 0, то в результат студента за критерий устанавливается максимальный балл
                            {
                                pointForCriteria = maxPoint;
                            }
                            if (withdrawPerc > 0) // если доступны Проценты снятия, то происходит перерасчет максимального балла критерия в балл с учетом допушенных ошибок,
                                                  // и записывается в резульат студента за криетрий
                            {
                                var per = withdrawPerc / 100;
                                pointForCriteria = maxPoint - ((maxPoint * per) * data.Point);
                                if (pointForCriteria < 0)
                                {
                                    pointForCriteria = 0;
                                }
                            }
                            if (removePoint > 0) // Так же как и для Процента снятия, происходит перерасчет для Балла снятия
                            {
                                pointForCriteria = maxPoint - (removePoint * data.Point);
                                if (pointForCriteria < 0)
                                {
                                    pointForCriteria = 0;
                                }
                            }
                        }
                        studentResult.NumberOfPointsForCriteria = pointForCriteria.ToString();
                        studentResults.Add(studentResult);
                        db.StudentResult.Add(studentResult);
                        db.SaveChanges();
                    }
                    i++;
                }

                
            }

            decimal maxPointStudent = 0;
            foreach (var stdRes in studentResults) //Расчет итогового балла студента за все принятые критерии
            {
                maxPointStudent = maxPointStudent + Convert.ToDecimal(stdRes.NumberOfPointsForCriteria.Trim(), new NumberFormatInfo() { NumberDecimalSeparator = "," });
            }

            Vedomosti vedomosti = new Vedomosti();
            vedomosti.IdAttestation = holdingAttestations.IdAttestation;
            vedomosti.IdStudent = student.IdStudent;
            vedomosti.RecordingDate = DateTime.Now;
            vedomosti.TheNumberOfPointsForTheExam = maxPointStudent.ToString();

            if (maxPointStudent == 0)
            {
                vedomosti.FinalGrade = "2";
            }
            else //Перевод оценки из 100 бальной системы в 5-ти бальную с учетом процентностного соотношения
            {
                decimal coeff = maxPointDiscipline / maxPointStudent;
                decimal percent = 100 / coeff;

                if (percent >= 81)
                    vedomosti.FinalGrade = "5";
                if (percent <= 80 && percent >= 71)
                    vedomosti.FinalGrade = "4";
                if (percent <= 70 && percent >= 51)
                    vedomosti.FinalGrade = "3";
                if (percent <= 50)
                    vedomosti.FinalGrade = "2";
            }

            db.Vedomosti.Add(vedomosti);
            db.SaveChanges();
            int idStud = student.IdStudent;
            studentList.RemoveAll(s => s.IdStudent == idStud); //Удаление студента из списка проверки и выставления оценки
            vedomosti = null;
            student = null;
            studentResults = null;


            responce.name = "AddSucces";
            responce.id = idStud;

            if (studentList.Count == 0) // Проверка на то, если всем студентам были выставлены оценки, то аттестация считается завершенной
            {
                responce.name = "AttestationComleted";
            }

            return Json(responce, JsonRequestBehavior.AllowGet);
        }

    }
}
