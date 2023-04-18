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

        public class DataGetTrue
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
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            holdingAttestations = db.Attestation.Find(id);
            studentList = db.Student.Where(s => s.IdGroup == holdingAttestations.IdGroup && !db.Vedomosti.Select(v => v.IdStudent).Contains(s.IdStudent)).ToList();
            criterias = db.Criteria.Where(c => c.IdDiscipline == holdingAttestations.IdDiscipline).ToList();

            decimal countPoint = 0;
            foreach (var crt in criterias)
            {
                countPoint = countPoint + Convert.ToDecimal(crt.NumberOfPionts);
            }

            maxPointDiscipline = countPoint;
            AttestationStudentViewModel asvm = new AttestationStudentViewModel { Attestations = holdingAttestations, Students = studentList, Criterias = criterias };

            return View(asvm);
        }

        [HttpPost]
        public ActionResult GetStudent(int? idStudent)
        {
            //student = null;
            student = db.Student.Find(idStudent);
            if (student != null)
                return PartialView(student);
            return null;
        }

        [HttpPost]
        public JsonResult SaveResult(DataGetTrue[] dataGets)
        {
            Responce responce = new Responce();
            List<StudentResult> studentResults = new List<StudentResult>();
            if (student == null)
            {
                responce.name = "StudentNotFound";
                responce.id = 0;
                return Json(responce, JsonRequestBehavior.AllowGet);
            }
            else
            {

                int i = 0;

                foreach (var data in dataGets)
                {
                    if (!data.Accepted)
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

                        if ((Decimal.TryParse(criteria.WithdrawPercent.Trim(), out withdrawPerc) && decimal.TryParse(criteria.RemoveAPoint.Trim(), out removePoint)))
                        {
                            if (withdrawPerc == 0 && removePoint == 0)
                            {
                                pointForCriteria = maxPoint;
                            }
                            if (withdrawPerc > 0)
                            {
                                var per = withdrawPerc / 100;
                                pointForCriteria = maxPoint - ((maxPoint * per) * data.Point);
                                if (pointForCriteria < 0)
                                {
                                    pointForCriteria = 0;
                                }
                            }
                            if (removePoint > 0)
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
            foreach (var stdRes in studentResults)
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
            else
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
            studentList.RemoveAll(s => s.IdStudent == idStud);
            vedomosti = null;
            student = null;
            studentResults = null;


            responce.name = "AddSucces";
            responce.id = idStud;

            if (studentList.Count == 0)
            {
                responce.name = "AttestationComleted";
            }

            return Json(responce, JsonRequestBehavior.AllowGet);
        }

    }
}
