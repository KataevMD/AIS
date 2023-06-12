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
    public class AdminTeachersController : Controller
    {
        private AISEntities db = new AISEntities();

        // GET: AdminTeachers
        public ActionResult Index()
        {
            var teachers = db.Teachers.Include(t => t.Speciality).Include(t => t.StatusTeacher);
            return View(teachers.ToList());
        }

        // GET: AdminTeachers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teachers teachers = db.Teachers.Find(id);
            if (teachers == null)
            {
                return HttpNotFound();
            }
            return View(teachers);
        }

        // GET: AdminTeachers/Create
        public ActionResult Create()
        {
            ViewBag.IdSpeciality = new SelectList(db.Speciality, "IdSpeciality", "Title");
            ViewBag.IdStatusTeachers = new SelectList(db.StatusTeacher, "IdStatusTeacher", "Title");
            return View();
        }

        // POST: AdminTeachers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdTeachers,LastName,FirstName,Patronymic,Email,IdStatusTeachers,IdSpeciality,Login,Password")] Teachers teachers)
        {
            if (ModelState.IsValid)
            {
               
                teachers.IdStatusTeachers = 3;
                db.Teachers.Add(teachers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdSpeciality = new SelectList(db.Speciality, "IdSpeciality", "Title", teachers.IdSpeciality);
            ViewBag.IdStatusTeachers = new SelectList(db.StatusTeacher, "IdStatusTeacher", "Title", teachers.IdStatusTeachers);
            return View(teachers);
        }

        // GET: AdminTeachers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teachers teachers = db.Teachers.Find(id);
            if (teachers == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdSpeciality = new SelectList(db.Speciality, "IdSpeciality", "Title", teachers.IdSpeciality);
            ViewBag.IdStatusTeachers = new SelectList(db.StatusTeacher, "IdStatusTeacher", "Title", teachers.IdStatusTeachers);
            return View(teachers);
        }

        // POST: AdminTeachers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdTeachers,LastName,FirstName,Patronymic,Email,IdStatusTeachers,IdSpeciality,Login,Password")] Teachers teachers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(teachers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdSpeciality = new SelectList(db.Speciality, "IdSpeciality", "Title", teachers.IdSpeciality);
            ViewBag.IdStatusTeachers = new SelectList(db.StatusTeacher, "IdStatusTeacher", "Title", teachers.IdStatusTeachers);
            return View(teachers);
        }

        // GET: AdminTeachers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teachers teachers = db.Teachers.Find(id);
            if (teachers == null)
            {
                return HttpNotFound();
            }
            return View(teachers);
        }

        // POST: AdminTeachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Teachers teachers = db.Teachers.Find(id);
            db.Teachers.Remove(teachers);
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
