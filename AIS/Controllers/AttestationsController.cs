using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using AIS.Entities;
using AIS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Word;
using Table = Microsoft.Office.Interop.Word.Table;

namespace AIS.Controllers
{
    // GET: Attestations
    [Authorize(Roles = "Преподаватель,Администратор")]
    public class AttestationsController : Controller
    {
        private readonly AISEntities db = new AISEntities();

        
        public ActionResult Index(int? idTypeAttestation)
        {
            IEnumerable<Attestation> attestations;

            var typeAttestations = db.TypeAttestation.ToList();
            typeAttestations.Insert(0, new TypeAttestation { Title = "Все", IdTypeAttestation = 0 });

            var idCurentUser = Int32.Parse(User.Identity.GetUserId());

            if (User.IsInRole("Администратор"))
            {
                attestations = db.Attestation.Include(a => a.Group).Include(a => a.Discipline).Include(a => a.Teachers).Include(a => a.TypeAttestation);
                
            }
            else
            {
                attestations = db.Attestation.Include(a => a.Group).Include(a => a.Discipline).Include(a => a.Teachers).Include(a => a.TypeAttestation).Where(a => a.IdTeachers == idCurentUser);
            }

            if (idTypeAttestation != null && idTypeAttestation != 0)
            {
                attestations = attestations.Where(a => a.IdTypeAttestation == idTypeAttestation);
            }
            

            AttestationListViewModel attestationListViewModel = new AttestationListViewModel 
            {
                Attestations = attestations, 
                TypeAttestations = new SelectList(typeAttestations, "IdTypeAttestation", "Title") 
            };

            return View(attestationListViewModel);
        }

        // GET: Attestations
        public ActionResult BlockAttestations(int? idAttestations)
        {
            var attestation = db.Attestation.Find(idAttestations);
            attestation.Сompleted = true;
            db.Entry(attestation).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }


        // GET: Attestations
        public FileResult GetAttestationVedomost(int? idAttestations)
        {

            var attestation = db.Attestation.Find(idAttestations);
            var vedomosti = db.Vedomosti.Where(v => v.IdAttestation == idAttestations).ToList();
            int count = vedomosti.Count;



            if (System.IO.File.Exists(HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость экзамен по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx")))
            {
                System.IO.File.Delete(HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость экзамен по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx"));
            }

            var items = new Dictionary<string, string>()
            {
                {"<DISCIPLINE>", attestation.Discipline.Title},
                {"<NUMBERCOURCE>", attestation.Group.CourseNumber},
                {"<TITLEGROUP>", attestation.Group.Title},
                {"<SPECIALITY>", attestation.Group.Speciality.Title},
                {"<FIOPREP>", attestation.Teachers.LastName + " " + attestation.Teachers.FirstName+ " " + attestation.Teachers.Patronymic},
                {"<DATE>", attestation.EndDate.ToString("«dd» MMMM yyyy")}
            };

            Application wordApp = null;
            Document wordDoc;
            Table wordTable;
            try
            {
                wordApp = new Application();

                object missing = Type.Missing;
                object fileName = HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость экзамен.docx");

                wordDoc = wordApp.Documents.Open(ref fileName, ref missing, ref missing, ref missing); //открываем шаблон ведомости

                foreach (var item in items)
                {
                    Find find = wordApp.Selection.Find;
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;

                    object wrap = WdFindWrap.wdFindContinue;
                    object replace = WdReplace.wdReplaceAll;

                    find.Execute(FindText: Type.Missing,
                        MatchCase: false, MatchWholeWord: false, MatchWildcards: false,
                        MatchSoundsLike: missing, MatchAllWordForms: false, Forward: true,
                        Wrap: wrap, Format: false, ReplaceWith: missing, Replace: replace);

                }

                wordTable = wordDoc.Tables[2];

                //заполняем ячейки таблицы
                for (int i = 2; i <= count + 1; i++)
                    for (int j = 1; j <= 5; j++)
                    {
                        var v = vedomosti[i - 2];
                        if (j == 1)
                            wordTable.Cell(i, j).Range.Text = Convert.ToString(i - 1);
                        if (j == 3)
                            wordTable.Cell(i, j).Range.Text = Convert.ToString(v.Student.FirstName + " " + v.Student.LastName + " " + v.Student.Patronymic);
                        //if (j == 4)
                        //    wordTable.Cell(i, j).Range.Text = Convert.ToString(v.TheNumberOfPointsForTheExam);
                        if (j == 4)
                        {
                            string finalGradeString = "";
                            if (v.FinalGrade == "5")
                                finalGradeString = " (отлично)";
                            if (v.FinalGrade == "4")
                                finalGradeString = " (хорошо)";
                            if (v.FinalGrade == "3")
                                finalGradeString = " (удовл.)";
                            if (v.FinalGrade == "2")
                                finalGradeString = " (неудовл.)";
                            wordTable.Cell(i, j).Range.Text = v.FinalGrade + finalGradeString;
                        }
                            
                    }

                object newFile = HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx");

                wordDoc.SaveAs2(newFile);
                wordApp.ActiveDocument.Close();
                wordApp?.Quit();


            }
            catch (Exception ex)
            {
                wordApp?.Quit();
                Console.WriteLine(ex.Message);
            }
            //finally
            //{
            //    wordApp?.Quit();
            //}


            string path = HttpContext.Server.MapPath("~/FilesVedomosti/Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx");
            string fileType = "application/word";
            // Имя файла - необязательно
            string file_name = "Ведомость по " + attestation.Discipline.Title + " группы " + attestation.Group.Title + ".docx";

            return File(path, fileType, file_name);
        }

        // GET: Attestations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);
            List<Student> studentlist = db.Student.Where(s => s.IdGroup == attestation.IdGroup && s.IdStatusStudent == 3).ToList();
            if (attestation == null)
            {
                return HttpNotFound();
            }
            return View(attestation);
        }

        // GET: Attestations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);
            if (attestation == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", attestation.IdGroup);
            ViewBag.IdDiscipline = new SelectList(db.Discipline, "IdDiscipline", "Title", attestation.IdDiscipline);
            ViewBag.IdTeachers = new SelectList(db.Teachers, "IdTeachers", "LastName", attestation.IdTeachers);
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title", attestation.IdTypeAttestation);
            return View(attestation);
        }

        // POST: Attestations/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdAttestation,IdDiscipline,StartDate,EndDate,IdTeachers,IdGroup,IdTypeAttestation")] Attestation attestation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(attestation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", attestation.IdGroup);
            ViewBag.IdDiscipline = new SelectList(db.Discipline, "IdDiscipline", "Title", attestation.IdDiscipline);
            ViewBag.IdTeachers = new SelectList(db.Teachers, "IdTeachers", "LastName", attestation.IdTeachers);
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title", attestation.IdTypeAttestation);
            return View(attestation);
        }

        // GET: Attestations/Create
        public ActionResult Create()
        {
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title");
            ViewBag.IdDiscipline = new SelectList(db.Discipline, "IdDiscipline", "Title");
            ViewBag.IdTeachers = new SelectList(db.Teachers, "IdTeachers", "LastName");
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title");
            return View();
        }

        // POST: Attestations/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdAttestation,IdDiscipline,StartDate,EndDate,IdTeachers,IdGroup,IdTypeAttestation")] Attestation attestation)
        {
            if (ModelState.IsValid)
            {
                db.Attestation.Add(attestation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", attestation.IdGroup);
            ViewBag.IdDiscipline = new SelectList(db.Discipline, "IdDiscipline", "Title", attestation.IdDiscipline);
            ViewBag.IdTeachers = new SelectList(db.Teachers, "IdTeachers", "LastName", attestation.IdTeachers);
            ViewBag.IdTypeAttestation = new SelectList(db.TypeAttestation, "IdTypeAttestation", "Title", attestation.IdTypeAttestation);
            return View(attestation);
        }



        // GET: Attestations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attestation attestation = db.Attestation.Find(id);
            if (attestation == null)
            {
                return HttpNotFound();
            }
            return View(attestation);
        }

        // POST: Attestations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Attestation attestation = db.Attestation.Find(id);
            db.Attestation.Remove(attestation);
            db.SaveChanges();
            return RedirectToAction("Index");
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
