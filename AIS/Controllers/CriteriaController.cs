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
    public class CriteriaController : Controller
    {
        private AISEntities db = new AISEntities();


        // GET: Criteria/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Criteria criteria = db.Criteria.Find(id);
            if (criteria == null)
            {
                return HttpNotFound();
            }
            return View(criteria);
        }

        // GET: Criteria/Create
        public ActionResult Create(int? IdAttestation)
        {

            ViewBag.IdAttestation = IdAttestation;
            return View();
        }

        // POST: Criteria/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCriteria,IdAttestation,Title,Description,NumberOfPionts,WithdrawPercent,RemoveAPoint,Deleted")] Criteria criteria)
        {
            if (ModelState.IsValid)
            {
                db.Criteria.Add(criteria);
                db.SaveChanges();
                return RedirectToAction("Edit", "Attestations", new { id = criteria.IdAttestation });
            }

            ViewBag.IdAttestation = criteria.IdAttestation;
            return View(criteria);
        }

        // GET: Criteria/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Criteria criteria = db.Criteria.Find(id);
            if (criteria == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdAttestation = criteria.IdAttestation;
            return View(criteria);
        }

        // POST: Criteria/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCriteria,IdAttestation,Title,Description,NumberOfPionts,WithdrawPercent,RemoveAPoint,Deleted")] Criteria criteria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(criteria).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edit", "Attestations", new { id = criteria.IdAttestation });
            }
            ViewBag.IdAttestation = criteria.IdAttestation;
            return View(criteria);
        }

        // GET: Criteria/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Criteria criteria = db.Criteria.Find(id);
            if (criteria == null)
            {
                return HttpNotFound();
            }
            return View(criteria);
        }

        // POST: Criteria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Criteria criteria = db.Criteria.Find(id);
            criteria.Deleted = true;
            db.Entry(criteria).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Edit", "Attestations", new { id = criteria.IdAttestation });
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
