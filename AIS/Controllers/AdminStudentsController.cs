using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AIS.Entities;

namespace AIS.Controllers
{
    public class AdminStudentsController : Controller
    {
        private AISEntities db = new AISEntities();

        // GET: AdminStudents
        public ActionResult Index()
        {
            var student = db.Student.Include(s => s.Group).Include(s => s.StatusStudent);
            return View(student.ToList());
        }

        // GET: AdminStudents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Student.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: AdminStudents/Create
        public ActionResult Create()
        {
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title");
            ViewBag.IdStatusStudent = new SelectList(db.StatusStudent, "IdStatusStudent", "Title");
            return View();
        }

        // POST: AdminStudents/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdStudent,FirstName,LastName,Patronymic,Email,Telephone,IdGroup,IdSpeciality,IdStatusStudent")] Student student)
        {
            if (ModelState.IsValid)
            {
                var group = db.Group.Find(student.IdGroup);
                student.IdSpeciality = group.IdSpeciality;
                student.IdStatusStudent = 3;

                db.Student.Add(student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", student.IdGroup);
            ViewBag.IdStatusStudent = new SelectList(db.StatusStudent, "IdStatusStudent", "Title", student.IdStatusStudent);
            return View(student);
        }

        // GET: AdminStudents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Student.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", student.IdGroup);
            ViewBag.IdStatusStudent = new SelectList(db.StatusStudent, "IdStatusStudent", "Title", student.IdStatusStudent);
            return View(student);
        }

        // POST: AdminStudents/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdStudent,FirstName,LastName,Patronymic,Email,Telephone,IdGroup,IdSpeciality,IdStatusStudent")] Student student)
        {
            if (ModelState.IsValid)
            {
                var group = db.Group.Find(student.IdGroup);
                student.IdSpeciality = group.IdSpeciality;

                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdGroup = new SelectList(db.Group, "IdGroup", "Title", student.IdGroup);
            ViewBag.IdStatusStudent = new SelectList(db.StatusStudent, "IdStatusStudent", "Title", student.IdStatusStudent);
            return View(student);
        }

        // GET: AdminStudents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Student.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: AdminStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Student.Find(id);
            db.Student.Remove(student);
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
